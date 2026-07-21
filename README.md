# Library Management System — Web API

A CRUD Web API built with **.NET 8**, **Entity Framework Core (Code-First)**, **MSSQL**, and **JWT Authentication**, following a layered architecture with the **Repository + Unit of Work** design pattern.

Built as a technical assessment project.

---

## 🏗️ Architecture

The solution is split into 4 projects, each with a single responsibility:

```
LibraryManagementSystem/
 ├── LibraryManagementSystem.API             → Controllers, Middleware, Program.cs, JWT & Swagger config
 ├── LibraryManagementSystem.Application     → DTOs, Services (business logic)
 ├── LibraryManagementSystem.Infrastructure  → DbContext, Repository implementations, Migrations
 └── LibraryManagementSystem.Core            → Entities, Interfaces (IGenericRepository, IUnitOfWork)
```

**Design Pattern: Repository + Unit of Work**
- `IGenericRepository<T>` — generic CRUD operations shared across all entities, keeps data access decoupled from business logic
- `IBorrowRecordRepository` — extends the generic repository with entity-specific queries (`.Include()` for eager loading), showing how the pattern scales beyond plain CRUD when needed
- `IUnitOfWork` — wraps all repositories and exposes a single `CompleteAsync()` (`SaveChangesAsync()`), so multi-entity operations (e.g. borrowing a book) commit as **one atomic transaction**

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
| Error Handling | Custom global exception middleware |

---

## 📦 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local, Docker, or Azure SQL)
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

### 2. Configure the database connection & JWT settings

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

> ⚠️ For anything beyond local testing, move `Jwt:Key` into **User Secrets** or environment variables instead of committing it in `appsettings.json`:
> ```bash
> dotnet user-secrets init --project LibraryManagementSystem.API
> dotnet user-secrets set "Jwt:Key" "your-real-secret-key" --project LibraryManagementSystem.API
> ```

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

The schema is generated entirely from the entity classes — no manual SQL scripts required.

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

1. **Register** → `POST /api/auth/register`
2. **Login** → `POST /api/auth/login` → returns a JWT token
3. In Swagger, click **Authorize** and enter:
   ```
   Bearer <your-token-here>
   ```
4. All protected endpoints will now accept your requests.
5. To verify auth is actually enforced: click **Authorize → Logout**, then retry a protected endpoint — expect `401 Unauthorized`.

---

## 📚 API Endpoints

### Auth

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register a new user | ❌ |
| POST | `/api/auth/login` | Login, receive JWT token | ❌ |

### Books

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/books` | Get all books (excludes soft-deleted) | ✅ |
| GET | `/api/books/{id}` | Get a book by id | ✅ |
| POST | `/api/books` | Create a new book | ✅ |
| PUT | `/api/books/{id}` | Update an existing book | ✅ |
| DELETE | `/api/books/{id}` | Soft-delete a book (`IsDeleted = true`) | ✅ |

**Sample — create book:**
```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "totalCopies": 5,
  "availableCopies": 5
}
```

### Members

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/members` | Get all members | ✅ |
| GET | `/api/members/{id}` | Get a member by id | ✅ |
| POST | `/api/members` | Create a new member | ✅ |
| PUT | `/api/members/{id}` | Update an existing member | ✅ |
| DELETE | `/api/members/{id}` | Delete a member | ✅ |

**Sample — create member:**
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com"
}
```

### Borrow Records

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/borrowrecords` | Get all borrow records | ✅ |
| POST | `/api/borrowrecords/borrow` | Borrow a book | ✅ |
| PUT | `/api/borrowrecords/return/{id}` | Return a borrowed book | ✅ |
| GET | `/api/borrowrecords/member/{memberId}` | Borrow history for a member | ✅ |
| GET | `/api/borrowrecords/book/{bookId}` | Borrow history for a book | ✅ |

**Sample — borrow a book:**
```json
POST /api/borrowrecords/borrow
{
  "bookId": 1,
  "memberId": 1
}
```

**Business rules enforced:**
- Book and Member must exist
- `AvailableCopies` must be greater than 0 to borrow
- Borrowing decrements `AvailableCopies`; returning increments it
- A book can't be "returned" twice
- Borrow + copy-count update commit together as **one transaction** via Unit of Work

---

## 🗄️ Entities

- **Book** — Title, Author, ISBN (unique), TotalCopies, AvailableCopies, IsDeleted (soft delete)
- **Member** — FullName, Email (unique), JoinedAt
- **BorrowRecord** — links a Book and Member, with BorrowedAt / ReturnedAt (nullable)
- **ApplicationUser** — Username (unique), PasswordHash (BCrypt), Role

