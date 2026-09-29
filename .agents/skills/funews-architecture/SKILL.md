---
name: funews-architecture
description: Apply AIVES MVC, BLL and DAL boundaries when implementing or reviewing features, entities, repositories or structure, with Group 6 viva-exam feedback and reporting as the current priority.
---

# AIVES architecture — Group 6 feedback and reports

The historical skill name is retained for compatibility. Current user-approved priority is student per-attempt reports and lecturer class statistics for a viva-exam system. Existing News code is not evidence that exam reporting works. Preserve it; do not migrate stacks or restore historical projects without authorization.

## Read only the relevant project context

- `docs/business-rules.md`: approved DEC-01..DEC-12; authoritative formulas and attempt semantics.
- `docs/ARCHITECTURE.md`: actual projects, target boundaries, persistence and test limits.
- `docs/TASKS.md`: Group 6 tasks, dependencies and unassigned ownership.
- `docs/G6-IMPLEMENTATION-HANDOFF.md`: allowed implementation packages, proposed contracts and explicit fixture settings. Read before feature implementation; its proposals do not establish production schema or grant Git/DB write authority.
- `docs/CODE-REVIEW-CHECKLIST.md`: review evidence/status and mock limits; read for reviews.
- `docs/archive/` describes legacy News only, not current exam requirements.

## Enforce architectural boundaries

Use MVC → BLL → DAL project references, with MVC referencing DAL at startup for DI.
The real data flow is View → Controller → Service → Repository → DAO → scoped DbContext → SQL Server.
Controllers use BLL interfaces, never repositories, DAOs, DbContext/DbSet or SQL. Keep ViewModels in MVC, business/report calculations in BLL, entity mapping and EF/LINQ persistence in DAL.
Service code must not use HTTP/View types or DbContext. Do not duplicate calculations independently in Views/charts.
Preserve Repository and DAO boundaries; don't add empty CRUD classes just to match examples.
DAOs use thread-safe Singleton instances without retaining scoped contexts. Scoped repositories pass context per operation. Do not run concurrent queries on the same context.

## Protect semantics and integration boundaries

Use approved per-attempt/aggregate rules, not legacy News roles, News.CreatedDate or highest-attempt defaults. Do not invent Student role codes or convert pass/fail to scores.
Resolve authenticated identity and class/student scope at trusted boundaries. Missing auth integration is a dependency, not permission to hard-code a production identity.
Retain original attempt settings and distinguish completed exams from completed grading. Fixtures may supply missing source data; the reporting logic under test must remain real.
Connection strings and credentials come from configuration. Existing SQL scripts describe News; they do not create exam-report data. Schema changes require a separate reviewed plan; no runtime migrations/initializers.

## Implementation and review

Before implementation, identify task, owner, existing branch changes and permitted files from the handoff. Reuse compatible contracts; stop only the conflicting part if teammate-owned files or missing authority prevent it.
Implement only the assigned package. Test normal, boundary and unauthorized scopes, pending grading, mixed scales and duplicate rows. Build and run safe relevant tests; never enable DB-write tests by assumption.
For review, report actual methods/files/commits and separate defects from not-yet-pushed dependencies. Mock pass does not prove HTTP authorization, UI behavior, SQL constraints or AI scoring.
Do not modify source during a review-only task, publish Git changes, or change shared components without the corresponding user instruction.
