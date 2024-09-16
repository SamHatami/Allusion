using System.IO;

namespace Allusion.WPFCore.Board;

public class RefBoardInfo
{
    //For open art board view model
    public RefBoardInfo(string name, string imageFolder, string baseFolder)
    {
        Name = name;
        ImageFolder = imageFolder;
        FilePath =Path.Combine(baseFolder,name+".json");
    }

    public string Name { get; set; }
    public string ImageFolder { get; set; }
    
    public string FilePath { get; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastWrite { get; set; }
    public DateTime LastAccess { get; set; }
}