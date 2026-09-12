# Settings & Themes

## Settings (`SettingsView` / `SettingsViewModel`)

Title-bar gear button (`MainView.xaml`, `OpenSettings`) opens the Settings dialog
(real modal `Window` with `DialogWindow` style – like every other dialog).
All changes apply and persist immediately via `AllusionConfiguration.Save` – no Apply button.

| Setting | Control | Notes |
|---|---|---|
| Theme | ComboBox of `IThemeService.AvailableThemes` | Applies live through `ThemeService` |
| Always on top | CheckBox | Same flag as the title-bar toggle |

(Board folder lives only in the Open dialog. Quick buttons stay in the title bar:
theme cycle (`ToggleTheme`), always-on-top (`SetTopMost`), update indicator
(`OpenUpdates`), Help, Settings gear.)

## Theming – centralized resources

## Theming – centralized resources

Each preset is ONE self-contained dictionary: `Allusion/Themes/<name>.xaml` holds
both the `Color` keys and every `SolidColorBrush` with literal colors, so each theme
fully describes the palette. All views and control styles (`Themes/Controls/*.xaml`)
bind through `DynamicResource` to those brush keys – no hardcoded colors in views,
no `StaticResource` for anything theme-driven.

Why this shape: brushes that resolve their color via a nested `DynamicResource`
do NOT re-evaluate when the theme dictionary is swapped (verified: brush kept the
old color while direct key lookup returned the new one). Brushes must live IN the
swapped dictionary so the whole brush object is replaced. `SolidColorBrushes.xaml`
was deleted in the consolidation; `App.xaml` merges only theme + Globals + Controls.

- Presets: `Dark`, `Light`, `Midnight` (blue dark), `Sand` (warm light).
- Every preset MUST define identical `Color` and brush key sets – enforced by
  `ThemeResourcesTests` (color + brush parity). Missing keys crash `DynamicResource`
  on .NET 10 instead of failing silently.
- `IThemeService` / `ThemeService` swaps the single theme dictionary (tracked by
  reference – loaded dictionaries have null `Source`, so never look them up by URI)
  via `Application.LoadComponent` with a pack URI (relative URIs don't resolve in code).
- Selection persisted as `AllusionConfiguration.Theme` (defaults to `Dark` for old configs).
  Applied on startup in `MainViewModel.OnViewLoaded` (no-op when already Dark).
- Title-bar button cycles all presets in order. Full choice lives in Settings.
- Future custom colors: add a user-override dictionary layered after the preset
  (e.g. `%LocalAppData%/Allusion/ThemeOverrides.xaml` loaded last); `ThemeService`
  is the single place to implement it.

## Dialogs – all real windows, one border

Every dialog is a modal `Window` with the shared `DialogWindow` style
(`BorderBrush = Dialog.Border`, thickness 2): Settings, Help, Arrange, Open/New/Remove
board pickers, generic `DialogView`, Focus, Welcome. The Open picker used to be
embedded inline in `MainView` – it is now a modal `OpenRefBoardView` window
(`MainViewModel.ShowStartBoardPickerAsync`), with the main window keeping only a
logo watermark as its empty state. Inner panels use the `InfoBox` border style,
which resolves to the same `Dialog.Border` brush.

## .NET version

Solution targets `net10.0-windows` (.NET 10 LTS). CI uses `setup-dotnet 10.0.x`.
`Allusion.Core/` (legacy, not in sln) still says `net8.0` – leave it until removal.
Note: .NET 10 XAML rejects empty `<Grid.ColumnDefinitions />` (fixed in `PageView.xaml`).
