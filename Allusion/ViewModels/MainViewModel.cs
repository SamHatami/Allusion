using Caliburn.Micro;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Artboard;
using Allusion.WPFCore.Handlers;
using Allusion.WPFCore.Helpers;

namespace Allusion.ViewModels;

public class MainViewModel : Screen
{
    public BindableCollection<ImageViewModel> Images { get; set; } = [];

    private ArtBoard _artBoard;
    private string _text;
    private ArtBoardHandler _artBoardHandler;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            NotifyOfPropertyChange(nameof(Text));
        }
    }

    public MainViewModel()
    {
        _artBoardHandler = new ArtBoardHandler();
        //Move to project-handler, testing json seriazible for now
    }

    public void OpenArtBoard()
    {
        var path = @"C:\Temp\project1.json";
        _artBoard = ArtBoard.Read(Path.GetDirectoryName(path));
        if (_artBoard == null)
        {
            _artBoard = new ArtBoard("AllusionTestProject1");
            ArtBoard.Save(_artBoard, path);
        }

        InitializeProject();
    }

    public void SaveArtBoard()
    {
        var path = @"C:\Temp\project1.json";
        //var imagesFromCollection = Images.Select(i => new ImageItem(i.ImageSource., i.PosX, i.PosY, 1.0)).ToArray();
        //_artBoardHandler.SaveImageOnArtBoard(imagesFromCollection);
    }

    private void InitializeProject()
    {
        Images.Clear();
        foreach (var imageItem in _artBoard.Images)
        {
            var bitmap = BitmapHelper.GetImageFromUri(imageItem.ImageUri);
            Images.Add(new ImageViewModel(bitmap)
            {
                PosX = imageItem.PosX,
                PosY = imageItem.PosY
            });
        }
    }

    public void PasteOnCanvas(Point e)
    {
        var pastedImages = ArtBoardHandler.GetPastedBitmaps();

        if (pastedImages == null) return;

        foreach (var pasted in pastedImages)
            Images.Add(new ImageViewModel(pasted));
    }

    private void ExtractUrlFromDataObject()
    {
        var dataObject = Clipboard.GetDataObject();
        if (dataObject != null)
            foreach (var format in dataObject.GetFormats())
            {
                var data = dataObject.GetData(format);
                // Inspect data for potential URL or source information
                Debug.WriteLine($"Format: {format}, Data: {data}");
            }
    }

    public void Delete()
    {
        Text = "Pressed delete";
    }

    public void AddDropppedImages(ImageSource[] bitmaps)
    {
        var counter = bitmaps.Length;
        foreach (var bitmap in bitmaps)
        {
            Images.Add(new ImageViewModel(bitmap) { PosX = 10 * counter, PosY = 10 * counter });
            counter++;
        }
    }
}