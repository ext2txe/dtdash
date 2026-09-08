using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Dtdash;

public partial class NoteWindow : Window
{
    private readonly string _notesPath;

    public NoteWindow(string notesPath)
    {
        _notesPath = notesPath;
        InitializeComponent();
        Opened += (_, _) => this.FindControl<TextBox>("NoteText")?.Focus();
        PointerPressed += NoteWindow_OnPointerPressed;
    }

    private void NoteText_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AppendAndClose();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            this.FindControl<TextBox>("NoteText")!.Clear();
            if ((e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift) Close();
            e.Handled = true;
        }
    }

    private void ClearButton_OnClick(object? sender, RoutedEventArgs e) =>
        this.FindControl<TextBox>("NoteText")!.Clear();

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) => Close();

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
