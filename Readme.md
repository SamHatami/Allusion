# ALLUSION - A Reference image helper for artists

## Adding images
Add content to current artboard from a browser or file explorer:

- Copy-Paste image
- Drag and Drop image
- Adding by folder?

## Artboard 
- Contains all the images (and future scribble/notes)
- Contains "section" or pages to since no pan or zoom function exist (perhaps being able to quickly switch between section ?)
- Saves all the images to the artboard folder and keeps simple data of the image:
  - Source (url)
  - Notes
  - ?

## Images
- Can be rescaled and moved inside the artboard (aspect-ratio lock)
- Are saved to png no matter the source type (might change)
- Transparency isn't supported yet, not sure if it's even useful.
- Changing name of the image (?)

## Configuration (more or less a wish list)
- Hopefully theme
- Some hotkeys
- Perhaps default artboard folder


# Dev Notes
ClipboardService
- Handles all actions from the clipboard

BitmapService


- Copy/paste from web - the clipboard handles this as an image.
- Drag and drop from web - the clipboard handles this a html data.

- Copy/Paste & Drag and Drop from file explorer - the clipboard handles this as FileDrop which are filepaths.


Copy paste are events in the window that are bound to the mainviewmodel via caliburn gesture action
drops are consumed by the CanvasBehavior are a DragEvent (System.Windows.DragEventArgs)

Each ImageItem are added to a single ImageViewModel, the view is a custom user control with a new dependency property called Selected which will be used for calls where the specific ImageViewModel will be involved in other events such as 
- Delete
- Move to other page
- Add note / Remove note