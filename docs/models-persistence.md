# Models & Persistence & Configuration

## Board models – `Allusion.WPFCore/Board/`

| File | Key fields |
|---|---|
| `ReferenceBoard.cs` | board root, pages, metadata |
| `BoardPage.cs` | `Description`, `ImageItem[]`, `NoteItem[]` |
| `ImageItem.cs` | `SourceImage`, `PosX/Y`, `Scale`, `ZIndex`, `Description` (+ `Source` url, `MemberOfPage`) |
| `NoteItem.cs` | note title/content (future scribble/notes) |
| `ReferenceBoardFile.cs` | on-disk envelope |
| `RefBoardInfo.cs` | listing info for open dialog |

`IItemOwner`, `IRemovableItem`, `IPageViewModel`, `IImageViewModel` in `Allusion.WPFCore/Interfaces/` define ownership/removal contracts.

## Image handling rule

- Images saved as **PNG regardless of source format** (may change – see `Readme.md`).
- Transparency not supported yet.
- Use `BitmapUtils` (`Allusion.WPFCore/Utilities/BitmapUtils.cs`) for load/convert/save; `IBitmapService` for URI/download.
- Artboard folder keeps PNGs + simple JSON data (`Source` url, notes).

## Board persistence

- `ReferenceBoardManager` handles CRUD + JSON save/load via `System.Text.Json`.
- Dirty tracking via `BoardIsModfiedEvent(bool)` → `MainViewModel.BoardIsModified`.
- `PageManager` handles add/remove/rename/clean pages.
- Each `ImageViewModel` setter publishes `BoardIsModfied(true)` – can spam; batch if editing.

## Configuration – `Allusion.WPFCore/AllusionConfiguration.cs:9`

```csharp
AllusionConfiguration.Read();  // reads %LocalAppData%/Allusion/AllusionConfiguration.json, creates if missing
AllusionConfiguration.Save(config);
```

| Property | Meaning |
|---|---|
| `GlobalFolder` | Default save location; getter falls back to `Documents/Allusion` and auto-creates dir (`AllusionConfiguration.cs:19`) |
| `TopMost` | Window always-on-top (default `true`) |
| `FirstStartUp` | First-run welcome flow |
| `IgnoredRefBoardFiles` | Files to hide in open dialog |
| `DataFolder` | `%LocalAppData%/Allusion`, `ConfigPath = DataFolder + AllusionConfiguration.json` |

- Root `AllusionConfiguration.json` in repo is a local sample (`C:\Users\...\OneDrive\Dokument\Allusion`) – do not commit/overwrite real user config.
- Tests must use `UseDataFolderForTests(tempDir)` – see `testing.md`.
