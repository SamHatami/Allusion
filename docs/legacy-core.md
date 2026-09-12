# Legacy – `Allusion.Core/`

> **Do not use. Deprecated.**

## Status

- 11 files, `net8.0`, `Nullable+ImplicitUsings`.
- **Not in `Allusion.sln`** (only `Allusion, WPFCore, Tests`).
- Not referenced by any active project. Tests target `WPFCore`, not `Core`.
- Duplicates `WPFCore.Board` types (`ArtBoard` vs `ReferenceBoard`, `BoardPage`, `ImageItem`, `NoteItem`, `IItem`).

## Why deprecated

- `Handlers/ArtBoardHandler.cs` buggy: `GetPastedBitmap()->C:\Temp\1.png` hardcoded, `OpenProjectFile` passes `GetDirectoryName(fullPath)` to `Read()`, `SaveToProject` null-risk.
- `Helpers/BitmapHelper.cs` uses `Console.WriteLine+throw`.
- `Extensions/Clipboard.cs` (`ExtractUrlFromDataObject`) stub returns `""`, only `Debug.WriteLine`.
- `AllusionConfiguration` (static, obsolete: `ProjectFolder, DefualtFolder [sic]=MyDocuments\Allusion`) conflicts with active `Allusion.WPFCore.AllusionConfiguration` (instance: `DataFolder=LocalAppData\Allusion`). Do not confuse.
- Brittle absolute `PresentationCore.dll -> C:\Program Files\dotnet\shared\...\8.0.8` – breaks on SDK bump.
- `Constants` (`internal`, `Allusion="Allusion"`), `Iitem.cs` lowercase – stale.

Active code is in `Allusion.WPFCore/`. See `services-managers.md` and `models-persistence.md`.

## Rules for agents

1. Do NOT add `Allusion.Core` to sln/refs without explicit migration approval.
2. Do NOT copy types from `Core` – use `WPFCore.Board.*`.
3. Do NOT fix bugs in `Core` – log in `tech-debt-roadmap.md` if blocking removal.
4. Safe removal path (future): verify no refs via grep, delete folder, ensure `dotnet build Allusion.sln` still passes.
