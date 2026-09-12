using Allusion.WPFCore.Interfaces;
using Velopack;
using Velopack.Sources;

namespace Allusion.Service;

public class VelopackUpdateInstaller : IUpdateInstaller
{
    private const string RepositoryUrl = "https://github.com/SamHatami/Allusion";

    public async Task<bool> DownloadAndInstallAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var manager = new UpdateManager(new GithubSource(RepositoryUrl, null, false));
            var update = await manager.CheckForUpdatesAsync().ConfigureAwait(false);
            if (update is null)
                return false;

            cancellationToken.ThrowIfCancellationRequested();
            await manager.DownloadUpdatesAsync(update).ConfigureAwait(false);
            manager.ApplyUpdatesAndRestart(update);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
