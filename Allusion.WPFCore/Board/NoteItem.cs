using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Board;

public class NoteItem : IItem
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid MemberOfPage { get; set; }
}
