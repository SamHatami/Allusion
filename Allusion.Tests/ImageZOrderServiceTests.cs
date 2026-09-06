using Allusion.ViewModels.Arrangement;
using FluentAssertions;

namespace Allusion.Tests;

public class ImageZOrderServiceTests
{
    // Bottom to top.
    private static readonly string[] BottomToTop = ["A", "B", "C", "D"];

    [Fact]
    public void Reorder_BringToFront_ShouldMoveSelectedAboveEverythingElse()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, ["B"], ZOrderOperation.BringToFront);

        result.Should().Equal("A", "C", "D", "B");
    }

    [Fact]
    public void Reorder_BringToFront_ShouldKeepTheRelativeOrderOfSeveralSelected()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, ["C", "A"], ZOrderOperation.BringToFront);

        result.Should().Equal("B", "D", "A", "C");
    }

    [Fact]
    public void Reorder_SendToBack_ShouldMoveSelectedBelowEverythingElse()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, ["C"], ZOrderOperation.SendToBack);

        result.Should().Equal("C", "A", "B", "D");
    }

    [Fact]
    public void Reorder_BringForward_ShouldStepEverySelectedOneSlotUp()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, ["B", "C"], ZOrderOperation.BringForward);

        result.Should().Equal("A", "D", "B", "C");
    }

    [Fact]
    public void Reorder_BringForward_ShouldLeaveAnItemAlreadyOnTopAlone()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, ["D"], ZOrderOperation.BringForward);

        result.Should().Equal(BottomToTop);
    }

    [Fact]
    public void Reorder_SendBackward_ShouldStepEverySelectedOneSlotDown()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, ["B", "C"], ZOrderOperation.SendBackward);

        result.Should().Equal("B", "C", "A", "D");
    }

    [Fact]
    public void Reorder_SendBackward_ShouldLeaveAnItemAlreadyAtBottomAlone()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, ["A"], ZOrderOperation.SendBackward);

        result.Should().Equal(BottomToTop);
    }

    [Fact]
    public void Reorder_ShouldReturnTheStackUnchangedWhenNothingIsSelected()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, Array.Empty<string>(), ZOrderOperation.BringToFront);

        result.Should().Equal(BottomToTop);
    }

    [Fact]
    public void Reorder_ShouldIgnoreASelectionThatIsNotInTheStack()
    {
        var result = ImageZOrderService.Reorder(BottomToTop, ["Z"], ZOrderOperation.SendToBack);

        result.Should().Equal(BottomToTop);
    }

    [Fact]
    public void Reorder_ShouldNeverLoseOrDuplicateAnItem()
    {
        foreach (var operation in Enum.GetValues<ZOrderOperation>())
        {
            var result = ImageZOrderService.Reorder(BottomToTop, ["B", "D"], operation);

            result.Should().BeEquivalentTo(BottomToTop);
        }
    }
}
