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

namespace Allusion.ViewModels
{
    public class OpenRefBoardViewModel : Screen
    {
        private readonly IReferenceBoardHandler _refBoardHandler;
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;
        public const string Title = "Open or Create new Art board";

        public BindableCollection<RefBoardInfo> RefBoardPaths { get; set; }= [];

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

        public OpenRefBoardViewModel(IReferenceBoardHandler refBoardHandler, IEventAggregator events, IWindowManager windowManager)
        {
            _refBoardHandler = refBoardHandler;
            _events = events;
            _windowManager = windowManager;

            GlobalFolder = _refBoardHandler.CurrentConfiguration.GlobalFolder;

            RefBoardPaths.AddRange(_refBoardHandler.GetAllRefBoardInfos());
   
        }

        public Task Cancel()
        {
            return TryCloseAsync(false);
        }

        public Task Open()
        {
            _refBoardHandler.OpenRefBoard(SelectedRefBoard.FilePath);
            return TryCloseAsync(true);
        }

        public void New()
        {
            var newBoardWin = _windowManager.ShowDialogAsync(new NewRefBoardViewModel(_events)).Result ?? false;

            if (newBoardWin)
                this.TryCloseAsync(true);
        }
    }
}
