using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Allusion.WPFCore.Service;
using FluentAssertions;

namespace Allusion.Tests;

public class UpdateServiceTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _json;
        private readonly HttpStatusCode _status;
        public int CallCount;

        public StubHandler(string json, HttpStatusCode status = HttpStatusCode.OK)
        {
            _json = json;
            _status = status;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            request.Headers.UserAgent.Should().NotBeEmpty("GitHub API requires a User-Agent");
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_json)
            });
        }
    }

    private static string ReleaseJson(string tag, string? body = "Notes here", bool withExe = true)
    {
        var assets = withExe
            ? """[{"name": "Allusion.exe", "browser_download_url": "https://example.com/Allusion.exe"}]"""
            : """[{"name": "checksums.txt", "browser_download_url": "https://example.com/checksums.txt"}]""";
        return $$"""
            {
                "tag_name": "{{tag}}",
                "html_url": "https://github.com/SamHatami/Allusion/releases/tag/{{tag}}",
                "body": "{{body}}",
                "assets": {{assets}}
            }
            """;
    }

    private static UpdateService CreateService(StubHandler handler, string current)
    {
        return new UpdateService(new HttpClient(handler), currentVersion: new Version(current));
    }

    [Fact]
    public async Task CheckForUpdates_ShouldReturnInfo_WhenStableReleaseIsNewer()
    {
        var service = CreateService(new StubHandler(ReleaseJson("v1.7")), "1.6.0");

        var result = await service.CheckForUpdatesAsync();

        result.Should().NotBeNull();
        result!.Version.Should().Be("v1.7");
        result.PageUrl.Should().Contain("v1.7");
        result.DownloadUrl.Should().Be("https://example.com/Allusion.exe");
        result.Notes.Should().Be("Notes here");
    }

    [Fact]
    public async Task CheckForUpdates_ShouldReturnNull_WhenAlreadyCurrent()
    {
        var service = CreateService(new StubHandler(ReleaseJson("v1.7")), "1.7.0");

        var result = await service.CheckForUpdatesAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdates_ShouldIgnorePrereleaseTags()
    {
        var service = CreateService(new StubHandler(ReleaseJson("v2.0-alpha")), "1.7.0");

        var result = await service.CheckForUpdatesAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdates_ShouldReturnNull_WhenNoReleaseExists()
    {
        var service = CreateService(new StubHandler("{}", HttpStatusCode.NotFound), "1.0.0");

        var result = await service.CheckForUpdatesAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdates_ShouldOmitDownloadUrl_WhenNoExeAsset()
    {
        var service = CreateService(new StubHandler(ReleaseJson("v1.8", withExe: false)), "1.7.0");

        var result = await service.CheckForUpdatesAsync();

        result.Should().NotBeNull();
        result!.DownloadUrl.Should().BeNull();
        result.PageUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckForUpdates_ShouldCacheSecondCall()
    {
        var handler = new StubHandler(ReleaseJson("v1.7"));
        var service = CreateService(handler, "1.6.0");

        await service.CheckForUpdatesAsync();
        await service.CheckForUpdatesAsync();

        handler.CallCount.Should().Be(1);
    }

    [Theory]
    [InlineData("v1.7", "1.7")]
    [InlineData("1.7.0", "1.7.0")]
    [InlineData("V2.0", "2.0")]
    [InlineData("v1.6-alpha", null)]
    [InlineData("not-a-version", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void ParseStableVersion_ShouldFilterPrereleasesAndJunk(string? tag, string? expected)
    {
        var result = UpdateService.ParseStableVersion(tag);

        result?.ToString().Should().Be(expected);
    }
}
