using Allusion.WPFCore.Service;
using FluentAssertions;
using System.Net;
using System.Net.Http;

namespace Allusion.Tests
{
    public class BitmapServiceTests
    {
        [Fact]
        public void Constructor_ShouldThrowForNullHttpClient()
        {
            Assert.Throws<ArgumentNullException>(() => new BitmapService(null!));
        }

        [Fact]
        public void GetFromUri_ShouldReturnNullForNonExistentFile()
        {
            var bitmapService = CreateBitmapService();

            var result = bitmapService.GetFromUri("C:\\nonexistent\\file.png");

            result.Should().BeNull();
        }

        [Fact]
        public void LoadFromUri_ShouldReturnEmptyForNullInput()
        {
            var bitmapService = CreateBitmapService();

            var result = bitmapService.LoadFromUri((string[]?)null);

            result.Should().BeEmpty();
        }

        [Fact]
        public void LoadFromUri_ShouldReturnOnlyExistingFiles()
        {
            var bitmapService = CreateBitmapService();
            var imagePath = CreateTempImageFile();
            var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

            try
            {
                var result = bitmapService.LoadFromUri([imagePath, missingPath]);

                result.Should().ContainSingle().Which.Should().NotBeNull();
            }
            finally
            {
                try
                {
                    File.Delete(imagePath);
                }
                catch (IOException)
                {
                    // BitmapImage can keep the file handle open until it is collected.
                }
            }
        }

        [Fact]
        public async Task DownloadAndConvert_ShouldReturnNullForNullUrl()
        {
            var bitmapService = CreateBitmapService();

            var result = await bitmapService.DownloadAndConvert(null!);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DownloadAndConvert_ShouldHandleBase64DataUrl()
        {
            var bitmapService = CreateBitmapService();
            var dataUrl = $"data:image/png;base64,{TransparentPngBase64}";

            var result = await bitmapService.DownloadAndConvert(dataUrl);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task DownloadAndConvert_ShouldReturnNullWhenRequestFails()
        {
            var bitmapService = CreateBitmapService((_, _) => throw new HttpRequestException("boom"));

            var result = await bitmapService.DownloadAndConvert("https://example.com/image.png");

            result.Should().BeNull();
        }

        [Fact]
        public async Task DownloadAndConvert_ShouldReturnNullWhenCancelled()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var bitmapService = CreateBitmapService((_, cancellationToken) => Task.FromCanceled<HttpResponseMessage>(cancellationToken));

            var result = await bitmapService.DownloadAndConvert("https://example.com/image.png", cancellationTokenSource.Token);

            result.Should().BeNull();
        }

        private static BitmapService CreateBitmapService(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? sendAsync = null)
        {
            var handler = new StubHttpMessageHandler(sendAsync ?? ((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            })));

            return new BitmapService(new HttpClient(handler));
        }

        private static string CreateTempImageFile()
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            File.WriteAllBytes(path, Convert.FromBase64String(TransparentPngBase64));
            return path;
        }

        private const string TransparentPngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsync(request, cancellationToken);
            }
        }
    }
}
