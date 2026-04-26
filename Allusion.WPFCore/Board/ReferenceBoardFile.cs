using Allusion.WPFCore.Service;
using Allusion.WPFCore.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Allusion.WPFCore.Board;

internal static class ReferenceBoardFile
{
    public static ReferenceBoard? Read(string projectFileFullPath)
    {
        if (!File.Exists(projectFileFullPath))
            return null;

        var projectJson = File.ReadAllText(projectFileFullPath);

        try
        {
            var project = JsonSerializer.Deserialize<ReferenceBoard>(projectJson);
            if (project is not null)
                NormalizeLoadedBoard(project, projectFileFullPath);

            return project;
        }
        catch (Exception e)
        {
            StaticLogger.Error("Could not open board", true, e.Message);
            return null;
        }
    }

    public static bool Save(ReferenceBoard board)
    {
        if (!Directory.Exists(board.BaseFolder))
            Directory.CreateDirectory(board.BaseFolder);

        var refBoardDataFile = Path.Combine(board.BaseFolder, board.Name + ".allusion");

        try
        {
            PrepareForSave(board);
            File.WriteAllText(refBoardDataFile, JsonSerializer.Serialize(board));

            foreach (var page in board.Pages)
            {
                BackupRemovedImages(page);
                SaveImagesToDisc(page);
            }

            return true;
        }
        catch (Exception e)
        {
            StaticLogger.Error("Could not save board", true, e.Message);
            return false;
        }
    }

    private static void PrepareForSave(ReferenceBoard board)
    {
        board.FileFormatVersion = ReferenceBoard.CurrentFileFormatVersion;
        if (board.BoardId == Guid.Empty)
            board.BoardId = Guid.NewGuid();

        board.BackupFolder = Path.Combine(board.BaseFolder, "Old");

        foreach (var page in board.Pages)
        {
            if (string.IsNullOrWhiteSpace(page.PageFolder))
                page.PageFolder = Path.Combine(board.BaseFolder, page.Name);

            page.RelativePageFolder = GetRelativePath(board.BaseFolder, page.PageFolder);
            page.BackupFolder = Path.Combine(page.PageFolder, "old");

            foreach (var image in page.ImageItems)
            {
                if (!string.IsNullOrWhiteSpace(image.ItemPath))
                    image.RelativeItemPath = GetRelativePath(board.BaseFolder, image.ItemPath);
            }
        }
    }

    private static void NormalizeLoadedBoard(ReferenceBoard board, string projectFileFullPath)
    {
        var boardFolder = Path.GetDirectoryName(projectFileFullPath);
        if (string.IsNullOrWhiteSpace(boardFolder))
            return;

        board.BaseFolder = boardFolder;
        board.BackupFolder = Path.Combine(board.BaseFolder, "Old");
        if (board.FileFormatVersion <= 0)
            board.FileFormatVersion = ReferenceBoard.CurrentFileFormatVersion;
        if (board.BoardId == Guid.Empty)
            board.BoardId = Guid.NewGuid();

        foreach (var page in board.Pages)
        {
            var storedPageFolder = page.PageFolder;
            page.PageFolder = !string.IsNullOrWhiteSpace(page.RelativePageFolder)
                ? ResolveBoardPath(board.BaseFolder, page.RelativePageFolder)
                : ResolveLegacyPath(board.BaseFolder, page.PageFolder, page.Name);

            page.BackupFolder = Path.Combine(page.PageFolder, "old");
            page.RelativePageFolder = GetRelativePath(board.BaseFolder, page.PageFolder);

            foreach (var image in page.ImageItems)
            {
                image.ItemPath = !string.IsNullOrWhiteSpace(image.RelativeItemPath)
                    ? ResolveBoardPath(board.BaseFolder, image.RelativeItemPath)
                    : ResolveLegacyImagePath(page.PageFolder, storedPageFolder, image.ItemPath);

                if (!string.IsNullOrWhiteSpace(image.ItemPath))
                    image.RelativeItemPath = GetRelativePath(board.BaseFolder, image.ItemPath);
            }
        }
    }

    private static string ResolveBoardPath(string boardFolder, string relativePath)
    {
        return Path.GetFullPath(Path.Combine(boardFolder, relativePath));
    }

    private static string ResolveLegacyPath(string boardFolder, string storedPath, string fallbackName)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
            return Path.Combine(boardFolder, fallbackName);

        if (!Path.IsPathRooted(storedPath))
            return ResolveBoardPath(boardFolder, storedPath);

        if (IsPathInsideDirectory(storedPath, boardFolder))
            return storedPath;

        var fileName = Path.GetFileName(storedPath);
        return string.IsNullOrWhiteSpace(fileName)
            ? Path.Combine(boardFolder, fallbackName)
            : Path.Combine(boardFolder, fileName);
    }

    private static string ResolveLegacyImagePath(string pageFolder, string storedPageFolder, string storedImagePath)
    {
        if (string.IsNullOrWhiteSpace(storedImagePath))
            return string.Empty;

        if (!Path.IsPathRooted(storedImagePath))
            return Path.GetFullPath(Path.Combine(pageFolder, storedImagePath));

        if (IsPathInsideDirectory(storedImagePath, pageFolder))
            return storedImagePath;

        if (!string.IsNullOrWhiteSpace(storedPageFolder) &&
            Path.IsPathRooted(storedPageFolder) &&
            IsPathInsideDirectory(storedImagePath, storedPageFolder))
        {
            return Path.Combine(pageFolder, Path.GetRelativePath(storedPageFolder, storedImagePath));
        }

        return Path.Combine(pageFolder, Path.GetFileName(storedImagePath));
    }

    private static string GetRelativePath(string boardFolder, string path)
    {
        return Path.GetRelativePath(boardFolder, path);
    }

    private static bool IsPathInsideDirectory(string path, string directory)
    {
        var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullDirectory = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private static void BackupRemovedImages(BoardPage page)
    {
        var imagePaths = GetImagePathsFromFolder(page);

        if (imagePaths == null)
        {
            Trace.WriteLine("imagepath was null");
            return;
        }

        var items = page.ImageItems.Select(i => i.ItemPath).ToArray();
        foreach (var path in imagePaths)
        {
            if (items.Contains(path)) continue;

            var toFile = GetBackupFilename(page.BackupFolder, path);

            try
            {
                File.Move(path, toFile);
            }
            catch (Exception)
            {
                StaticLogger.Warning("Could not backup images", true);
            }
        }
    }

    private static string GetBackupFilename(string backUpFolder, string originalFilePath)
    {
        var id = Guid.NewGuid().ToString();
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd");
        var fileName = $"{timestamp}_{id}_{Path.GetFileName(originalFilePath)}";
        return Path.Combine(backUpFolder, fileName);
    }

    private static string[] GetImagePathsFromFolder(BoardPage page)
    {
        var allFiles = Directory.GetFiles(page.PageFolder);
        return allFiles.Where(f => f.EndsWith(".png")).ToArray();
    }

    private static void SaveImagesToDisc(BoardPage page)
    {
        if (!Directory.Exists(page.PageFolder))
        {
            Directory.CreateDirectory(page.PageFolder);
            Directory.CreateDirectory(Path.Combine(page.BackupFolder));
        }

        foreach (var image in page.ImageItems)
            BitmapUtils.SaveToFile(image.SourceImage, image.ItemPath);
    }
}
