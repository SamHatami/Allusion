using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Handlers;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels.Dialogs
{
    public class OpenRefBoardViewModel : Screen
    {
        private readonly IReferenceBoardManager _refBoardManager;
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;
        public const string Title = "Open or Create new Art board";

        public BindableCollection<RefBoardInfo> RefBoardPaths { get; set; } = [];

        private string _globalFolder;

        public string GlobalFolder
        {
            get => _globalFolder;
            set
            {
                _globalFolder = value;
                NotifyOfPropertyChange(nameof(GlobalFolder));
            }
        }

        private RefBoardInfo _selectedRefBoard;

        public RefBoardInfo SelectedRefBoard
        {
            get => _selectedRefBoard;
            set
            {
                _selectedRefBoard = value;
                NotifyOfPropertyChange(nameof(SelectedRefBoard));

            }
        }

        public OpenRefBoardViewModel(IReferenceBoardManager refBoardManager, IEventAggregator events, IWindowManager windowManager)
        {
            _refBoardManager = refBoardManager;
            _events = events;
            _windowManager = windowManager;

            GlobalFolder = _refBoardManager.CurrentConfiguration.GlobalFolder;

            RefBoardPaths.AddRange(_refBoardManager.GetAllRefBoardInfos());

        }

        public Task Cancel()
        {
            return TryCloseAsync(false);
        }

        public Task Open()
        {
            var openedRefBoard = _refBoardManager.Open(SelectedRefBoard.FilePath);

            if (openedRefBoard is null) 
                //TODO: Some message or exception here?
                return Task.CompletedTask;

            _events.PublishOnBackgroundThreadAsync(openedRefBoard);
            return TryCloseAsync(true);
        }

        public void New()
        {
            var newBoardWin = _windowManager.ShowDialogAsync(new NewRefBoardViewModel(_events)).Result ?? false;

            if (newBoardWin)
                TryCloseAsync(true);
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
