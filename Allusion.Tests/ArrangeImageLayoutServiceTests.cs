using Allusion.ViewModels.Arrangement;
using FluentAssertions;

namespace Allusion.Tests;

public class ArrangeImageLayoutServiceTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(12, 0)]
    [InlineData(13, 25)]
    [InlineData(37, 25)]
    [InlineData(38, 50)]
    [InlineData(-12, 0)]
    [InlineData(-13, -25)]
    public void CanvasGridSnap_ShouldSnapToNearestGridLine(double value, double expected)
    {
        CanvasGridSnap.Snap(value).Should().Be(expected);
    }

    [Fact]
    public void Arrange_ShouldLayOutImagesInGridWithMargin()
    {
        var service = new ArrangeImageLayoutService();
        var items = new[]
        {
            new ArrangeImageLayoutItem(100, 50, 1),
            new ArrangeImageLayoutItem(80, 60, 1),
            new ArrangeImageLayoutItem(40, 40, 1)
        };

        var result = service.Arrange(items, new ArrangeImageLayoutOptions { Margin = 10 });

        result.Should().HaveCount(3);
        result[0].X.Should().Be(0);
        result[0].Y.Should().Be(0);
        result[1].X.Should().Be(110);
        result[1].Y.Should().Be(0);
        result[2].X.Should().Be(0);
        result[2].Y.Should().Be(70);
    }

    [Fact]
    public void Arrange_ShouldScaleImagesToAverageHeight()
    {
        var service = new ArrangeImageLayoutService();
        var items = new[]
        {
            new ArrangeImageLayoutItem(100, 50, 1),
            new ArrangeImageLayoutItem(100, 150, 1)
        };

        var result = service.Arrange(items, new ArrangeImageLayoutOptions
        {
            ScaleMode = ArrangeScaleMode.AverageHeight
        });

        result[0].Scale.Should().BeApproximately(2, 0.0001);
        result[1].Scale.Should().BeApproximately(0.6667, 0.0001);
    }

    [Fact]
    public void Arrange_ShouldScaleImagesToSmallestHeight()
    {
        var service = new ArrangeImageLayoutService();
        var items = new[]
        {
            new ArrangeImageLayoutItem(100, 50, 1),
            new ArrangeImageLayoutItem(100, 150, 1)
        };

        var result = service.Arrange(items, new ArrangeImageLayoutOptions
        {
            ScaleMode = ArrangeScaleMode.SmallestHeight
        });

        result[0].Scale.Should().Be(1);
        result[1].Scale.Should().BeApproximately(0.3333, 0.0001);
    }
}
