# Allusion Startup / Lifetime Sequence

## Order of initialization

```text
1.  App.App() constructor
    └── VelopackApp.Build().Run()  (install/update/restart hooks)

2.  Bootstrapper.Bootstrapper()
    └── Initialize()
        ├── SelectAssemblies()  → [Allusion.exe]
        ├── Configure()
        │    ├── CreateKeyMagic()
        │    ├── ConfigMessageBinder()
        │    ├── AllusionConfiguration.Read()  (load %LocalAppData%/Allusion/AllusionConfiguration.json)
        │    │    └── CreateNew() only if file missing (FirstStartUp=true)
        │    ├── SimpleContainer setup (singletons + per-request registrations)
        │    │    ├── Singleton: WindowManager, EventAggregator
        │    │    ├── Singleton: ClipboardService, ImageItemService (via PageManager)
        │    │    ├── Singleton: ReferenceBoardManager, UpdateService, ThemeService, UpdateInstaller
        │    │    ├── PerRequest: BitmapService
        │    │    ├── Singleton: MainViewModel (lazy-resolved, but container holds it)
        │    │    └── RegisterInstance("Config", config)  ← file-backed config instance
        │    └── Singleton: MainViewModel (container creates / resolves)
        └── OnStartup(StartupEventArgs)
             └── DisplayRootViewForAsync<MainViewModel>()
                  └── MainViewModel constructor injected with the same config instance

3.  MainViewModel.OnViewLoaded()
    ├── FirstTime()  → WelcomeView if config.FirstStartUp == true; sets false + Save()
    ├── ThemeService.ApplyTheme(CurrentTheme)
    ├── CheckForUpdatesOnStartupAsync()  (background, catches all)
    └── ShowStartBoardPickerAsync()  (if no board open)

4.  ImageItemService  (created inside PageManager, not in container)
    └── Subscribes to DragDropEvent + PasteOnCanvasEvent  (once, background thread)
```

## Key points

- `Bootstrapper` creates `SimpleContainer` in `Configure()`.
- `AllusionConfiguration` is read from disk once (`AllusionConfiguration.Read()`), registered by key `"Config"`, then injected into everything that asks (`MainViewModel`, `ThemeService`, `ReferenceBoardManager`).
- `MainViewModel` subscribes to `EventAggregator` in constructor (`SubscribeOnUIThread` only — no double-subscribe after fix).
- `FirstTime()` writes `FirstStartUp = false` back to the same file instance; no new file created unless it was missing.
- `ImageItemService` lives only inside `PageManager` (not container singleton) to prevent double events.
- `UpdateService` uses `Assembly.GetEntryAssembly()` (not `GetExecutingAssembly()`) so `.csproj` `<Version>` is the version compared against GitHub tags.
- `ParseStableVersion` handles both `vX.Y.Z` and `v.X.Y.Z` tag formats.
