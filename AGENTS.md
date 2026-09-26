# PRN222 Assignment 01 — Agent & Team Instructions: FU News Management System

This document directs agents to the project's architecture and Git skills. Read `.agents/skills/funews-architecture/SKILL.md` before architecture or feature work and `.agents/skills/team-git-workflow/SKILL.md` before explicitly requested Git operations. Do not search outside this workspace; preserve teammate changes; build and run relevant safe tests; never push or merge without explicit instruction.

## 1. Project Overview & Target
- **Course:** PRN222 (Application Development with .NET Core)
- **Project:** FU News Management System (`FUNewsManagementSystem`)
- **Technology Stack:** ASP.NET Core MVC (Web App), Entity Framework Core, LINQ, Microsoft SQL Server
- **Main Demonstration Flow:** **News Article Management** (Staff)

## 2. Hard Architectural Rules (Violations may result in 0 marks)
1. **Strict 3-Layer Architecture:**
   - Presentation: `StudentNameMVC` (Controllers, Views, ViewModels)
   - Business Logic: `FUNewsManagement.BLL` (Service interfaces & implementations)
   - Data Access: `FUNewsManagement.DAL` (Entities, Repository interfaces & classes, DAOs & DbContext)
2. **Controller Database Access Rule:**
   - **ABSOLUTE BAN:** Controllers MUST NOT inject or directly access `FUNewsManagementDbContext`, `DbSet`, or execute SQL queries.
   - Controllers depend EXCLUSIVELY on Service interfaces (`INewsArticleService`, `ICategoryService`, `ISystemAccountService`, etc.).
3. **Repository & DAO Pattern:**
   - Services call Repositories.
   - Repositories call Data Access Objects (DAOs).
   - DAOs query the `FUNewsManagementDbContext` via LINQ.
4. **Singleton Pattern:**
   - All DAOs (`NewsArticleDAO`, `CategoryDAO`, `SystemAccountDAO`, `TagDAO`) MUST implement a thread-safe **Singleton Pattern** (via `Instance` property).
5. **Configuration Sources:**
   - Database connection string MUST come from `appsettings.json` (`ConnectionStrings:DefaultConnection`).
   - Default Administrator account credentials MUST come from `appsettings.json` (`DefaultAdmin:Email` and `DefaultAdmin:Password`). Never hard-code passwords in C# files.
6. **Default Route:**
   - The default URL route MUST land on `Account/Login`.
7. **UI Requirements:**
   - Create and Update operations for News, Categories, and Accounts MUST use **Popup Modals / Dialogs**.
   - Delete operations MUST display an explicit **Confirmation Modal / Dialog** with Cancel and Confirm buttons.
8. **Business Rules:**
   - A Category CANNOT be deleted if it is currently referenced by any News Article. This rule MUST be enforced in `CategoryService.DeleteCategory()`.

## 3. Pull Request & Quality Gate
- Before any PR is merged to `main`, automated unit tests MUST pass in GitHub Actions.
- Never commit broken builds, hardcoded database credentials, or empty exception catches.

## 4. Team workflow: architecture review before implementation
- The owner has authorized the structural refactor on `kien/restructure-mvc-bll-dal`. This does not authorize implementing unrelated features, changing database state, or claiming empty named files constitute completed MVC/layers.
- The current zero-byte `.cs` and feature `.cshtml` files are placeholders. A project/folder/reference diagram is only an architectural plan; a working three-layer flow must later be verified through real methods and a successful build.
- Every implementation change must belong to one assigned GitHub issue with function IDs, explicit file ownership, dependencies, acceptance criteria, and a named reviewer. Read the issue and the function plan when available before editing. Do not expand into another member's files without agreement on the issue/PR.
- One member owns each shared file at a time, especially `Program.cs`, project files, entity/DbContext mapping, `_Layout.cshtml`, and shared JavaScript. Agree on interfaces and model contracts in an issue before parallel implementation. Use separate branches/PRs; do not merge a scaffold-only branch into `main` while the solution is broken.
- Target a framework explicitly allowed by Assignment 1 (.NET 5/6/7/8) and make the CI SDK match; the solution now targets .NET 8.
- Do not infer table columns, key types, or News–Tag cardinality from filenames or generated planning text. The exact instructor/database schema must be provided or identified within the approved project scope before EF mapping or tag behavior is implemented. Mark schema-dependent issues blocked until then.
- For completed implementation issues, show the real `View → Controller → Service → Repository → DAO/DbContext` call path, backend role/ownership checks, server validation, build/test results, and manual UI checks where relevant. A green test run with no meaningful tests is not sufficient evidence.
- Structure review approves only names, boundaries, dependencies, and ownership; it does not waive the later build/test quality gate.
- **DB-first handoff:** the project owner controls schema, `FUNewsManagement.DAL`, database configuration, and DB-related `Program.cs` registrations until the read-only connection/contract gate passes. Other members request data-contract changes through an issue and owner review. This is not permission to run migrations or mutate a database.
- After the DB contract is approved, feature owners implement their Service/MVC files against the agreed repository interfaces. Cross-module News queries needed by History, Public/Lecturer, Category deletion, and Report must be included in the contract before parallel feature work. A later contract change requires its own reviewed issue.
