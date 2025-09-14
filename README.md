# GRWMJobs - Get Ready With Me Jobs Platform

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-green)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-9.0-orange)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-red)

A curated Q&A platform featuring handpicked interview questions and answers across different career tracks and categories. Users can register, browse questions, and view detailed answers to prepare for job interviews.

**Author:** [Muhamed Hamed](https://github.com/muhamedhamedvl) | [LinkedIn](https://www.linkedin.com/in/muhamed-muhamed-3a2a25250/)

## 🚀 Features

### Core Functionality
- **User Authentication & Authorization** - Secure login/register system with role-based access
- **Question Management** - Create, edit, and organize handpicked interview questions
- **Answer System** - View curated answers to interview questions
- **Category & Subcategory Organization** - Structured question categorization
- **Track Management** - Different career tracks (backend , frontend , mobile Dev, etc.)

### User Experience
- **Modern UI/UX** - Beautiful, responsive design with smooth animations
- **Password Visibility Toggle** - Enhanced security with show/hide password functionality
- **Real-time Validation** - Instant feedback on form inputs
- **Mobile Responsive** - Optimized for all device sizes

## 🏗️ Architecture

The project follows a clean architecture pattern with three main layers:

### 1. **Presentation Layer (GRWMJobs.PL)**
- ASP.NET Core MVC application
- Controllers for handling HTTP requests
- Views with Razor syntax
- Middleware for security and session tracking

### 2. **Business Logic Layer (GRWM.BLL)**
- Service classes for business logic
- Interface definitions for dependency injection
- Email services and other business operations

### 3. **Data Access Layer (GRWMJobs.DAL)**
- Entity Framework Core with SQL Server
- Repository pattern implementation
- Database models and migrations
- Password hashing utilities

## 📁 Project Structure

```
GRWMJobs/
├── GRWMJobs.PL/                 # Presentation Layer
│   ├── Controllers/             # MVC Controllers
│   │   ├── AdminController.cs
│   │   ├── AuthController.cs
│   │   ├── QuestionController.cs
│   │   ├── AnswerController.cs
│   │   └── ...
│   ├── Views/                   # Razor Views
│   │   ├── Auth/
│   │   │   ├── Login.cshtml
│   │   │   └── Register.cshtml
│   │   └── ...
│   ├── Models/                  # ViewModels
│   ├── Middleware/              # Custom Middleware
│   └── wwwroot/                 # Static files
├── GRWM.BLL/                    # Business Logic Layer
│   ├── Services/                # Business Services
│   │   ├── QuestionService.cs
│   │   ├── UserService.cs
│   │   └── ...
│   └── IServices/               # Service Interfaces
├── GRWMJobs.DAL/                # Data Access Layer
│   ├── Models/                  # Entity Models
│   │   ├── User.cs
│   │   ├── Question.cs
│   │   ├── Answer.cs
│   │   └── ...
│   ├── Data/                    # DbContext and Configuration
│   ├── Repositores/             # Repository Pattern
│   └── Migrations/              # EF Core Migrations
└── GRWMJobs.sln                 # Solution File
```

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 9.0, C# 12
- **Database**: SQL Server with Entity Framework Core 9.0
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap 5
- **Authentication**: Cookie-based authentication
- **Architecture**: Clean Architecture with Repository Pattern
- **Dependency Injection**: Built-in ASP.NET Core DI Container

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone <https://github.com/muhamedhamedvl/GRWMJobs>
   cd GRWMJobs
   ```

2. **Configure the database connection**
   
   Update the connection string in `GRWMJobs.PL/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Your-SQL-Server-Connection-String"
     }
   }
   ```

3. **Restore packages and build**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run database migrations**
   ```bash
   cd GRWMJobs.PL
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run --project GRWMJobs.PL
   ```

## 🔐 Security Features

- **Password Hashing**: Secure password storage using custom hashing
- **Session Management**: Secure session handling with configurable timeouts
- **CSRF Protection**: Anti-forgery token validation
- **Role-based Authorization**: Admin and User role separation
- **Input Validation**: Server-side validation for all inputs
- **SQL Injection Protection**: Entity Framework parameterized queries

## 🎨 UI/UX Features

- **Modern Design**: Clean, professional interface with gradient backgrounds
- **Responsive Layout**: Mobile-first design that works on all devices
- **Interactive Elements**: Smooth animations and hover effects
- **Password Visibility Toggle**: Enhanced security with show/hide functionality
- **Real-time Validation**: Instant feedback on form inputs
- **Loading States**: User-friendly loading indicators


## 👥 Authors

- **Muhamed Hamed**  - [GitHub](https://github.com/muhamedhamedvl) | [LinkedIn](https://www.linkedin.com/in/muhamed-muhamed-3a2a25250/)

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Entity Framework team for the ORM
- Bootstrap team for the UI framework
- All contributors who helped make this project better
---

