using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service.Strategies;
using FakeItEasy;
using FluentAssertions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.Tests
{
    public class ClipboardStrategiesTests
    {
        [Fact]
        public void ImageDataStrategy_CanHandle_ShouldReflectBitmapPresence()
        {
            var dataObject = A.Fake<IDataObject>();
            var strategy = new ImageDataStrategy();
            A.CallTo(() => dataObject.GetDataPresent(DataFormats.Bitmap)).Returns(true);

            var result = strategy.CanHandle(dataObject);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ImageDataStrategy_ExtractBitmapsAsync_ShouldReturnBitmapWhenPresent()
        {
            var dataObject = A.Fake<IDataObject>();
            var expected = new BitmapImage();
            var strategy = new ImageDataStrategy();

            A.CallTo(() => dataObject.GetData(DataFormats.Bitmap)).Returns(expected);

            var result = await strategy.ExtractBitmapsAsync(dataObject);

            result.Should().ContainSingle().Which.Should().BeSameAs(expected);
        }

        [Fact]
        public void FileDropStrategy_CanHandle_ShouldReflectFileDropPresence()
        {
            var dataObject = A.Fake<IDataObject>();
            var bitmapService = A.Fake<IBitmapService>();
            var strategy = new FileDropStrategy(bitmapService);
            A.CallTo(() => dataObject.GetDataPresent(DataFormats.FileDrop)).Returns(true);

            var result = strategy.CanHandle(dataObject);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task FileDropStrategy_ExtractBitmapsAsync_ShouldReturnOnlyExistingImageFiles()
        {
            var imagePath = CreateTempFile(".png");
            var textPath = CreateTempFile(".txt");
            var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            var dataObject = A.Fake<IDataObject>();
            var bitmapService = A.Fake<IBitmapService>();
            var expected = new BitmapImage();
            var strategy = new FileDropStrategy(bitmapService);

            try
            {
                A.CallTo(() => dataObject.GetData(DataFormats.FileDrop, true)).Returns(new[] { imagePath, textPath, missingPath });
                A.CallTo(() => bitmapService.GetFromUri(imagePath)).Returns(expected);

                var result = await strategy.ExtractBitmapsAsync(dataObject);

                result.Should().ContainSingle().Which.Should().BeSameAs(expected);
                A.CallTo(() => bitmapService.GetFromUri(imagePath)).MustHaveHappenedOnceExactly();
                A.CallTo(() => bitmapService.GetFromUri(textPath)).MustNotHaveHappened();
                A.CallTo(() => bitmapService.GetFromUri(missingPath)).MustNotHaveHappened();
            }
            finally
            {
                File.Delete(imagePath);
                File.Delete(textPath);
            }
        }

        [Fact]
        public async Task FileDropStrategy_ExtractBitmapsAsync_ShouldReturnEmptyWhenFileArrayIsMissing()
        {
            IDataObject dataObject = new DataObject();
            var bitmapService = A.Fake<IBitmapService>();
            var strategy = new FileDropStrategy(bitmapService);

            var result = await strategy.ExtractBitmapsAsync(dataObject);

            result.Should().BeEmpty();
        }

        private static string CreateTempFile(string extension)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
            File.WriteAllText(path, "test");
            return path;
        }
    }
}
