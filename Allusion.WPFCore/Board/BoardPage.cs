using System.IO;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;

namespace Allusion.WPFCore.Board
{
    [Serializable]
    public class BoardPage
    {
        public string Name { get; set; }
        private string _pageFolder;

        public string PageFolder
        {
            get => _pageFolder;
            set
            {
                _pageFolder = value;
                BackupFolder = Path.Combine(_pageFolder, "old");
            }
        }
        public string BackupFolder { get; set; }
        public string Description { get; set; }
        public List<ImageItem> ImageItems { get; set; } = [];
        public List<NoteItem> NoteItems { get; set; } = [];
        public Guid BoardId { get; }

        [JsonIgnore] public ReferenceBoard ParentBoard { get;}

        public BoardPage(ReferenceBoard parentBoard)
        {
            if(parentBoard is null) return; //Not good if this is the case

            ParentBoard = parentBoard;
            BoardId = Guid.NewGuid();
            SetFolders();
        }

        public void Rename(string newName)
        {
            Name = newName;
            var newPageFolder = PageFolder = Path.Combine(ParentBoard.BaseFolder, newName);
            Directory.Move(PageFolder, newPageFolder);

        }

        private void SetFolders()
        {
            if (string.IsNullOrEmpty(Name))
                Name = "UnnamedPage-" + ParentBoard.Pages.Count;
            if (!string.IsNullOrEmpty(BackupFolder) && !string.IsNullOrEmpty(PageFolder)) return;
           
            PageFolder = Path.Combine(ParentBoard.BaseFolder, Name);
            BackupFolder = Path.Combine(PageFolder, "old");
            Directory.CreateDirectory(PageFolder);
            Directory.CreateDirectory(BackupFolder);

        }
    }


}
