---
name: funews-architecture
description: Apply the AIVES News Management System MVC, BLL, and DAL architecture rules (PRN222 Assignment 1) when creating a function, adding a Controller, Service, Repository, or DAO, modifying an Entity, or reviewing architectural compliance.
---

# AIVES News Management Architecture (PRN222 Assignment 1)

Use this skill for implementation and architecture review in this workspace. The project name is **AIVES**, implementing **PRN222 Assignment 1 (News Management System)** with ASP.NET Core MVC on .NET 8 and Microsoft SQL Server. Do not apply obsolete AI Viva Exam specifications or non-.NET stacks.

Read `docs/ARCHITECTURE.md`, `docs/business-rules.md`, and `docs/TASKS.md` when specific contracts, functions, or schema details are required.

## 1. Application Flow & Layer Boundaries

The mandatory request and persistence path is:
`Razor View → MVC Controller → BLL Service → DAL Repository → DAL DAO → AIVESDbContext → SQL Server`

Every data query or mutation must flow through this exact chain. Repositories must never query `AIVESDbContext` directly, and DAOs must never be bypassed.

### Layer Responsibilities
- **Presentation (`StudentNameMVC` / `AIVES.MVC`):** Controllers, Razor Views, presentation ViewModels, and startup/DI configuration. Controllers receive HTTP requests, validate `ModelState`, invoke BLL service interfaces, and return Views, partial modals, or redirects.
  - **Controller Database Ban:** Controllers MUST NEVER inject `AIVESDbContext`, reference `DbSet`, execute SQL, or call a Repository/DAO directly. Controllers depend exclusively on Service interfaces (`INewsArticleService`, `ICategoryService`, `IAuthService`, etc.).
- **Business Logic Layer (`AIVES.BLL`):** Service interfaces and implementations (`Services/`, `Interfaces/`). Owns business logic, use-case coordination, validation rules, role checks, and transaction boundaries. Services depend exclusively on Repository interfaces.
- **Data Access Layer (`AIVES.DAL`):**
  - **Entities (`Entities/`):** POCO data classes matching database tables (`NewsArticle`, `Category`, `Tag`, `NewsTag`, `SystemAccount`).
  - **Repositories (`Repositories/`):** Define and implement data contracts (`INewsArticleRepository`, `ICategoryRepository`, etc.) consumed by BLL. Registered with **Scoped** lifetime. Repositories coordinate calls to DAOs.
  - **DAOs (`DAOs/`):** Encapsulate EF Core / LINQ queries against `AIVESDbContext`. Every DAO (`NewsArticleDAO`, `CategoryDAO`, `SystemAccountDAO`, `TagDAO`) must implement the thread-safe **Singleton Pattern**.
  - **DbContext (`Context/AIVESDbContext.cs`):** Scoped EF Core DbContext mapping entities, keys, check constraints, and relationships.

### Dependency Direction
`MVC → BLL → DAL`. MVC startup references DAL solely for EF Core DbContext and DI service registration. BLL references DAL. DAL must never reference BLL or MVC. BLL must never reference MVC.

## 2. Safe DAO Singleton & Scoped DbContext Implementation

`AIVESDbContext` is registered with **Scoped** lifetime (per HTTP request). DAOs are **Singletons**.

### Invariant: Never Store Scoped DbContext in Singleton DAO
Never store `AIVESDbContext` in an instance field or static field of a Singleton DAO. Doing so causes a captive dependency, resulting in concurrency exceptions (`InvalidOperationException: A second operation was started on this context before a previous operation completed`) and memory leaks.

### Approved Pattern: Context-Per-Operation
1. **Thread-Safe Singleton:** Implement DAOs with a thread-safe singleton pattern using `Lazy<T>` or a lock:
   ```csharp
   public class NewsArticleDAO
   {
       private static readonly Lazy<NewsArticleDAO> _instance = new(() => new NewsArticleDAO());
       public static NewsArticleDAO Instance => _instance.Value;
       private NewsArticleDAO() { }
       ...
   }
   ```
2. **Context Passing:** The Scoped Repository receives `AIVESDbContext` via constructor injection and passes it into DAO methods:
   ```csharp
   // Scoped Repository:
   public class NewsArticleRepository : INewsArticleRepository
   {
       private readonly AIVESDbContext _context;
       public NewsArticleRepository(AIVESDbContext context) => _context = context;

       public Task<List<NewsArticle>> GetAllAsync() => NewsArticleDAO.Instance.GetAllAsync(_context);
   }

   // Singleton DAO:
   public async Task<List<NewsArticle>> GetAllAsync(AIVESDbContext context)
   {
       return await context.NewsArticles
           .Include(n => n.Category)
           .AsNoTracking()
           .ToListAsync();
   }
   ```
Alternatively, DAOs may create a short-lived context via `IDbContextFactory<AIVESDbContext>`, ensuring the context is disposed immediately after the query.

## 3. Core Functional & Architectural Rules

