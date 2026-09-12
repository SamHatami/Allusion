# Updates – In-App Update Notification

Notify + download link. No auto-install (installer is a later step).

## How it works

- `IUpdateService` / `UpdateService` (`Allusion.WPFCore/Service/UpdateService.cs`) queries
  `GET https://api.github.com/repos/SamHatami/Allusion/releases/latest`.
  GitHub's `latest` endpoint already excludes drafts and prereleases; the client
  additionally rejects any tag containing `-` (`ParseStableVersion`), so only
  stable tags like `v1.7` qualify – `v1.6-alpha` never triggers.
- The tag is compared against the running assembly version. Newer → `UpdateInfo(Version, PageUrl, DownloadUrl, Notes)`.
  `DownloadUrl` is the first `.exe` release asset, else the release page is used.
- All failures (404 = no stable release yet, offline, bad JSON, cancelled) return `null`
  silently – the check never crashes or nags.
- Results are cached in memory for 1 hour.

## UX

- **Silent background check** on startup (`MainViewModel.CheckForUpdatesOnStartupAsync`).
  Skipped for unstamped builds (local `1.0.0.0` or CI `0.0.0-dev`) so dev builds don't nag.
- **Title-bar up-arrow** (`MainView.xaml`, `OpenUpdates`) appears only when `HasUpdate`.
  Tooltip shows the version; click opens Help on the Updates topic.
- **Help → Updates topic** (`UpdateViewModel`/`UpdateView`): current version, status text,
  `Check now` button (disabled while checking), release notes of the new version,
  `Download update` button (opens `.exe` asset, falls back to release page).

## Releasing (required for the check to ever fire)

1. Tag stable as `vX.Y` (no `-alpha` suffix): `git tag v1.7 && git push origin v1.7`.
2. `release.yml` stamps the build: `-p:Version=$tagWithoutV` (falls back to `0.0.0-dev`
   for manual dispatches). Attach `Allusion.exe` to the GitHub Release as usual.
3. Prepend a `ReleaseNoteEntry` in `ReleaseNotesViewModel` so Help documents the release.

## Tests

`Allusion.Tests/UpdateServiceTests.cs` – stub `HttpMessageHandler`: newer → info,
current → null, prerelease → null, 404 → null, missing `.exe` → page-only,
1-hour cache, `ParseStableVersion` theory. No network access in tests.

## Future (installer step)

Auto-download + replace needs an updater stub (single-file exe can't replace itself
while running). When that lands, `UpdateInfo.DownloadUrl` is already the right hook.
