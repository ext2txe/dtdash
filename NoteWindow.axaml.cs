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
    private readonly bool _sticky;
    private readonly NoteStore? _noteStore;

    public NoteWindow() : this(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "data", "notes.txt"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "note-window.json"), false) { }

    public NoteWindow(string notesPath, string geometryPath, bool sticky = false, NoteStore? noteStore = null)
    {
        _notesPath = notesPath;
        _geometryPath = geometryPath;
        _sticky = sticky;
        _noteStore = noteStore;
        InitializeComponent();
        Opened += (_, _) =>
        {
            RestoreGeometry();
            var tagText = this.FindControl<TextBox>("TagText");
            if (tagText is not null)
                tagText.Text = LoadLastTag();
            this.FindControl<CheckBox>("StickyCheckBox")!.IsChecked = _sticky;
            this.FindControl<TextBox>("NoteText")?.Focus();
        };
        Closing += (_, _) =>
        {
            this.FindControl<CheckBox>("StickyCheckBox")!.IsChecked = _sticky;
            SaveGeometry();
        };
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
            var tagText = this.FindControl<TextBox>("TagText")!;
            tagText.CaretIndex = tagText.Text?.Length ?? 0;
            tagText.Focus();
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

    private void TagText_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            this.FindControl<TextBox>("NoteText")!.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.T && (e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
        {
            this.FindControl<TextBox>("TagText")!.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    private void AddTagMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var tagText = this.FindControl<TextBox>("TagText")!;
        tagText.Focus();
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
        var tag = this.FindControl<TextBox>("TagText")?.Text?.Trim();
        var text = this.FindControl<TextBox>("NoteText")?.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(text))
        {
            if (_noteStore?.UseSchema == true)
                _noteStore.Add(tag, text);
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_notesPath)!);
                File.AppendAllText(_notesPath, $"{DateTime.Now:yyyyMMdd HH:mm:ss} - {tag} - {text}{Environment.NewLine}");
            }
        }

        if (this.FindControl<CheckBox>("StickyCheckBox")!.IsChecked == true)
        {
            this.FindControl<TextBox>("NoteText")!.Clear();
            this.FindControl<TextBox>("NoteText")!.Focus();
            return;
        }

        Close();
    }

    private string LoadLastTag()
    {
        try
        {
            if (_noteStore?.UseSchema == true) return _noteStore.LoadLastTag() ?? string.Empty;
            if (!File.Exists(_notesPath)) return string.Empty;
            var lastLine = File.ReadLines(_notesPath).LastOrDefault(line => !string.IsNullOrWhiteSpace(line));
            if (lastLine is null) return string.Empty;
            var separator = lastLine.IndexOf(" - ", StringComparison.Ordinal);
            if (separator < 0) return string.Empty;
            var noteStart = lastLine.IndexOf(" - ", separator + 3, StringComparison.Ordinal);
            return noteStart > separator ? lastLine[(separator + 3)..noteStart].Trim() : string.Empty;
        }
        catch { return string.Empty; }
    }
}
