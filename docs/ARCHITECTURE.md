# FU News Management System architecture

## Current state (structural refactor)

The solution is `FUNewsManagementSystem.sln`. The existing MVC project is named `StudentNameMVC`; that name was retained, not selected as a student's real name. The previous scaffold split BusinessObjects, DataAccess, Repositories, and Services into four separate projects. Those source files contained zero bytes. They were moved into two projects without inventing entity fields, methods, or database relationships:

```text
FUNewsManagementSystem.sln
├─ StudentNameMVC/                  ASP.NET Core MVC presentation
│  ├─ Controllers/                   existing empty module placeholders
│  ├─ Views/                         existing Razor views; most feature views are empty
│  ├─ ViewModels/                    moved empty presentation-model placeholders
│  ├─ Models/ErrorViewModel.cs       template error-page model
│  ├─ wwwroot/
│  ├─ Program.cs                     MVC registration, middleware, Account/Login route
│  └─ appsettings.json               connection/admin configuration values
├─ FUNewsManagement.BLL/
│  ├─ Interfaces/                    moved empty service-interface placeholders
│  └─ Services/                      moved empty service placeholders
├─ FUNewsManagement.DAL/
│  ├─ Entities/                      moved empty entity placeholders
│  ├─ Context/                       moved empty DbContext placeholder
│  ├─ DAOs/                          moved empty DAO placeholders
│  └─ Repositories/
│     ├─ Interfaces/                 moved empty repository-interface placeholders
│     └─ Implementations/            moved empty repository placeholders
└─ FUNewsManagement.Tests/          existing empty test placeholders
```

All four projects target .NET 8, matching the CI SDK. A successful compile verifies project references and Razor syntax, **not** a working News flow. There is presently no implemented EF Core DbContext, SQL Server provider configuration, CRUD, authentication, service, repository, DAO, singleton, or meaningful test. Do not present placeholder names as completed assignment requirements. The database/schema contract must be approved before filling Entities and mappings.

## Responsibilities and dependencies

| Layer | Project | Responsibility |
| --- | --- | --- |
| Presentation | `StudentNameMVC` | HTTP requests, `ModelState`, UI authorization entry points, ViewModels, Razor views and redirects. Controllers call service interfaces only. |
| Business logic | `FUNewsManagement.BLL` | Use cases, business validation and authorization, including rejecting deletion of a category used by News. Services call Repository interfaces. |
| Data access | `FUNewsManagement.DAL` | Entities, EF Core DbContext/mappings, LINQ queries, repository contracts/implementations, and DAOs when useful. |

Project references are `StudentNameMVC → FUNewsManagement.BLL → FUNewsManagement.DAL`, plus `StudentNameMVC → FUNewsManagement.DAL` for future startup/DI wiring. `FUNewsManagement.Tests` references BLL and DAL. DAL references neither BLL nor MVC; BLL does not reference MVC. The MVC→DAL project reference is **not** permission for Controllers to inject repositories, DAOs, or DbContext.

Entities represent approved database tables/relationships and stay in DAL. ViewModels represent form and display input/output and stay in MVC. The six existing UI-specific ViewModel filenames have been moved out of the MVC `Models` folder; `Models/ErrorViewModel.cs` remains for the template error view.

Repository interfaces are the BLL-facing persistence contracts. When a separate DAO is actually implemented, the DAO performs concrete EF Core operations and LINQ queries below the Repository. Do not implement two identical CRUD layers merely to satisfy names. The existing team rule calls for a thread-safe DAO Singleton; any such instance must not retain a scoped DbContext. At this stage all Repository/DAO files are empty, so this boundary is a **design contract, not an implemented call chain**.

`Program.cs` currently registers MVC and configures middleware/default routing only. It does not register Services, Repositories, or DbContext because none has a concrete implementation. When those are implemented, configure the SQL Server DbContext from `ConnectionStrings:DefaultConnection` with an appropriate scoped lifetime; register compatible Service/Repository lifetimes. Never register DbContext as Singleton. Default-admin credentials are configured under `DefaultAdmin`; do not duplicate them in C#.

## News Article request flow

The required demonstration flow is `News Razor View → NewsArticleController → INewsArticleService/NewsArticleService → INewsArticleRepository/NewsArticleRepository → NewsArticleDAO or DbContext → SQL Server`. Search should use LINQ in DAL. Create/Update must use real modals and server validation; Delete must confirm; Category and Tag handling must follow the approved schema. Currently `Views/NewsArticle/*.cshtml`, `NewsArticleController.cs`, the corresponding BLL/DAL `.cs` files, and relevant tests are empty. **List, Search, Create, Update, Delete, Category, and Tags cannot be traced or demonstrated yet.** The configured default route points to `Account/Login`, but `AccountController` and its view are also empty, so a working login route is not established.

## Adding a feature

First read the assigned issue/function ID, ownership, and approved database contract. Determine its business rules and ViewModel; add only needed Service interface/implementation, Repository operations, DAL EF Core query/mapping, Controller action, and View. Wire DI after concrete implementations exist. Keep authorization and business checks on the backend, validation on the server, and UI dialogs where the assignment requires them. Verify the actual View → Controller → Service → Repository → DAO/DbContext path, then build, run meaningful safe tests, and manually check UI/database behavior without destructive migrations.

## Architecture verification checklist

- [x] MVC, BLL, and DAL project boundaries and one-way project references exist.
- [x] No current Controller accesses DbContext (the Controllers are empty, not yet compliant implementations).
- [x] Solution compiles after restore on the available .NET 10 SDK targeting .NET 8.
- [ ] Real Service → Repository → DAO/DbContext call path and DI registrations.
- [ ] EF Core SQL Server mapping based on approved schema, safe DbContext lifetime, LINQ queries.
- [ ] Deliberate valid Singleton implementation without scoped-lifetime capture.
- [ ] News main flow, role/ownership checks, server validation, modal/confirmation UI.
- [ ] Meaningful automated tests and manual runtime/database verification. The local machine lacks the .NET 8 runtime, so its testhost cannot currently run; CI is configured for .NET 8.
