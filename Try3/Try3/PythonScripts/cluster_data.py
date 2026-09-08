import pandas as pd
import numpy as np
from sklearn.preprocessing import MinMaxScaler, LabelEncoder
from sklearn.cluster import KMeans
from sqlalchemy import create_engine, text
import urllib

# ==========================================================
# 1. الاتصال بقاعدة البيانات
# ==========================================================
print("📂 الاتصال بقاعدة البيانات...")
server = 'LAPTOP-9USQE2CG'
database = 'Try8'  # أو Try4
username = 'sa'
password = '1111'

connection_string = f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={server};DATABASE={database};UID={username};PWD={password};TrustServerCertificate=yes;"
params = urllib.parse.quote_plus(connection_string)
engine = create_engine("mssql+pyodbc:///?odbc_connect=" + params)

try:
    with engine.connect() as conn:
        conn.execute(text("SELECT 1"))
        print("   ✅ تم الاتصال بقاعدة البيانات بنجاح.")
except Exception as e:
    print(f"   ❌ فشل الاتصال: {e}")
    exit()

# ==========================================================
# 2. قراءة البيانات (العقارات النشطة غير المباعة)
# ==========================================================
query = """
SELECT 
    Id,
    Code,
    Price,
    Area,
    Rooms,
    Bathrooms,
    CityId,
    PropertyTypeId,
    Neighborhood,
    (SELECT NameAr FROM Cities WHERE Id = CityId) AS CityName
FROM Properties
WHERE IsActive = 1 AND (IsSold IS NULL OR IsSold = 0)
"""
df = pd.read_sql(query, engine)
print(f"   ✅ تم تحميل {len(df)} عقار نشط.")

if df.empty:
    print("❌ لا توجد بيانات نشطة!")
    exit()

# ==========================================================
# 3. حذف البيانات القديمة
# ==========================================================
with engine.connect() as conn:
    conn.execute(text("DELETE FROM ClusterCenters"))
    conn.execute(text("DELETE FROM NormalizationParams"))
    conn.execute(text("DELETE FROM NeighborhoodEncodings"))
    conn.execute(text("UPDATE Properties SET ClusterId = NULL WHERE IsActive = 1"))
    conn.commit()
print("   🗑️ تم مسح البيانات القديمة.")

# ==========================================================
# 4. تطبيق K-Means لكل مدينة مع أوزان للميزات الفئوية
# ==========================================================
print("⚡ بدء تجميع العقارات (K-Means++ مع أوزان)...")

# 🔥 الأوزان المخصصة (عدّل منها حسب الحساسية المطلوبة)
WEIGHT_PRICE = 2.0
WEIGHT_AREA = 1.5
WEIGHT_ROOMS = 1.0
WEIGHT_BATHROOMS = 1.0
WEIGHT_PROPERTY_TYPE = 3.0   # 🔥 زيادة الوزن لنوع العقار
WEIGHT_NEIGHBORHOOD = 2.5    # 🔥 زيادة الوزن للحي

feature_cols = ['Price', 'Area', 'Rooms', 'Bathrooms', 'PropertyTypeId', 'Neighborhood']
all_centers = []

