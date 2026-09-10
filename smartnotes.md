# Smart Notes

Created: 2026-09-10 16:27

The current note flow is plain text:

- `NoteWindow` uses an Avalonia `TextBox`.
- Notes are saved as single lines in `.txt` or Markdown files.
- `MainWindow` displays notes as strings in a `ListBox`.
- There is no attachment model or image rendering path.

A good implementation would follow the same pattern as Codex/VS Code:

1. Add image input methods:
   - Paste an image from the clipboard.
   - Drag and drop an image.
   - Optional “Attach image” button using a file picker.

2. Copy the image into a predictable attachment folder, for example:

```text
.dtdash/data/attachments/20260910-143012-screenshot.png
```

3. Insert a Markdown reference into the note:

```markdown
20260910 14:30:12 - tag - See this screenshot ![screenshot](attachments/20260910-143012-screenshot.png)
```

For Obsidian notes, use an Obsidian-compatible relative path:

```markdown
![screenshot](../attachments/20260910-143012-screenshot.png)
```

4. Add a thumbnail preview in the note window. The simplest compatible design is:

```text
[Tag]
[Note text                         ]
[thumbnail] screenshot.png
[Attach image] [Remove]
```

The thumbnail can be an Avalonia `Image` bound to the selected attachment. Clicking it could open the full-size image with the default system application.

5. Replace the main `ListBox` string template with a note view model containing:

```csharp
public sealed class NoteItem
{
    public string Text { get; init; } = "";
    public IReadOnlyList<string> ImagePaths { get; init; } = [];
}
```

The list item can then show the text and one or more scaled thumbnails.

## Storage design

- For `.md` notes: store images beside the Markdown file or in an `attachments` subfolder and insert Markdown image links. This is the most interoperable option.
- For `.txt` notes: either keep Markdown-style image references or introduce a sidecar JSON file. Markdown references are simpler and preserve compatibility.
- Avoid embedding Base64 image data directly in the notes file; it makes files large and difficult to edit externally.

## Incremental implementation

1. Add clipboard/file-picker attachment support.
2. Copy attachments and insert Markdown references.
3. Add thumbnail previews in the note window.
4. Add thumbnails to the main notes list.
5. Preserve existing text-only notes unchanged.

The current `TextBox` cannot display inline images inside the text itself. To render images directly inside the editor, the note editor would need to be replaced with a Markdown-aware or rich-text control. A separate thumbnail preview is lower-risk and fits the existing architecture better.
