using Avalonia;
using Dtdash;

if (AppInstance.TryAcquire())
{
    try
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    finally
    {
        AppInstance.Release();
    }
}
else
{
    AppInstance.SignalExistingInstance();
}

static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .LogToTrace();
