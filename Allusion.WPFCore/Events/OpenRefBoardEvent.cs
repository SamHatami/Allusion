using Allusion.WPFCore.Board;

namespace Allusion.WPFCore.Events;

public class OpenRefBoardEvent
{
    public ReferenceBoard Board { get; }

    public OpenRefBoardEvent(ReferenceBoard board)
    {
        Board = board;
    }
}