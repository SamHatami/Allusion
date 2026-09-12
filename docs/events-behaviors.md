# Events & Behaviors & Input

> Full reference (Option B). EventAggregator is the primary decoupling mechanism.

## Threading rule

- Selection / drag / new images → `PublishOnUIThreadAsync`.
- State / persistence / dialogs → `PublishOnBackgroundThreadAsync`.
- `PageViewModel.Handle(NewImageItems)` does manual `Dispatcher.Invoke` – keep UI-thread discipline when touching `BindableCollection`.

Dual subscription anti-pattern: `MainViewModel` and `PageViewModel` subscribe both `SubscribeOnBackgroundThread` + `SubscribeOnUIThread` – risk of double-handle. Don't copy.

## Event catalog

### `Allusion.WPFCore/Events/`

| Event | Payload | Publishers | Subscribers | Thread |
|---|---|---|---|---|
| `NewRefBoardEvent` (`NewRefBoardEvent.cs:9`) | `name: string` | `OpenRefBoardViewModel:116` (`NewRefBoardEvent(dialog.ResultName)`) | `MainViewModel : IHandle<NewRefBoardEvent>` | background |
| `BoardIsModfiedEvent` [sic – typo, see tech-debt] (`BoardIsModfiedEvent.cs:3`) | `bool IsModfied` | `ReferenceBoardViewModel:69,83`, `ImageViewModel:78,104,119,134,149`, `PageViewModel:187,282,316,347,380` | `MainViewModel : IHandle<BoardIsModfiedEvent>` → sets dirty flag / title | background |
| `BoardOpenedEvent` (`OpenRefBoardViewModel.cs:230`) | `ReferenceBoard board` | `MainViewModel:94`, `OpenRefBoardViewModel:98` | `MainViewModel : IHandle<BoardOpenedEvent>` | background |
| `OpenRefBoardEvent` (`OpenRefBoardEvent.cs:5`) | – (signal) | dialogs | `MainViewModel` flow | background |
| `NewImageItemsEvent` (`NewImageItemsEvent.cs:5`) | `ImageItem[]` | `ImageItemService:47` | `PageViewModel : IHandle<NewImageItemsEvent>` → `AddItems` | **UI** |
| `PasteOnCanvasEvent` (`PasteOnCanvasEvent.cs:5`) | `Size windowSize` | `MainViewModel:181` (`PasteOnCanvasEvent(_windowSize)`) | `ImageItemService : IHandle<PasteOnCanvasEvent>` | background |
| `DragDropEvent` (`DragDropEvent.cs:6`) | `IDataObject, Point dropPoint, Size windowSize` | `CanvasBehavior:56` | `ImageItemService : IHandle<DragDropEvent>` | **UI** |
| `DropOnTabEvent` (`DropOnTabEvent.cs:10`) | `images, targetPage` | `TabButtonBehavior:72` | `PageViewModel : IHandle<DropOnTabEvent>` | background (note: inconsistent – should be UI) |
| `PageSelectedEvent` (`PageSelectedEvent.cs:5`) | `IPageViewModel page` | `PageViewModel:452` (`PageSelectedEvent(this)`) | `ReferenceBoardViewModel : IHandle<PageSelectedEvent>`, `PageViewModel` | background |
| `ItemRemovedEvent` (`SessionEvents.cs:9`) | `IRemovableItem Item, IItemOwner Owner` | item removal paths | `SessionManager : IHandle<ItemRemovedEvent>` (orphaned) | – |
| `UndoItemRemovedEvent` (`SessionEvents.cs:21`) | `IItemOwner Owner` | (no active publisher – `MainViewModel.UndoRemove()` empty) | `SessionManager` (orphaned) | – |

### `Allusion/Events/`

| Event | Payload | Publishers | Subscribers |
|---|---|---|---|
| `SelectionEvent` (`SelectionEvent.cs:5`) | `ImageViewModel[]? images, SelectionType {Single, Multi, DeSelect}` | `CanvasBehavior:31` (DeSelect), `CanvasSelectionBoxBehavior:79,96`, `SelectionBehavior:36`, `ImageViewModel:212` | `PageViewModel : IHandle<SelectionEvent>` |
| `SelectionBoxEvent` (`SelectionBoxEvent.cs:5`) | `ImageViewModel[] images` | selection-box flow | canvas/selection |
| `ClearSelectionEvent` (`ClearSelectionEvent.cs:3`) | – | clear paths | selection owners |
| `MultiMoveEvent` (`MultiMoveEvent.cs:9`) | move delta (internal) | multi-drag | page/canvas |

Typical flow:

```
CanvasBehavior/TabButtonBehavior/ImageViewModel
  -> DragDropEvent / DropOnTabEvent / SelectionEvent / PasteOnCanvasEvent
  -> ImageItemService -> NewImageItemsEvent -> PageViewModel.AddItems
  -> BoardIsModfiedEvent -> MainViewModel (dirty)
PageSelectedEvent -> ReferenceBoardViewModel.ActivePageViewModel
```

## Behaviors – `Allusion/Behaviors/` (all `Microsoft.Xaml.Behaviors.Behavior<T>`)

| Behavior | Target | Does |
|---|---|---|
| `CanvasBehavior.cs` | `UIElement` canvas | click-empty → `SelectionEvent(DeSelect)`; Drop → `DragDropEvent` |
| `CanvasSelectionBoxBehavior.cs` | selection rect | drag rect → `SelectionEvent(Multi/DeSelect)` |
| `ImageBehavior.cs` | image thumb | drag/resize; note `ImageBehavior:75` race – wants `SelectionService` because EventAggregator too slow before `CaptureMouse` |
| `SelectionBehavior.cs` | image | click → `SelectionEvent([vm])` (note `SelectionBehavior.cs:19` double `;;`) |
| `CanvasViewportBehavior.cs` | viewport | pan/zoom → drives `CanvasViewport` |
| `FocusBehavior.cs` | focus window | focus popup wiring |
| `RenameBehavior.cs` | rename box | inline board/page rename |
| `TabButtonBehavior.cs` | page tab | tab drop → `DropOnTabEvent` |

Pattern: `IoC.Get<IEventAggregator>()` in `OnAttached()`, publish in UI handlers. No ctor DI possible. ViewModels must NOT copy this pattern.

## Input – `Allusion/Input/`

- `MultiKeyGesture.cs`, `KeyTrigger.cs`, `KeySequence.cs`, `MultiKeyGestureConverter.cs`.
- XAML: `<Button cal:Message.Attach="[Gesture(Ctrl+Shift+N)] = [Action CreateNewBoard]" />`.
- `Bootstrapper.CreateKeyMagic` overrides `Parser.CreateTrigger` for `Key`/`Gesture`. Limitation: only `mkg.KeySequences[0]` is used – multi-chord gestures truncated.
- `$mousepoint` in `ConfigMessageBinder` only works for `MouseEventArgs`.
