using Caliburn.Micro;

namespace Allusion.ViewModels;

public class HelpViewModel : Conductor<object>
{
    public HelpViewModel()
    {
        Topics.Add(new HelpTopicsViewModel());
        Topics.Add(new ReleaseNotesViewModel());
        SelectedTopic = Topics[0];
    }

    public BindableCollection<Screen> Topics { get; } = [];

    private Screen? _selectedTopic;

    public Screen? SelectedTopic
    {
        get => _selectedTopic;
        set
        {
            if (Equals(_selectedTopic, value)) return;

            _selectedTopic = value;
            NotifyOfPropertyChange();
            if (value is not null)
                _ = ActivateItemAsync(value);
        }
    }

    public void Close()
    {
        TryCloseAsync();
    }
}
