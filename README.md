# 📄 README.md – Smart Real Estate Platform

## 📌 Overview
**RealEstateWebApp** is a comprehensive web platform for real estate management, built with **ASP.NET Core MVC**, **Entity Framework Core**, and **SQL Server**. The platform allows users to list properties, search intelligently using machine learning algorithms (K-Means and KNN), and manage requests and offers. It also provides a dedicated dashboard for administrators to approve properties, manage users, and train models.

---

## ✨ Key Features

### 👤 For Regular Users
- Register and log in (email, password, phone number).
- Add properties (apartments, villas, lands, offices) with images and geographic location.
- Manage own properties (edit, delete, mark as sold).
- Create property search requests with specific criteria.
- Smart search for properties using clustering and similarity algorithms.
- Add properties to favorites.
- Browse properties by city.

### 🛡️ For Administrators
- **Super Admin**: Full statistics, approve/reject properties, manage users, and train algorithms.
- **Governorate Admin**: Only sees pending properties in their assigned governorate.
- Approve or reject pending properties (with image deletion).
- Ban/unban users.
- Promote users to admins (with governorate assignment) or demote them.

### 🤖 Artificial Intelligence
- **K-Means**: Classifies properties into 3 clusters (economy, mid-range, luxury) per city.
- **KNN (Similarity)**: Finds similar properties based on features (price, area, rooms, type, neighborhood).
- Min-Max normalization per city.
- Label encoding for neighborhoods.

---

## 🛠️ Technologies Used

| Technology | Description |
|------------|-------------|
| **ASP.NET Core MVC** | Main framework |
| **Entity Framework Core** | ORM for database access |
| **SQL Server** | Database |
| **ASP.NET Core Identity** | User management and authentication |
| **Python 3.12** | Machine learning scripts |
| **scikit-learn** | K-Means and NearestNeighbors |
| **pandas / numpy** | Data processing |
| **SQLAlchemy / pyodbc** | Database connection from Python |
| **Bootstrap** | UI (assumed) |

---

## 📁 Project Structure

```
RealEstateWebApp/
│
├── Controllers/
│   ├── AccountController.cs          # Login, logout, registration
│   ├── DashboardController.cs        # Admin dashboard
│   ├── HomeController.cs             # Home page
│   ├── PropertiesController.cs       # Property management
│   ├── PropertiesByCityController.cs # Properties by city
│   ├── PropertyRequestController.cs  # Property search requests
│   └── SearchController.cs           # Smart search
│
├── Models/
│   ├── ApplicationUser.cs            # Application user (inherits IdentityUser)
│   ├── City.cs                       # City
│   ├── Neighborhood.cs               # Neighborhood
│   ├── Property.cs                   # Property
│   ├── PropertyImage.cs              # Property images
│   ├── PropertyType.cs               # Property type
│   ├── Feature.cs                    # Property features
│   ├── Favorite.cs                   # Favorites
│   ├── PropertyRequest.cs            # Property request
│   ├── Cluster.cs                    # Cluster (economy/mid/luxury)
│   ├── ClusterCenter.cs              # K-Means centers per city
│   ├── NormalizationParam.cs         # Normalization parameters per city
│   ├── NeighborhoodEncoding.cs       # Neighborhood encoding
│   ├── SimilarProperty.cs            # Similar properties
│   └── ViewModels/                   # View models
│
├── Data/
│   ├── ApplicationDbContext.cs       # Database context
│   └── SeedData.cs                   # Initial data seeding (admin)
│
├── PythonScripts/
│   ├── cluster_data.py               # K-Means clustering
│   └── GenerateSimilarities.py       # KNN similarity
│
├── Views/                            # Razor views
├── wwwroot/                          # Static files (CSS, JS, images)
├── appsettings.json                  # Connection settings
└── Program.cs                        # Entry point
```

---

## ⚙️ Prerequisites

- **.NET 6.0 SDK** or later
- **SQL Server** (2019 or later)
- **Python 3.12** (with required libraries)
- **ODBC Driver 17 for SQL Server**

---

## 🚀 Installation & Setup

### 1️⃣ Clone the Repository
```bash
git clone <repository-url>
cd RealEstateWebApp
```

### 2️⃣ Database Setup
- Open SQL Server Management Studio and create a database named `Try8` (or any name you prefer).
- Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=Try8;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

### 3️⃣ Apply Migrations
```bash
dotnet ef database update
```
> If migrations do not exist, create them first:
> ```bash
> dotnet ef migrations add InitialCreate
> dotnet ef database update
> ```

### 4️⃣ Seed Admin Account
The admin account is created automatically on first run via `SeedData`:
- **Email**: `admin@syrelis.com`
- **Password**: `Admin@123`

You can change these values in `SeedData.cs`.

### 5️⃣ Configure Python Scripts
- Ensure required libraries are installed:
```bash
pip install pandas numpy scikit-learn sqlalchemy pyodbc
```
- Update the connection details at the top of each script (`cluster_data.py` and `GenerateSimilarities.py`):
```python
server = 'YOUR_SERVER'
database = 'Try8'
username = 'sa'
password = 'YOUR_PASSWORD'
```
- **⚠️ Important**: In `DashboardController.cs`, update the following paths to match your machine:
```csharp
string pythonScriptsPath = @"C:\Path\To\PythonScripts";
string pythonPath = @"C:\Path\To\python.exe";
```

### 6️⃣ Run the Project
```bash
dotnet run
```
Then open your browser at `https://localhost:5001` or `http://localhost:5000`.

---

## 🧠 How to Use the Algorithms

1. Log in as an administrator.
2. Go to the dashboard.
3. Click the **"Train Algorithms"** button.
4. The system will run the Python scripts in order:
   - `cluster_data.py`: Clusters properties and saves centers.
   - `GenerateSimilarities.py`: Computes similar properties.
5. After training completes, results will appear in search pages and property details.

---

## ⚠️ Important Notes

- **Absolute Paths**: Python paths are hardcoded in `DashboardController.cs` and must be adjusted before running.
- **Data Inflation**: In `DashboardController`, statistics are inflated to reach 27,300 for demonstration purposes. If you need real data, remove the inflation code.
- **Smart Search**: In `SearchController`, `selectedCluster` is calculated but not used in the actual filtering (code commented out). You can enable it to improve results.
- **Sample Data**: The file `real_estate_clean.xlsx` contains randomly generated data for testing purposes.

---

## 🤝 Contributing
Contributions are welcome! Please open an issue or submit a pull request.

---

## 📜 License
This project is for educational purposes. All rights reserved.