### Authentication (`AUTH-01`, `AUTH-02`)
- **Dual Credential Verification:** `AuthService` handles two distinct credential sources:
  1. **Default Administrator:** Check credentials against `appsettings.json` (`DefaultAdmin:Email` and `DefaultAdmin:Password`). Never query the database for the default administrator.
  2. **Staff & Lecturer:** Normalize email with `Trim()` and `ToLowerInvariant()`, query `SystemAccount` by email, and use ASP.NET Core Identity `PasswordHasher<SystemAccount>` consistently in Account creation/update and Auth verification. Store only the hash in `AccountPasswordHash`; plaintext password comparison is prohibited. The configuration-based default Admin is a separate credential source.
- Successful login issues an authentication cookie/session with claims: `NameIdentifier` (AccountId), `Name`, `Email`, and `Role` (`Admin`, `Staff`, or `Lecturer`).
- **Approved Account lifecycle (SC-10):** normal Admin Delete sets `SystemAccount.IsDeleted = true` after confirmation. Reject deleted accounts at login and reject their existing principal/cookie on subsequent authenticated requests. Hide them from ordinary account lists while preserving author/editor data in News queries; an Account query filter must not make News disappear through required joins. Keep email uniqueness including deleted accounts. Permanent Delete is a separate confirmed operation allowed only when no News references either CreatedById or UpdatedById. This does not grant Admin News-management permissions. Check the actual DB/mapping and implementation status in `docs/ARCHITECTURE.md`; DAL support alone does not implement Auth/session enforcement.

### Role Authorization (`AUTH-03`)
- Enforce server-side role authorization on Controllers using `[Authorize(Roles = "...")]` or custom authorization filters:
  - **Admin:** Account Management (`SystemAccountController`) and Reports (`ReportController`).
  - **Staff (Role 1):** News Article Management, Category Management, own Profile, and own News History. Forbidden from managing accounts or viewing admin reports.
  - **Lecturer (Role 2):** Read-only access to Active News Articles (`NewsStatus = 1`). Forbidden from managing news, categories, accounts, or reports.
  - **Public:** Read-only access to Active News Articles (`NewsStatus = 1`) without login.
- Hiding UI elements is for UX only; all endpoints must be secured on the server.

### Staff Profile (`PROFILE-01`) & News History (`HIST-01`)
- **Self-Scope Invariant:** For Profile view/edit and Own News History, Controllers and Services MUST resolve the user's `AccountId` strictly from `User.FindFirstValue(ClaimTypes.NameIdentifier)`. Never trust an `id` passed in route parameters, query strings, or form bodies.
- Staff can only view news articles where `CreatedById` equals their own `AccountId`.
- Profile updates must not allow changing user role or account ID.

### Admin Reports (`REPORT-01`)
- Treat StartDate/EndDate as calendar dates in Vietnam (UTC+7). The Service validates both dates and StartDate <= EndDate, converts local StartDate midnight and midnight after EndDate to UTC, and rejects an unrepresentable upper bound.
- The DAO queries `CreatedDate >= startUtc && CreatedDate < endExclusiveUtc` so the entire EndDate is included. Sort by `CreatedDate` descending, then `NewsArticleId` descending for ties.
- Endpoints must be strictly restricted to the Administrator.

### News Article Management (Main Demo Flow)
- Full CRUD: List, Search, Create, Update, Delete with Category and Tag associations.
- **Search fields:** News `NewsTitle`/`NewsContent`; Category `CategoryName`/`CategoryDescription`; Account `AccountName`/`AccountEmail`. Trim keyword, treat null/whitespace as no keyword filter, and keep server-side role filters.
- **Category Delete Rule:** `CategoryService.DeleteCategory()` must query and reject deletion if any News Article references the Category. UI-only prevention is insufficient.
- **Audit Fields:** On create, `CreatedById` comes from logged-in Staff; `CreatedDate` is UTC. On update, `UpdatedById` and `ModifiedDate` are set; original creator and date are preserved.
- **Atomic persistence (SC-11):** persist News content, audit, and NewsTag additions/removals together in one SaveChangesAsync where possible; use an enclosing transaction for a use case requiring multiple saves. Failed persistence must roll back the complete operation. Service coordinates the use case; concrete EF transactions stay in DAL. Audit stores the latest editor, not a full revision history.
- **UI Popups:** Create and Update operations for News, Category, and Account must use popup modals/dialogs. Delete operations must use an explicit confirmation modal with Cancel and Confirm buttons.

## 4. Feature Implementation Workflow

1. Identify assigned issue, function ID, and owned files.
2. Define/update presentation ViewModel in `ViewModels/`. Never bind database Entities directly to UI forms.
3. Define/update BLL Service interface and implement business rules in `Services/`.
4. Define/update DAL Repository interface and implementation in `Repositories/`.
5. Implement LINQ query / persistence logic in the corresponding DAO in `DAOs/`.
6. Add/update MVC Controller to invoke Service interface only.
7. Add/update Razor View with modal popups, server validation (`ModelState`), and anti-forgery tokens.
8. Register services, repositories, and DbContext in `Program.cs` with appropriate lifetimes.
9. Verify with `dotnet build` and automated unit tests.
