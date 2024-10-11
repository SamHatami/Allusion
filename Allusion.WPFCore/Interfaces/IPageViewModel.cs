using Allusion.WPFCore.Board;

namespace Allusion.WPFCore.Interfaces;

public interface IPageViewModel
{
    public bool PageIsSelected { get; set; }
    public string DisplayName { get; }

    public void TransferImageItems();

    public void PageSelected();

    void DeleteSelectedImages();
}