# SmartSave - AGENTS.md

## Goal
Windows WPF tray app that watches Downloads and suggests:
- top 3 existing folders
- OR "create new folder + move" if no good match.
Local-first, safe, later: undo/history.

## Commands
- Build: dotnet build SmartSave.sln
- Run: dotnet run --project .\SmartSave.App\SmartSave.App.csproj

## Conventions
- Small, compilable commits
- Services: SmartSave.App/Services, UI: SmartSave.App/UI, Models: SmartSave.App/Models
- Never delete user files; moves should be reversible later
- Handle partial downloads (.crdownload, locked files): wait until stable
