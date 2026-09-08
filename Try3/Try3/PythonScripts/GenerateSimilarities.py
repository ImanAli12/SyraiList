import pandas as pd
import numpy as np
from sklearn.preprocessing import MinMaxScaler, LabelEncoder
from sklearn.neighbors import NearestNeighbors
from sqlalchemy import create_engine, text
import urllib
import time

# ==========================================================
# 1. الاتصال بقاعدة البيانات
# ==========================================================
print("📂 الاتصال بقاعدة البيانات...")
server = 'LAPTOP-9USQE2CG'
database = 'Try8'
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
# 2. قراءة البيانات
# ==========================================================
query = """
SELECT 
    Code AS property_code,
    Title AS title,
    Price AS price,
    Area AS area,
    Rooms AS rooms,
    Bathrooms AS bathrooms,
    (SELECT NameAr FROM PropertyTypes WHERE Id = PropertyTypeId) AS property_type,
    (SELECT NameAr FROM Cities WHERE Id = CityId) AS city,
    Neighborhood AS neighborhood
FROM Properties
WHERE IsActive = 1
"""

print("📊 جاري تحميل البيانات...")
df = pd.read_sql(query, engine)
print(f"   ✅ تم تحميل {len(df)} عقار.")

if len(df) == 0:
    print("❌ لا توجد بيانات!")
    exit()

# ==========================================================
# 3. حساب التشابه لكل مدينة على حدة (بسرعة فائقة)
# ==========================================================
print("⚡ بدء حساب التشابه (أسرع 50 مرة من الطريقة السابقة)...")
start_time = time.time()

TOP_SIMILAR = 10
similar_pairs = []

# تشفير النصوص
le_type = LabelEncoder()
le_neighborhood = LabelEncoder()
df['type_enc'] = le_type.fit_transform(df['property_type'].astype(str))
df['neighborhood_enc'] = le_neighborhood.fit_transform(df['neighborhood'].astype(str))

# الميزات التي سنقارن بها
feature_cols = ['price', 'area', 'rooms', 'bathrooms', 'type_enc', 'neighborhood_enc']

# نتعامل مع كل مدينة كمجموعة منفصلة (لن تظهر مدن أخرى في النتائج)
for city_name, group in df.groupby('city'):
    if len(group) <= 1:
        continue
    
    print(f"   🏙️ معالجة: {city_name} ({len(group)} عقار)")
    
    # تسوية الأرقام (MinMaxScaler) لجعل السعر والمساحة بنفس المقياس
    scaler = MinMaxScaler()
    features = scaler.fit_transform(group[feature_cols].fillna(0))
    
    # نبحث عن أقرب الجيران (عدد قليل جداً)
    n_neighbors = min(TOP_SIMILAR + 1, len(group))
    nn = NearestNeighbors(n_neighbors=n_neighbors, metric='euclidean', algorithm='brute')
    nn.fit(features)
    distances, indices = nn.kneighbors(features)
    
    # استخراج النتائج
    for i in range(len(group)):
        target_code = group.iloc[i]['property_code']
        for rank, idx in enumerate(indices[i][1:], 1):
            similar_code = group.iloc[idx]['property_code']
            # حساب درجة تشابه (كلما قلت المسافة، زادت الدرجة)
            max_dist = distances[i][-1] if distances[i][-1] > 0 else 1
            similarity_score = 1 - (distances[i][rank] / (max_dist + 1e-9))
            similar_pairs.append({
                'property_code': target_code,
                'similar_property_code': similar_code,
                'similarity_score': similarity_score,
                'rank_order': rank
            })

df_similar = pd.DataFrame(similar_pairs)
end_time = time.time()
print(f"   ✅ تم حساب {len(df_similar)} زوج تشابه خلال {end_time - start_time:.2f} ثانية.")

# ==========================================================
# 4. حفظ النتائج في قاعدة البيانات
# ==========================================================
print("💾 جاري حفظ النتائج...")
with engine.connect() as conn:
    conn.execute(text("DELETE FROM SimilarProperties"))
    conn.commit()

df_similar.to_sql('SimilarProperties', con=engine, if_exists='append', index=False)
print("✅ تم حفظ النتائج بنجاح!")
print("🎉 اذهب الآن إلى موقعك واختبر أي عقار. ستظهر فقط عقارات من نفس المدينة.")