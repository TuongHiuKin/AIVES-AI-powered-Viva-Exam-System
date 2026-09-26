---
name: team-git-workflow
description: Use when the user explicitly asks to start a feature branch, commit changes, push changes, prepare a Pull Request, or follow the FU News team's Git workflow; do not publish merely because coding finished.
---

# Team Git workflow

Git publishing requires an explicit user instruction. Never push or create a PR automatically after an ordinary coding task. Work only in this repository and preserve every pre-existing change.

## Identity and branch

- Determine the actual responsible member from the task before creating or pushing a branch. If the member is not supplied and cannot be determined reliably, ask. Never borrow another teammate's identity.
- Each member uses one coherent-task branch named `<member-name>/<feature-name>`, with lowercase ASCII and hyphens, no spaces: `kien/news-management`, `vy/category-management`, `an/authentication`, `minh/account-management`.
- For the explicitly authorized architecture restructuring task, use `kien/restructure-mvc-bll-dal` unless its operator supplies a different name.

## Preflight before any Git modification

Run the equivalents of `git status`, `git branch --show-current`, and `git remote -v`. Identify current branch, staged/unstaged/untracked files, remote, and which changes belong to other tasks. Check whether the target branch already exists. Create a feature branch from the appropriate current base before new coding; do not switch to or pull `main` when uncommitted work makes that unsafe. Never reset, clean, stash, or discard teammate changes to simplify branching.

If the worktree is dirty, separate task changes from pre-existing work. If separation or ownership is unclear, stop the unsafe Git operation and ask/report the conflict. A branch switch does not transfer ownership of pre-staged content to the new task.

## Commit

- Stage only feature-related paths explicitly. Avoid `git add .` when unrelated files exist. Inspect both staged names and staged diff before committing.
- Do not commit secrets, password files, local configuration, generated binaries/build output, unrelated teammate changes, or a broken build as if it passed.
- Use a clear message, for example `refactor: organize mvc bll dal architecture`, `feat: implement news management`, `fix: prevent deletion of used categories`, or `docs: add architecture and team skills`.
- Explain changes to shared `Program.cs`, `appsettings.json`, DbContext, Entities, shared Layout, and Service/Repository interfaces in the commit or PR description.

## Push and PR

- Push only the current feature branch to origin, equivalent to `git push -u origin <member-name>/<feature-name>`. Never push directly to `main`, force-push, or push another member's branch without explicit authorization.
- Prepare a PR only when explicitly requested, against the agreed integration branch. Include implemented feature, changed files/modules, Assignment requirements, build/test results, and known issues. Do not merge automatically. A push is not a review or merge.
- Report the branch, committed files, commit hash, actual push result, remote branch, and PR link only if a PR was really created. Never claim success without checking command results.
