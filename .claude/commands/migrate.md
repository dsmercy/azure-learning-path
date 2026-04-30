# /migrate

Create and apply Entity Framework Core migrations.

## Usage
```
/migrate <MigrationName>        # Create + apply locally
/migrate <MigrationName> azure  # Create + apply to Azure SQL
```

## What This Command Does

### Local
```bash
# Create migration
dotnet ef migrations add <MigrationName> --project src/<ProjectName>

# Apply to local DB
dotnet ef database update --project src/<ProjectName>
```

### Azure
```bash
# Read Azure SQL connection string from context/project-context.md
# Set as environment variable (not hardcoded)
export ConnectionStrings__DefaultConnection="<azure-sql-connection-string>"

dotnet ef database update --project src/<ProjectName>

unset ConnectionStrings__DefaultConnection
```

## Notes
- Never commit `appsettings.Development.json` with real passwords
- Migrations folder: `src/<Project>/Data/Migrations/` — commit these files
- If migration fails on Azure: check firewall rule includes your current IP
- Check current IP: `curl -s https://api.ipify.org`
