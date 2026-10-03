# FIX-03: Password recovery

Database Staff/Lecturer accounts can request a reset from Account/ForgotPassword.
The configuration administrator is recovered by updating DefaultAdmin configuration.
The request response is the same for unknown, locked and active accounts, including
delivery failures. Check server logs for configuration/delivery failure types.

Tokens are encrypted and authenticated with ASP.NET Data Protection, expire after
20 minutes, and bind the account ID, normalized email and current password hash
fingerprint. No plaintext token is stored in the database. A conditional DAO update
checks the old password hash and active state, so only one concurrent reset succeeds.
Changing the password invalidates every outstanding reset link for the account.
Use persistent, shared Data Protection keys when deploying multiple application instances.

SMTP configuration is read from the `PasswordResetEmail` section. Supply these keys
through environment variables or local secrets; never commit SMTP credentials:

- `PasswordResetEmail__Host`
- `PasswordResetEmail__Port` (default 587)
- `PasswordResetEmail__EnableSsl` (default true)
- `PasswordResetEmail__From`
- `PasswordResetEmail__Username`
- `PasswordResetEmail__Password`
- `PasswordResetEmail__PublicBaseUrl` (HTTPS origin; HTTP permitted for loopback only)

Links use the configured origin, not the incoming Host header. Emails contain a
reset link and never a password. POST endpoints require anti-forgery tokens and
limit recovery requests to five per IP per minute.

Automated tests cover token expiry, invalid/replayed tokens, email mismatch, locked
accounts, new-password authentication and the conditional update on temporary SQL
Server databases. SMTP delivery needs the project's local SMTP configuration.
