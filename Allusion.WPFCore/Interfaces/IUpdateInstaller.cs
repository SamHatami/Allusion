namespace Allusion.WPFCore.Interfaces;

public interface IUpdateInstaller
{
    Task<bool> DownloadAndInstallAsync(CancellationToken cancellationToken = default);
}
