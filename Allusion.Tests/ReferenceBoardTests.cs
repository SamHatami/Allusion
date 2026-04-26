using Allusion.WPFCore.Board;
using Allusion.WPFCore.Managers;
using Allusion.WPFCore;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using FakeItEasy;
using FluentAssertions;
using System.Text.Json;

namespace Allusion.Tests
{
    public class ReferenceBoardTests
    {
        private readonly string _testDirectory;

        public ReferenceBoardTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "AllusionTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        [Fact]
        public void Constructor_ShouldInitializeBoardWithDefaultPage()
        {
            string boardName = "TestBoard";
            string baseFolder = Path.Combine(_testDirectory, boardName);

            var board = new ReferenceBoard(boardName, baseFolder);

            board.Name.Should().Be(boardName);
            board.BaseFolder.Should().Be(baseFolder);
            board.BackupFolder.Should().Be(Path.Combine(baseFolder, "Old"));
            board.Pages.Should().HaveCount(1);
            Directory.Exists(baseFolder).Should().BeTrue();
            Directory.Exists(board.BackupFolder).Should().BeTrue();
        }

        [Fact]
        public void Read_ShouldReturnNullForNonExistentFile()
        {
            string nonExistentFile = Path.Combine(_testDirectory, "nonexistent.allusion");

            var result = ReferenceBoard.Read(nonExistentFile);

            result.Should().BeNull();
        }

        [Fact]
        public void Read_ShouldDeserializeValidJsonFile()
        {
            var board = new ReferenceBoard("TestBoard", Path.Combine(_testDirectory, "TestBoard"));
            string filePath = Path.Combine(board.BaseFolder, "TestBoard.allusion");

            ReferenceBoard.Save(board);

            var loadedBoard = ReferenceBoard.Read(filePath);

            loadedBoard.Should().NotBeNull();
            loadedBoard!.Name.Should().Be(board.Name);
            loadedBoard.BaseFolder.Should().Be(board.BaseFolder);
            loadedBoard.Pages.Should().HaveCount(board.Pages.Count);
        }

        [Fact]
        public void Read_ShouldDeserializeBoardWithImageItems()
        {
            string boardFolder = Path.Combine(_testDirectory, "TestBoard");
            Directory.CreateDirectory(boardFolder);
            string filePath = Path.Combine(boardFolder, "TestBoard.allusion");
            string imagePath = Path.Combine(boardFolder, "image.png");
            var json = $$"""
                         {
                           "Name": "TestBoard",
                           "BaseFolder": "{{boardFolder.Replace("\\", "\\\\")}}",
                           "BackupFolder": "{{Path.Combine(boardFolder, "Old").Replace("\\", "\\\\")}}",
                           "Pages": [
                             {
                               "Name": "Page 1",
                               "PageFolder": "{{boardFolder.Replace("\\", "\\\\")}}",
                               "BackupFolder": "{{Path.Combine(boardFolder, "old").Replace("\\", "\\\\")}}",
                               "Description": "",
                               "ImageItems": [
                                 {
                                   "ItemPath": "{{imagePath.Replace("\\", "\\\\")}}",
                                   "PosX": 12.0,
                                   "PosY": 34.0,
                                   "Scale": 0.5,
                                   "Description": "reference",
                                   "MemberOfPage": "00000000-0000-0000-0000-000000000000"
                                 }
                               ],
                               "NoteItems": [],
                               "BoardId": "00000000-0000-0000-0000-000000000000"
                             }
                           ]
                         }
                         """;
            File.WriteAllText(filePath, json);

            var loadedBoard = ReferenceBoard.Read(filePath);

            loadedBoard.Should().NotBeNull();
            var image = loadedBoard!.Pages.Single().ImageItems.Single();
            image.PosX.Should().Be(12.0);
            image.PosY.Should().Be(34.0);
            image.Scale.Should().Be(0.5);
            image.Description.Should().Be("reference");
            image.ItemPath.Should().Be(imagePath);
        }

        [Fact]
        public void Read_ShouldReturnNullForInvalidJson()
        {
            string invalidFilePath = Path.Combine(_testDirectory, "invalid.allusion");
            File.WriteAllText(invalidFilePath, "{ invalid json");

            var result = ReferenceBoard.Read(invalidFilePath);

            result.Should().BeNull();
        }

        [Fact]
        public void Save_ShouldCreateDirectoriesAndSaveFile()
        {
            var board = new ReferenceBoard("TestBoard", Path.Combine(_testDirectory, "TestBoard"));
            string expectedFilePath = Path.Combine(board.BaseFolder, "TestBoard.allusion");

            var result = ReferenceBoard.Save(board);

            result.Should().BeTrue();
            File.Exists(expectedFilePath).Should().BeTrue();
            Directory.Exists(board.BaseFolder).Should().BeTrue();
            Directory.Exists(board.BackupFolder).Should().BeTrue();
        }

