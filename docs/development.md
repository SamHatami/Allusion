# Development – Build / Run / Debug

## Prerequisites

- Windows only (`net10.0-windows` + WPF + `FrameworkReference Microsoft.WindowsDesktop.App`).
- .NET 10 SDK.

## Commands

```powershell
dotnet restore Allusion.sln
dotnet build Allusion.sln -c Release
dotnet test Allusion.Tests/Allusion.Tests.csproj -c Release
dotnet run --project Allusion/Allusion.csproj -c Debug
```

- Startup project is `Allusion/` (`WinExe`).
- Opening by file association: `Bootstrapper.OnStartup` (`Allusion/Bootstrapper.cs:73`) handles `e.Args[0]` → `MainViewModel.StartUpByFile`.
- Config file auto-created on first run – see `models-persistence.md`.

## CI

- Active: `.github/workflows/release.yml` – `windows-latest`, `setup-dotnet 8.0.x`, `restore sln` → `dotnet test -c Release --no-restore` → `publish Allusion -r win-x64 --self-contained -p:PublishSingleFile=true`, upload `Allusion.exe`, `softprops/action-gh-release` on `v*` + `workflow_dispatch`.
- Stale: `.github/workflows/dotnet-desktop.yml` – template placeholders (`Solution_Name`, `Wap_*`), MSIX/signing. Do not use as-is.

## Debugging tips

- Caliburn binding failures: check `x:Name` matches ViewModel property/action exactly.
- Custom gestures: `Allusion/Input/MultiKeyGesture.cs`, `KeyTrigger.cs`, `MultiKeyGestureConverter.cs`. Note `Bootstrapper.CreateKeyMagic` (`Allusion/Bootstrapper.cs:90`) only uses first key sequence – multi-chord gestures are truncated.
- `$mousepoint` special value registered in `ConfigMessageBinder` (`Allusion/Bootstrapper.cs:123`) – only works for `MouseEventArgs`.
- Viewport wiring lives in `PageView.xaml.cs`, not XAML alone.

## What not to commit

Per `.gitignore`: `bin/`, `obj/`, `TestResults/`, `coverage*.xml/json`, `*.user/suo`, `.vs/.idea/.dotnet/.vscode`, `Log/`, `test_image.png`.
Never commit user-specific `AllusionConfiguration.json` paths (e.g. `C:\Users\...\OneDrive\Dokument\Allusion`) or temp test dirs.
