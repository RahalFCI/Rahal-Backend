# Rahal

## Local admin seed

The API can create or update a test admin user during startup without hardcoding credentials. Set these environment variables before running the app:

```powershell
$env:ADMIN_USER_EMAIL="admin@example.com"
$env:ADMIN_USER_PASSWORD="ChangeMe!123"
$env:ADMIN_USER_DISPLAY_NAME="Admin"
```

When running through Docker Compose, these values are passed into the API container as `AdminUser__Email`, `AdminUser__Password`, and `AdminUser__DisplayName`. If email or password is missing, admin seeding is skipped. If the configured admin already exists, startup confirms the email, assigns the admin role, and updates the password to the configured value.

The password must satisfy the configured Identity policy: at least 6 characters, uppercase, lowercase, digit, and non-alphanumeric character.
