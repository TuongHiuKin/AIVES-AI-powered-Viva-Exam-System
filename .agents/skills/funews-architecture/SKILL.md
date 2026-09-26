---
name: funews-architecture
description: Apply the FU News Management System MVC, BLL, and DAL rules when creating a function, adding a Controller, Service, or Repository, modifying an Entity, refactoring project structure, or reviewing architectural compliance.
---

# FU News architecture

Use this skill for implementation and architecture review in this workspace. Read `docs/ARCHITECTURE.md` when project-specific structure, current implementation status, or contracts need clarification. Read the assigned issue and approved function plan, when available, before editing feature code. If the approved structure or database schema is unavailable, identify the missing contract instead of inventing it.

## Required boundaries

- Request path: Razor View → MVC Controller → BLL Service → DAL Repository → DAL DAO/EF Core DbContext → SQL Server. If the approved DAL intentionally uses Repository → DbContext, document that boundary rather than duplicating CRUD in a ceremonial DAO.
- Controllers receive input, check `ModelState`, call service interfaces, prepare ViewModels, and return views or redirects. They must never inject DbContext or DbSet, execute SQL, or call a Repository/DAO directly.
- MVC owns Controllers, Views, presentation ViewModels, static assets, and startup. BLL owns service interfaces, implementations, use cases, business validation, and business authorization. DAL owns Entities, DbContext/mappings, Repository interfaces/implementations, DAOs where used, and LINQ persistence queries.
- Dependency direction is MVC → BLL → DAL. MVC startup may reference DAL for EF Core and DI wiring; that exception never permits a Controller → DAL dependency. DAL must not reference BLL/MVC; BLL must not reference MVC. Avoid project-reference cycles.
- Keep database Entities distinct from UI ViewModels. Preserve existing entity fields and relationships; do not guess columns, key types, or News–Tag cardinality from filenames.

## Adding or changing a feature

1. Identify the issue, business requirements, role/ownership rules, and owned files.
2. Determine whether a presentation ViewModel is needed; reuse an existing one where suitable.
3. Define or reuse a BLL service interface and implement the use case/business rules in its service.
4. Define or reuse only the necessary DAL Repository operations.
5. Implement persistence in the DAL Repository/DAO boundary agreed for this project, using EF Core and LINQ where appropriate.
6. Add or update the MVC Controller to invoke the service interface only.
7. Add or update its Razor View and form handling.
8. Register required services/repositories/DbContext with safe lifetimes; DbContext must never be Singleton.
9. Build, run relevant safe tests, and check the end-to-end call path. Create no unused components or placeholder implementations.

## Assignment invariants

- Preserve ASP.NET Core MVC, real three-layer separation, Repository Pattern, a deliberate safe Singleton Pattern, EF Core, LINQ, SQL Server, CRUD, Search, and server-side validation. A class name or empty file is not proof of implementation.
- Connection strings and default-admin credentials come from ASP.NET configuration, never hard-coded C# values. Do not commit local secrets.
- Category deletion must be rejected by `CategoryService` when any News Article references the category; UI-only prevention is insufficient.
- Account, Category, and News Create/Update require actual popup/modal behavior. Delete requires a confirmation dialog with Cancel and Confirm. POST actions must validate on the server and use anti-forgery protection where appropriate.
- News Article Management is the main demo flow. Preserve and trace List, Search, Create, Update, Delete, Category, and Tag integration through View → Controller → Service → Repository → DAO/DbContext. Do not assume the Tag schema.
- Do not move business rules into Controllers, add direct Controller database access, reorganize the solution without task authorization, duplicate existing implementations, create unrelated infrastructure, or substitute another architecture. If a requested change conflicts, explain the conflict before proceeding.

## Completion check

Verify file placement, dependency direction, Controller isolation from DAL, DI registrations and lifetimes, configuration, real request flow, build, and relevant safe tests. Report manual UI/database behavior as unverified until it is actually exercised. Preserve teammate changes and never run destructive database operations without explicit scope and authorization.
