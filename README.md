# 🛡️ PassGuard – Residential Visitor Access Management System

PassGuard is a full-stack **ASP.NET Core MVC** web application designed to manage visitor access within residential estates.
It provides a secure, role-based system for **estate administrators, homeowners, and security personnel** to coordinate and monitor visitor entry in real time.

---

## 📌 Key Highlights

* 🔐 Role-based authentication (Admin / HomeOwner / Security)
* 🎫 Secure visitor pass system with hashed pass codes
* 🚪 Real-time gate check-in tracking
* 📊 Admin dashboards with audit logs and analytics
* 🧱 Clean 4-layer architecture (MVC + BLL + DAL + Models)
* 🗄️ Code-first database design with Entity Framework Core

---

## 🧠 System Overview

PassGuard is built around three core roles:

### 👨‍💼 Admin

* Manage estates and homes
* Create users and assign roles
* View audit logs and analytics dashboard

### 🏠 HomeOwner

* Create visitors
* Generate visit passes
* Track visit history and revoke passes

### 🛡️ Security

* Verify pass codes
* Record gate check-ins
* Monitor daily expected visitors

---

## 🏗️ Tech Stack

* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* ASP.NET Identity
* Code-First Migrations

---

## 🧩 Architecture

This project follows a clean separation of concerns:

* **PassGuard (Web Layer)** – Controllers & Views
* **PassGuard.Models** – Domain & View Models
* **PassGuard.DAL** – Database context & repositories
* **PassGuard.BLL** – Business logic & services

---

## 🔐 Security Features

* Hashed pass-code storage
* One-time visible pass codes
* Role-based authorization
* Validation on sensitive operations
* Immutable rules:

  * Used passes cannot be edited or deleted
  * Approved passes are locked

---

## 📊 Core Database Design

Entities:

* Estate
* Home
* Visitor
* VisitPass
* GateCheckIn
* AuditLog
* ApplicationUser

Relationships:

* One Estate → Many Homes
* One Home → Many VisitPasses
* One Visitor → Many VisitPasses
* One VisitPass → One GateCheckIn

---

## ▶️ How to Run

### 1. Configure Database

Update `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "your-sql-server-connection"
}
```

### 2. Build Project

```bash
dotnet restore
dotnet build
```

### 3. Apply Migrations

```bash
dotnet ef database update --project PassGuard.DAL --startup-project PassGuard
```

### 4. Run App

```bash
dotnet run --project PassGuard
```

---

## 👤 Demo Accounts

| Role      | Email                                             | Password      |
| --------- | ------------------------------------------------- | ------------- |
| Admin     | [admin@email.com](mailto:admin@email.com)         | Admin123!     |
| HomeOwner | [homeowner@email.com](mailto:homeowner@email.com) | HomeOwner123! |
| Security  | [security@email.com](mailto:security@email.com)   | Security123!  |

---

## 🎯 Demo Workflow

1. Login as **Admin**
2. Create Estate + Home
3. Assign HomeOwner
4. Login as **HomeOwner**
5. Create Visitor + Visit Pass
6. Login as **Security**
7. Verify pass & check-in
8. View updated dashboard

---

## 📸 Screenshots

### 🔐 Login Page
![Login](login.png)

The login page enables secure authentication for different user roles, including Admin, HomeOwner, and Security, ensuring controlled access to system features.

---

### 📊 Admin Dashboard
![Admin Dashboard](admin-dashboard.png)

The admin dashboard provides a centralized overview of the system, allowing administrators to manage estates, homes, users, and monitor overall activity.

---

### 🎫 Pass Creation & Security Check-in
![Pass](pass-security.png)

This screen highlights the core functionality of the system, where homeowners generate visit passes and security staff verify them at the gate, completing the full visitor access workflow.



## 🚀 Future Improvements

* Email-based onboarding
* Advanced search & filtering
* Scheduled visits
* Report export system

---

## 👨‍💻 Author

**Cunwei Qin （And other Group 2 members) Winnipeg, Canada

---

