# 🤖 Botic API - Hybrid Application Tracking System

A powerful ASP.NET Core 8.0 API for managing job applications with hybrid automation and manual admin control.

---

## 📋 Table of Contents

1. [Project Overview](#project-overview)
2. [Prerequisites](#prerequisites)
3. [Installation & Setup](#installation--setup)
4. [Running the Project](#running-the-project)
5. [Folder Structure](#folder-structure)
6. [API Endpoints](#api-endpoints)
7. [Default Credentials](#default-credentials)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 Project Overview

**Botic** is a hybrid Application Tracking System (ATS) that combines:

- ✅ **Automated Bot Processing** for technical roles
- ✅ **Manual Admin Management** for non-technical roles
- ✅ **JWT Authentication** for secure access
- ✅ **Role-Based Access Control** (Applicant, Admin, Bot)
- ✅ **PostgreSQL Database** with Entity Framework Core

### Key Features

- Create and track job applications
- Automated status progression for technical roles
- Manual status updates by admins for non-technical roles
- Activity logging for all changes
- Dashboard metrics by user role
- Dry-run mode for bot testing

---

## 🔧 Prerequisites

Before you start, ensure you have installed:

| Requirement | Version | Download |
|------------|---------|----------|
| **.NET SDK** | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/) |
| **PostgreSQL** | 12+ | [postgresql.org](https://www.postgresql.org/) |
| **Visual Studio / VS Code** | Latest | [visualstudio.com](https://visualstudio.microsoft.com/) |
| **Git** | Latest | [git-scm.com](https://git-scm.com/) |

### Verify Installation

```bash
dotnet --version
psql --version
```

---

## 💻 Installation & Setup

### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/BoticAPI.git
cd BoticAPI
```

### Step 2: Create Local Database

Open PostgreSQL and create a database:

```sql
CREATE DATABASE botic_development;
```

### Step 3: Update Connection String

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=botic_development;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Disable"
  }
}
```

**Replace:**
- `YOUR_PASSWORD` → Your PostgreSQL password
- `localhost` → Your database host

### Step 4: Restore Dependencies

```bash
dotnet restore
```

### Step 5: Apply Database Migrations

```bash
dotnet ef database update
```

This will:
- Create all required tables
- Seed default roles and users
- Set up indexes and relationships

---

## 🚀 Running the Project

### Option 1: Using Visual Studio

1. Open `BoticAPI.sln` in Visual Studio
2. Set startup profile to **HTTPS** (recommended)
3. Press **F5** or click **Start**
4. Swagger UI opens at `https://localhost:7136/swagger`

### Option 2: Using CLI

```bash
# Development mode
dotnet run --launch-profile https

# Or with watch mode (auto-reload on changes)
dotnet watch run --launch-profile https
```

### Option 3: Using VS Code

1. Install C# DevKit extension
2. Open terminal in project folder
3. Run:
   ```bash
   dotnet run
   ```

### ✅ Verify It's Running

You should see:
```
✅ Connection string found
✅ Database initialization completed successfully
Now listening on: https://localhost:7136
```

Access Swagger UI: **https://localhost:7136/swagger**

---

## 📁 Folder Structure

```
BoticAPI/
│
├── 📂 Controllers/               # API Endpoints
│   ├── AdminController.cs         # Admin operations (roles, users, apps)
│   ├── ApplicationsController.cs   # Application CRUD operations
│   ├── AuthController.cs          # Login & registration
│   ├── BotController.cs           # Bot trigger & job status
│   └── DashboardController.cs     # Dashboard metrics
│
├── 📂 Services/                  # Business Logic Layer
│   ├── IAuthService.cs           # Auth interface
│   ├── AuthService.cs            # Login & JWT generation
│   ├── IApplicationService.cs    # Application interface
│   ├── ApplicationService.cs     # App status & transitions
│   ├── IBotService.cs            # Bot interface
│   ├── BotService.cs             # Automated processing
│   ├── IDashboardService.cs      # Dashboard interface
│   └── DashboardService.cs       # Metrics calculation
│
├── 📂 Models/                    # Data Entities
│   ├── User.cs                   # User entity
│   ├── Role.cs                   # Role entity
│   ├── Application.cs            # Application entity
│   ├── ActivityLog.cs            # Audit trail
│   └── BotJob.cs                 # Bot job tracking
│
├── 📂 Data/                      # Database Context & Migrations
│   ├── BoticDbContext.cs         # Entity Framework context
│   ├── SeedData.cs               # Database seeding logic
│   └── Migrations/               # EF Core migrations
│       ├── 20251116053304_InitialCreate.cs
│       ├── 20251116053304_InitialCreate.Designer.cs
│       └── BoticDbContextModelSnapshot.cs
│
├── 📂 DTOs/                      # Data Transfer Objects
│   └── RequestModels.cs          # Request/response models
│
├── 📂 Properties/                # Project Configuration
│   └── launchSettings.json       # Launch profiles
│
├── 📄 Program.cs                 # Application startup & configuration
├── 📄 BoticAPI.csproj            # NuGet packages & project settings
├── 📄 appsettings.json           # Production settings
├── 📄 appsettings.Development.json  # Development settings
├── 📄 .gitignore                 # Git ignore rules
└── 📄 BoticAPI.http              # HTTP test requests

```

### 📊 Folder Descriptions

#### **Controllers/** - API Entry Points
Handles HTTP requests from clients. Each controller manages specific business domains:
- **AdminController**: Manage roles, view all users/applications
- **ApplicationsController**: Create applications, update status, view logs
- **AuthController**: User login and registration
- **BotController**: Trigger automated bot, view bot job status
- **DashboardController**: Get role-specific metrics

#### **Services/** - Business Logic
Contains reusable business logic separated into interfaces and implementations:
- **AuthService**: Handles password hashing (BCrypt) and JWT token generation
- **ApplicationService**: Application CRUD, status validation, status transitions
- **BotService**: Automated application processing for technical roles
- **DashboardService**: Calculates metrics based on user role

#### **Models/** - Database Entities
Represents your database tables as C# classes:
- **User**: Stores user information and role assignment
- **Role**: Defines roles (Admin, Bot, Applicant, Developer, etc.)
- **Application**: Job application records
- **ActivityLog**: Audit trail of all status changes
- **BotJob**: Tracks automated bot execution history

#### **Data/** - Database Layer
Manages database connections and migrations:
- **BoticDbContext**: EF Core DbContext with table configurations
- **Migrations/**: Version control for database schema changes
- **SeedData**: Populates initial roles and users

#### **DTOs/** - Data Transfer Objects
Request/response models for API validation:
- **LoginRequest**: Email + password
- **RegisterRequest**: User registration details
- **CreateApplicationRequest**: New application data
- **UpdateStatusRequest**: Status update with comment
- **BotRunRequest**: Bot execution parameters

---

## 🔌 API Endpoints

### Authentication
```
POST   /api/auth/register          # Register new user
POST   /api/auth/login             # Get JWT token
```

### Applications
```
POST   /api/applications           # Create application
GET    /api/applications/{id}      # Get application details
GET    /api/applications/my-applications  # Get user's applications
PUT    /api/applications/{id}/status      # Update status (Admin/Bot)
GET    /api/applications/{id}/activity-logs  # Get activity history
```

### Admin Management
```
POST   /api/admin/roles            # Create new role
GET    /api/admin/roles            # List all roles
GET    /api/admin/users            # List all users
GET    /api/admin/applications     # List non-technical applications
GET    /api/admin/all-applications # List all applications
PUT    /api/admin/applications/{id}/status  # Update application status
```

### Bot Operations
```
POST   /api/bot/run                # Trigger bot processing
GET    /api/bot/jobs               # Get recent bot jobs
GET    /api/bot/jobs/{id}          # Get specific bot job
```

### Dashboard
```
GET    /api/dashboard/metrics      # Get role-specific metrics
```

---

## 🔐 Default Credentials

After running migrations, use these credentials to login:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@botic.local | Admin@123 |
| Bot | bot@botic.local | Bot@123 |
| Applicant | applicant@botic.local | Applicant@123 |

### Test Login

```bash
curl -X POST https://localhost:7136/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@botic.local","password":"Admin@123"}'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Login successful"
}
```

---

## 🐛 Troubleshooting

### ❌ Database Connection Failed

**Error**: `Cannot connect to database`

**Solution**:
```bash
# Check PostgreSQL is running
psql -U postgres -c "SELECT version();"

# Verify connection string in appsettings.Development.json
# Test connection:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

### ❌ Migration Error

**Error**: `Pending migrations`

**Solution**:
```bash
# Remove database and reseed
dotnet ef database drop --force
dotnet ef database update
```

### ❌ JWT Token Invalid

**Error**: `401 Unauthorized`

**Solution**:
1. Ensure `appsettings.json` has valid JWT configuration
2. Check token is included: `Authorization: Bearer {token}`
3. Verify token hasn't expired (default: 24 hours)

### ❌ Port Already in Use

**Error**: `Port 7136 already in use`

**Solution**:
```bash
# Use different port
dotnet run -- --urls="https://localhost:7137"

# Or kill the process using the port
# Windows:
netstat -ano | findstr :7136
taskkill /PID {PID} /F

# Linux/Mac:
lsof -i :7136
kill -9 {PID}
```

### ❌ Entity Framework Tools Missing

**Error**: `dotnet ef is not found`

**Solution**:
```bash
dotnet tool install --global dotnet-ef
```

---

## 📚 Development Workflow

### 1. **Making Database Changes**

```bash
# Create a new migration
dotnet ef migrations add YourMigrationName

# Apply the migration
dotnet ef database update

# Revert to previous state
dotnet ef database update PreviousMigrationName
```

### 2. **Adding a New Endpoint**

1. Create DTO in `DTOs/RequestModels.cs`
2. Add logic in `Services/YourService.cs`
3. Add controller method in `Controllers/YourController.cs`
4. Test in Swagger UI

### 3. **Environment Variables**

For production deployment (e.g., Railway):

```bash
# Set environment variables
ConnectionStrings__DefaultConnection=your-db-url
Jwt__Key=your-secret-key
Jwt__Issuer=BoticProd
Jwt__Audience=BoticUsers
Bot__MinSecondsInStage=10
```

---

## 🚢 Deployment

### Deploy to Railway

1. Push code to GitHub
2. Connect to Railway
3. Add PostgreSQL addon
4. Set environment variables
5. Deploy

See `Program.cs` for Railway PORT configuration.

---

## 📖 Additional Resources

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://jwt.io/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

## 📞 Support

For issues or questions:
1. Check **Troubleshooting** section
2. Review application logs in console
3. Check database with: `psql -U postgres -d botic_development`

---

## 📄 License

This project is part of the Botic ATS platform.

---

**Happy Coding! 🚀**
