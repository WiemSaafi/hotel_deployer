# 🏨 TunisiaStay — Application de Réservation Hôtelière

> Mini-projet ASP.NET Core 8 MVC — Hôtels tunisiens
> Inspiré de Bookify (NourEldeenMahmoud/Bookify)

---

## 📋 Fonctionnalités

### 👤 Utilisateur (User)
- Inscription / Connexion sécurisée (ASP.NET Core Identity)
- Recherche de chambres avec filtres avancés (ville, prix, capacité, dates)
- Consultation détaillée des hôtels et chambres
- Réservation de chambres avec calcul automatique du prix
- Gestion de ses réservations (voir, annuler)
- Carte interactive (Leaflet + OpenStreetMap) pour chaque hôtel

### 🔧 Administrateur (Admin)
- Tableau de bord avec statistiques et graphiques (Chart.js)
- **CRUD complet** : Hôtels, Chambres, Clients, Réservations
- Gestion des statuts de réservation (Confirmer / Annuler)
- Upload d'images pour hôtels et chambres
- Gestion des équipements (aménités) par chambre

### 🏗 Architecture & Patrons de conception
- **Repository Pattern** + **Unit of Work** (services)
- **Fluent API** + **Data Annotations** (contraintes)
- **Area** MVC pour l'espace Admin
- **ASP.NET Core Identity** pour authentification/rôles
- **Entity Framework Core** avec SQLite (facilement basculable sur SQL Server)
- Tables : `Hotel`, `Chambre`, `Client`, `Reservation`, `Paiement`, `Amenite`, `ChambreAmenite`, `Avis`, `AspNetUsers` (Compte)

---

## 🚀 Instructions pour lancer le projet

### Prérequis
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Visual Studio 2022+ **OU** VS Code avec extension C#

---

### Option A — Visual Studio 2022

1. Ouvrir `TunisiaStay.sln` dans Visual Studio
2. Attendre la restauration des packages NuGet
3. Ouvrir **Package Manager Console** (Outils → Gestionnaire de package NuGet → Console)
4. Exécuter :
```
Add-Migration InitialCreate
Update-Database
```
5. Appuyer sur **F5** pour lancer

---

### Option B — Terminal / CLI

```bash
# 1. Naviguer dans le dossier
cd TunisiaStay

# 2. Restaurer les dépendances
dotnet restore

# 3. Créer et appliquer la migration (base de données SQLite)
dotnet ef migrations add InitialCreate
dotnet ef database update

# 4. Lancer le projet
dotnet run

# L'app sera disponible sur : https://localhost:5001 ou http://localhost:5000
```

---

### 🔑 Compte Admin par défaut

| Champ    | Valeur                  |
|----------|-------------------------|
| Email    | admin@tunisiastay.tn    |
| Password | Admin@123               |

> L'admin est créé automatiquement au premier démarrage.

---

### 🗄 Changer de base de données

#### SQLite (par défaut — `appsettings.json`)
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=TunisiaStay.db"
}
```

#### SQL Server
1. Remplacer dans `appsettings.json` :
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TunisiaStayDb;Trusted_Connection=True;"
```
2. Dans `Program.cs`, remplacer `UseSqlite(...)` par `UseSqlServer(...)`

#### MySQL
```json
"DefaultConnection": "Server=localhost;Database=TunisiaStay;User=root;Password=yourpass;"
```
Ajouter le package : `dotnet add package Pomelo.EntityFrameworkCore.MySql`

---

## 📁 Structure du projet

```
TunisiaStay/
├── Areas/
│   └── Admin/               ← Espace Administration (Area MVC)
│       └── Views/           ← Vues Admin
├── Controllers/
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── MainControllers.cs   ← Rooms, Hotels, Reservations
│   └── AdminControllers.cs  ← CRUD Admin
├── Data/
│   └── ApplicationDbContext.cs   ← EF Core + Fluent API + Seed
├── Models/
│   └── Models.cs            ← Hotel, Chambre, Client, Reservation, etc.
├── Services/
│   └── Services.cs          ← Repository, UnitOfWork, StatisticsService
├── ViewModels/
│   └── ViewModels.cs        ← LoginVM, RegisterVM, SearchVM, etc.
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _AdminLayout.cshtml
│   ├── Home/Index.cshtml    ← Landing page
│   ├── Account/             ← Login, Register, Profile
│   ├── Hotels/              ← Index, Details avec carte
│   ├── Rooms/               ← Search + Details
│   └── Reservations/        ← Create, MyReservations
├── wwwroot/
│   ├── css/site.css         ← Thème luxe tunisien
│   └── js/site.js
├── Program.cs               ← DI + Auth + Seed auto
├── appsettings.json
└── TunisiaStay.csproj
```

---

## 🛠 Technologies utilisées

| Technologie | Usage |
|-------------|-------|
| ASP.NET Core 8 MVC | Framework principal |
| Entity Framework Core 8 | ORM + Migrations |
| ASP.NET Core Identity | Auth + Rôles |
| SQLite / SQL Server | Base de données |
| Bootstrap 5.3 | UI responsive |
| Font Awesome 6 | Icônes |
| Leaflet.js | Cartes interactives |
| Chart.js 4 | Graphiques dashboard |
| Playfair Display / Jost | Typographie |

---

## 📊 Tables de la base de données

| Table | Description |
|-------|-------------|
| `AspNetUsers` | Comptes (login, password, role via Identity) |
| `AspNetRoles` | Rôles (Admin, User) |
| `Hotels` | Hôtels tunisiens avec localisation GPS |
| `Chambres` | Chambres avec prix, surface, capacité |
| `Clients` | Profils clients liés aux comptes |
| `Reservations` | Réservations avec statut |
| `Paiements` | Paiements liés aux réservations |
| `Amenites` | Équipements (WiFi, Piscine, etc.) |
| `ChambreAmenites` | Table de liaison Chambre ↔ Amenite |
| `Avis` | Avis et notes des clients |

---

*Projet réalisé dans le cadre du cours Atelier de Développement .NET*
