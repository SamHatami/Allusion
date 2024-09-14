using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Allusion.Input;
using Allusion.ViewModels;
using Allusion.Views;
using Allusion.WPFCore.Handlers;
using Caliburn.Micro;

namespace Allusion;

public class Bootstrapper : BootstrapperBase
{
    private SimpleContainer container;

    public Bootstrapper()
    {
        Initialize();
    }

    protected override void Configure()
    {
        CreateKeyMagic();
        ConfigMessageBinder();

        container = new SimpleContainer();

        container.Singleton<IWindowManager, WindowManager>();
        container.Singleton<IEventAggregator, EventAggregator>();
        container.PerRequest<ArtBoardHandler>();
        container.PerRequest<MainViewModel>();
        container.PerRequest<OpenArtBoardViewModel>();
        container.PerRequest<NewBoardViewModel>();
    }

    protected override object GetInstance(Type service, string key)
    {
        return container.GetInstance(service, key);
    }

    protected override IEnumerable<object> GetAllInstances(Type service)
    {
        return container.GetAllInstances(service);
    }

    protected override void BuildUp(object instance)
    {
        container.BuildUp(instance);
    }

    protected override IEnumerable<Assembly> SelectAssemblies()
    {
        return new[] { Assembly.GetExecutingAssembly() };
    }

    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        DisplayRootViewForAsync<MainViewModel>();
    }

    private void CreateKeyMagic()
    {
        //Taken from caliburn samples: Input folder
        //https://github.com/Caliburn-Micro/Caliburn.Micro/tree/master/samples/scenarios/Scenario.KeyBinding
        var defaultCreateTrigger = Parser.CreateTrigger;

        Parser.CreateTrigger = (target, triggerText) =>
        {
            if (triggerText == null) return defaultCreateTrigger(target, null);

            var triggerDetail = triggerText
                .Replace("[", string.Empty)
                .Replace("]", string.Empty);

            var splits = triggerDetail.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            switch (splits[0])
            {
                case "Key":
                    var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
                    return new KeyTrigger { Key = key };

                case "Gesture":
                    var mkg = (MultiKeyGesture)new MultiKeyGestureConverter().ConvertFrom(splits[1]);
                    return new KeyTrigger
                        { Modifiers = mkg.KeySequences[0].Modifiers, Key = mkg.KeySequences[0].Keys[0] };
            }

            return defaultCreateTrigger(target, triggerText);
        };
    }

    private void ConfigMessageBinder()
    {
        //Not sure if this works unless the mouse is clicked?
        //https://stackoverflow.com/questions/12951648/caliburn-micro-capture-mouse-position
        MessageBinder.SpecialValues.Add("$mousepoint", ctx =>
        {
            var e = ctx.EventArgs as MouseEventArgs;
            if (e == null)
                return null;

            return e.GetPosition(ctx.Source);
        });
    }
}