        [Fact]
        public void Save_ShouldMoveRemovedImagesToBackupFolder()
        {
            var board = new ReferenceBoard("TestBoard", Path.Combine(_testDirectory, "TestBoard"));
            var page = board.Pages.Single();
            string removedImagePath = Path.Combine(page.PageFolder, "removed.png");
            File.WriteAllText(removedImagePath, "orphan-image");

            var result = ReferenceBoard.Save(board);

            result.Should().BeTrue();
            File.Exists(removedImagePath).Should().BeFalse();
            Directory.GetFiles(page.BackupFolder, "*removed.png").Should().ContainSingle();
        }

        [Fact]
        public void Save_ShouldSerializeBoardToJson()
        {
            var board = new ReferenceBoard("TestBoard", Path.Combine(_testDirectory, "TestBoard"));
            string filePath = Path.Combine(board.BaseFolder, "TestBoard.allusion");

            ReferenceBoard.Save(board);

            File.Exists(filePath).Should().BeTrue();
            var jsonContent = File.ReadAllText(filePath);
            jsonContent.Should().Contain("TestBoard");
            jsonContent.Should().Contain("Pages");

            var deserialized = JsonSerializer.Deserialize<ReferenceBoard>(jsonContent);
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be(board.Name);
        }

        [Fact]
        public void RenameBoard_ShouldMoveFolderAndUpdateBoardFile()
        {
            var manager = CreateReferenceBoardManager();
            var board = manager.CreateNew("OldBoard");
            var oldFolder = board.BaseFolder;
            var oldBoardFile = Path.Combine(oldFolder, "OldBoard.allusion");

            manager.RenameBoard(board, "NewBoard");

            var newFolder = Path.Combine(_testDirectory, "NewBoard");
            Directory.Exists(oldFolder).Should().BeFalse();
            Directory.Exists(newFolder).Should().BeTrue();
            File.Exists(oldBoardFile).Should().BeFalse();
            File.Exists(Path.Combine(newFolder, "NewBoard.allusion")).Should().BeTrue();
            board.Name.Should().Be("NewBoard");
            board.BaseFolder.Should().Be(newFolder);
            board.BackupFolder.Should().Be(Path.Combine(newFolder, "Old"));

            var loadedBoard = ReferenceBoard.Read(Path.Combine(newFolder, "NewBoard.allusion"));
            loadedBoard.Should().NotBeNull();
            loadedBoard!.Name.Should().Be("NewBoard");
            loadedBoard.BaseFolder.Should().Be(newFolder);
        }

        [Fact]
        public void RenameBoard_ShouldRejectExistingBoardNameWithoutMovingOriginal()
        {
            var manager = CreateReferenceBoardManager();
            var board = manager.CreateNew("OldBoard");
            manager.CreateNew("ExistingBoard");

            var act = () => manager.RenameBoard(board, "ExistingBoard");

            act.Should().Throw<IOException>();
            Directory.Exists(Path.Combine(_testDirectory, "OldBoard")).Should().BeTrue();
            Directory.Exists(Path.Combine(_testDirectory, "ExistingBoard")).Should().BeTrue();
            board.Name.Should().Be("OldBoard");
        }

        [Fact]
        public void RemoveFromBoardList_ShouldIgnoreBoardWhenLocalFilesAreKept()
        {
            var manager = CreateReferenceBoardManager();
            manager.CreateNew("VisibleBoard");
            manager.CreateNew("IgnoredBoard");
            var ignoredBoard = manager.GetAllRefBoardInfos().Single(board => board.Name == "IgnoredBoard");

            manager.RemoveFromBoardList(ignoredBoard, deleteLocalFiles: false);

            manager.CurrentConfiguration.IgnoredRefBoardFiles.Should().Contain(Path.GetFullPath(ignoredBoard.FilePath));
            manager.GetAllRefBoardInfos().Select(board => board.Name).Should().ContainSingle().Which.Should().Be("VisibleBoard");
            Directory.Exists(ignoredBoard.ImageFolder).Should().BeTrue();
        }

        [Fact]
        public void RemoveFromBoardList_ShouldDeleteBoardFolderWhenRequested()
        {
            var manager = CreateReferenceBoardManager();
            manager.CreateNew("DeletedBoard");
            var deletedBoard = manager.GetAllRefBoardInfos().Single();

            manager.RemoveFromBoardList(deletedBoard, deleteLocalFiles: true);

            Directory.Exists(deletedBoard.ImageFolder).Should().BeFalse();
            Directory.Exists(_testDirectory).Should().BeTrue();
            manager.GetAllRefBoardInfos().Should().BeEmpty();
        }

        private ReferenceBoardManager CreateReferenceBoardManager()
        {
            var configuration = new AllusionConfiguration { GlobalFolder = _testDirectory };
            return new ReferenceBoardManager(
                A.Fake<IEventAggregator>(),
                configuration,
                A.Fake<IBitmapService>());
        }
    }
}
