# Arrange Changes

Date: 2026-09-12. Goal: make Arrange predictable and reversible without code comments.

## What was wrong

- Spacing used `SnapUp` on every step, so `Margin=10` produced 25px gaps (`100+10 → 125`). Margin did not mean what was typed.
- Column count fixed to `ceil(sqrt(N))`, no user control.
- Input order was collection order, not visual order – scattered images jumped unpredictably.
- No note height – rows with descriptions overlapped the next row.
- Zero/negative sizes and negative margins unguarded.
- Dialog reset `Margin=24` every open, no columns control, raw textbox only.
- Quick actions (`KeepCurrent/Average/Smallest`) ignored dialog margin.
- No undo – one bad arrange destroyed manual layout.

## What changed

### `Allusion/ViewModels/Arrangement/ArrangeImageLayoutService.cs`

- `ArrangeImageLayoutItem` gains `ExtraHeight = 0` for note area.
- `ArrangeImageLayoutOptions` is now a `record` and gains `Columns = 0` (`0 = Auto` → `ceil(sqrt(N))`).
- Layout cursor is exact (`xCursor += width + margin`, `yCursor += rowHeight + margin`).
- Final positions: `X = Snap(xCursor)`, `Y = SnapUp(yCursor)`. Horizontal gaps stay close to typed margin (±12.5 grid rounding); vertical rows use `SnapUp` to guarantee no row overlap.
- `rowHeight = max(Height + ExtraHeight)` so notes reserve space.
- `GetArrangedSize` clamps negatives, returns safe item for `height <= 0` instead of dividing.

### `Allusion/ViewModels/PageViewModel.cs`

- Images sorted `OrderBy(PosY).ThenBy(PosX)` before arrange – reading order, deterministic.
- `ArrangeImages` pushes `ImageArrangeSnapshot(image, x, y, scale)` onto `_arrangeUndo` stack, exposes `CanUndoArrange` + `UndoArrange()` restoring `Scale/PosX/PosY` and marking board dirty.
- Layout items pass `ExtraHeight = Description empty ? 0 : DescriptorHeight (30)`, widths/heights clamped `>= 0`.
- Origin still `Snap(min Pos)` so cluster stays where it was.
- `_lastArrangeOptions` remembered; quick actions reuse its `Margin/Columns` and only override `ScaleMode`.

### `Allusion/ViewModels/Dialogs/ArrangeImagesViewModel.cs`

- Static memory `_lastMargin/_lastScaleMode/_lastColumns` pre-fills each open; `CreateOptions()` saves back.
- New `ColumnOptions = [0..6]`, `SelectedColumns` clamped `0..6`.

### `Allusion/Views/Dialogs/ArrangeImagesView.xaml`

- Height `220 → 330`.
- New `Columns (0 = Auto)` combo bound to `SelectedColumns`.
- Margin gets `Slider 0..100` two-way bound plus existing textbox for precise entry.

### `Allusion.Tests/ArrangeImageLayoutServiceTests.cs`

- Updated grid expectation `125 → 100` for second item (exact cursor + `Snap`).
- Added: explicit columns, extra-height row clearance, empty/zero-size handling, negative-margin clamp.

## Behavior now

- Typed margin is honored within grid rounding; rows never overlap vertically.
- Same selection arranged twice gives same result (sorted input).
- Notes no longer overlapped.
- `UndoArrange` reverts last arrange; quick actions respect last dialog margin/columns.
- Settings persist across dialog opens within the session.

## Verification

- `dotnet build Allusion.sln -c Release` – 0 errors.
- `dotnet test Allusion.Tests/Allusion.Tests.csproj -c Release --no-build` – 96 passed (92 existing + 4 new).

## Known limits / next steps

- Horizontal `Snap` can still touch by a few px when rounding opposes; vertical is safe via `SnapUp`.
- No auto `ZoomToExtent` after arrange – viewport size plumbing (`_windowSize` unused) needs wiring first.
- Undo is per-page in-memory stack, not persisted; full undo revival still needs `SessionManager` work (see `tech-debt-roadmap.md`).
- Settings memory is session-only; persist to `AllusionConfiguration` if wanted.
