using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Allusion.Input;
using Allusion.ViewModels;
using Allusion.Views;
using Allusion.WPFCore;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using Autofac;
using Autofac.Core;
using Allusion.ViewModels.Dialogs;
using Allusion.WPFCore.Managers;
using Allusion.WPFCore.Service;

namespace Allusion;

public class Bootstrapper : BootstrapperBase
{
    private SimpleContainer _container;

    public Bootstrapper()
    {
        Initialize();
    }

    protected override void Configure()
    {
        CreateKeyMagic();
        ConfigMessageBinder();
        var config = AllusionConfiguration.Read();
        _container = new SimpleContainer();

        _container.Singleton<IWindowManager, WindowManager>();
        _container.Singleton<IEventAggregator, EventAggregator>();
        _container.PerRequest<IBitmapService, BitmapService>();
        _container.Singleton<IClipboardService, ClipboardService>();

        _container.Singleton<IReferenceBoardManager, ReferenceBoardManager>();
        _container.Singleton<IPageManager, PageManager>();
        _container.RegisterInstance(typeof(AllusionConfiguration),"Config",config);
        _container.PerRequest<MainViewModel>();
        _container.PerRequest<OpenRefBoardViewModel>();
        _container.PerRequest<NewRefBoardViewModel>();
        _container.PerRequest<DialogViewModel>();

    }

    protected override object GetInstance(Type service, string key)
    {
        return _container.GetInstance(service, key);
    }

    protected override IEnumerable<object> GetAllInstances(Type service)
    {
        return _container.GetAllInstances(service);
    }

    protected override void BuildUp(object instance)
    {
        _container.BuildUp(instance);
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