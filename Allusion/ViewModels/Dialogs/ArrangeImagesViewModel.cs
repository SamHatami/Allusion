using Allusion.ViewModels.Arrangement;
using Caliburn.Micro;

namespace Allusion.ViewModels.Dialogs;

public enum ArrangeScope
{
    AllImages,
    SelectedImages
}

public class ArrangeImagesViewModel : Screen
{
    private static double _lastMargin = 24;
    private static ArrangeScaleMode _lastScaleMode = ArrangeScaleMode.KeepCurrent;
    private static int _lastColumns;

    private double _margin = _lastMargin;
    private ArrangeScaleMode _selectedScaleMode = _lastScaleMode;
    private ArrangeScope _selectedScope;
    private int _selectedColumns = _lastColumns;

    public string Title => "Arrange Images";

    public IReadOnlyList<ArrangeScaleMode> ScaleModes { get; } =
    [
        ArrangeScaleMode.KeepCurrent,
        ArrangeScaleMode.AverageHeight,
        ArrangeScaleMode.SmallestHeight
    ];

    public IReadOnlyList<ArrangeScope> Scopes { get; }

    public IReadOnlyList<int> ColumnOptions { get; } = [0, 1, 2, 3, 4, 5, 6];

    public int SelectedColumns
    {
        get => _selectedColumns;
        set
        {
            var clamped = Math.Clamp(value, 0, 6);
            if (_selectedColumns == clamped) return;

            _selectedColumns = clamped;
            NotifyOfPropertyChange(nameof(SelectedColumns));
        }
    }

    public double Margin
    {
        get => _margin;
        set
        {
            if (Math.Abs(_margin - value) < 0.001) return;

            _margin = Math.Max(0, value);
            NotifyOfPropertyChange(nameof(Margin));
        }
    }

    public ArrangeScaleMode SelectedScaleMode
    {
        get => _selectedScaleMode;
        set
        {
            if (_selectedScaleMode == value) return;

            _selectedScaleMode = value;
            NotifyOfPropertyChange(nameof(SelectedScaleMode));
        }
    }

    public ArrangeScope SelectedScope
    {
        get => _selectedScope;
        set
        {
            if (_selectedScope == value) return;

            _selectedScope = value;
            NotifyOfPropertyChange(nameof(SelectedScope));
        }
    }

    public ArrangeImagesViewModel(bool hasSelection = false)
    {
        Scopes = hasSelection
            ? [ArrangeScope.SelectedImages, ArrangeScope.AllImages]
            : [ArrangeScope.AllImages];
        SelectedScope = Scopes[0];
    }

    public ArrangeImageLayoutOptions CreateOptions()
    {
        _lastMargin = Margin;
        _lastScaleMode = SelectedScaleMode;
        _lastColumns = SelectedColumns;
        return new ArrangeImageLayoutOptions
        {
            Margin = Margin,
            ScaleMode = SelectedScaleMode,
            Columns = SelectedColumns
        };
    }

    public Task Apply() => TryCloseAsync(true);

    public Task Cancel() => TryCloseAsync(false);
}
