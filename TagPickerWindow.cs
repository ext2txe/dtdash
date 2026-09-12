using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace Dtdash;

public sealed class TagPickerWindow : Window
{
    private readonly IReadOnlyList<string> _tags;
    private readonly Action<string> _selectTag;
    private readonly Action<string> _addTag;

    public TagPickerWindow(IReadOnlyList<string> tags, Action<string> selectTag, Action<string> addTag)
    {
        _tags = tags;
        _selectTag = selectTag;
        _addTag = addTag;
        Title = "Recent tags";
        Width = 280;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        ShowInTaskbar = false;
        KeyDown += OnKeyDown;
        Opened += (_, _) => ((TextBox)((StackPanel)Content!).Children[1]).Focus();

        var panel = new StackPanel { Spacing = 4, Margin = new Thickness(10) };
        panel.Children.Add(new TextBlock { Text = "Select a recent tag", FontWeight = Avalonia.Media.FontWeight.Bold });
        var newTag = new TextBox { Watermark = "New tag, then Enter" };
        newTag.KeyDown += (_, e) =>
        {
            var index = NumberIndex(e.Key);
            if (index >= 0 && index < _tags.Count)
            {
                Select(index);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(newTag.Text))
            {
                _addTag(newTag.Text.Trim());
                e.Handled = true;
            }
        };
        panel.Children.Add(newTag);
        for (var index = 0; index < _tags.Count; index++)
        {
            var tagIndex = index;
            var button = new Button
            {
                Content = $"{index + 1}.  {_tags[index]}",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            button.Click += (_, _) => Select(tagIndex);
            panel.Children.Add(button);
        }
        Content = panel;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var index = NumberIndex(e.Key);
        if (index >= 0 && index < _tags.Count)
        {
            Select(index);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    private static int NumberIndex(Key key) => key switch
    {
        Key.D1 or Key.NumPad1 => 0, Key.D2 or Key.NumPad2 => 1,
        Key.D3 or Key.NumPad3 => 2, Key.D4 or Key.NumPad4 => 3,
        Key.D5 or Key.NumPad5 => 4, Key.D6 or Key.NumPad6 => 5,
        Key.D7 or Key.NumPad7 => 6, Key.D8 or Key.NumPad8 => 7,
        Key.D9 or Key.NumPad9 => 8, Key.D0 or Key.NumPad0 => 9,
        _ => -1
    };

    private void Select(int index)
    {
        _selectTag(_tags[index]);
        Close();
    }
}
