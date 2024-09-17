using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using Caliburn.Micro;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Allusion.WPFCore.Handlers;

public class ReferenceBoardManager : IReferenceBoardManager
{
    public ReferenceBoard? CurrentRefBoard { get; private set; }
    public AllusionConfiguration CurrentConfiguration { get; private set; }
    private readonly IEventAggregator _events;
    private BitmapService _bitmapService = new();

    public ReferenceBoardManager(IEventAggregator events, AllusionConfiguration configuration)
    {
        _events = events;
        CurrentConfiguration = configuration;
    }

    

    public ReferenceBoard Open(string fullPath = "")
    {
        try
        {
            var path = string.IsNullOrEmpty(fullPath) ? CurrentRefBoard.BaseFolder : fullPath;

            if (!File.Exists(path)) return ; //använd catchen

            CurrentRefBoard = ReferenceBoard.Read(path);

            _events.PublishOnBackgroundThreadAsync(new OpenRefBoardEvent(CurrentRefBoard));
        }
        catch (Exception e)

        {
            Trace.WriteLine(e);
            throw;
        }
    }

    public void CreateNew(string name = "UntitledRefBoard")
    {
        var RefBoardPath = Path.Combine(CurrentConfiguration.GlobalFolder, name);
        CurrentRefBoard = new ReferenceBoard(name, RefBoardPath);
        ReferenceBoard.Save(CurrentRefBoard);
    }

    public async Task<bool> Save()
    {
        bool saved;

        try
        {
            saved = await Task.Run(() => ReferenceBoard.Save(CurrentRefBoard));
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            throw;
        }

        return saved;
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