using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Board;

public class NoteItem : IItem
{
    public string Title { get; set; }
    public string Content { get; set; }
    public Guid MemberOfPage { get; set; }
}