for city_name, group in df.groupby('CityName'):
    if len(group) < 3:
        print(f"   ⚠️ تخطي مدينة {city_name} (عدد العقارات أقل من 3)")
        continue

    print(f"   🏙️ معالجة: {city_name} ({len(group)} عقار)")

    # ----- أ. تشفير الحي -----
    le = LabelEncoder()
    group['NeighborhoodEnc'] = le.fit_transform(group['Neighborhood'].astype(str))

    city_id_val = int(group['CityId'].iloc[0])
    with engine.connect() as conn:
        for name, enc in zip(group['Neighborhood'], group['NeighborhoodEnc']):
            conn.execute(
                text("""
                    INSERT INTO NeighborhoodEncodings (CityId, NeighborhoodName, EncodedValue)
                    VALUES (:city, :name, :enc)
                """),
                {"city": city_id_val, "name": name, "enc": int(enc)}
            )
        conn.commit()
    print(f"      ✅ تم حفظ ترميز الأحياء ({len(group['Neighborhood'].unique())} حي فريد).")

    # ----- ب. تجهيز الميزات للتطبيع مع الأوزان -----
    data = group[['Price', 'Area', 'Rooms', 'Bathrooms', 'PropertyTypeId', 'NeighborhoodEnc']].fillna(0)

    # ----- ج. تطبيع Min-Max -----
    scaler = MinMaxScaler()
    scaled_data = scaler.fit_transform(data)

    # 🔥 تطبيق الأوزان على الميزات الفئوية بعد التطبيع
    # الميزات: [Price, Area, Rooms, Bathrooms, PropertyTypeId, NeighborhoodEnc]
    weights = np.array([WEIGHT_PRICE, WEIGHT_AREA, WEIGHT_ROOMS, WEIGHT_BATHROOMS,
                        WEIGHT_PROPERTY_TYPE, WEIGHT_NEIGHBORHOOD])
    weighted_scaled_data = scaled_data * weights

    # حفظ حدود التطبيع (Min/Max) مع تحويل numpy.float64 إلى float
    with engine.connect() as conn:
        for i, col in enumerate(['Price', 'Area', 'Rooms', 'Bathrooms', 'PropertyTypeId', 'NeighborhoodEnc']):
            min_val = float(scaler.data_min_[i])
            max_val = float(scaler.data_max_[i])
            conn.execute(
                text("""
                    INSERT INTO NormalizationParams (CityId, FeatureName, MinValue, MaxValue)
                    VALUES (:city, :name, :min, :max)
                """),
                {"city": city_id_val, "name": col, "min": min_val, "max": max_val}
            )
        conn.commit()
    print(f"      ✅ تم حفظ حدود التطبيع (6 ميزات).")

    # ----- د. تدريب K-Means++ (3 مجموعات) على البيانات الموزونة -----
    kmeans = KMeans(
        n_clusters=3,
        init='k-means++',
        n_init=10,
        max_iter=300,
        random_state=42,
        algorithm='lloyd'
    )
    clusters = kmeans.fit_predict(weighted_scaled_data)

    # تحديث ClusterId في جدول Properties
    with engine.connect() as conn:
        for idx, cluster_id in zip(group['Id'], clusters):
            conn.execute(
                text(f"UPDATE Properties SET ClusterId = {int(cluster_id)} WHERE Id = {int(idx)}")
            )
        conn.commit()
    print(f"      ✅ تم تحديث ClusterId لـ {len(group)} عقار.")

    # ----- هـ. حفظ مراكز التجميع (بالقيم الأصلية غير الموزونة) -----
    centers = scaler.inverse_transform(kmeans.cluster_centers_ / weights)  # نعيد فك التطبيع
    for i, center in enumerate(centers):
        all_centers.append({
            'CityId': city_id_val,
            'ClusterId': int(i),
            'AvgPrice': float(center[0]),
            'AvgArea': float(center[1]),
            'AvgRooms': float(center[2]),
            'AvgBathrooms': float(center[3]),
            'AvgPropertyTypeId': float(center[4]),
            'AvgNeighborhoodEnc': float(center[5])
        })

    print(f"      📊 Inertia (SSE) = {kmeans.inertia_:.2f}")

# ----- و. حفظ المراكز في جدول ClusterCenters -----
if all_centers:
    df_centers = pd.DataFrame(all_centers)
    df_centers.to_sql('ClusterCenters', con=engine, if_exists='append', index=False)
    print(f"   ✅ تم حفظ مراكز التجميع ({len(df_centers)} سجل).")

print("\n🎉 انتهى التجميع بنجاح!")
print(f"   ✅ تم تجميع {len(df)} عقار في {len(df['CityName'].unique())} مدينة.")

import matplotlib.pyplot as plt
from sklearn.decomposition import PCA

