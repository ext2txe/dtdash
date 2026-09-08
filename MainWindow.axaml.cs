using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dtdash;

public partial class MainWindow : Window
{
    private readonly string _configPath;
    private readonly bool _skipGeometryRestore;
    private const string GeometryFile = "window.json";
    private const ulong ShiftModifierFlag = 0x00020000;
    private readonly DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private SettingsWindow? _settingsWindow;

    public MainWindow(string configPath)
    {
        _configPath = configPath;
        _skipGeometryRestore = IsShiftPressedAtStartup();
        InitializeComponent();
        Title = $"dtDash {typeof(MainWindow).Assembly.GetName().Version?.ToString(3) ?? "0.1.19"}";
        if (this.FindControl<TextBlock>("ConfigText") is { } text)
            text.Text = $"Configuration: {_configPath}";
        SetStatus("Ready");
        UpdateClock();
        _clockTimer.Tick += (_, _) => UpdateClock();
        _clockTimer.Start();
        Opened += (_, _) => RestoreGeometry();
        Closing += (_, _) =>
        {
            _clockTimer.Stop();
            _settingsWindow?.Close();
            SaveGeometry();
        };
    }

    public void SetStatus(string message)
    {
        if (this.FindControl<TextBlock>("StatusText") is { } text)
            text.Text = $"{DateTime.Now:HH:mm:ss} {message}";
    }

    public void ActivateFromSecondInstance()
    {
        WindowState = WindowState.Normal;
        Topmost = true;
        Activate();
        Topmost = false;
        Focus();
    }

    private void UpdateClock()
    {
        if (this.FindControl<TextBlock>("ClockText") is { } text)
            text.Text = DateTime.Now.ToString("HH:mm:ss");
    }

    private async void SettingsButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(Path.Combine(Path.GetDirectoryName(_configPath)!, "settings-window.json"))
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        await _settingsWindow.ShowDialog(this);
        _settingsWindow = null;
    }

    private void LogButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var logPath = AppEventLog.GetLogPath(_configPath);
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        using (File.Open(logPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)) { }
        Process.Start(new ProcessStartInfo
        {
            FileName = logPath,
            UseShellExecute = true
        });
    }

    private string StatePath => Path.Combine(Path.GetDirectoryName(_configPath)!, GeometryFile);

    private void RestoreGeometry()
    {
        if (_skipGeometryRestore) return;

        try
        {
            if (!File.Exists(StatePath)) return;
            var state = System.Text.Json.JsonSerializer.Deserialize<WindowGeometry>(File.ReadAllText(StatePath));
            if (state is null) return;
            Width = Math.Clamp(state.Width, MinWidth, 3000);
            Height = Math.Clamp(state.Height, MinHeight, 2000);
            Position = new PixelPoint(state.X, state.Y);
        }
        catch { }
    }

    private void SaveGeometry()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StatePath)!);
            var state = new WindowGeometry(Position.X, Position.Y, Width, Height);
            File.WriteAllText(StatePath, System.Text.Json.JsonSerializer.Serialize(state));
        }
        catch { }
    }

    private sealed record WindowGeometry(int X, int Y, double Width, double Height);

    private static bool IsShiftPressedAtStartup()
    {
        if (!OperatingSystem.IsMacOS()) return false;
        return (CGEventSourceFlagsState(0, ShiftModifierFlag) & ShiftModifierFlag) != 0;
    }

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern ulong CGEventSourceFlagsState(uint sourceState, ulong flags);
}
