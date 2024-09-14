using Allusion.WPFCore.Board;
using Allusion.WPFCore.Handlers;
using Caliburn.Micro;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Media;
using Allusion.Views;
using Allusion.WPFCore.Events;

namespace Allusion.ViewModels;

public class MainViewModel : Conductor<object>, IHandle<NewImageDropsEvent>, IHandle<NewBoardEvent>
{
    //TODO: Booleans on states -> enums

    public bool Saving;
    public bool Loading;

    public BindableCollection<ImageViewModel> Images { get; set; } = [];

    private ArtBoard _artBoard;
    private string _text;
    public ArtBoardHandler BoardHandler { get; }
    private IEventAggregator _events { get;}
    private IWindowManager _windowManager;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            NotifyOfPropertyChange(nameof(Text));
        }
    }

    public MainViewModel(IWindowManager windowManager, IEventAggregator events, ArtBoardHandler artBoardHandler)
    {
        _windowManager = windowManager;
        _events = events;
        _events.SubscribeOnBackgroundThread(this);
        BoardHandler =  artBoardHandler;
        //OpenArtBoard();
    }

    public void PackToView()
    {
        //Perhaps use https://github.com/ThomasMiz/RectpackSharp
    }

    public void NewArtBoard()
    {
        var option = _windowManager.ShowDialogAsync(IoC.Get<NewBoardViewModel>());

        //Handled with Event, see IHandle.
    }

    public Task OpenArtBoard()
    {

        var option = _windowManager.ShowDialogAsync(IoC.Get<OpenArtBoardViewModel>());

        InitializeArtBoard();

        return Task.CompletedTask;
    }

    public async Task SaveArtBoard()
    {
        var imageItems = Images.Select(i => i.Item).ToArray();

        await BoardHandler.SaveImageOnArtBoard(imageItems);
    }

    private void InitializeArtBoard()
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



    public Task HandleAsync(NewImageDropsEvent message, CancellationToken cancellationToken)
    {
        foreach (var item in message.DroppedItems)
            Images.Add(new ImageViewModel(item));

        return Task.CompletedTask;
    }

    public Task HandleAsync(NewBoardEvent message, CancellationToken cancellationToken)
    {
        
        BoardHandler.CreateNewArtBoard(message.Name);

        return Task.CompletedTask;
    }
}