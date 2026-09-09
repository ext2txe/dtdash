using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace Dtdash;

public partial class NoteWindow : Window
{
    private readonly string _notesPath;
    private readonly string _geometryPath;

    public NoteWindow() : this(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "data", "notes.txt"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "note-window.json")) { }

    public NoteWindow(string notesPath, string geometryPath)
    {
        _notesPath = notesPath;
        _geometryPath = geometryPath;
        InitializeComponent();
        Opened += (_, _) =>
        {
            RestoreGeometry();
            this.FindControl<TextBox>("NoteText")?.Focus();
        };
        Closing += (_, _) => SaveGeometry();
        PointerPressed += NoteWindow_OnPointerPressed;
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

    private void NoteText_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var noteText = this.FindControl<TextBox>("NoteText")!;
        if (e.Key == Key.T && (e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
        {
            noteText.CaretIndex = 0;
            noteText.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            AppendAndClose();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift || string.IsNullOrEmpty(noteText.Text))
                Close();
            else
                noteText.Clear();
            e.Handled = true;
        }
    }

    private void AddTagMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var noteText = this.FindControl<TextBox>("NoteText")!;
        if (!(noteText.Text ?? string.Empty).Contains(" - ", StringComparison.Ordinal))
            noteText.Text = $" - {noteText.Text}";
        noteText.CaretIndex = 0;
        noteText.Focus();
    }

    private void CopyMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        this.FindControl<TextBox>("NoteText")!.Copy();

    private void PasteMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        this.FindControl<TextBox>("NoteText")!.Paste();

    private void CutMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        this.FindControl<TextBox>("NoteText")!.Cut();

    private void ClearMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        this.FindControl<TextBox>("NoteText")!.Clear();

    private void CancelMenuItem_OnClick(object? sender, RoutedEventArgs e) => Close();

    private void NoteWindow_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is TextBox or Button) return;
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void AppendAndClose()
    {
        var text = this.FindControl<TextBox>("NoteText")?.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(text))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_notesPath)!);
            File.AppendAllText(_notesPath, $"{DateTime.Now:yyyyMMdd HH:mm:ss} | {text}{Environment.NewLine}");
        }
        Close();
    }
}
