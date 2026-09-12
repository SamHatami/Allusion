using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class HelpViewModel : Conductor<object>
{
    public HelpViewModel(IUpdateService updateService, IUpdateInstaller installer)
    {
        Topics.Add(new HelpTopicsViewModel());
        Topics.Add(new ReleaseNotesViewModel());
        Topics.Add(new UpdateViewModel(updateService, installer));
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

    public void ShowUpdatesTopic()
    {
        var updates = Topics.OfType<UpdateViewModel>().FirstOrDefault();
        if (updates is not null)
            SelectedTopic = updates;
    }

    public void Close()
    {
        TryCloseAsync();
    }
}
