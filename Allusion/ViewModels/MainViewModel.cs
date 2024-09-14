using Allusion.WPFCore.Board;
using Allusion.WPFCore.Handlers;
using Caliburn.Micro;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Media;
using Allusion.WPFCore;

namespace Allusion.ViewModels;

public class MainViewModel : Screen, IHandle<NewImageDrops>
{
    //TODO: Booleans on states -> enums

    public bool Saving;
    public bool Loading;

    public BindableCollection<ImageViewModel> Images { get; set; } = [];

    private ArtBoard _artBoard;
    private string _text;
    public ArtBoardHandler BoardHandler { get; }
    private IEventAggregator _events { get;}

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
        _events= new EventAggregator();
        _events.SubscribeOnBackgroundThread(this);
        BoardHandler = new ArtBoardHandler(_events);
        //Move to project-handler, testing json seriazible for now
 
        OpenArtBoard();
    }

    public void NewArtBoard()
    {
        //show new artboard dialog (new name and ok button)

        BoardHandler.CreateNewArtBoard();

    }

    public void OpenArtBoard()
    {
        //open viewmodel on current available artboards
        //_artBoardHandler.OpenArtBoard();

        BoardHandler.CreateNewArtBoard();

        InitializeProject();
    }

    public async Task SaveArtBoard()
    {
        var imageItems = Images.Select(i => i.Item).ToArray();

        await BoardHandler.SaveImageOnArtBoard(imageItems);
    }

    private void InitializeProject()
    {
        Images.Clear();

        foreach (var imageItem in BoardHandler.CurrentArtBoard.Images)
            Images.Add(new ImageViewModel(imageItem)
            {
                PosX = imageItem.PosX,
                PosY = imageItem.PosY
            });
    }

    public async Task PasteOnCanvas()
    {
        var items = await BoardHandler.GetPastedImageItems(0);

        if (items == null) return;

        foreach (var item in items)
            Images.Add(new ImageViewModel(item));
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



    public Task HandleAsync(NewImageDrops message, CancellationToken cancellationToken)
    {
        foreach (var item in message.DroppedItems)
            Images.Add(new ImageViewModel(item));

        return Task.CompletedTask;
    }
}