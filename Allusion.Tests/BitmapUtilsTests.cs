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
        public void CreateFromBytes_NotSupportedForEmptyByteArray()
        {
            // Arrange
            var emptyBytes = Array.Empty<byte>();

            // Act

            // Assert
            Assert.Throws<NotSupportedException>(() => BitmapUtils.CreateFromBytes(emptyBytes));
        }

        [Fact]
        public void CreateFromBytes_NotSupportedForNull()
        {
            // Arrange

            // Act

            // Assert
            Assert.Throws<NotSupportedException>(() => BitmapUtils.CreateFromBytes(null));
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