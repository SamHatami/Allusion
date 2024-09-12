using Allusion.WPFCore.Handlers;
using Allusion.WPFCore.Service;
using Caliburn.Micro;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Allusion.WPFCore.Board;

namespace Allusion.ViewModels;

public class MainViewModel : Screen
{
    //TODO: Booleans on states -> enums

    public bool Saving;
    public bool Loading;

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
        //open viewmodel on current available artboards
        //_artBoardHandler.OpenArtBoard();
        var path = @"C:\Temp\project1.json";

        //Only for testing
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
        var imageItems = Images.Select(i => i.Item).ToArray();

        Task.Run(() => _artBoardHandler.SaveImageOnArtBoard(imageItems));
    }

    private void InitializeProject()
    {
        Images.Clear();

        if (_artBoard.Images is null) return;

        foreach (var imageItem in _artBoard.Images)
        {
            Images.Add(new ImageViewModel(imageItem)
            {
                PosX = imageItem.PosX,
                PosY = imageItem.PosY
            });
        }
    }

    public void PasteOnCanvas()
    {
        var items = _artBoardHandler.GetPastedImageItems(0);

        var bitmap = Clipboard.GetImage();

        //if (items == null) return;

        //foreach (var item in items)
        //    Images.Add(new ImageViewModel(item));
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
        //var counter = bitmaps.Length;
        //foreach (var bitmap in bitmaps)
        //{
        //    Images.Add(new ImageViewModel(bitmap) { PosX = 10 * counter, PosY = 10 * counter });
        //    counter++;
        //}
    }
}