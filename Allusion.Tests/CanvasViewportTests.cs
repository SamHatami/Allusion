using System.Windows;
using Allusion.ViewModels;
using FluentAssertions;

namespace Allusion.Tests;

public class CanvasViewportTests
{
    [Fact]
    public void PanBy_ShouldMoveViewportOffset()
    {
        var viewport = new CanvasViewport();

        viewport.PanBy(new Vector(25, -10));

        viewport.OffsetX.Should().Be(25);
        viewport.OffsetY.Should().Be(-10);
    }

    [Fact]
    public void ZoomAt_ShouldPreserveWorldPointUnderCursor()
    {
        var viewport = new CanvasViewport();
        viewport.PanBy(new Vector(40, 20));
        var cursor = new Point(200, 150);
        var worldX = (cursor.X - viewport.OffsetX) / viewport.Zoom;
        var worldY = (cursor.Y - viewport.OffsetY) / viewport.Zoom;

        viewport.ZoomAt(cursor, 120);

        ((cursor.X - viewport.OffsetX) / viewport.Zoom).Should().BeApproximately(worldX, 0.0001);
        ((cursor.Y - viewport.OffsetY) / viewport.Zoom).Should().BeApproximately(worldY, 0.0001);
    }

    [Fact]
    public void ZoomAt_ShouldClampZoom()
    {
        var viewport = new CanvasViewport();

        for (var i = 0; i < 100; i++)
            viewport.ZoomAt(new Point(0, 0), 120);

        viewport.Zoom.Should().Be(CanvasViewport.MaxZoom);

        for (var i = 0; i < 100; i++)
            viewport.ZoomAt(new Point(0, 0), -120);

        viewport.Zoom.Should().Be(CanvasViewport.MinZoom);
    }

    [Fact]
    public void Reset_ShouldRestoreDefaultViewport()
    {
        var viewport = new CanvasViewport();
        viewport.PanBy(new Vector(50, 75));
        viewport.ZoomAt(new Point(100, 100), 120);

        viewport.Reset();

        viewport.Zoom.Should().Be(CanvasViewport.DefaultZoom);
        viewport.OffsetX.Should().Be(0);
        viewport.OffsetY.Should().Be(0);
    }
}
