# Architecture

Allusion is a Caliburn.Micro MVVM WPF app for artists to manage reference images.

## Solution map

`Allusion.sln` contains 3 projects (AnyCPU Debug|Release):

- `Allusion/` – `WinExe net8.0-windows`, `UseWPF`. Shell, Views/ViewModels, Behaviors, `Bootstrapper.cs`, `Input/`, `Controls/ImageControl.cs`.
- `Allusion.WPFCore/` – `net8.0-windows`, `UseWPF`. Active shared code: `Board/`, `Service/`, `Managers/`, `Events/`, `Interfaces/`, `Utilities/`, `AllusionConfiguration.cs`.
- `Allusion.Tests/` – `net8.0-windows` test project referencing `WPFCore` + `Allusion`.
- `Allusion.Core/` – **legacy, not in sln**. Do not use. See `legacy-core.md`.

## MVVM with Caliburn.Micro

- `Bootstrapper : BootstrapperBase` in `Allusion/Bootstrapper.cs:17` uses `SimpleContainer`.
- Bases: `Conductor<object>` (shell), `Screen`, `PropertyChangedBase`, `BindableCollection<T>`.
- View resolution by naming convention: `XxxViewModel` ↔ `XxxView`. `x:Name` in XAML binds to property/action.
- Dialogs via `IWindowManager.ShowDialogAsync / ShowWindowAsync`.
- Actions via `cal:Message.Attach`, e.g. `[Gesture(Ctrl+Shift+N)] = [Action CreateNewBoard]`.

Key ViewModels in `Allusion/ViewModels/`:

| ViewModel | Base | Role |
|---|---|---|
| `MainViewModel` | `Conductor<object>` | Shell, owns `RefBoardViewModel`, save/new/open, paste, `BoardIsModified` flag |
| `ReferenceBoardViewModel` | `PropertyChangedBase, IHandle<PageSelectedEvent>` | Owns `Pages`, `ActivePageViewModel` |
| `PageViewModel` | `Screen, IPageViewModel, IRemovableItem, IItemOwner, IHandle<NewImageItemsEvent, DropOnTabEvent, PageSelectedEvent, SelectionEvent>` | Canvas owner, `Images`, `SelectedImages`, `Viewport`, arrange/align/z-order |
| `ImageViewModel` | `PropertyChangedBase, IRemovableItem, IImageViewModel` | Wraps `ImageItem`, `PosX/Y, Scale, ZIndex, Description, IsSelected` |
| `Focus/Welcome/Notification/Help/CanvasViewport` | `Screen/PropertyChangedBase` | Popup, first-run, toast, help, pan/zoom |
| `Dialogs/*, Arrangement/*` | various | Modals + pure layout services (`ArrangeImageLayoutService`, `CanvasGridSnap`, `ImageAlignmentService`, `ImageZOrderService`) |

## Dependency injection

`Bootstrapper.Configure()` (`Allusion/Bootstrapper.cs:26`):

```csharp
_container.Singleton<IWindowManager, WindowManager>();
_container.Singleton<IEventAggregator, EventAggregator>();
_container.RegisterInstance(typeof(HttpClient), "HttpClient", new HttpClient { Timeout = TimeSpan.FromSeconds(30) });
_container.PerRequest<IBitmapService, BitmapService>();
_container.Singleton<IClipboardService, ClipboardService>();
_container.Singleton<IReferenceBoardManager, ReferenceBoardManager>();
_container.Singleton<ImageItemService>();
_container.Singleton<IPageManager, PageManager>();
_container.RegisterInstance(typeof(AllusionConfiguration),"Config",config);
_container.Singleton<HelpViewModel>();
_container.Singleton<MainViewModel>();
_container.PerRequest<OpenRefBoardViewModel>();
_container.PerRequest<NewRefBoardViewModel>();
_container.PerRequest<ArrangeImagesViewModel>();
_container.PerRequest<DialogViewModel>();
```

Rules for agents:

- Prefer constructor injection for ViewModels/services.
- Do NOT add new `IoC.Get<>` service-locator calls. Existing Behaviors use it only because `Behavior<T>` has no ctor DI (`CanvasBehavior`, `ImageBehavior`, etc.) – do not copy to ViewModels.
- Do NOT `new PageViewModel / ImageViewModel / ReferenceBoardViewModel / FocusViewModel` manually in new code – bypasses container. Follow existing pattern only when fixing in place.
- `HttpClient` is a registered singleton with 30s timeout – inject it, don't `new HttpClient()`.

## Event-driven communication

`IEventAggregator` is the primary decoupling mechanism. See `events-behaviors.md` for full table.

- UI/selection/drag: `PublishOnUIThreadAsync`.
- State/persistence: `PublishOnBackgroundThreadAsync`.
- Example: `await _events.PublishOnUIThreadAsync(new SelectionEvent(imageItem, SelectionType.Select));`

## File organization

- Views in `Allusion/Views/` (+ `Dialogs/`), code-behind minimal except `PageView.xaml.cs` (viewport), `ImageView.xaml.cs` (`IoC.Get<IEventAggregator>`), `ReferenceBoardView.xaml.cs` (rename box).
- ViewModels in `Allusion/ViewModels/` (+ `Dialogs/`, `Arrangement/`).
- Services in `Allusion.WPFCore/Service/`, managers in `Allusion.WPFCore/Managers/`, events in `Allusion.WPFCore/Events/` + `Allusion/Events/`, converters in `Allusion.WPFCore/Converter/`, controls in `Controls/`.
