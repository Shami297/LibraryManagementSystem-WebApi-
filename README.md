# Library Management System — Web API

A basic CRUD Web API built with **.NET 8**, **Entity Framework Core (Code-First)**, **MSSQL**, and **JWT Authentication**, following a layered architecture with the **Repository + Unit of Work** design pattern.

Built as a technical assessment project.

---

## 🏗️ Architecture

The solution is split into 4 projects, each with a single responsibility:

```
LibraryManagementSystem/
 ├── LibraryManagementSystem.API             → Controllers, Program.cs, JWT & Swagger config
 ├── LibraryManagementSystem.Application     → DTOs, Services (business logic), AutoMapper profiles
 ├── LibraryManagementSystem.Infrastructure  → DbContext, Repository implementations, Migrations
 └── LibraryManagementSystem.Core            → Entities, Interfaces (IGenericRepository, IUnitOfWork)
```

**Design Pattern:** Repository + Unit of Work
- `IGenericRepository<T>` — generic CRUD operations per entity, keeps data access decoupled from business logic
- `IUnitOfWork` — wraps repositories and exposes a single `CompleteAsync()` (i.e. `SaveChangesAsync()`) so multiple changes commit as one atomic transaction

---

## 🧰 Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8 / ASP.NET Core Web API |
| ORM | Entity Framework Core 8 (Code-First) |
| Database | Microsoft SQL Server (MSSQL) |
| Auth | JWT Bearer Authentication |
| Password Hashing | BCrypt.Net-Next |
| API Docs | Swagger / Swashbuckle |
| Object Mapping | AutoMapper |

---

## 📦 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local, Docker, or Azure SQL) — connection string configurable
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## ⚙️ Setup & Run

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd LibraryManagementSystem
```

### 2. Configure the database connection

Update `LibraryManagementSystem.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LibraryManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "THIS_IS_A_LONG_SECRET_KEY_MIN_32_CHARS!!",
    "Issuer": "LibraryManagementSystem",
    "Audience": "LibraryManagementSystemUsers",
    "ExpiryMinutes": 10
  }
}
```

> ⚠️ Replace the `Jwt:Key` with your own secret (minimum 32 characters) before deploying anywhere beyond local testing.

### 3. Restore dependencies

```bash
dotnet restore
```

### 4. Create the database via EF Core migrations

Run from the **solution root**:

```bash
dotnet ef migrations add InitialCreate --project LibraryManagementSystem.Infrastructure --startup-project LibraryManagementSystem.API

dotnet ef database update --project LibraryManagementSystem.Infrastructure --startup-project LibraryManagementSystem.API
```

This generates the schema in MSSQL directly from the entity classes — no manual SQL scripts required.

### 5. Run the API

```bash
cd LibraryManagementSystem.API
dotnet run
```

Swagger UI will be available at:
```
https://localhost:7268/swagger
```

---

## 🔐 Authentication Flow

1. **Register** a user → `POST /api/auth/register`
2. **Login** → `POST /api/auth/login` → returns a JWT token
3. Click **Authorize** in Swagger and enter:
   ```
   Bearer <your-token-here>
   ```
4. All protected endpoints (e.g. `/api/books`) will now accept your requests.

---

## 📚 API Endpoints

### Auth

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| POST | `/api/auth/register` | Register a new user | ❌ |
| POST | `/api/auth/login` | Login and receive JWT token | ❌ |

### Books

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| GET | `/api/books` | Get all books | ✅ |
| GET | `/api/books/{id}` | Get a book by id | ✅ |
| POST | `/api/books` | Create a new book | ✅ |
| PUT | `/api/books/{id}` | Update an existing book | ✅ |
| DELETE | `/api/books/{id}` | Delete a book | ✅ |

**Sample request body — create book:**
```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "totalCopies": 5,
  "availableCopies": 5
}
```

---

## 🗄️ Entities

- **Book** — Title, Author, ISBN, TotalCopies, AvailableCopies
- **Member** — FullName, Email, JoinedAt
- **BorrowRecord** — links a Book and Member with borrow/return dates
- **ApplicationUser** — Username, PasswordHash (BCrypt), Role

---

## 🧪 Testing

1. Run the project (`dotnet run`)
2. Open Swagger UI (`/swagger`)
3. Register → Login → copy JWT token
4. Click **Authorize**, paste `Bearer <token>`
5. Test CRUD endpoints under `/api/books`
6. To confirm auth is enforced: click **Logout** in the Authorize popup, then retry a request — you should get `401 Unauthorized`

---

## 📁 Project Structure Reference

```
Core/
 ├── Entities/           → Book, Member, BorrowRecord, ApplicationUser
 └── Interfaces/         → IGenericRepository, IUnitOfWork

Infrastructure/
 ├── Data/                → LibraryDbContext
 ├── Repositories/        → GenericRepository, UnitOfWork
 └── Migrations/          → EF Core generated migrations

Application/
 ├── DTOs/                → RegisterDto, LoginDto
 └── Services/            → AuthService

API/
 ├── Controllers/         → AuthController, BooksController
 └── Program.cs           → DI, JWT, Swagger, EF Core configuration
```

---

## 📝 Notes

- Passwords are hashed using **BCrypt** before storage — plain-text passwords are never persisted.
- Database schema is fully **Code-First** — the DB is generated and versioned entirely through EF Core migrations.
- JWT tokens are signed using **HMAC-SHA256** with a symmetric key from configuration.

---

## 📌 Future Improvements

- Add Member and BorrowRecord CRUD endpoints
- Implement borrow/return logic with available-copy tracking
- Add role-based authorization (Admin vs User)
- Add FluentValidation for stricter input validation
- Add unit tests for services and repositories
- Add refresh token support
