using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allusion.WPFCore;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using Microsoft.Win32;

namespace Allusion.ViewModels.Dialogs
{
    public class OpenRefBoardViewModel : Screen
    {
        private readonly IReferenceBoardManager _refBoardManager;
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;
        public const string Title = "Open or Create new Art board";
        public bool CloseWhenCompleted { get; set; } = true;

        public BindableCollection<RefBoardInfo> RefBoardPaths { get; set; } = [];

        private string _globalFolder = string.Empty;

        public string GlobalFolder
        {
            get => _globalFolder;
            set
            {
                _globalFolder = value;
                NotifyOfPropertyChange(nameof(GlobalFolder));
            }
        }

        private RefBoardInfo? _selectedRefBoard;
        private string _newBoardName = "UntitledRefBoard";

        public RefBoardInfo? SelectedRefBoard
        {
            get => _selectedRefBoard;
            set
            {
                _selectedRefBoard = value;
                NotifyOfPropertyChange(nameof(SelectedRefBoard));

            }
        }

        public string NewBoardName
        {
            get => _newBoardName;
            set
            {
                _newBoardName = value;
                NotifyOfPropertyChange(nameof(NewBoardName));
            }
        }

        public OpenRefBoardViewModel(IReferenceBoardManager refBoardManager, IEventAggregator events, IWindowManager windowManager)
        {
            _refBoardManager = refBoardManager;
            _events = events;
            _windowManager = windowManager;

            GlobalFolder = _refBoardManager.CurrentConfiguration.GlobalFolder;

            RefreshBoards();

        }

        public void RefreshBoards()
        {
            RefBoardPaths.Clear();
            RefBoardPaths.AddRange(_refBoardManager.GetAllRefBoardInfos());

            SelectedRefBoard = RefBoardPaths.FirstOrDefault();
        }

        public Task Cancel()
        {
            return TryCloseAsync(false);
        }

        public Task Open()
        {
            if (SelectedRefBoard is null || string.IsNullOrEmpty(SelectedRefBoard.FilePath) ) return Task.CompletedTask;

            var openedRefBoard = _refBoardManager.Open(SelectedRefBoard.FilePath);

            if (openedRefBoard is null) 
                //TODO: Some message or exception here?
                return Task.CompletedTask;

            _events.PublishOnBackgroundThreadAsync(new BoardOpenedEvent(openedRefBoard));
            return CloseWhenCompleted ? TryCloseAsync(true) : Task.CompletedTask;
        }

        public async Task New()
        {
            var dialog = new NewRefBoardViewModel(_events)
            {
                Title = "Create New Board",
                Prompt = "Board name",
                OkText = "Create",
                NewBoardName = GetDefaultBoardName()
            };

            var accepted = await _windowManager.ShowDialogAsync(dialog);
            if (accepted != true || string.IsNullOrWhiteSpace(dialog.ResultName))
                return;

            await _events.PublishOnBackgroundThreadAsync(new NewRefBoardEvent(dialog.ResultName));
            if (CloseWhenCompleted)
                await TryCloseAsync(true);
        }

        public async Task Rename()
        {
            if (SelectedRefBoard is null)
                return;

            var dialog = new NewRefBoardViewModel(_events)
            {
                Title = "Rename Board",
                Prompt = "Board name",
                OkText = "Rename",
                NewBoardName = SelectedRefBoard.Name
            };

            var accepted = await _windowManager.ShowDialogAsync(dialog);
            if (accepted != true || string.IsNullOrWhiteSpace(dialog.ResultName))
                return;

            var openedRefBoard = _refBoardManager.Open(SelectedRefBoard.FilePath);
            if (openedRefBoard is null)
                return;

            try
            {
                _refBoardManager.RenameBoard(openedRefBoard, dialog.ResultName);
                RefreshBoards();
                SelectedRefBoard = RefBoardPaths.FirstOrDefault(board => string.Equals(board.Name, dialog.ResultName, StringComparison.Ordinal));
            }
            catch (Exception e)
            {
                var errorDialog = new DialogViewModel("Could not rename board", e.Message, DialogType.Information);
                await _windowManager.ShowDialogAsync(errorDialog);
            }
        }

        public void OpenLocation()
        {
            if (SelectedRefBoard is null || !Directory.Exists(SelectedRefBoard.ImageFolder))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SelectedRefBoard.ImageFolder,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Shell handoff can fail when Explorer is unavailable; the board list should stay usable.
            }
        }

        public async Task Remove()
        {
            if (SelectedRefBoard is null)
                return;

            var dialog = new RemoveRefBoardViewModel(SelectedRefBoard.Name);
            var accepted = await _windowManager.ShowDialogAsync(dialog);
            if (accepted != true || dialog.DialogResult != DialogResultType.Ok)
                return;

            try
            {
                _refBoardManager.RemoveFromBoardList(SelectedRefBoard, dialog.DeleteLocalFiles);
                RefreshBoards();
            }
            catch (Exception e)
            {
                var errorDialog = new DialogViewModel("Could not remove board", e.Message, DialogType.Information);
                await _windowManager.ShowDialogAsync(errorDialog);
            }
        }

        public void BrowseGlobalFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Choose Global Art Board Folder",
                InitialDirectory = Directory.Exists(GlobalFolder) ? GlobalFolder : AllusionConfiguration.DefaultFolder
            };

            if (dialog.ShowDialog() != true)
                return;

            GlobalFolder = dialog.FolderName;
            _refBoardManager.CurrentConfiguration.GlobalFolder = GlobalFolder;
            AllusionConfiguration.Save(_refBoardManager.CurrentConfiguration);
            RefreshBoards();
        }

        private string GetDefaultBoardName()
        {
            const string defaultName = "UntitledRefBoard";
            var globalFolder = _refBoardManager.CurrentConfiguration.GlobalFolder;
            var name = defaultName;
            var counter = 2;

            while (Directory.Exists(Path.Combine(globalFolder, name)))
            {
                name = $"{defaultName} {counter}";
                counter++;
            }

            return name;
        }
    }

    public class BoardOpenedEvent
    {
        public ReferenceBoard Board { get; }

        public BoardOpenedEvent(ReferenceBoard board)
        {
            Board = board;
        }
    }
}
