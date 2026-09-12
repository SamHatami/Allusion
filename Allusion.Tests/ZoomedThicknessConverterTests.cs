using System.Globalization;
using System.Windows;
using Allusion.WPFCore.Converter;
using FluentAssertions;

namespace Allusion.Tests;

public class ZoomedThicknessConverterTests
{
    private readonly ZoomedThicknessConverter _converter = new();

    [Theory]
    [InlineData(1.0, 4.0, 4.0)]
    [InlineData(2.0, 4.0, 2.0)]
    [InlineData(0.5, 4.0, 8.0)]
    [InlineData(1.0, 0.0, 0.0)]
    public void Convert_ShouldDivideThicknessByZoom(double zoom, double thickness, double expected)
    {
        var result = (Thickness)_converter.Convert([zoom, thickness], typeof(Thickness), null, CultureInfo.InvariantCulture);

        result.Left.Should().BeApproximately(expected, 0.0001);
    }

    [Theory]
    [InlineData(0.0, 4.0, 4.0)]
    [InlineData(-2.0, 4.0, 4.0)]
    [InlineData(1.0, -4.0, 0.0)]
    public void Convert_ShouldGuardInvalidInput(double zoom, double thickness, double expected)
    {
        var result = (Thickness)_converter.Convert([zoom, thickness], typeof(Thickness), null, CultureInfo.InvariantCulture);

        result.Left.Should().BeApproximately(expected, 0.0001);
    }
}
