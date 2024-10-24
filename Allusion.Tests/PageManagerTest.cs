using Allusion.WPFCore.Board;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Managers;
using Caliburn.Micro;
using FakeItEasy;
using FluentAssertions;

namespace Allusion.Tests
{
    public class PageManagerTests
    {
        private PageManager _pageManager;
        private IEventAggregator _fakeEventAggregator;
        private IClipboardService _fakeClipboardService;
        private BoardPage _boardPage;
        private ReferenceBoard _parentBoard;

        public PageManagerTests()
        {
            _fakeEventAggregator = A.Fake<IEventAggregator>();
            _fakeClipboardService = A.Fake<IClipboardService>();
            _pageManager = new PageManager(_fakeEventAggregator, _fakeClipboardService);

            // Mock the parent ReferenceBoard
            _parentBoard = new ReferenceBoard("TestBoard", "C:\\TestBoardFolder");


            // Set up a dummy BoardPage
            _boardPage = new BoardPage(_parentBoard)
            {
                Name = "TestPage",
                PageFolder = "C:\\TestBoardFolder\\TestPage"
            };
        }

        [Fact]
        public void CleanPage_ShouldRemoveItemsWithNonExistingPaths()
        {
            // Arrange
            var imageItem1 = new ImageItem(0,0,1) { ItemPath = "C:\\TestBoardFolder\\TestPage\\image1.png" };
            var imageItem2 = new ImageItem(0,0,1) { ItemPath = "C:\\TestBoardFolder\\TestPage\\image2.png" };

            _boardPage.ImageItems.Add(imageItem1);
            _boardPage.ImageItems.Add(imageItem2);

            // Simulate that image1 exists, but image2 does not
            File.Create(imageItem1.ItemPath).Dispose();
            File.Exists(imageItem1.ItemPath).Should().BeTrue();
            File.Exists(imageItem2.ItemPath).Should().BeFalse();

            // Act
            _pageManager.CleanPage(_boardPage);

            // Assert
            _boardPage.ImageItems.Should().Contain(imageItem1);
            _boardPage.ImageItems.Should().NotContain(imageItem2);
        }

        [Fact]
        public void AddImage_ShouldAddImageToPageAndSetItemPath()
        {
            // Arrange
            var imageItem = new ImageItem(0,0,1);
            string expectedPath = Path.Combine(_boardPage.PageFolder, "randomFileName.png");

            // Act
            _pageManager.AddImage(imageItem, _boardPage);

            // Assert
            _boardPage.ImageItems.Should().Contain(imageItem);
            imageItem.MemberOfPage.Should().Be(_boardPage.BoardId);
            imageItem.ItemPath.Should().StartWith(_boardPage.PageFolder);
        }

        [Fact]
        public void RemoveImage_ShouldRemoveImageFromPage()
        {
            // Arrange
            var imageItem = new ImageItem(0,0,1);
            _boardPage.ImageItems.Add(imageItem);

            // Act
            _pageManager.RemoveImage(imageItem, _boardPage);

            // Assert
            _boardPage.ImageItems.Should().NotContain(imageItem);
        }

        [Fact]
        public void RenamePage_ShouldChangePageNameAndFolder()
        {
            // Arrange
            string oldName = "TestPage";
            string newName = "RenamedPage";
            _boardPage.Name = oldName;
            _boardPage.PageFolder = "C:\\TestBoardFolder\\TestPage";
            _boardPage.BackupFolder = "C:\\TestBoardFolder\\TestPage\\old";

            var imageItem = new ImageItem(0, 0, 1) { ItemPath = "C:\\TestBoardFolder\\TestPage\\image.png" };
            _boardPage.ImageItems.Add(imageItem);

            // Act
            _pageManager.RenamePage(_boardPage, newName);

            // Assert
            _boardPage.Name.Should().Be(newName);
            _boardPage.PageFolder.Should().Be("C:\\TestBoardFolder\\RenamedPage");
            _boardPage.BackupFolder.Should().Be("C:\\TestBoardFolder\\RenamedPage\\old");
            imageItem.ItemPath.Should().Be("C:\\TestBoardFolder\\RenamedPage\\image.png");
        }

        [Fact]
        public void OpenPageFolder_ShouldCreateDirectoryIfNotExists()
        {
            // Arrange
            string pageFolder = "C:\\TestBoardFolder\\TestPage";
            _boardPage.PageFolder = pageFolder;

            // Act
            _pageManager.OpenPageFolder(_boardPage);

            // Assert
            Directory.Exists(pageFolder).Should().BeTrue();
        }

        [Fact]
        public void OpenPageFolder_ShouldThrowExceptionIfDirectoryCannotBeOpened()
        {
            // Arrange
            string invalidPageFolder = "C:\\InvalidFolder";
            _boardPage.PageFolder = invalidPageFolder;

            // Act
            System.Action action = () => _pageManager.OpenPageFolder(_boardPage);

            // Assert
            action.Should().Throw<DirectoryNotFoundException>();
        }
    }
}
