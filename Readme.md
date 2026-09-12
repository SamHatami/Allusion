# ALLUSION - reference images for artists

Allusion is a small artboard app for keeping reference images around while you work.
It stays on top if you want it to. Made mostly for painters and concept artists. Works as a art board too!

Feedback is very welcome, send to mhatami at hotmail.com

## Adding images
Copy-paste or drag and drop, from a browser or from file explorer, straight onto a page.

- Copy-paste image (Ctrl+V, or right-click Paste on the canvas)
- Drag and drop image files or browser images
- Pasting from the web usually just works, sometimes it needs a second try

## Boards and pages
Everything lives in a board. A board has pages (tabs), each page is a canvas you can paste images in, pan and zoom however you like.
You can organize items in the pages, create a new page, rename it. Move items between pages by selecting and dragging to the correct tab header.

- New board with Ctrl+N, open with Ctrl+O (or the folder button in the title bar)
- Boards are saved as folders with pngs plus a small data file (source url, notes, positions)
- Pages are renamed by double clicking the tab, removed by right clicking it
- Drag an image above the page tabs to move it to another page

## Images
Images can be moved and rescaled on the canvas (aspect ratio stays locked).
Everything is saved as png no matter the source type. Transparency is not really
supported, not sure if it's even useful. Once you add an image to your board it will be copied over to your allusion folder. 

- Right-click an image for notes and sizes
- Notes are added/removed from the image menu, edited by single-clicking the note text
- Arrange a mess of images from the canvas menu: keep size, average height or
  smallest height, with settings for scope, columns and margin. Undo exists if arrange
  makes it worse
- Align (left/right/top/bottom/centers) and ordering (bring to front and friends)
  live in the canvas menu too
- Image border thickness is in Settings, and holding Shift while dragging snaps to grid
  (a grid overlay shows while you hold it)

## Focus view
Double click an image and it pops up in a small always-on-top window. Handy to keep
above your painting app while you work. (Still not fully functional)

## Themes and settings
The gear button opens Settings: theme, always on top, image border thickness.
The boards folder is picked in the Open dialog instead.

Themes right now: Dark, Light, Midnight, Sand. The half-moon button in the title bar
cycles through them. Custom colors per theme might come later.

Quick buttons in the title bar: open board, always on top, theme, settings, help.
They stay where they are.

## Updates
Releases come as a small per-user installer (Setup.exe, no admin needed) plus a
portable exe. Grab the latest here: https://github.com/SamHatami/Allusion/releases/latest
The app checks for stable releases quietly on startup, an arrow shows
up in the title bar when something is new, and Help -> Updates downloads and
restarts into it. Prereleases (the -alpha tags) are ignored.

## Help
F1 or the question mark button. Topics on the left, release notes on the right.

## Running the code
Windows only, .NET 10. Caliburn.Micro MVVM, xunit tests.

```
dotnet build Allusion.sln -c Release
dotnet test Allusion.Tests/Allusion.Tests.csproj -c Release
```

Config lives in %LocalAppData%/Allusion/AllusionConfiguration.json. 
