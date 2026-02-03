# WatchDog

A desktop project management application built with C# and Avalonia UI for organizing, tracking, and collaborating on projects.

## Features

- **User Authentication** - Secure login/registration with BCrypt password hashing
- **Role-Based Access Control** - SuperAdmin and User roles with appropriate permissions
- **Project Management** - Create and track projects through their lifecycle (NotStarted → InProgress → Completed → Closed)
- **Task Assignment** - Assign tasks to team members with completion percentage tracking
- **Subtask Support** - Break down tasks into smaller, manageable subtasks
- **Timeline Messages** - Post project-level announcements and updates (with pinning support)
- **Progress Comments** - Track task-level progress with comments
- **Team Collaboration** - Manage project membership and team assignments

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 9.0 |
| UI | Avalonia UI |
| Architecture | MVVM (CommunityToolkit.MVVM) |
| Database | PostgreSQL |
| ORM | Dapper |
| Password Hashing | BCrypt.Net-Next |
| Testing | xUnit, Moq |
| Containerization | Docker / Docker Compose |

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) or [Docker](https://www.docker.com/get-started)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/yourusername/WatchDog.git
cd WatchDog
```

### 2. Set up the database

**Option A: Using Docker (Recommended)**

```bash
docker-compose --profile dev up -d
```

This starts a PostgreSQL container on port 5432 with automatic schema initialization.

**Option B: Manual PostgreSQL Setup**

1. Create a PostgreSQL database
2. Run the initialization script located at `WatchDog/Scripts/init.sql`
3. Update the connection string in `appsettings.Development.json`

### 3. Run the application

```bash
cd WatchDog
dotnet run
```

### Default Admin Credentials

After initialization, you can log in with:
- **Email:** `admin@watchdog.com`
- **Password:** `admin123`

## Configuration

Configuration files are located in the `WatchDog` project directory:

- `appsettings.Development.json` - Development environment settings
- `appsettings.Production.json` - Production environment settings

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=watchdog;Username=postgres;Password=yourpassword"
  }
}
```

## Project Structure

```
WatchDog/
├── WatchDog/                 # Main application
│   ├── Models/              # Domain entities
│   ├── Services/            # Business logic
│   ├── ViewModels/          # MVVM view models
│   ├── Views/               # Avalonia UI views
│   ├── Data/                # Repositories & data access
│   ├── Converters/          # UI value converters
│   ├── Styles/              # Application styling
│   └── Scripts/             # Database scripts
├── WatchDogTest/            # Unit tests
└── docker-compose.yml       # Docker configuration
```

### Key Components

| Directory | Description |
|-----------|-------------|
| `Models/` | Domain entities (User, Project, Task, SubTask, etc.) |
| `Services/` | Business logic layer (authentication, CRUD operations) |
| `ViewModels/` | MVVM presentation logic |
| `Views/` | Avalonia XAML UI definitions |
| `Data/` | Repository pattern implementation for database access |

## Running Tests

```bash
dotnet test
```

Tests are located in the `WatchDogTest` project and cover the service layer using xUnit and Moq.

## Docker Deployment

### Development

```bash
docker-compose --profile dev up -d
```

### Production

```bash
docker-compose --profile prod up -d
```

Production mode runs the application container alongside PostgreSQL on port 5433.

## User Roles

| Role | Permissions |
|------|-------------|
| SuperAdmin | Full system access, create projects, register users |
| User | View assigned projects, manage assigned tasks |

## Project Status Values

- **NotStarted** - Project created but work hasn't begun
- **InProgress** - Active development
- **Completed** - All work finished
- **Closed** - Project archived

## License

This project is licensed under the MIT License.
