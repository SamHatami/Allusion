using System.IO;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;

namespace Allusion.WPFCore.Board
{
    [Serializable]
    public class BoardPage
    {
        public string Name { get; set; } = string.Empty;
        private string _pageFolder = string.Empty;

        public string PageFolder
        {
            get => _pageFolder;
            set
            {
                _pageFolder = value;
                BackupFolder = Path.Combine(_pageFolder, "old");
            }
        }
        public string RelativePageFolder { get; set; } = string.Empty;
        public string BackupFolder { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ImageItem> ImageItems { get; set; } = [];
        public List<NoteItem> NoteItems { get; set; } = [];
        public Guid BoardId { get; }

        [JsonIgnore] public ReferenceBoard? ParentBoard { get; }

        public BoardPage(ReferenceBoard? parentBoard)
        {
            ParentBoard = parentBoard;
            BoardId = Guid.NewGuid();
            if (ParentBoard is not null)
                SetFolders();
        }

        public void Rename(string newName)
        {
            Name = newName;
            if (ParentBoard is null) return;
            var newPageFolder = PageFolder = Path.Combine(ParentBoard.BaseFolder, newName);
            Directory.Move(PageFolder, newPageFolder);

        }

        private void SetFolders()
        {
            if (string.IsNullOrEmpty(Name))
                Name = "UnnamedPage-" + ParentBoard!.Pages.Count;
            if (!string.IsNullOrEmpty(BackupFolder) && !string.IsNullOrEmpty(PageFolder)) return;
           
            PageFolder = Path.Combine(ParentBoard!.BaseFolder, Name);
            BackupFolder = Path.Combine(PageFolder, "old");
            Directory.CreateDirectory(PageFolder);
            Directory.CreateDirectory(BackupFolder);

        }
    }


}
