using Allusion.Core.Interfaces;

namespace Allusion.Core.Artboard;

public class NoteItem : IItem
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int MemberOfPage { get; set; }
}