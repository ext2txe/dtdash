using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace Dtdash;

public partial class NoteWindow : Window
{
    private const double SingleLineHeight = 88;
    private readonly string _notesPath;
    private readonly string _geometryPath;
    private readonly string _tagsPath;
    private readonly bool _sticky;
    private readonly bool _keepOnTop;
    private readonly Action<bool>? _saveStickySetting;
    private readonly Action? _toggleMainWindow;
    private bool _restoringGeometry;

    public NoteWindow() : this(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "data", "notes.txt"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "note-window.json"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "recent-tags.json"), false, true, null, null) { }

    public NoteWindow(string notesPath, string geometryPath, bool sticky = false)
        : this(notesPath, geometryPath, Path.Combine(Path.GetDirectoryName(geometryPath)!, "recent-tags.json"), sticky, true, null, null) { }

    public NoteWindow(string notesPath, string geometryPath, bool sticky, Action<bool>? saveStickySetting)
        : this(notesPath, geometryPath, Path.Combine(Path.GetDirectoryName(geometryPath)!, "recent-tags.json"), sticky, true, saveStickySetting, null) { }

    public NoteWindow(string notesPath, string geometryPath, string tagsPath, bool sticky, bool keepOnTop, Action<bool>? saveStickySetting, Action? toggleMainWindow)
    {
        _notesPath = notesPath;
        _geometryPath = geometryPath;
        _tagsPath = tagsPath;
        _sticky = sticky;
        _keepOnTop = keepOnTop;
        _saveStickySetting = saveStickySetting;
        _toggleMainWindow = toggleMainWindow;
        InitializeComponent();
        Topmost = _keepOnTop;
        this.FindControl<MenuItem>("VersionMenuItem")!.Header = $"Version {GetVersion()}";
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
            _saveStickySetting?.Invoke(this.FindControl<CheckBox>("StickyCheckBox")!.IsChecked == true);
            SaveGeometry();
        };
        PositionChanged += (_, _) => SaveGeometry();
        SizeChanged += (_, _) => SaveGeometry();
        PointerPressed += NoteWindow_OnPointerPressed;
    }

    private void RestoreGeometry()
    {
        _restoringGeometry = true;
        try
        {
            if (!File.Exists(_geometryPath)) return;
            var state = System.Text.Json.JsonSerializer.Deserialize<WindowGeometry>(File.ReadAllText(_geometryPath));
            if (state is null) return;
            Width = Math.Clamp(state.Width, MinWidth, 3000);
            Height = SingleLineHeight;
            Position = new PixelPoint(state.X, state.Y);
        }
        catch { }
        finally
        {
            _restoringGeometry = false;
            SaveGeometry();
        }
    }

    private void SaveGeometry()
    {
        if (_restoringGeometry) return;
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
        if (e.Key == Key.M && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
        {
            _toggleMainWindow?.Invoke();
            e.Handled = true;
        }
        else if (e.Key == Key.T && (e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
        {
            OpenTagPicker();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
            {
                var caret = noteText.CaretIndex;
                noteText.Text = noteText.Text?.Insert(caret, Environment.NewLine);
                noteText.CaretIndex = caret + Environment.NewLine.Length;
            }
            else
            {
                AppendAndClose();
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift || string.IsNullOrEmpty(noteText.Text))
                Close();
            else
                ClearNoteText();
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
            OpenTagPicker();
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

    private async void OpenTagPicker()
    {
        var tags = TagStore.Load(_tagsPath);
        var picker = new TagPickerWindow(tags, tag =>
        {
            this.FindControl<TextBox>("TagText")!.Text = tag;
        }, tag =>
        {
            TagStore.Add(_tagsPath, tag);
            this.FindControl<TextBox>("TagText")!.Text = tag;
        });
        picker.Closed += (_, _) => this.FindControl<TextBox>("NoteText")!.Focus();
        picker.WindowStartupLocation = WindowStartupLocation.Manual;
        picker.Position = new PixelPoint(Position.X + 20, Position.Y + (int)Height + 4);
        await picker.ShowDialog(this);
    }

    public void SaveStateOnShutdown()
    {
        _saveStickySetting?.Invoke(this.FindControl<CheckBox>("StickyCheckBox")!.IsChecked == true);
        SaveGeometry();
    }

    private void ToggleMainWindowMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        _toggleMainWindow?.Invoke();

    private void CopyMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        this.FindControl<TextBox>("NoteText")!.Copy();

    private void PasteMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        this.FindControl<TextBox>("NoteText")!.Paste();

    private void CutMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        this.FindControl<TextBox>("NoteText")!.Cut();

    private void ClearMenuItem_OnClick(object? sender, RoutedEventArgs e) =>
        ClearNoteText();

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
            TagStore.Add(_tagsPath, tag ?? string.Empty);
            Directory.CreateDirectory(Path.GetDirectoryName(_notesPath)!);
            var line = $"{DateTime.Now:yyyyMMdd HH:mm:ss} - {tag} - {text.Replace("\r\n", "\n").Replace("\n", "\\n")}";
            File.AppendAllText(_notesPath, $"{(IsObsidianNotesFile() ? "- " : string.Empty)}{line}{Environment.NewLine}");
        }

        if (this.FindControl<CheckBox>("StickyCheckBox")!.IsChecked == true)
        {
            ClearNoteText();
            this.FindControl<TextBox>("NoteText")!.Focus();
            return;
        }

        Close();
    }

    private void ClearNoteText()
    {
        this.FindControl<TextBox>("NoteText")!.Clear();
        Height = SingleLineHeight;
    }

    private string LoadLastTag()
    {
        try
        {
            if (!File.Exists(_notesPath)) return string.Empty;
            var lastLine = File.ReadLines(_notesPath).LastOrDefault(line => !string.IsNullOrWhiteSpace(line));
            if (lastLine is null) return string.Empty;
            lastLine = IsObsidianNotesFile() && lastLine.TrimStart().StartsWith("- ")
                ? lastLine.TrimStart()[2..]
                : lastLine;
            var separator = lastLine.IndexOf(" - ", StringComparison.Ordinal);
            if (separator < 0) return string.Empty;
            var noteStart = lastLine.IndexOf(" - ", separator + 3, StringComparison.Ordinal);
            return noteStart > separator ? lastLine[(separator + 3)..noteStart].Trim() : string.Empty;
        }
        catch { return string.Empty; }
    }

    private bool IsObsidianNotesFile() =>
        _notesPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

    private static string GetVersion() =>
        typeof(NoteWindow).Assembly.GetName().Version?.ToString(3) ?? "0.1.19";
}
