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
    private double _margin = 24;
    private ArrangeScaleMode _selectedScaleMode = ArrangeScaleMode.KeepCurrent;
    private ArrangeScope _selectedScope;

    public string Title => "Arrange Images";

    public IReadOnlyList<ArrangeScaleMode> ScaleModes { get; } =
    [
        ArrangeScaleMode.KeepCurrent,
        ArrangeScaleMode.AverageHeight,
        ArrangeScaleMode.SmallestHeight
    ];

    public IReadOnlyList<ArrangeScope> Scopes { get; }

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
        return new ArrangeImageLayoutOptions
        {
            Margin = Margin,
            ScaleMode = SelectedScaleMode
        };
    }

    public Task Apply()
    {
        return TryCloseAsync(true);
    }

    public Task Cancel()
    {
        return TryCloseAsync(false);
    }
}
