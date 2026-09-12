# Services & Managers

All interfaces live in `Allusion.WPFCore/Interfaces/`. Inject via Caliburn `SimpleContainer` – see `architecture.md`.

## `IClipboardService` – `Allusion.WPFCore/Service/ClipboardService.cs`

Strategy pattern over clipboard formats. `IClipboardDataStrategy` + 3 strategies in `Service/Strategies/`:

| Strategy | Handles |
|---|---|
| `TextUrlStrategy` | URLs from text/HTML clipboard data |
| `ImageDataStrategy` | Direct bitmap clipboard data |
| `FileDropStrategy` | File-drop paths |

Key APIs (note `CancellationToken`):

- `GetPastedBitmaps(CancellationToken)`
- `GetDroppedOnCanvasBitmaps(IDataObject, CancellationToken)` – `IBitmapService.DownloadAndConvert` added `CancellationToken` param.

Copy/paste from browser arrives as image; drag-drop from browser arrives as HTML; file-explorer copy/drop arrives as `FileDrop`.

## `IBitmapService` – `Allusion.WPFCore/Service/BitmapService.cs`

- Registered `PerRequest`, `HttpClient` injected (singleton, 30s timeout in `Bootstrapper.cs:35`).
- `GetFromUri / LoadFromUri / DownloadAndConvert(url, CancellationToken)`.
- Handles `HttpRequestException`, `OperationCanceledException`, general exceptions with `StaticLogger`.
- Supports `data:image` base64 URIs.
- `BitmapUtils` (`Allusion.WPFCore/Utilities/BitmapUtils.cs`): PNG save/load, `LoadImageFromUri`, `CreateFromBytes` (null-safe). Images saved as PNG regardless of source.

Do not `new HttpClient()` – inject `IBitmapService`.

## `ImageItemService` – `Allusion.WPFCore/Service/ImageItemService.cs`

- `IHandle<DragDropEvent>, IHandle<PasteOnCanvasEvent>`.
- Creates `ImageItem[]` from drop/paste, publishes `NewImageItemsEvent` via `PublishOnUIThreadAsync` (`ImageItemService.cs:47`).
- `SupportedImageFormats.cs`, `DataObjectImageExtractor.cs`, `StatusMessageService.cs` are helpers here.

## Managers – `Allusion.WPFCore/Managers/`

| Manager | Role |
|---|---|
| `ReferenceBoardManager : IReferenceBoardManager` | Board CRUD + JSON persistence to disk |
| `PageManager : IPageManager` | Add/remove/rename/clean pages within a board |
| `SessionManager : IHandle<ItemRemovedEvent, UndoItemRemovedEvent>` | Undo bin – **currently orphaned: never registered/subscribed, undo is dead**. See `tech-debt-roadmap.md` |

`ImageItem` ctor takes `IBitmapService` + `ImageItemService` via DI. `ReferenceBoardManager` and `PageManager` also take `IBitmapService`.
