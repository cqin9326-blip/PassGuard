# PassGuard

PassGuard is an ASP.NET Core MVC application for residential estate visitor management. It helps estate administrators, home owners, and security staff manage visitor access through role-based authentication, secure visitor passes, and gate check-in tracking.

## Overview

The application is designed around three main user roles:

- `Admin`: manages estates, homes, users, roles, logs, and system-wide dashboard views
- `HomeOwner`: creates visitor records and visit passes for their assigned home
- `Security`: verifies pass codes, records gate check-ins, and monitors expected visitors

PassGuard Uses:

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- Code-first migrations

## Built Features

### Authentication and Authorization

- ASP.NET Identity login/logout
- Role-based access for `Admin`, `HomeOwner`, and `Security`
- Forced password change for first login on seeded `Home Owner` and `Security` accounts
- Admin-created users with role assignment
- Change password flow for authenticated users

### Admin Features

- Estate management
- Home management
- User and role management
- Audit log viewing
- Estate Command Dashboard
- Estate details page showing homes and estate-specific activity
- Estate filter on the homes page
- Controlled delete flow for homes with attached visit passes

### HomeOwner Features

- Homeowner dashboard
- Create visitor records
- Create visit passes for the assigned home
- View only their own passes
- Revoke active passes
- View active passes, revoked count, and visit history
- Visitor selection on pass creation limited to visitors created by that homeowner

### Security Features

- Security dashboard
- Pass-code verification
- Gate check-in creation and editing
- Upcoming visits for the day
- Recent check-ins list
- Pass detail review before gate decisions

### Visit Pass Security

- Secure random pass-code generation
- Hashing of pass codes before storage
- Plain code shown only once at creation time
- Pass verification against stored hashes
- Status enforcement for:
  - `Active`
  - `Used`
  - `Expired`
  - `Revoked`

### Validations

- Model validation attributes
- Duplicate checks for key records
- Role ownership checks
- Authorization checks on sensitive actions
- Used / approved visit passes cannot be edited
- Used visit passes cannot be deleted

### Logging and Reporting

- Audit logging for key system actions
- Login tracking
- Pass creation / revocation / verification logging
- Gate check-in result logging
- Admin dashboard analytics and activity summaries

## Core Database Model

The main entities in the application are:

- `Estate`
- `Home`
- `Visitor`
- `VisitPass`
- `GateCheckIn`
- `AuditLog`
- `ApplicationUser`

Key relationships:

- One `Estate` has many `Homes`
- One `Home` has many `VisitPasses`
- One `Visitor` can have many `VisitPasses`
- One `VisitPass` has one `GateCheckIn`

## Project Structure

The solution is split into four projects:

- `PassGuard`: MVC web application
- `PassGuard.Models`: domain models and view models
- `PassGuard.DAL`: Entity Framework context, repositories, migrations, Identity user
- `PassGuard.BLL`: business logic and dashboard services

## Seeded Demo Accounts

The application seeds three role-based demo accounts in [PassGuard/Infrastructure/IdentitySeedData.cs](PassGuard/Infrastructure/IdentitySeedData.cs):

- `Admin`
  - Email: `admin@email.com`
  - Password: `Admin123!`
- `HomeOwner`
  - Email: `homeowner@email.com`
  - Password: `HomeOwner123!`
- `Security`
  - Email: `security@email.com`
  - Password: `Security123!`


## Running the App

### 1. Configure the database connection

Update the connection string in [PassGuard/appsettings.json](PassGuard/appsettings.json) to match your SQL Server environment.

Current app configuration expects a SQL Server connection under:

- `ConnectionStrings:DefaultConnection`

### 2. Restore and build

```bash
dotnet restore
dotnet build PassGuard.sln
```

### 3. Apply migrations

```bash
dotnet ef database update --project PassGuard.DAL --startup-project PassGuard
```

### 4. Run the application

```bash
dotnet run --project PassGuard
```

## Creating New Migrations

When you change the data model, create and apply a new migration:

```bash
dotnet ef migrations add MigrationName --project PassGuard.DAL --startup-project PassGuard
dotnet ef database update --project PassGuard.DAL --startup-project PassGuard
```

## Suggested Demo Flow

1. Log in as `Admin`
2. Create an estate and a home
3. Assign a homeowner to the home
4. Create a new user or use the seeded homeowner account
5. Log in as `HomeOwner`
6. Create a visitor
7. Create a visit pass
8. Log in as `Security`
9. Verify the pass code and record the check-in
10. Return to dashboards to review the updated pass and audit activity

## Future Enhancements

- Email-based account onboarding
- Estate-to-security assignment
- Scheduled visit date/time separate from pass creation date
- Exportable reports
- Search and filter improvements across dashboards
