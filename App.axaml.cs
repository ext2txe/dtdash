using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Dtdash;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            var configPath = ConfigStore.ResolvePath(desktop.Args ?? Array.Empty<string>());
            AppEventLog.WriteStartup(configPath);
            desktop.ShutdownRequested += (_, _) => AppEventLog.WriteShutdown(configPath);
            desktop.MainWindow = new MainWindow(configPath);
            AppInstance.StartActivationListener(() =>
                Avalonia.Threading.Dispatcher.UIThread.Post(() => ((MainWindow)desktop.MainWindow).ActivateFromSecondInstance()));
            desktop.Exit += (_, _) => AppInstance.StopActivationListener();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
