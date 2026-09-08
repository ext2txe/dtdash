using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace Dtdash;

public partial class SettingsWindow : Window
{
    private readonly string _geometryPath;

    public SettingsWindow(string geometryPath)
    {
        _geometryPath = geometryPath;
        InitializeComponent();
        Opened += (_, _) => RestoreGeometry();
        Closing += (_, _) => SaveGeometry();
    }

    private void RestoreGeometry()
    {
        try
        {
            if (!File.Exists(_geometryPath)) return;
            var state = System.Text.Json.JsonSerializer.Deserialize<WindowGeometry>(File.ReadAllText(_geometryPath));
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
            Directory.CreateDirectory(Path.GetDirectoryName(_geometryPath)!);
            var state = new WindowGeometry(Position.X, Position.Y, Width, Height);
            File.WriteAllText(_geometryPath, System.Text.Json.JsonSerializer.Serialize(state));
        }
        catch { }
    }

    private sealed record WindowGeometry(int X, int Y, double Width, double Height);
}
