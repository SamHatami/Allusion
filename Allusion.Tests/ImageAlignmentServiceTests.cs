using Allusion.ViewModels.Arrangement;
using FluentAssertions;

namespace Allusion.Tests;

public class ImageAlignmentServiceTests
{
    // Different sizes at different positions so every edge yields a distinct value.
    // Combined bounds: left 100, top 200, right 380, bottom 460.
    private static readonly ImageAlignItem[] Items =
    [
        new(100, 200, 40, 20),
        new(300, 400, 80, 60)
    ];

    [Fact]
    public void Align_Left_ShouldShareTheLeftMostEdge()
    {
        var result = ImageAlignmentService.Align(Items, AlignEdge.Left);

        result.Should().Equal(new ImageAlignResult(100, 200), new ImageAlignResult(100, 400));
    }

    [Fact]
    public void Align_Right_ShouldShareTheRightMostEdge()
    {
        var result = ImageAlignmentService.Align(Items, AlignEdge.Right);

        result.Should().Equal(new ImageAlignResult(340, 200), new ImageAlignResult(300, 400));
    }

    [Fact]
    public void Align_Top_ShouldShareTheTopMostEdge()
    {
        var result = ImageAlignmentService.Align(Items, AlignEdge.Top);

        result.Should().Equal(new ImageAlignResult(100, 200), new ImageAlignResult(300, 200));
    }

    [Fact]
    public void Align_Bottom_ShouldShareTheBottomMostEdge()
    {
        var result = ImageAlignmentService.Align(Items, AlignEdge.Bottom);

        result.Should().Equal(new ImageAlignResult(100, 440), new ImageAlignResult(300, 400));
    }

    [Fact]
    public void Align_HorizontalCenters_ShouldCenterOnTheCombinedBounds()
    {
        var result = ImageAlignmentService.Align(Items, AlignEdge.HorizontalCenters);

        // Center line is 240, so each X is 240 minus half its own width.
        result.Should().Equal(new ImageAlignResult(220, 200), new ImageAlignResult(200, 400));
    }

    [Fact]
    public void Align_VerticalCenters_ShouldCenterOnTheCombinedBounds()
    {
        var result = ImageAlignmentService.Align(Items, AlignEdge.VerticalCenters);

        // Center line is 330, so each Y is 330 minus half its own height.
        result.Should().Equal(new ImageAlignResult(100, 320), new ImageAlignResult(300, 300));
    }

    [Fact]
    public void Align_ShouldLeaveASingleItemWhereItIs()
    {
        var result = ImageAlignmentService.Align([Items[0]], AlignEdge.Right);

        result.Should().Equal(new ImageAlignResult(100, 200));
    }

    [Fact]
    public void Align_ShouldReturnOneResultPerItem()
    {
        foreach (var edge in Enum.GetValues<AlignEdge>())
            ImageAlignmentService.Align(Items, edge).Should().HaveCount(Items.Length);
    }
}
