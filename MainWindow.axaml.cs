using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
    private const short ShiftKey = 0x10;
    private readonly DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private SettingsWindow? _settingsWindow;
    private NoteWindow? _noteWindow;

    public MainWindow() : this(ConfigStore.ResolvePath(Array.Empty<string>())) { }

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
        Opened += (_, _) =>
        {
            RestoreGeometry();
            if (ShouldStartMinimized())
                Dispatcher.UIThread.Post(() => WindowState = WindowState.Minimized);
        };
        Closing += (_, _) =>
        {
            _clockTimer.Stop();
            _noteWindow?.Close();
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

    public void OpenNoteFromHotkey()
    {
        var wasMinimized = WindowState == WindowState.Minimized;
        if (!wasMinimized)
            ActivateFromSecondInstance();
        OpenNoteWindow(false);
        if (wasMinimized)
            WindowState = WindowState.Minimized;
    }

    private void UpdateClock()
    {
        if (this.FindControl<TextBlock>("ClockText") is { } text)
            text.Text = DateTime.Now.ToString("HH:mm:ss");
    }

    private bool ShouldStartMinimized()
    {
        var setting = SettingsStore.Load(_configPath).FirstOrDefault(s => s.Name == "StartMinimized");
        return setting is null || !bool.TryParse(setting.Value, out var startMinimized) || startMinimized;
    }

    private async void SettingsButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(
            Path.Combine(Path.GetDirectoryName(_configPath)!, "settings-window.json"),
            SettingsStore.GetPath(_configPath))
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        await _settingsWindow.ShowDialog(this);
        _settingsWindow = null;
    }

    private void NoteButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => OpenNoteWindow();

    public void OpenNoteWindow(bool useMainWindowAsOwner = true)
    {
        if (_noteWindow is not null)
        {
            _noteWindow.Activate();
            return;
        }

        var settings = SettingsStore.Load(_configPath);
        var notesPath = settings.FirstOrDefault(s => s.Name == "PathToNotes")?.Value
            ?? SettingsStore.Load(_configPath).First(s => s.Name == "PathToNotes").DefaultValue;
        _noteWindow = new NoteWindow(
            notesPath,
            Path.Combine(Path.GetDirectoryName(_configPath)!, "note-window.json"))
        { WindowStartupLocation = WindowStartupLocation.CenterOwner };
        _noteWindow.Closed += (_, _) => _noteWindow = null;
        if (useMainWindowAsOwner)
            _noteWindow.Show(this);
        else
            _noteWindow.Show();
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
            var width = Math.Clamp(state.Width, MinWidth, 3000);
            var height = Math.Clamp(state.Height, MinHeight, 2000);
            var restoredBounds = new PixelRect(
                state.X,
                state.Y,
                (int)Math.Ceiling(width),
                (int)Math.Ceiling(height));

            if (!Screens.All.Any(screen => Contains(screen.WorkingArea, restoredBounds)))
            {
                var message = "Saved window geometry is outside display bounds; using default startup geometry.";
                AppEventLog.WriteGeometryIssue(_configPath, message);
                SetStatus(message);
                return;
            }

            Width = width;
            Height = height;
            Position = new PixelPoint(state.X, state.Y);
        }
        catch { }
    }

    private static bool Contains(PixelRect display, PixelRect window) =>
        window.X >= display.X &&
        window.Y >= display.Y &&
        window.Right <= display.Right &&
        window.Bottom <= display.Bottom;

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
        if (OperatingSystem.IsWindows())
            return (GetAsyncKeyState(ShiftKey) & 0x8000) != 0;

        if (OperatingSystem.IsMacOS())
            return (CGEventSourceFlagsState(0, ShiftModifierFlag) & ShiftModifierFlag) != 0;

        return false;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern ulong CGEventSourceFlagsState(uint sourceState, ulong flags);
}