**Relationships:** `BorrowRecord` has both a foreign key (`BookId`, `MemberId`) and a navigation property (`Book`, `Member`). The FK is what's physically stored in the database; the navigation property lets EF Core eager-load related data via `.Include()` without extra manual queries — used in the borrow-history endpoints above.

---

## 🧹 Soft Delete (Books)

`Book.IsDeleted` defaults to `false` at the database level (`HasDefaultValue(false)`), and a **global query filter** (`HasQueryFilter(b => !b.IsDeleted)`) is applied in `LibraryDbContext`. This means:
- Every query against `Book` automatically excludes soft-deleted rows — no need to repeat `.Where(!IsDeleted)` anywhere
- `DELETE /api/books/{id}` sets `IsDeleted = true` instead of physically removing the row
- To view or restore soft-deleted rows, the filter can be bypassed explicitly with `.IgnoreQueryFilters()`

---

## 🛡️ Global Error Handling

A custom `ExceptionHandlingMiddleware` sits early in the request pipeline and catches any unhandled exception, returning a **consistent JSON error shape** instead of a raw stack trace:

```json
{
  "statusCode": 500,
  "message": "An unexpected error occurred. Please try again later."
}
```

| Exception Type | Status Code |
|---|---|
| `KeyNotFoundException` | 404 |
| `UnauthorizedAccessException` | 401 |
| `ArgumentException` / `InvalidOperationException` | 400 |
| Anything else | 500 (real message logged server-side, hidden from client) |

Expected validation failures (e.g. "book not found," "no available copies") are still returned directly from controllers via `NotFound()` / `BadRequest()` — the middleware is a safety net for genuinely unexpected failures, not a replacement for normal validation.

---

## 🧪 Testing via Swagger

1. Run the project (`dotnet run`) and open `/swagger`
2. `POST /api/auth/register` → then `POST /api/auth/login` → copy the JWT token
3. Click **Authorize**, enter `Bearer <token>`
4. Test Books / Members CRUD
5. Borrow a book via `POST /api/borrowrecords/borrow`, confirm `AvailableCopies` drops on `GET /api/books/{id}`
6. Return it via `PUT /api/borrowrecords/return/{id}`, confirm `AvailableCopies` goes back up
7. Check `GET /api/borrowrecords/member/{memberId}` and `GET /api/borrowrecords/book/{bookId}` for history views
8. Log out of Authorize and retry any protected endpoint → confirm `401 Unauthorized`

---

## 📁 Project Structure Reference

```
Core/
 ├── Entities/            → Book, Member, BorrowRecord, ApplicationUser
 └── Interfaces/          → IGenericRepository, IBorrowRecordRepository, IUnitOfWork

Infrastructure/
 ├── Data/                 → LibraryDbContext (query filters, indexes, relationships)
 ├── Repositories/         → GenericRepository, BorrowRecordRepository, UnitOfWork
 └── Migrations/           → EF Core generated migrations

Application/
 ├── DTOs/                 → RegisterDto, LoginDto, BorrowBookDto
 └── Services/             → AuthService

API/
 ├── Controllers/          → AuthController, BooksController, MembersController, BorrowRecordsController
 ├── Middleware/            → ExceptionHandlingMiddleware
 └── Program.cs             → DI, JWT, Swagger, EF Core configuration
```

---

## 📝 Key Implementation Notes

- Passwords are hashed with **BCrypt** — plain-text passwords are never persisted.
- Database schema is fully **Code-First** — generated and versioned entirely through EF Core migrations.
- JWT tokens are signed with **HMAC-SHA256** using a symmetric key from configuration.
- Borrowing/returning uses **Unit of Work** to ensure the book's copy count and the borrow record update atomically in one transaction.
- Soft delete on `Book` uses EF Core **global query filters** rather than manual filtering in every query.
- A **global exception middleware** ensures all unhandled errors return a consistent JSON shape and never leak internal exception details to the client.

---

## 📌 Future Improvements

- Add DTOs + AutoMapper for all entities (currently entities are bound directly for simplicity)
- Role-based authorization (Admin vs Member-facing endpoints)
- Soft delete for `Member` for consistency with `Book`
- FluentValidation for richer input validation
- Unit tests for services and repositories
- Refresh token support for JWT
- Pagination on list endpoints (`GET /api/books`, `GET /api/members`)
