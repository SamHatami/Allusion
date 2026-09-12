using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Service;

public class UpdateService : IUpdateService
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(1);
    private static readonly Version UnstampedVersion = new(1, 0, 0, 0);

    private readonly HttpClient _httpClient;
    private readonly string _repository;
    private readonly Version _currentVersion;
    private readonly string _informationalVersion;
    private UpdateInfo? _cachedResult;
    private DateTime _cachedAt;

    public UpdateService(HttpClient httpClient, string repository = "SamHatami/Allusion", Version? currentVersion = null)
    {
        _httpClient = httpClient;
        _repository = repository;
        _currentVersion = currentVersion ?? Assembly.GetExecutingAssembly().GetName().Version ?? UnstampedVersion;
        _informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
    }

    public bool IsUnstampedBuild => _informationalVersion.Contains('-')
        || _currentVersion is { Major: 1, Minor: 0, Build: 0, Revision: 0 }
        or { Major: 0, Minor: 0, Build: 0, Revision: 0 };

    public async Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedResult is not null && DateTime.UtcNow - _cachedAt < CacheLifetime)
            return _cachedResult;

        var result = await FetchLatestStableAsync(cancellationToken).ConfigureAwait(false);
        if (result is not null)
        {
            _cachedResult = result;
            _cachedAt = DateTime.UtcNow;
        }
        return result;
    }

    private async Task<UpdateInfo?> FetchLatestStableAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.github.com/repos/{_repository}/releases/latest");
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Allusion", _currentVersion.ToString()));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var release = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            return ToUpdateInfo(release.RootElement);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private UpdateInfo? ToUpdateInfo(JsonElement release)
    {
        var tag = release.TryGetProperty("tag_name", out var tagElement) ? tagElement.GetString() : null;
        var latest = ParseStableVersion(tag);
        if (latest is null || latest <= _currentVersion)
            return null;

        var pageUrl = release.TryGetProperty("html_url", out var urlElement) ? urlElement.GetString() : null;
        if (string.IsNullOrEmpty(pageUrl))
            return null;

        var notes = release.TryGetProperty("body", out var bodyElement) ? bodyElement.GetString() : null;
        return new UpdateInfo(tag!, pageUrl, FindInstallerUrl(release), notes);
    }

    private static string? FindInstallerUrl(JsonElement release)
    {
        if (!release.TryGetProperty("assets", out var assetsElement) || assetsElement.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var asset in assetsElement.EnumerateArray())
        {
            var name = asset.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
            var url = asset.TryGetProperty("browser_download_url", out var urlElement) ? urlElement.GetString() : null;
            if (!string.IsNullOrEmpty(url) && name?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true)
                return url;
        }

        return null;
    }

    internal static Version? ParseStableVersion(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return null;

        var cleaned = tag.Trim().TrimStart('v', 'V');
        if (cleaned.Contains('-'))
            return null;

        return Version.TryParse(cleaned, out var version) ? version : null;
    }
}
