# Allusion AI Agent Guidelines

> Details moved to `docs/`. Start with `docs/README.md`, then `docs/architecture.md`.

## Architecture Overview
Allusion is a Caliburn.Micro MVVM WPF application for artists to manage reference images. DI via SimpleContainer, communication via IEventAggregator. See `docs/architecture.md` and `docs/events-behaviors.md`.

**Key Projects:**
- `Allusion/` - Main WPF application with views, viewmodels, and behaviors
- `Allusion.WPFCore/` - Active shared services, managers, events, board models
- `Allusion.Tests/` - xUnit + FakeItEasy + FluentAssertions (see `docs/testing.md`)
- `Allusion.Core/` - **LEGACY, not in sln, do not use** (see `docs/legacy-core.md`)

## Core Services & Dependencies
Always inject via Caliburn.Micro's IoC container (see `docs/services-managers.md`):
- `IClipboardService` - Clipboard paste/drag-drop (strategy pattern)
- `IBitmapService` - Image loading/download (`HttpClient` singleton, 30s timeout, `CancellationToken`)
- `IReferenceBoardManager` - Board persistence
- `IPageManager` - Page navigation
- `IThemeService` - Theme presets (`Themes/*.xaml`), persisted as config `Theme` – see `docs/settings-themes.md`
- `IUpdateService` - Stable-only GitHub update check – see `docs/updates.md`

Do NOT `new HttpClient()`, do NOT add `IoC.Get<>` in ViewModels, do NOT `new PageViewModel/ImageViewModel` manually.

## Event-Driven Communication
Use `IEventAggregator` (see `docs/events-behaviors.md` for full table):
```csharp
await _events.PublishOnUIThreadAsync(new SelectionEvent(imageItem, SelectionType.Select));
```

UI/selection/drag → `PublishOnUIThreadAsync`; state → `PublishOnBackgroundThreadAsync`. Note `BoardIsModfiedEvent` typo – see `docs/tech-debt-roadmap.md`.

## Configuration Management
`AllusionConfiguration.Read()` / `Save(config)` at `%LocalAppData%/Allusion/AllusionConfiguration.json` (see `docs/models-persistence.md`):
- `GlobalFolder` - Falls back to Documents/Allusion, auto-creates dir
- `TopMost` - Always-on-top
- `Theme` - Active preset, applied by `IThemeService` on startup

## Custom Input System
Uses `MultiKeyGesture` (see `docs/events-behaviors.md`):
```xaml
<Button cal:Message.Attach="[Gesture(Ctrl+Shift+N)] = [Action CreateNewBoard]" />
```

## WPF Behaviors Pattern
See `docs/events-behaviors.md`:
- `CanvasBehavior` - Mouse, drag-drop → `DragDropEvent`
- `ImageBehavior` - Manipulation (has capture race – see tech-debt)
- `SelectionBehavior` - Selection → `SelectionEvent`

## Image Handling
Images saved as PNG regardless of source. Use `BitmapUtils` + `IBitmapService` (see `docs/models-persistence.md`).

## Testing Patterns
- xUnit with FluentAssertions, FakeItEasy for mocks
- Only `[Fact]/[Theory]` – no `[WpfFact]` in codebase
- Config tests: `[Collection("Allusion configuration")]` + `UseDataFolderForTests()` + unique temp dir, fake `IDataObject`
- See `docs/testing.md`

## Build & Run
- Windows-only .NET 10 WPF (see `docs/development.md`)
- `dotnet restore Allusion.sln; dotnet build Allusion.sln -c Release`
- Tests: `dotnet test Allusion.Tests/Allusion.Tests.csproj -c Release`

## File Organization
- Views in `Views/`, ViewModels in `ViewModels/` (Caliburn naming)
- Services in `Allusion.WPFCore/Service/`, Managers in `Allusion.WPFCore/Managers/`
- Events in `Allusion.WPFCore/Events/` + `Allusion/Events/`
- See `docs/architecture.md`

## Further reading
- `docs/README.md` – index
- `docs/tech-debt-roadmap.md` – known issues (check before filing)
- `docs/legacy-core.md` – why not to touch `Allusion.Core/`
