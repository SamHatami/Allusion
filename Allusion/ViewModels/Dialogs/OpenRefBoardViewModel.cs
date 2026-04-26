using System;
using System.Collections.Generic;
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

        public OpenRefBoardViewModel(IReferenceBoardManager refBoardManager, IEventAggregator events)
        {
            _refBoardManager = refBoardManager;
            _events = events;

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

        public Task New()
        {
            if (string.IsNullOrWhiteSpace(NewBoardName))
                return Task.CompletedTask;

            _events.PublishOnBackgroundThreadAsync(new NewRefBoardEvent(NewBoardName.Trim()));
            return CloseWhenCompleted ? TryCloseAsync(true) : Task.CompletedTask;
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
