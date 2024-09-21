using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using Caliburn.Micro;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Allusion.WPFCore.Managers;

public class ReferenceBoardManager : IReferenceBoardManager
{
    public AllusionConfiguration CurrentConfiguration { get; private set; }
    private readonly IEventAggregator _events;
    private BitmapService _bitmapService = new();

    public ReferenceBoardManager(IEventAggregator events, AllusionConfiguration configuration)
    {
        _events = events;
        CurrentConfiguration = configuration;
    }



    public ReferenceBoard? Open(string fullPath = "")
    {
        if (!File.Exists(fullPath)) return null;

        ReferenceBoard? openedRefBoard;
        try
        {
            openedRefBoard = ReferenceBoard.Read(fullPath);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            //TODO: NLOG
            throw;
        }

        return openedRefBoard;
    }

    public ReferenceBoard CreateNew(string name = "UntitledRefBoard")
    {
        var RefBoardPath = Path.Combine(CurrentConfiguration.GlobalFolder, name);
        var newRefBoard = new ReferenceBoard(name, RefBoardPath);
        ReferenceBoard.Save(newRefBoard);

        return newRefBoard;
    }

    public void RenameBoard(ReferenceBoard board, string newName)
    {
        board.Name = newName;
        var newFolder = Path.Combine(CurrentConfiguration.GlobalFolder, newName);
        Directory.Move(board.BaseFolder, newFolder);
    }

    public async Task<bool> Save(ReferenceBoard referenceBoard)
    {
        bool saved;

        try
        {
            saved = await Task.Run(() => ReferenceBoard.Save(referenceBoard));
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            throw;
        }

        return saved;
    }

    public BoardPage AddPage(ReferenceBoard board, string pageName = "")
    {
        var newPage = new BoardPage(board) { Name = string.IsNullOrEmpty(pageName) ? $"Untitled - {board.Pages.Count + 1}" : pageName };
        board.Pages.Add(newPage);
        return newPage;
    }

    public void DeletePage(ReferenceBoard board, BoardPage page)
    {
        board.Pages.Remove(page);
    }

    private string[] GetAllRefBoardFolders()
    {
        return Directory.GetDirectories(CurrentConfiguration.GlobalFolder);
    }

    public RefBoardInfo[] GetAllRefBoardInfos()
    {
        var folders = GetAllRefBoardFolders();

        List<RefBoardInfo> infos = new();

        foreach (var folder in folders)
        {
            var name = Path.GetFileName(folder);
            var creationTime = Directory.GetCreationTime(folder);
            var changeTime = Directory.GetLastWriteTime(folder);
            var lastOpenTime = Directory.GetLastAccessTime(folder);
            var filePath = Path.Combine(CurrentConfiguration.GlobalFolder, name);

            infos.Add(new RefBoardInfo(name, folder, filePath)
            {
                CreatedDate = creationTime,
                LastWrite = changeTime,
                LastAccess = lastOpenTime
            });
        }

        return infos.ToArray();
    }



    private void SaveToGlobalRefBoardList()
    {
        //TODO:
    }
}