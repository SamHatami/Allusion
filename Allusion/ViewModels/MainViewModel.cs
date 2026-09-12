using Allusion.ViewModels.Dialogs;
using Allusion.WPFCore;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Allusion.WPFCore.Service;

namespace Allusion.ViewModels;

public class MainViewModel : Conductor<object>, IHandle<NewRefBoardEvent>,
    IHandle<BoardIsModfiedEvent>, IHandle<BoardOpenedEvent>, IHandle<SettingsChangedEvent>
{
    //TODO: Booleans on states -> enums

    private bool BoardIsModified; //TODO: Byt till BoardState

    
    private ReferenceBoard _currentRefBoard { get; set; }

    private NotificationViewModel _notification;
    public NotificationViewModel Notification
    {
        get => _notification;
        set
        {
            _notification = value;
            NotifyOfPropertyChange(nameof(Notification));
        }
    }


    private ReferenceBoardViewModel? _refBoardViewModel;
    public ReferenceBoardViewModel? RefBoardViewModel
    {
        get => _refBoardViewModel;
        set
        {
            _refBoardViewModel = value;
            NotifyOfPropertyChange(nameof(RefBoardViewModel));
        }
    }

    private readonly IEventAggregator _events;//
    private readonly IReferenceBoardManager _boardManager;
    private readonly IWindowManager _windowManager;
    private readonly IUpdateService _updateService;
    private readonly IUpdateInstaller _updateInstaller;
    private readonly IThemeService _themeService;
    private AllusionConfiguration _configuration;

    public AllusionConfiguration Configuration
    {
        get => _configuration;
        set
        {
            _configuration = value;
            NotifyOfPropertyChange(nameof(Configuration));
        }
    }
    private Size _windowSize;
    private readonly HelpViewModel _help;
    private OpenRefBoardViewModel? _pickerDialog;

    public MainViewModel(IWindowManager windowManager, IEventAggregator events,
        IReferenceBoardManager refBoardManager, AllusionConfiguration configuration, HelpViewModel help,
        IUpdateService updateService, IThemeService themeService, IUpdateInstaller updateInstaller)
    {
        _windowManager = windowManager;
        _configuration = configuration;
        _events = events;
        _events.SubscribeOnBackgroundThread(this);
        _events.SubscribeOnUIThread(this);
        _boardManager = refBoardManager;
        _updateService = updateService;
        _updateInstaller = updateInstaller;
        _themeService = themeService;
        _help = help;

        StaticLogger.LogEvent += OnLogEvent;
    }

    private void OnLogEvent(string message, StaticLogger.LogLevel loglevel)
    {
        Notification = new NotificationViewModel(message, loglevel);
    }

    public void StartUpByFile(string filePath)
    {
        var openedRefBoard = _boardManager.Open(filePath);
        if (openedRefBoard is null)
            return;

        _events.PublishOnBackgroundThreadAsync(new BoardOpenedEvent(openedRefBoard));
    }

    private async Task<DialogResultType> AskSaveDialog()
    {
        var dialog = new DialogViewModel("Save file ?",
            "The board has been modified since last save. Do you want to save before continuing?",
            DialogType.Choice);

        await _windowManager.ShowDialogAsync(dialog);

        return dialog.DialogResult;
    }

    public async Task NewRefBoardDialog()
    {
        if (RefBoardViewModel is null)
        {
            await ShowStartBoardPickerAsync();
            return;
        }

        if (BoardIsModified)
        {
            var dialogResult = await AskSaveDialog().ConfigureAwait(true);

            switch (dialogResult)
            {
                case DialogResultType.Yes:
                    await _refBoardViewModel.Save();
                    await ShowStartBoardPickerAsync();
                    break;

                case DialogResultType.No:
                    await ShowStartBoardPickerAsync();
                    break;
            }
        }
        else
        {
            await ShowStartBoardPickerAsync();
        }
    }

    public void SetTopMost()
    {
        Configuration.TopMost = !Configuration.TopMost;
        NotifyOfPropertyChange(nameof(Configuration));
        AllusionConfiguration.Save(_configuration);
    }

    public IThemeService Theme => _themeService;

    public string AppVersion
    {
        get
        {
            var informational = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var version = informational?.Split('+')[0]
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? "dev";
            return "V" + version;
        }
    }

    public void ToggleTheme()
    {
        _themeService.CycleTheme();
    }

    public async Task OpenRefBoardDialog()
    {
        if (RefBoardViewModel is null)
        {
            await ShowStartBoardPickerAsync();
            return;
        }

        //replace with something else.
        if (BoardIsModified)
        {
            var dialogResult = await AskSaveDialog().ConfigureAwait(true);

            switch (dialogResult)
            {
                case DialogResultType.Yes:
                    await _refBoardViewModel.Save();
                    await ShowStartBoardPickerAsync();
                    break;

                case DialogResultType.No:
                    await ShowStartBoardPickerAsync();
                    break;

                case DialogResultType.Cancel: return;
            }
        }
        else
        {
            await ShowStartBoardPickerAsync();
        }
    }

    private async Task ShowStartBoardPickerAsync()
    {
        RefBoardViewModel = null;
        BoardIsModified = false;
        _pickerDialog = new OpenRefBoardViewModel(_boardManager, _events, _windowManager);
        await _windowManager.ShowDialogAsync(_pickerDialog);
        _pickerDialog = null;
    }

    public async Task PasteOnCanvas()
    {
        await _events.PublishOnBackgroundThreadAsync(new PasteOnCanvasEvent(_windowSize));
    }

    public void Remove() //Key: Remove
    {
        //DeleteEvent -> any pageviewmodel who has the state active + selected image will remove that
        RefBoardViewModel?.RemoveSelectedImage();
    }

    public void RemovePage()
    {
        RefBoardViewModel?.RemovePage();
    }

    public void UndoRemove() //Key Gesture: Ctrl-z
    {
    }

    public void ShowHelp()
    {
        if(_help.IsActive)
            _help.Close();
        _windowManager.ShowDialogAsync(_help);
    }

    public async Task OpenSettings()
    {
        var dialog = new SettingsViewModel(_configuration, _themeService, _events, _updateService, _updateInstaller);
        await _windowManager.ShowDialogAsync(dialog);
    }

    private UpdateInfo? _availableUpdate;

    public UpdateInfo? AvailableUpdate
    {
        get => _availableUpdate;
        private set
        {
            if (Equals(_availableUpdate, value)) return;

            _availableUpdate = value;
            NotifyOfPropertyChange(nameof(AvailableUpdate));
            NotifyOfPropertyChange(nameof(HasUpdate));
            NotifyOfPropertyChange(nameof(UpdateTooltip));
        }
    }

    public bool HasUpdate => AvailableUpdate is not null;

    public string UpdateTooltip => AvailableUpdate is null
        ? "Check for updates"
        : $"Allusion {AvailableUpdate.Version} is available";

    public void OpenUpdates()
    {
        _ = OpenSettings();
    }

    private async Task CheckForUpdatesOnStartupAsync()
    {
        try
        {
            if (_updateService is UpdateService concrete && concrete.IsUnstampedBuild)
                return;

            AvailableUpdate = await _updateService.CheckForUpdatesAsync().ConfigureAwait(true);
        }
        catch
        {
            AvailableUpdate = null;
        }
    }

    public async Task Save()
    {
        if (RefBoardViewModel is null) return;
        await RefBoardViewModel.Save();
    }

    public Task HandleAsync(NewRefBoardEvent message, CancellationToken cancellationToken)
    {
        _currentRefBoard = _boardManager.CreateNew(message.Name);
        InitializeRefBoard();

        return Task.CompletedTask;
    }

    public Task HandleAsync(BoardIsModfiedEvent message, CancellationToken cancellationToken)
    {
        BoardIsModified = message.IsModfied;

        return Task.CompletedTask;
    }

    public Task HandleAsync(BoardOpenedEvent message, CancellationToken cancellationToken)
    {
        _currentRefBoard = message.Board;
        InitializeRefBoard();

        return Task.CompletedTask;
    }

    public Task HandleAsync(SettingsChangedEvent message, CancellationToken cancellationToken)
    {
        NotifyOfPropertyChange(nameof(Configuration));

        return Task.CompletedTask;
    }

    private void InitializeRefBoard()
    {
        BoardIsModified = false;
        Debug.Assert(_boardManager is not null, "Holup");
        RefBoardViewModel = new ReferenceBoardViewModel(_events, _boardManager, _currentRefBoard, this);
        if (_pickerDialog?.IsActive == true)
            _pickerDialog.TryCloseAsync(true);
    }

    protected override void OnViewLoaded(object view)
    {
        base.OnViewLoaded(view);

        if (view is Window currentWindow)
        {
            _windowSize = currentWindow.RenderSize;
            currentWindow.SizeChanged += OnCurrentWindowSizeChange;

            currentWindow.Show();
            
        }
        
        FirstTime();
        _themeService.ApplyTheme(_themeService.CurrentTheme);
        _ = CheckForUpdatesOnStartupAsync();
        if (RefBoardViewModel is null)
            _ = ShowStartBoardPickerAsync();

    }



    private void OnCurrentWindowSizeChange(object sender, SizeChangedEventArgs e)
    {
        _windowSize = e.NewSize;
    }

    private void FirstTime()
    {
        if (_configuration.FirstStartUp)
        {
            _windowManager.ShowDialogAsync(new WelcomeViewModel());
            _configuration.FirstStartUp = false;
            AllusionConfiguration.Save(_configuration);
        }
    }
}
