using System.Net.Mime;
using Allusion.WPFCore.Board;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Allusion.WPFCore.Interfaces;

public interface IReferenceBoardManager
{
    public AllusionConfiguration CurrentConfiguration { get; } //as IAllusionConfiguration

    public ReferenceBoard Open(string fullPath = "");

    public ReferenceBoard CreateNew(string name = "UntitledRefBoard");

    public void RenameBoard(ReferenceBoard board, string newName);

    public Task<bool> Save(ReferenceBoard board);

    public BoardPage AddPage(ReferenceBoard board, string pageName);

    public void DeletePage(ReferenceBoard board, BoardPage page);

    public RefBoardInfo[] GetAllRefBoardInfos();
}