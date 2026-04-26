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
    private readonly IBitmapService _bitmapService;

    public ReferenceBoardManager(IEventAggregator events, AllusionConfiguration configuration, IBitmapService bitmapService)
    {
        _events = events;
        CurrentConfiguration = configuration;
        _bitmapService = bitmapService ?? throw new ArgumentNullException(nameof(bitmapService));
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
        var boardName = ValidateBoardName(name);
        var refBoardPath = Path.Combine(CurrentConfiguration.GlobalFolder, boardName);
        if (Directory.Exists(refBoardPath))
            throw new IOException($"A board named '{boardName}' already exists.");

        var newRefBoard = new ReferenceBoard(boardName, refBoardPath);
        ReferenceBoard.Save(newRefBoard);

        return newRefBoard;
    }

    public void RenameBoard(ReferenceBoard board, string newName)
    {
        ArgumentNullException.ThrowIfNull(board);

        var boardName = ValidateBoardName(newName);
        if (string.Equals(board.Name, boardName, StringComparison.Ordinal))
            return;

        var oldName = board.Name;
        var oldFolder = board.BaseFolder;
        if (string.IsNullOrWhiteSpace(oldFolder) || !Directory.Exists(oldFolder))
            throw new DirectoryNotFoundException($"Could not find the board folder '{oldFolder}'.");

        var parentFolder = Path.GetDirectoryName(oldFolder) ?? CurrentConfiguration.GlobalFolder;
        var newFolder = Path.Combine(parentFolder, boardName);
        if (Directory.Exists(newFolder))
            throw new IOException($"A board named '{boardName}' already exists.");

        Directory.Move(oldFolder, newFolder);

        try
        {
            UpdateBoardPaths(board, oldFolder, newFolder, boardName);

            if (!ReferenceBoard.Save(board))
                throw new IOException($"The board folder was renamed, but '{boardName}' could not be saved.");

            var oldBoardFile = Path.Combine(newFolder, oldName + ".allusion");
            if (!string.Equals(oldName, boardName, StringComparison.Ordinal) && File.Exists(oldBoardFile))
                File.Delete(oldBoardFile);
        }
        catch
        {
            if (Directory.Exists(newFolder) && !Directory.Exists(oldFolder))
                Directory.Move(newFolder, oldFolder);

            UpdateBoardPaths(board, newFolder, oldFolder, oldName);
            throw;
        }
    }

    public void RemoveFromBoardList(RefBoardInfo boardInfo, bool deleteLocalFiles)
    {
        ArgumentNullException.ThrowIfNull(boardInfo);

        var boardFile = NormalizePath(boardInfo.FilePath);

        if (deleteLocalFiles)
        {
            DeleteBoardFolder(boardInfo, boardFile);
            CurrentConfiguration.IgnoredRefBoardFiles.RemoveAll(path => string.Equals(NormalizePath(path), boardFile, StringComparison.OrdinalIgnoreCase));
        }
        else if (!CurrentConfiguration.IgnoredRefBoardFiles.Any(path => string.Equals(NormalizePath(path), boardFile, StringComparison.OrdinalIgnoreCase)))
        {
            CurrentConfiguration.IgnoredRefBoardFiles.Add(boardFile);
        }

        AllusionConfiguration.Save(CurrentConfiguration);
    }

    private void DeleteBoardFolder(RefBoardInfo boardInfo, string boardFile)
    {
        var boardFolder = NormalizePath(boardInfo.ImageFolder);
        var globalFolder = NormalizePath(CurrentConfiguration.GlobalFolder);

        if (!File.Exists(boardFile))
            throw new FileNotFoundException("Could not find the board file.", boardFile);

        if (!Directory.Exists(boardFolder))
            throw new DirectoryNotFoundException($"Could not find the board folder '{boardFolder}'.");

        if (!IsPathWithinDirectory(boardFolder, globalFolder))
            throw new InvalidOperationException("The selected board is outside the configured global board folder.");

        Directory.Delete(boardFolder, true);
    }

    private static string ValidateBoardName(string name)
    {
        var trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            throw new ArgumentException("A board name is required.", nameof(name));

        if (trimmedName is "." or ".." || trimmedName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Board names cannot contain path characters.", nameof(name));

        return trimmedName;
    }

    private static void UpdateBoardPaths(ReferenceBoard board, string oldFolder, string newFolder, string newName)
    {
        board.Name = newName;
        board.BaseFolder = newFolder;
        board.BackupFolder = Path.Combine(newFolder, "Old");

        foreach (var page in board.Pages)
        {
            if (!string.IsNullOrWhiteSpace(page.PageFolder))
                page.PageFolder = ReplacePathPrefix(page.PageFolder, oldFolder, newFolder);
            else
                page.PageFolder = Path.Combine(newFolder, page.Name);

            if (!string.IsNullOrWhiteSpace(page.BackupFolder))
                page.BackupFolder = ReplacePathPrefix(page.BackupFolder, oldFolder, newFolder);
            else
                page.BackupFolder = Path.Combine(page.PageFolder, "old");

            foreach (var image in page.ImageItems)
            {
                if (!string.IsNullOrWhiteSpace(image.ItemPath))
                    image.ItemPath = ReplacePathPrefix(image.ItemPath, oldFolder, newFolder);
            }
        }
    }

    private static string ReplacePathPrefix(string path, string oldPrefix, string newPrefix)
    {
        var fullPath = Path.GetFullPath(path);
        var fullOldPrefix = Path.GetFullPath(oldPrefix).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (!fullPath.StartsWith(fullOldPrefix, StringComparison.OrdinalIgnoreCase))
            return path;

        var relativePath = Path.GetRelativePath(fullOldPrefix, fullPath);
        return relativePath == "."
            ? newPrefix
            : Path.Combine(newPrefix, relativePath);
    }

    public async Task<bool> Save(ReferenceBoard referenceBoard)
    {
        bool saved;

        try
        {
            saved = await Task.Run(() => ReferenceBoard.Save(referenceBoard));
            if(saved)
                StaticLogger.Info($"{referenceBoard.Name} saved", true,true);
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
        var allusionBoards = Directory.GetFiles(CurrentConfiguration.GlobalFolder, "*.allusion", SearchOption.AllDirectories);

        List<string> folders = [];
        var ignoredFiles = CurrentConfiguration.IgnoredRefBoardFiles
            .Select(NormalizePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var allusionBoard in allusionBoards)
        {
            if (ignoredFiles.Contains(NormalizePath(allusionBoard)))
                continue;

            var refBoard = ReferenceBoard.Read(allusionBoard);
            if (refBoard is null || string.IsNullOrWhiteSpace(refBoard.BaseFolder))
                continue;

            folders.Add(refBoard.BaseFolder);
        }

        return folders.ToArray();
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
            var filePath = folder;

            infos.Add(new RefBoardInfo(name, folder, filePath)
            {
                CreatedDate = creationTime,
                LastWrite = changeTime,
                LastAccess = lastOpenTime
            });
        }

        return infos.ToArray();
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path);
    }

    private static bool IsPathWithinDirectory(string path, string directory)
    {
        var fullPath = NormalizePath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullDirectory = NormalizePath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return !string.Equals(fullPath, fullDirectory, StringComparison.OrdinalIgnoreCase)
               && fullPath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
    }



    private void SaveToGlobalRefBoardList()
    {
        //TODO:
    }
}
