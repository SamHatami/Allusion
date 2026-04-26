using System.IO;

namespace Allusion.WPFCore.Board;

[Serializable]
public class ReferenceBoard
{
    public const int CurrentFileFormatVersion = 1;

    public int FileFormatVersion { get; set; } = CurrentFileFormatVersion;
    public Guid BoardId { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string BaseFolder { get; set; }
    public string BackupFolder { get; set; }

    public List<BoardPage> Pages { get; set; } = [];

    public ReferenceBoard(string name, string baseFolder)
    {
        Name = name;
        BaseFolder = baseFolder;

        Initialize();
    }

    private void Initialize()
    {
        BackupFolder = Path.Combine(BaseFolder, "Old");
        Directory.CreateDirectory(BaseFolder);
        Directory.CreateDirectory(Path.Combine(BackupFolder));

        if (Pages.Count == 0)
            Pages.Add(new BoardPage(this));
    }

    public static ReferenceBoard? Read(string projectFileFullPath)
    {
        return ReferenceBoardFile.Read(projectFileFullPath);
    }

    public static bool Save(ReferenceBoard board)
    {
        return ReferenceBoardFile.Save(board);
    }
}

public enum BoardState
{
    Idle,
    Saving,
    Loading,
    Modified
}
