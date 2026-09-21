using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace Dtdash;

public sealed class TagPickerWindow : Window
{
    private readonly List<string> _tags;
    private readonly Action<string> _selectTag;
    private readonly Action<string> _addTag;
    private readonly Func<string, bool> _canDeleteTag;
    private readonly Func<string, bool> _deleteTag;
    private readonly StackPanel _tagPanel;

    public TagPickerWindow(
        IReadOnlyList<string> tags,
        Action<string> selectTag,
        Action<string> addTag,
        Func<string, bool> canDeleteTag,
        Func<string, bool> deleteTag)
    {
        _tags = tags.ToList();
        _selectTag = selectTag;
        _addTag = addTag;
        _canDeleteTag = canDeleteTag;
        _deleteTag = deleteTag;
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
                e.Handled = true;
                try
                {
                    _addTag(newTag.Text.Trim());
                }
                finally
                {
                    Close();
                }
            }
        };
        panel.Children.Add(newTag);
        _tagPanel = new StackPanel { Spacing = 4 };
        panel.Children.Add(_tagPanel);
        foreach (var tag in _tags)
            AddTagRow(tag);
        Content = panel;
    }

    private void AddTagRow(string tag)
    {
        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        var selectButton = new Button
        {
            Content = $"{_tags.IndexOf(tag) + 1}.  {tag}",
            HorizontalContentAlignment = HorizontalAlignment.Left,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        selectButton.Click += (_, _) => SelectTag(tag);
        row.Children.Add(selectButton);

        var deleteButton = new Button
        {
            Content = "Delete",
            IsEnabled = _canDeleteTag(tag),
            Margin = new Thickness(4, 0, 0, 0)
        };
        deleteButton.Click += (_, _) =>
        {
            if (!_canDeleteTag(tag) || !_deleteTag(tag)) return;
            _tags.Remove(tag);
            _tagPanel.Children.Remove(row);
            RefreshTagNumbers();
        };
        Grid.SetColumn(deleteButton, 1);
        row.Children.Add(deleteButton);
        _tagPanel.Children.Add(row);
    }

    private void RefreshTagNumbers()
    {
        for (var index = 0; index < _tagPanel.Children.Count; index++)
        {
            if (_tagPanel.Children[index] is not Grid row || row.Children[0] is not Button button)
                continue;
            var tag = _tags[index];
            button.Content = $"{index + 1}.  {tag}";
        }
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
        SelectTag(_tags[index]);
    }

    private void SelectTag(string tag)
    {
        _selectTag(tag);
        Close();
    }
}
