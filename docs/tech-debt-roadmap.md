# Tech Debt & Roadmap

Curated from code TODOs and inspection. Check here before filing a new issue.

## Must-fix (correctness)

- [ ] `BoardIsModfiedEvent` / `IsModfied` typo propagated everywhere (`BoardIsModfiedEvent.cs:3`, `MainViewModel`, `PageViewModel`, `ImageViewModel`, `ReferenceBoardViewModel`). Rename to `BoardIsModifiedEvent` in one breaking pass.
- [ ] `SessionManager` (`Managers/SessionManager.cs:8`) orphaned – never registered in `Bootstrapper`, never subscribed. `MainViewModel.UndoRemove()` empty, `UndoItemRemovedEvent` has no publisher. Decide: revive undo or delete.
- [ ] `PageViewModel` dual `SubscribeOnBackgroundThread + SubscribeOnUIThread` + manual `Dispatcher.Invoke` for `NewImageItems` – risk double-handle. Pick one thread.
- [ ] `MainViewModel` dual subscribe, duplicated save-prompt in `NewRefBoardDialog/OpenRefBoardDialog`, `bool BoardIsModified → enum BoardState` (`MainViewModel:16,18`).
- [ ] `ImageViewModel:143` – `PageMember:int` TODO `MoveToPageEvent`; per-setter `Publish(BoardIsModfied)` spam; `Scale` setter touches `ImageSource` (NRE risk).
- [ ] `ImageBehavior:75` race – EventAggregator too slow before `CaptureMouse`, TODO wants `SelectionService`.
- [ ] `ReferenceBoardManager:39,285`, `PageManager:57`, `OpenRefBoardViewModel:95` TODOs (NLOG, silent null).

## Clean-up

- [ ] Replace `IoC.Get<>` + `new PageViewModel/ImageViewModel/ReferenceBoardViewModel/FocusViewModel` with DI/`IWindowManager` injection. Files: `PageViewModel`, `ReferenceBoardViewModel`, `ImageView.xaml.cs`, all `Behaviors/` (Behaviors excepted – no ctor DI).
- [ ] `Bootstrapper.CreateKeyMagic` only uses first key sequence – fix or document single-chord limit.
- [ ] `PageViewModel`: dead commented `SelectedImage/ImageSelectionEvent` code, `new Random()` per drop, `Intersect/RemoveRange` casts, unused `System.Drawing/Forms` usings, `FitToView()` stub, `_arrangeService = new()` + `IoC.Get<IWindowManager>`.
- [ ] Duplicate `Controls/ImageControl.cs` (app + WPFCore) – converge on one.
- [ ] Typos: `Iitem.cs` filename lowercase, `_windowManger` in `PageViewModel`, `SelectionBehavior.cs:19` double `;;`, `ReferenceBoardViewModel` asserts `"Holup"`, Swedish TODOs (`Byt till`, `Ersätt`, `Gör behavior`), “Not sure” comments in `PageView.xaml.cs`/`ReferenceBoardView.xaml.cs`/`Buttons.xaml`.
- [ ] `AllusionConfiguration.GlobalFolder` getter does I/O (`Directory.CreateDirectory`) – move to explicit init.

## Roadmap ideas (from `Readme.md` + code)

- Add-by-folder import, image rename, notes/scribble on artboard, pan/zoom polish (`FitToView`), theming, hotkey settings, default artboard folder UI.
- PNG-only rule re-evaluation, transparency support decision.

## Conventions for new code

- Events: `XxxEvent`, handler `HandleAsync(msg, CancellationToken)`.
- Behaviors: `XxxBehavior : Behavior<T>`; converters `XxxConverter`.
- Tests: `[Fact]/[Theory]`, FakeItEasy + FluentAssertions, temp GUID dirs, no real Clipboard.
