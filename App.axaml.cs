using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Dtdash;

public partial class App : Application
{
    private IDisposable? _globalHotkey;

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
            MacApplicationIcon.Set();
            Action openNote = () =>
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var mainWindow = (MainWindow)desktop.MainWindow;
                    mainWindow.OpenNoteFromHotkey();
                });
            desktop.MainWindow.Opened += (_, _) => _globalHotkey = GlobalHotkey.Register(openNote);
            AppInstance.StartActivationListener(() =>
                Avalonia.Threading.Dispatcher.UIThread.Post(() => ((MainWindow)desktop.MainWindow).ActivateFromSecondInstance()));
            desktop.Exit += (_, _) =>
            {
                _globalHotkey?.Dispose();
                AppInstance.StopActivationListener();
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
