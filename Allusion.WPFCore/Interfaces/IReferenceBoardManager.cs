using System.Net.Mime;
using Allusion.WPFCore.Board;
using System.Windows;
using System.Windows.Media;

namespace Allusion.WPFCore.Interfaces;

public interface IReferenceBoardManager
{
    public ReferenceBoard? CurrentRefBoard { get; }
    public AllusionConfiguration CurrentConfiguration { get; } //as IAllusionConfiguration

    public ReferenceBoard Open(string fullPath = "");

    public ReferenceBoard CreateNew(string name = "UntitledRefBoard");

    public void Rename(ReferenceBoard board, string newName);

    public Task<ImageItem[]> GetPastedImageItems(int pageNr); //TODO move to ClipboardService

    public Task<ImageItem[]> GetDroppedImageItems(IDataObject dataobject); //TODO move to ClipboardService

    public Task<bool> Save(ReferenceBoard board);

    public BoardPage AddNewPage(ReferenceBoard board);
    public void RenamePage(BoardPage page, string newName);
    public void DeletePage(BoardPage page);

    public RefBoardInfo[] GetAllRefBoardInfos();
}