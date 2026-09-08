using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace Dtdash;

public partial class SettingsWindow : Window
{
    private readonly string _geometryPath;
    private readonly string _settingsPath;
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(string geometryPath, string settingsPath)
    {
        _geometryPath = geometryPath;
        _settingsPath = settingsPath;
        _viewModel = new SettingsViewModel(SettingsStore.Load(settingsPath));
        InitializeComponent();
        DataContext = _viewModel;
        Opened += (_, _) => RestoreGeometry();
        Closing += (_, _) =>
        {
            SaveSettings();
            SaveGeometry();
        };
    }

    private void SaveButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SaveSettings();

    private void SaveSettings() => SettingsStore.Save(_settingsPath, _viewModel.Settings);

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

    private sealed class SettingsViewModel
    {
        public List<SettingsStore.SettingEntry> Settings { get; }

        public SettingsViewModel(List<SettingsStore.SettingEntry> settings) => Settings = settings;
    }
}
