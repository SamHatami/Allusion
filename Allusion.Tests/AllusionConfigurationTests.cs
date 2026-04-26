using Allusion.WPFCore;
using FluentAssertions;

namespace Allusion.Tests;

[Collection(AllusionConfigurationCollection.Name)]
public class AllusionConfigurationTests
{
    [Fact]
    public void ConfigPath_ShouldUseLocalApplicationDataByDefault()
    {
        var expectedDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Allusion");

        AllusionConfiguration.DataFolder.Should().Be(expectedDataFolder);
        AllusionConfiguration.ConfigPath.Should().Be(Path.Combine(expectedDataFolder, "AllusionConfiguration.json"));
    }

    [Fact]
    public void SaveAndRead_ShouldUseDataFolder()
    {
        var testDataFolder = Path.Combine(Path.GetTempPath(), "AllusionTests", Guid.NewGuid().ToString());

        using (AllusionConfiguration.UseDataFolderForTests(testDataFolder))
        {
            var config = new AllusionConfiguration
            {
                FirstStartUp = false,
                TopMost = false,
                IgnoredRefBoardFiles = ["ignored.allusion"]
            };

            AllusionConfiguration.Save(config);
            var loaded = AllusionConfiguration.Read();

            File.Exists(Path.Combine(testDataFolder, "AllusionConfiguration.json")).Should().BeTrue();
            loaded.FirstStartUp.Should().BeFalse();
            loaded.TopMost.Should().BeFalse();
            loaded.IgnoredRefBoardFiles.Should().ContainSingle().Which.Should().Be("ignored.allusion");
        }
    }
}
