# Allusion – agent notes

Windows-only WPF app, .NET 10, Caliburn.Micro MVVM, xunit tests.

## Commands

```
dotnet build Allusion.sln -c Release
dotnet test Allusion.Tests/Allusion.Tests.csproj -c Release
```

Config lives in `%LocalAppData%/Allusion/AllusionConfiguration.json`.

## Conventions

- No code comments unless asked.
- Caliburn.Micro `EventAggregator`: subscribe exactly once per instance.
  Never combine `SubscribeOnBackgroundThread` + `SubscribeOnUIThread` on the
  same object — every handler would run twice.
- `ImageItemService` has exactly one instance, created inside `PageManager`.
  Do not register or resolve another one — a second subscriber imports every
  paste/drop twice with distinct `ImageItem` objects.
- Clipboard/drop strategies (`ClipboardService`): first non-empty result wins,
  order FileDrop → Bitmap → Text/URL. `Bitmap` format is owned by
  `ImageDataStrategy`; `DataObjectImageExtractor.GetWebBitmapAsync` handles
  Html/Text downloads only. A browser drop exposes the same image in several
  formats, so overlapping handlers duplicate imports.
- `UpdateService` compares the GitHub `releases/latest` tag against the entry
  assembly version, not the executing assembly. Only 404 means "no update"
  (null); network/parse/HTTP failures throw so callers can report them
  instead of a false "up to date". See `docs/updates.md`.
- Releasing: tag stable `vX.Y.Z` (no suffix; `-alpha` tags are ignored by the
  updater and flagged prerelease by `release.yml`). CI stamps the build from
  the tag. After releasing, bump `<Version>` in `Allusion/Allusion.csproj` to
  the next `-dev` and prepend a `ReleaseNoteEntry` in `ReleaseNotesViewModel`.
