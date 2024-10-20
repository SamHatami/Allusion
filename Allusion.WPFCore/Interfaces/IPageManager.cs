﻿using Allusion.WPFCore.Board;

namespace Allusion.WPFCore.Interfaces;

public interface IPageManager
{
    public void CleanPage(BoardPage page);
    public void AddImage(ImageItem item, BoardPage page);

    public void RemoveImage(ImageItem item, BoardPage page);

    public void AddNoteToImage(ImageItem item, BoardPage page, NoteItem note);

    public void AddNote(BoardPage page, NoteItem note);

    public void RenamePage(BoardPage page, string newName);

    public void OpenPageFolder(BoardPage page);

}