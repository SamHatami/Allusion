using Allusion.WPFCore.Extensions;
using Allusion.WPFCore.Interfaces;
using FakeItEasy;
using FluentAssertions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.Tests
{
    public class DataObjectImageExtractorTests
    {
        private readonly IBitmapService _bitmapService;
        private readonly DataObjectImageExtractor _extractor;

        public DataObjectImageExtractorTests()
        {
            _bitmapService = A.Fake<IBitmapService>();
            _extractor = new DataObjectImageExtractor(_bitmapService);
        }

        [Fact]
        public void GetBitmapFromLocal_ShouldReturnNullWhenNoFormatsArePresent()
        {
            var dataObject = A.Fake<IDataObject>();
            A.CallTo(() => dataObject.GetFormats(true)).Returns(Array.Empty<string>());

            var result = _extractor.GetBitmapFromLocal(dataObject);

            result.Should().BeNull();
        }

        [Fact]
        public void GetBitmapFromLocal_ShouldLoadBitmapsFromFileContents()
        {
            var dataObject = A.Fake<IDataObject>();
            string[] filePaths = ["C:\\images\\a.png"];
            BitmapImage?[] expected = [new BitmapImage()];

            A.CallTo(() => dataObject.GetFormats(true)).Returns(["FileContents"]);
            A.CallTo(() => dataObject.GetData("FileContents")).Returns(filePaths);
            A.CallTo(() => _bitmapService.LoadFromUri(filePaths)).Returns(expected);

            var result = _extractor.GetBitmapFromLocal(dataObject);

            result.Should().ContainSingle().Which.Should().BeSameAs(expected[0]);
        }

        [Fact]
        public void GetBitmapFromLocal_ShouldLoadBitmapsFromFileName()
        {
            var dataObject = A.Fake<IDataObject>();
            string[] filePaths = ["C:\\images\\a.png"];
            BitmapImage?[] expected = [new BitmapImage()];

            A.CallTo(() => dataObject.GetFormats(true)).Returns(["FileName"]);
            A.CallTo(() => dataObject.GetData("FileName")).Returns(filePaths);
            A.CallTo(() => _bitmapService.LoadFromUri(filePaths)).Returns(expected);

            var result = _extractor.GetBitmapFromLocal(dataObject);

            result.Should().ContainSingle().Which.Should().BeSameAs(expected[0]);
        }

        [Fact]
        public async Task GetWebBitmapAsync_ShouldReturnBitmapFromBitmapData()
        {
            var dataObject = A.Fake<IDataObject>();
            var expected = new BitmapImage();

            A.CallTo(() => dataObject.GetDataPresent(DataFormats.Bitmap)).Returns(true);
            A.CallTo(() => dataObject.GetData(DataFormats.Bitmap)).Returns(expected);

            var result = await _extractor.GetWebBitmapAsync(dataObject);

            result.Should().BeSameAs(expected);
            A.CallTo(() => _bitmapService.DownloadAndConvert(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task GetWebBitmapAsync_ShouldDownloadBitmapFromHtmlImageTag()
        {
            var dataObject = A.Fake<IDataObject>();
            var expected = new BitmapImage();
            const string imageUrl = "https://example.com/image.png";
            const string html = "<html><body><img src=\"https://example.com/image.png\" /></body></html>";

            A.CallTo(() => dataObject.GetDataPresent(DataFormats.Bitmap)).Returns(false);
            A.CallTo(() => dataObject.GetDataPresent(DataFormats.Html)).Returns(true);
            A.CallTo(() => dataObject.GetData(DataFormats.Html)).Returns(html);
            A.CallTo(() => _bitmapService.DownloadAndConvert(imageUrl, A<CancellationToken>._)).Returns(expected);

            var result = await _extractor.GetWebBitmapAsync(dataObject);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetWebBitmapAsync_ShouldDownloadBitmapFromTextUrl()
        {
            var dataObject = A.Fake<IDataObject>();
            var expected = new BitmapImage();
            const string imageUrl = "https://example.com/image.png";

            A.CallTo(() => dataObject.GetDataPresent(DataFormats.Bitmap)).Returns(false);
            A.CallTo(() => dataObject.GetDataPresent(DataFormats.Html)).Returns(false);
            A.CallTo(() => dataObject.GetDataPresent(DataFormats.Text)).Returns(true);
            A.CallTo(() => dataObject.GetData(DataFormats.Text)).Returns(imageUrl);
            A.CallTo(() => _bitmapService.DownloadAndConvert(imageUrl, A<CancellationToken>._)).Returns(expected);

            var result = await _extractor.GetWebBitmapAsync(dataObject);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public void GetLocalFileUrl_ShouldTrimWrappingQuotes()
        {
            var dataObject = A.Fake<IDataObject>();

            A.CallTo(() => dataObject.GetDataPresent(DataFormats.StringFormat)).Returns(true);
            A.CallTo(() => dataObject.GetData(DataFormats.StringFormat)).Returns("\"C:\\images\\a.png\"");

            var result = _extractor.GetLocalFileUrl(dataObject);

            result.Should().Be("C:\\images\\a.png");
        }
    }
}
