using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Runtime.InteropServices;

namespace Dtdash;

public partial class MainWindow : Window
{
    private readonly string _configPath;
    private readonly bool _skipGeometryRestore;
    private const string GeometryFile = "window.json";
    private const ulong ShiftModifierFlag = 0x00020000;

    public MainWindow(string configPath)
    {
        _configPath = configPath;
        _skipGeometryRestore = IsShiftPressedAtStartup();
        InitializeComponent();
        Title = $"DTDash {typeof(MainWindow).Assembly.GetName().Version?.ToString(3) ?? "0.1.1"}";
        if (this.FindControl<TextBlock>("ConfigText") is { } text)
            text.Text = $"Configuration: {_configPath}";
        Opened += (_, _) => RestoreGeometry();
        Closing += (_, _) => SaveGeometry();
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
