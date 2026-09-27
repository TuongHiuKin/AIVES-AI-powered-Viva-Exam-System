# AIVES — Agent & Team Instructions: PRN222 Assignment 01 (News Management System)

This document establishes the project identity, architecture rules, and Agent Skills routing for this repository.

Read `.agents/skills/funews-architecture/SKILL.md` before performing architecture, feature, or review work.
Read `.agents/skills/team-git-workflow/SKILL.md` before executing explicitly requested Git operations.
Do not search outside this workspace; preserve teammate changes; run relevant automated tests; never push or merge without explicit user instruction.

Authoritative project documents in `docs/`: `docs/business-rules.md` (business rules and approved schema decisions SC-01..SC-11), `docs/ARCHITECTURE.md` (structure, DAL contracts, verification and current implementation status), and `docs/TASKS.md` (31 functions, task assignments, ownership, and handoff).

---

## 1. Project Identity & Context

- **Course:** PRN222 (Application Development with .NET Core) — Assignment 01
- **Project Name:** **AIVES** (`AIVESSystem.sln`) — News Management System
- **Technology Stack:** ASP.NET Core MVC (.NET 8), Entity Framework Core, LINQ, Microsoft SQL Server
- **Main Demonstration Flow:** **News Article Management** (Staff)

> [!IMPORTANT]
> **Disregard Obsolete Historical Specifications:**
> This repository is intentionally dedicated to PRN222 Assignment 1 (News Management System).
> Historical planning documents in git history describing an "AI-powered Viva Exam System" (Python/FastAPI, NestJS, Next.js, PostgreSQL) are **obsolete**.
> Do NOT migrate this solution to other languages, frameworks, or database engines.

---

## 2. Solution & Project Mapping (3-Layer Architecture)

The solution is organized into strict layers targeting .NET 8:

| Layer | Project Folder | Project File | Key Contents & Responsibilities |
|---|---|---|---|
| **Presentation** (`AIVES.MVC`) | `StudentNameMVC` | `StudentNameMVC.csproj` | Controllers, Razor Views with modals, ViewModels, `Program.cs`, `appsettings.json`. (*`StudentNameMVC` is the physical template project mapping to `AIVES.MVC` per assignment submission rules*). |
| **Business Logic** (`AIVES.BLL`) | `AIVES.BLL` | `AIVES.BLL.csproj` | Service interfaces (`Interfaces/`), service implementations (`Services/`), business validation, use case coordination. |
| **Data Access** (`AIVES.DAL`) | `AIVES.DAL` | `AIVES.DAL.csproj` | `Context/AIVESDbContext.cs`, `Entities/`, thread-safe Singleton `DAOs/`, and `Repositories/`. |
| **Testing** | `AIVES.Tests` | `AIVES.Tests.csproj` | Automated unit tests (`dotnet test`). |
| **Database** | `database/` | `001-create-schema.sql` | DB-first SQL Server schema script for database `AIVES`. |

**Dependency Direction:** `StudentNameMVC → AIVES.BLL → AIVES.DAL`.
- `AIVES.BLL` references `AIVES.DAL`.
- `AIVES.DAL` must NEVER reference `AIVES.BLL` or `StudentNameMVC`.
- `AIVES.BLL` must NEVER reference `StudentNameMVC`.
- `StudentNameMVC` references `AIVES.DAL` solely for DI registration in `Program.cs`.

---

## 3. Mandatory Architectural & Assignment Rules

### Documented Assignment Requirements
1. **Application Call Flow:**
   `Razor View → Controller → Service → Repository → DAO → AIVESDbContext → SQL Server`.
2. **Controller Database Access Ban:**
   Controllers MUST NOT inject or access `AIVESDbContext`, `DbSet`, or execute SQL queries. Controllers depend exclusively on BLL Service interfaces (`INewsArticleService`, `ICategoryService`, `IAuthService`, etc.).
3. **Repository & DAO Flow:**
   Services call Repositories. Repositories call DAOs. DAOs query `AIVESDbContext` via LINQ. DAOs must NOT be bypassed.
4. **Thread-Safe Singleton Pattern on DAOs:**
   All DAOs (`NewsArticleDAO`, `CategoryDAO`, `SystemAccountDAO`, `TagDAO`) MUST implement a thread-safe Singleton Pattern (via `Instance` property).
5. **Configuration Sources:**
   - Connection string from `appsettings.json` (`ConnectionStrings:DefaultConnection`).
   - Default Administrator credentials from `appsettings.json` (`DefaultAdmin:Email` and `DefaultAdmin:Password`). Never hard-code passwords in C# files.
6. **Default Route:**
   Default URL route must land on `Account/Login`.
7. **UI Requirements:**
   - Create and Update operations for News, Categories, and Accounts MUST use **Popup Modals / Dialogs**.
   - Delete operations MUST display an explicit **Confirmation Modal / Dialog** with Cancel and Confirm buttons.
8. **Business Rules:**
   - A Category CANNOT be deleted if it is currently referenced by any News Article (`CategoryService.DeleteCategory()`).
   - Staff Profile and Own News History must resolve identity strictly from authenticated claims, never trusting client-supplied IDs.
   - Admin Reports query News Articles within `[StartDate, EndDate]` ordered descending by `CreatedDate`.

### Team Engineering Decisions
1. **Safe Singleton DAO Lifetime (Context-Per-Operation):**
   `AIVESDbContext` is Scoped. Singletons MUST NOT store a scoped `AIVESDbContext` in an instance or static field. The Scoped Repository passes `AIVESDbContext` into the DAO method as a parameter.
2. **Framework Target:** Solution targets .NET 8 across all projects; CI workflows use .NET 8 SDK.
3. **DB-First Schema Management:** Database schema is authored in `database/001-create-schema.sql` and mapped into `AIVESDbContext`. Do not mix runtime migrations.

---

## 4. Git & Team Collaboration Rules

1. **Branch Naming:** Every member works on a task branch following `<member-name>/<feature-name>` (e.g. `kien/news-management`, `vy/category-management`, `an/authentication`, `minh/account-management`).
2. **Explicit Publishing Only:** Committing, pushing, and creating Pull Requests require explicit user authorization. Never push or open a PR automatically after code generation.
3. **Working-Tree Protection:** Never run `git reset`, `git clean`, or `git stash drop` on uncommitted or teammate changes. Always inspect `git status` before touching Git state.
4. **PR Quality Gate:** Automated unit tests in `AIVES.Tests` must pass (`dotnet test`) before PR review and merge. Merging directly to `main` without review is prohibited.
