# Allusion Docs – Index for Agents + Humans

> Start here. `AGENTS.md` (repo root) is the slim entry-point. This folder holds details.

## How to use

1. New agent? Read in this order: `architecture.md` → `development.md` → `testing.md` → `services-managers.md` → `events-behaviors.md` → `models-persistence.md`.
2. Fixing a bug? Check `tech-debt-roadmap.md` first – it may already be known.
3. Touching `Allusion.Core/`? Read `legacy-core.md` first – don't use it.

## Files

| File | What it answers |
|------|-----------------|
| `architecture.md` | Solution map, Caliburn.Micro MVVM, DI, View/ViewModel conventions |
| `development.md` | Build/run/debug on Windows, .NET 8, CI |
| `testing.md` | xUnit + FakeItEasy + FluentAssertions patterns, config isolation |
| `services-managers.md` | Clipboard/Bitmap/ImageItem services, board/page managers |
| `events-behaviors.md` | Full event table (Option B), behaviors, `MultiKeyGesture` input |
| `models-persistence.md` | `ReferenceBoard`/`BoardPage`/`ImageItem` JSON, PNG rule, `AllusionConfiguration` |
| `updates.md` | Update check service, release process, title-bar indicator |
| `settings-themes.md` | Settings dialog, ThemeService presets, .NET version |
| `tech-debt-roadmap.md` | Known issues, typos, anti-patterns, improvement backlog |
| `legacy-core.md` | Why `Allusion.Core/` is deprecated |

## Rules for agents

- Prefer editing existing files over creating new ones.
- Never add `Allusion.Core` to `Allusion.sln` or reference it – see `legacy-core.md`.
- Never use real `Clipboard.GetDataObject()` in tests – fake `IDataObject`.
- Use `await _events.PublishOnUIThreadAsync(...)` for selection/drag, `PublishOnBackgroundThreadAsync(...)` for state.
- Verify with `dotnet build Allusion.sln -c Release` and `dotnet test Allusion.Tests/Allusion.Tests.csproj -c Release`.
