# /new-phase

Scaffold a new .NET project for the specified phase.

## Usage
```
/new-phase <phase-number>
```

## Example
```
/new-phase 1
/new-phase 3
```

## What This Command Does

1. Read `context/project-context.md` to confirm which phase is active
2. Read `azure-setup/phase-0N.md` — check that Azure resources are already created
3. If resources not yet created, **stop and ask the user to complete the Azure setup first**
4. Scaffold the project:
   ```bash
   cd Phase-0N-Name/src
   dotnet new webapi -n ProjectName
   dotnet new sln -n Phase0N-Name
   dotnet sln Phase0N-Name.sln add ProjectName/ProjectName.csproj
   ```
5. Add required NuGet packages for that phase (see `context/azure-services.md`)
6. Create folder structure: `Controllers/`, `Models/`, `Services/`, `Data/` (if needed)
7. Create base files following patterns in `skills.md`:
   - `appsettings.json` with PLACEHOLDER values
   - `appsettings.Development.json` with local values from `context/project-context.md`
   - `Properties/launchSettings.json` opening Swagger on startup
   - `Program.cs` wired up for that phase's services
8. Tell the user: **"Open Phase0N-Name.sln in Visual Studio and press F5"**

## Notes
- Never hardcode real connection strings — use values from `context/project-context.md`
- `appsettings.Development.json` is pre-populated for local dev
- Always add `.gitignore` if one doesn't exist in the phase folder
