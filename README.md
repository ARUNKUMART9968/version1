# 🤖 Botic API - Hybrid Application Tracking System

A powerful ASP.NET Core 8.0 API for managing job applications with hybrid automation and manual admin control.

---

## 📋 Table of Contents

1. [About](#about)
2. [Key Features](#key-features)
3. [Architecture](#architecture)
4. [Database Schema](#database-schema)
5. [Prerequisites](#prerequisites)
6. [Installation & Setup](#installation--setup)
7. [Running the Project](#running-the-project)
8. [Folder Structure](#folder-structure)
9. [API Endpoints](#api-endpoints)
10. [Default Credentials](#default-credentials)
11. [Troubleshooting](#troubleshooting)

---

## 📖 About

Botic is a hybrid job application tracker that automates technical role processing while enabling manual admin control for non-technical positions using ASP.NET Core and PostgreSQL.

### 🎯 What We Solve

- ✅ **Automate repetitive tasks** - Bot processes technical applications 24/7
- ✅ **Maintain quality control** - Admins manually handle non-technical roles
- ✅ **Track everything** - Complete audit trail of all changes
- ✅ **Scale easily** - Handle thousands of applications efficiently
- ✅ **Secure by default** - JWT tokens, BCrypt hashing, role-based access

---

## ✨ Key Features

| Feature | Description |
|---------|-------------|
| 🤖 **Smart Bot** | Automatically processes technical role applications |
| 👤 **Manual Control** | Admins manually review non-technical applications |
| 🔐 **JWT Auth** | Secure token-based authentication |
| 👥 **RBAC** | Role-Based Access Control (Admin, Bot, Applicant) |
| 📝 **Audit Trail** | Complete activity logging of all changes |
| 📊 **Dashboard** | Real-time metrics based on user role |
| 🔄 **Status Flow** | Intelligent application status progression |
| 🗄️ **PostgreSQL** | Reliable relational database |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Client Layer                            │
│              Web Browser / Mobile App                       │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼ HTTP/REST
┌─────────────────────────────────────────────────────────────┐
│                    API Layer (ASP.NET Core)                 │
│  ┌──────────┬──────────┬──────────┬──────────┬──────────┐  │
│  │  Auth    │  App     │  Bot     │  Admin   │ Dashboard│  │
│  │Controller│Controller│Controller│Controller│Controller│  │
│  └──────────┴──────────┴──────────┴──────────┴──────────┘  │
└────────────────────┬────────────────────────────────────────┘
                     │
     ┌───────────────┼───────────────┐
     ▼               ▼               ▼
┌──────────┐  ┌──────────────┐  ┌──────────┐
│Auth Svc  │  │Application   │  │Dashboard │
│          │  │Service       │  │Service   │
└──────────┘  └──────────────┘  └──────────┘
     │               │               │
     └───────────────┼───────────────┘
                     ▼
            ┌─────────────────┐
            │  🤖 Bot Service │
            └────────┬────────┘
                     ▼
            ┌─────────────────┐
            │Automation Engine│
            └────────┬────────┘
                     ▼
        ┌────────────────────────┐
        │   PostgreSQL Database  │
        │  (Users, Apps, Logs)   │
        └────────────────────────┘
```

### System Layers

**🖥️ Client Layer** - Web browsers and mobile apps  
**🔌 API Layer** - REST endpoints for all operations  
**⚙️ Business Logic** - Services handling core functionality  
**🔐 Security** - JWT tokens, BCrypt, role-based access  
**🤖 Automation** - Bot engine for technical applications  
**🗄️ Database** - PostgreSQL with complete schema  

---

## 🗄️ Database Schema

### Entity Relationship Diagram

```
┌─────────────┐         ┌──────────────┐
│   ROLE      │────1:M──│   USER       │
│             │         │              │
│ • id (PK)   │         │ • id (PK)    │
│ • name (UK) │         │ • name       │
│ • isTech    │         │ • email (UK) │
└─────────────┘         │ • password   │
      │                 │ • roleId(FK) │
      │                 └──────────────┘
      │                        │
    1:M                      1:M
      │                        │
      ▼                        ▼
┌──────────────────────────────────────┐
│       APPLICATION                    │
│ • id (PK)                            │
│ • applicantId (FK) ──┐               │
│ • roleAppliedId (FK)─┼─► Links to   │
│ • currentStatus      │   User & Role │
│ • createdAt          │               │
│ • lastBotRunAt       │               │
│ • botLockToken       │               │
└────────┬─────────────┘               │
         │                             │
       1:M                             │
         │                             │
         ▼                             │
┌─────────────────────┐                │
│  ACTIVITY_LOG       │                │
│ • id (PK)           │                │
│ • appId (FK) ◄──────┘                │
│ • oldStatus         │                │
│ • newStatus         │                │
│ • updatedBy         │                │
│ • updatedByRole     │                │
│ • comment           │                │
│ • createdAt         │                │
└─────────────────────┘                │
                                       │
┌──────────────────────────────────────┘
│
▼
┌──────────────────┐
│   BOT_JOB        │
│ • id (PK)        │
│ • triggeredBy     │
│ • triggeredAt     │
│ • status          │
│ • totalProcessed  │
│ • totalSucceeded  │
│ • totalFailed     │
│ • details         │
└──────────────────┘
```

### Tables Overview

| Table | Purpose |
|-------|---------|
| **ROLE** | Defines job roles and their type (technical/non-technical) |
| **USER** | Stores user accounts with encrypted passwords |
| **APPLICATION** | Job applications with current status and bot metadata |
| **ACTIVITY_LOG** | Audit trail of all status changes and who made them |
| **BOT_JOB** | Tracks automated bot execution history and results |

---

## 🔧 Prerequisites

Before you start, ensure you have installed:

| Requirement | Version | Download |
|------------|---------|----------|
| **.NET SDK** | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/) |
| **PostgreSQL** | 12+ | [postgresql.org](https://www.postgresql.org/) |
| **Git** | Latest | [git-scm.com](https://git-scm.com/) |
| **Visual Studio / VS Code** | Latest | [visualstudio.com](https://visualstudio.microsoft.com/) |

### Verify Installation

```bash
dotnet --version
psql --version
git --version
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

### Step 4: Restore Dependencies

```bash
dotnet restore
```

### Step 5: Apply Database Migrations

```bash
dotnet ef database update
```

This will:
- ✅ Create all required tables
- ✅ Seed default roles and users
- ✅ Set up indexes and relationships

---

## 🚀 Running the Project

### Option 1: Using Visual Studio

1. Open `BoticAPI.sln` in Visual Studio
2. Set startup profile to **HTTPS**
3. Press **F5** or click **Start**
4. Swagger UI opens at `https://localhost:7136/swagger`

### Option 2: Using CLI (Recommended)

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

**Access Swagger UI:** https://localhost:7136/swagger

---

## 📁 Folder Structure

```
BoticAPI/
│
├── 📂 Controllers/               # API Endpoints
│   ├── AdminController.cs         # Admin: roles, users, applications
│   ├── ApplicationsController.cs   # Applications: CRUD, status updates
│   ├── AuthController.cs          # Login & registration
│   ├── BotController.cs           # Bot: trigger, job status
│   └── DashboardController.cs     # Dashboard: metrics
│
├── 📂 Services/                  # Business Logic Layer
│   ├── IAuthService.cs           # Auth interface
│   ├── AuthService.cs            # Login & JWT generation
│   ├── IApplicationService.cs    # Application interface
│   ├── ApplicationService.cs     # Status & transitions
│   ├── IBotService.cs            # Bot interface
│   ├── BotService.cs             # Automation logic
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
│   ├── SeedData.cs               # Database seeding
│   └── Migrations/               # EF Core migrations
│
├── 📂 DTOs/                      # Data Transfer Objects
│   └── RequestModels.cs          # Request/response models
│
├── 📂 Properties/                # Project Configuration
│   └── launchSettings.json       # Launch profiles
│
├── 📄 Program.cs                 # Application startup
├── 📄 BoticAPI.csproj            # NuGet packages
├── 📄 appsettings.json           # Production settings
├── 📄 appsettings.Development.json  # Development settings
├── 📄 .gitignore                 # Git ignore rules
└── 📄 README.md                  # This file
```

### 📊 Detailed Folder Breakdown

#### **Controllers/** - API Entry Points
Handles HTTP requests. Each controller manages specific domains:
- `AdminController` - Role creation, user management
- `ApplicationsController` - CRUD operations and status updates
- `AuthController` - User login and registration
- `BotController` - Trigger bot execution and view job status
- `DashboardController` - Get role-specific metrics

#### **Services/** - Business Logic
Reusable business logic separated into interfaces and implementations:
- `AuthService` - Password hashing (BCrypt) and JWT token generation
- `ApplicationService` - Application management and status validation
- `BotService` - Automated application processing
- `DashboardService` - Metrics calculation by role

#### **Models/** - Database Entities
Represents database tables as C# classes:
- `User` - User information and role assignment
- `Role` - Job roles (Admin, Bot, Applicant, Developer, etc.)
- `Application` - Job application records
- `ActivityLog` - Audit trail of all changes
- `BotJob` - Automated bot execution history

#### **Data/** - Database Layer
Manages database connections and schema:
- `BoticDbContext` - EF Core DbContext with configurations
- `Migrations/` - Version control for database schema
- `SeedData` - Populates initial roles and users

#### **DTOs/** - Data Transfer Objects
Request/response models for API validation:
- `LoginRequest` - Email + password
- `RegisterRequest` - User registration
- `CreateApplicationRequest` - New application
- `UpdateStatusRequest` - Status update with comment
- `BotRunRequest` - Bot execution parameters

---

## 🔌 API Endpoints

### Authentication
```
POST   /api/auth/register          # Register new user
POST   /api/auth/login             # Get JWT token
```

### Applications
```
POST   /api/applications                      # Create application
GET    /api/applications/{id}                 # Get details
GET    /api/applications/my-applications      # User's applications
PUT    /api/applications/{id}/status          # Update status (Admin/Bot)
GET    /api/applications/{id}/activity-logs   # Activity history
```

### Admin Management
```
POST   /api/admin/roles                       # Create role
GET    /api/admin/roles                       # List roles
GET    /api/admin/users                       # List users
GET    /api/admin/applications                # Non-technical apps
GET    /api/admin/all-applications            # All applications
PUT    /api/admin/applications/{id}/status    # Update app status
```

### Bot Operations
```
POST   /api/bot/run                # Trigger bot
GET    /api/bot/jobs               # Recent jobs
GET    /api/bot/jobs/{id}          # Job details
```

### Dashboard
```
GET    /api/dashboard/metrics      # Role-specific metrics
```

---

## 🔐 Default Credentials

After running migrations, use these to login:

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

## 🎯 Application Status Flow

```
Applied
   │
   ▼
Reviewed
   │
   ├─► Rejected ✗
   │
   ▼
CodingRound
   │
   ├─► Rejected ✗
   │
   ▼
TechnicalInterview
   │
   ├─► Rejected ✗
   │
   ▼
HRInterview
   │
   ├─► Rejected ✗
   │
   ▼
Offer
   │
   ├─► Rejected ✗
   │
   ▼
Hired ✅
```

---

## 🐛 Troubleshooting

### ❌ Database Connection Failed

**Error**: `Cannot connect to database`

**Solution**:
```bash
# Check PostgreSQL is running
psql -U postgres -c "SELECT version();"

# Verify connection string
# Test connection from appsettings.Development.json
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
1. Ensure JWT config in `appsettings.json`
2. Include token: `Authorization: Bearer {token}`
3. Check token expiry (default: 24 hours)

### ❌ Port Already in Use

**Error**: `Port 7136 already in use`

**Solution**:
```bash
# Windows
netstat -ano | findstr :7136
taskkill /PID {PID} /F

# Linux/Mac
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

### Making Database Changes

```bash
# Create migration
dotnet ef migrations add YourMigrationName

# Apply migration
dotnet ef database update

# Revert migration
dotnet ef database update PreviousMigrationName
```

### Adding New Endpoint

1. Create DTO in `DTOs/RequestModels.cs`
2. Add logic in `Services/YourService.cs`
3. Add method in `Controllers/YourController.cs`
4. Test in Swagger UI

### Environment Variables

For production (e.g., Railway):
```bash
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

---

## 📖 Additional Resources

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://jwt.io/)
- [PostgreSQL Docs](https://www.postgresql.org/docs/)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/)

---

## 💡 Tech Stack

| Technology | Purpose |
|-----------|---------|
| **ASP.NET Core 8.0** | Backend framework |
| **C#** | Programming language |
| **PostgreSQL** | Database |
| **Entity Framework Core** | ORM |
| **JWT** | Authentication |
| **BCrypt** | Password hashing |
| **Swagger/OpenAPI** | API documentation |

---

## 📞 Support

For issues:
1. Check **Troubleshooting** section
2. Review console logs
3. Check database: `psql -U postgres -d botic_development`
4. Open an issue on GitHub


**Built with ❤️ using ASP.NET Core**

**Happy Coding! 🚀**
