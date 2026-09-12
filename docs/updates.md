# Updates – In-App Update Notification + Velopack Installer

Per-user install, no UAC. Notify + one-click download/apply/restart.

## How it works

- `IUpdateService` / `UpdateService` (`Allusion.WPFCore/Service/UpdateService.cs`) queries
  `GET https://api.github.com/repos/SamHatami/Allusion/releases/latest`.
  GitHub's `latest` endpoint already excludes drafts and prereleases; the client
  additionally rejects any tag containing `-` (`ParseStableVersion`), so only
  stable tags like `v1.0.0` qualify – `v1.6-alpha` never triggers.
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
  `Download update` button.

## Install / apply (`Velopack`)

- `VelopackApp.Build().Run()` runs first in `App()` – handles install/update/uninstall
  hooks, per-user shortcuts and Add/Remove Programs entry. No UAC (never writes to
  `Program Files` / `HKLM`).
- Clicking Download tries `IUpdateInstaller` (`VelopackUpdateInstaller` in
  `Allusion/Service/`): `UpdateManager` + `GithubSource` (prereleases off) →
  download → `ApplyUpdatesAndRestart`. If that path fails (dev build, no feed yet),
  it falls back to opening the release page in the browser.
- Version detection without elevation: Velopack tracks `%LocalAppData%/Allusion`
  (`RELEASES` feed file); the running assembly version is stamped by CI (see below).

## Releasing (required for the check to ever fire)

1. Tag stable as `vX.Y.Z` (no `-alpha` suffix): `git tag v1.0.0 && git push origin v1.0.0`.
2. `release.yml` stamps the build (`-p:Version=` from tag, `0.0.0-dev` for manual runs),
   publishes the single-file exe, then `vpk pack` produces `Allusion-win-Setup.exe`,
   `Allusion-<v>-full.nupkg`, portable zip and `releases.win.json` – all uploaded to
   the GitHub Release (`Releases/*`). First-time users run `Setup.exe`; existing
   installs update in-app. The raw `Allusion.exe` asset stays as portable fallback.
   (Setup is unsigned – expect a SmartScreen prompt until signing is set up.)
3. Prepend a `ReleaseNoteEntry` in `ReleaseNotesViewModel` so Help documents the release.

## Tests

`Allusion.Tests/UpdateServiceTests.cs` – stub `HttpMessageHandler`: newer → info,
current → null, prerelease → null, 404 → null, missing `.exe` → page-only,
1-hour cache, `ParseStableVersion` theory. No network access in tests.
`VelopackUpdateInstaller` is intentionally thin (all failures → browser fallback)
and untested – it only runs against the live feed.
