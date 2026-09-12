namespace Allusion.WPFCore.Interfaces;

public sealed record UpdateInfo(string Version, string PageUrl, string? DownloadUrl, string? Notes);

public interface IUpdateService
{
    Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
}
