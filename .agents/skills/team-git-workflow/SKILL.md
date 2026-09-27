---
name: team-git-workflow
description: Use when the user explicitly asks to start a feature branch, commit changes, push changes, prepare a Pull Request, or follow the AIVES team's Git workflow; do not publish merely because coding finished.
---

# Team Git Workflow

Git publishing operations (committing, pushing, creating Pull Requests, and merging) strictly require explicit user authorization. Never commit, push, or open a PR automatically upon completing an ordinary coding task. Work exclusively inside this repository and preserve all pre-existing changes.

## 1. Identity & Branch Naming Convention

- Determine the responsible team member from the assigned task before creating or checking out a branch. If ambiguous, ask. Never use another teammate's name.
- Every member works on a task branch following the convention:
  `<member-name>/<feature-name>`
  Format rules: lowercase ASCII, words separated by hyphens, no spaces.
  Examples: `kien/news-management`, `vy/category-management`, `an/authentication`, `minh/account-management`.
- Never commit directly to `main`. Create feature branches from an up-to-date, agreed base.

## 2. Preflight Inspection (Before Any Git State Change)

Run read-only inspection commands:
`git status`, `git branch --show-current`, and `git remote -v`.

Before performing any branch creation, staging, or switching:
1. Identify the current branch and upstream tracking status.
2. Identify all staged, unstaged, and untracked files.
3. Determine which changes belong to the current task versus other tasks or teammates.
4. Check whether the target branch already exists locally or remotely.
5. **Preserve Teammate Changes:** Never run `git reset`, `git clean`, `git checkout -- .`, or `git stash drop` to clear changes. If uncommitted work makes switching branches unsafe, stop and report the situation.

If the worktree contains unrelated changes:
- Separate task files explicitly during staging.
- If separation is unclear or overlapping, stop and request user clarification before proceeding.

## 3. Non-Destructive Git Error Diagnosis

When Git encounters an unexpected state or error, diagnose using non-destructive commands:

- **Target Branch Already Exists:**
  - Check local and remote branches: `git branch -a --list "*<feature-name>*"`
  - If the branch exists locally, inspect its last commit: `git log -n 1 <branch>`
  - Switch to the existing branch only if safe: `git checkout <branch>`
  - Never delete (`git branch -D`) or overwrite existing branches without explicit authorization.
- **Checkout Blocked by Working Tree Changes:**
  - Run `git status -s` to list conflicting files.
  - Never run `git checkout -f` or `git reset --hard`.
  - Report conflicting files to the user and request instructions (e.g. stashing or committing to the current branch).
- **Push Rejected (Non-Fast-Forward / Diverged):**
  - Fetch remote state: `git fetch origin`
  - Inspect divergence: `git log HEAD..origin/<branch> --oneline`
  - Never use `--force` or `+` to push.
  - Report the incoming commits to the user and ask whether to rebase or merge.
- **Missing Remote or Authentication Failure:**
  - Verify remote configuration: `git remote -v`
  - Check remote connectivity non-destructively: `git ls-remote origin`
  - Never print passwords, tokens, or credentials to chat logs or commit history.

## 4. Commit Discipline

- **Explicit Authorization Required:** Only commit when the user explicitly instructs to commit.
- **Explicit File Staging:** Stage only files relevant to the specific task: `git add <file1> <file2>`. Avoid `git add .` or `git add -A` when unrelated files or untracked scaffold files are present.
- **Diff Inspection:** Inspect staged files and diff before committing (`git diff --staged`).
- **Forbidden in Commits:** Never commit local configuration secrets, passwords, connection strings with credentials, build binaries (`bin/`, `obj/`), test logs, or broken builds.
- **Commit Message Format:** Use Conventional Commits with clear intent:
  - `feat: implement news article create and update modals`
  - `fix: prevent deletion of category referenced by active news`
  - `refactor: extract singleton news article dao LINQ query`
  - `docs: update architecture skill and task assignments`
- Document changes to shared files (`Program.cs`, `appsettings.json`, `AIVESDbContext.cs`, shared layout) clearly in commit messages.

## 5. Push & Pull Request Guidelines

- **Explicit Authorization Required:** Only push or create PRs upon explicit user request.
- **Push Target:** Push only the current feature branch to its remote counterpart:
  `git push -u origin <member-name>/<feature-name>`
  Never push directly to `main`. Never push another teammate's branch.
- **Pull Request Preparation:**
  - Target the agreed integration branch (or `main`).
  - Document: function ID, implemented features, changed files/layers, automated test results, and known limitations.
- **No Automatic Merging:** Never merge a PR or branch automatically. Merging requires peer review and explicit user approval.
- Verify and report the actual command exit code, commit hash, and remote branch name.
