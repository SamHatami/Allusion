using System.Windows.Media.Imaging;
using Allusion.WPFCore.Utilities;
using FluentAssertions;

namespace Allusion.Tests
{
    public class BitmapUtilsTests
    {
        // Existing tests...

        [Fact]
        public void SaveToFile_ShouldThrowExceptionForNullBitmap()
        {
            // Arrange
            BitmapImage bitmap = null;
            var fileName = "test_image";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => BitmapUtils.SaveToFile(bitmap, fileName));
        }

        [Fact]
        public void LoadImageFromUri_ShouldThrowExceptionForInvalidUri()
        {
            // Arrange
            var invalidUri = "invalid_uri";

            // Act & Assert
            Assert.Throws<UriFormatException>(() => BitmapUtils.LoadImageFromUri(invalidUri));
        }

        [Fact]
        public void CreateFromBytes_ReturnsNullForEmptyByteArray()
        {
            // Arrange
            var emptyBytes = Array.Empty<byte>();

            // Act
            var result = BitmapUtils.CreateFromBytes(emptyBytes);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void CreateFromBytes_ReturnsNullForNull()
        {
            // Arrange

            // Act
            var result = BitmapUtils.CreateFromBytes(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetUrl_ShouldHandleNullBitmap()
        {
            // Arrange
            BitmapImage bitmap = null;

            // Act & Assert
            var url = BitmapUtils.GetUrl(bitmap);

            url.Should().BeEquivalentTo(string.Empty);
        }

   }
}