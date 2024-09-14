using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allusion.WPFCore.Handlers;
using Caliburn.Micro;

namespace Allusion.ViewModels
{
    public class OpenArtBoardViewModel : Screen
    {
        private readonly ArtBoardHandler _artBoardHandler;
        private readonly EventAggregator _events;
        public const string Title = "Open or Create new Art board";

        private BindableCollection<string> ArtBoardPaths { get; set; }= new BindableCollection<string>();

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

        public OpenArtBoardViewModel(ArtBoardHandler artBoardHandler, EventAggregator events)
        {
            _artBoardHandler = artBoardHandler;
            _events = events;

            GlobalFolder = _artBoardHandler.CurrentConfiguration.GlobalFolder;
            _artBoardHandler.GetAllArtBoardFolders();
        }
    }
}
