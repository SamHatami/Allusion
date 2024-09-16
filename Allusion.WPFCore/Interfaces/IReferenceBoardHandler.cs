using System.Net.Mime;
using Allusion.WPFCore.Board;
using System.Windows;

namespace Allusion.WPFCore.Interfaces;

public interface IReferenceBoardHandler
{
    public ReferenceBoard? CurrentRefBoard { get; }
    public AllusionConfiguration CurrentConfiguration { get; }

    public void OpenRefBoard(string fullPath = "");

    public void CreateNewRefBoard(string name = "UntitledRefBoard");

    public Task<ImageItem[]> GetPastedImageItems(int pageNr);

    public Task<ImageItem[]> GetDroppedImageItems(IDataObject dataobject);

    public Task<bool> SaveRefBoard(ImageItem[] imageItems);

    public void AddImage(ImageItem item, BoardPage page);

    public void RemoveImage(ImageItem item);

    public RefBoardInfo[] GetAllRefBoardInfos();
}