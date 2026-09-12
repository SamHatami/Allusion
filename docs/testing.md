# Testing

## Stack

- `xunit 2.9.2` + `xunit.runner.visualstudio 2.8.2` + `Microsoft.NET.Test.Sdk 17.11.1` + `coverlet.collector 6.0.2`.
- `FakeItEasy 8.3.0` for mocks: `A.Fake<IEventAggregator|IBitmapService|IDataObject|IClipboardService>()`.
- `FluentAssertions 6.12.1`: `.Should().Be/BeTrue/BeNull/BeEmpty/ContainSingle/Throw<IOException>/BeApproximately()`.

Test projects files in `Allusion.Tests/` (13 sources):

`AllusionConfigurationTests`, `ArrangeImageLayoutServiceTests`, `BitmapUtilsTests`, `BitmapServiceTests`, `CanvasViewportTests`, `ClipboardServiceTests`, `ClipboardStrategiesTests`, `DataObjectImageExtractorTests`, `ImageAlignmentServiceTests`, `ImageZOrderServiceTests`, `PageManagerTest` (class `PageManagerTests`), `ReferenceBoardTests`.

Note: `AGENTS.md` historically suggested `[WpfFact]/[WpfTheory]` – none exist in code. Only `[Fact]/[Theory]+[InlineData]` are used.

## Isolation pattern (required for config/board tests)

```csharp
[Collection("Allusion configuration")] // see AllusionConfigurationCollection.cs (DisableParallelization=true)
using (AllusionConfiguration.UseDataFolderForTests(Path.Combine(Path.GetTempPath(), "AllusionTests", Guid.NewGuid().ToString())))
{
    // real FS I/O in unique temp dir
}
```

- Always use `UseDataFolderForTests()` + unique GUID temp dir.
- Keep the `Collection` attribute – do not run config tests in parallel.
- Real FS I/O is expected (`Directory.CreateDirectory`, `File.WriteAllBytes(base64 1px png)`, `File.Exists`).
- Cancellation path: `new CancellationTokenSource().Cancel()` then assert `OperationCanceledException` / empty result.
- Fake `IDataObject`, never call real `Clipboard.GetDataObject()`.

## What to test

- Pure layout: `ArrangeImageLayoutService`, `CanvasGridSnap`, `ImageAlignmentService`, `ImageZOrderService`, `CanvasViewport` – fast, no UI thread.
- Services: `BitmapService.DownloadAndConvert(url, CancellationToken)` (`HttpRequestException`/`OperationCanceledException` paths, `data:image` base64), `ClipboardService` strategies, `BitmapUtils` PNG save/load.
- Managers: `PageManager` add/remove/rename, `ReferenceBoardManager` CRUD + persistence.

## Run

```powershell
dotnet test Allusion.Tests/Allusion.Tests.csproj -c Release
```
