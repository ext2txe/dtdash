# Tagging Implementation Plan

Created: 2026-09-10 17:39

## Recommended tag design

Use Obsidian-compatible inline tags as the shared format:

\`\`\`markdown
- 20260910 17:30:00 - #work #urgent - Follow up with supplier
\`\`\`

This keeps tags visible to Obsidian and other Markdown tools. Support:

- Multiple tags per note.
- Nested tags such as \`#project/dtdash\`.
- Tag names containing letters, numbers, \`_\`, \`-\`, and \`/\`.
- Existing simple tags migrated as \`#tag\`.
- Notes without tags remain valid.

## Internal model

Replace string-only notes with a shared model:

\`\`\`csharp
public sealed class NoteItem
{
    public DateTime? Timestamp { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string Text { get; init; } = "";
    public string? SourcePath { get; init; }
    public int? SourceLine { get; init; }
}
\`\`\`

The parser and serializer should be shared by quick edit, main-window display, tag management, and future runner functionality.

## Quick-edit changes

Replace the single tag text box with a tag editor:

- Current tags displayed as removable chips.
- Typing a tag and pressing \`Enter\` creates/selects it.
- Suggestions come from tags already found in notes.
- A new tag is created automatically when it does not exist.
- Multiple tags can be selected.
- Existing keyboard behavior remains:
  - \`Enter\`: save note.
  - \`Shift+Enter\`: insert a new line.
  - \`Ctrl+M\`: toggle main window.

The note is serialized with all selected tags as Markdown tags.

## Tag association and editing

Add a note context menu with:

- \`Edit note\`
- \`Add tag\`
- \`Remove tag\`
- \`Replace tags\`
- \`Copy note\`
- \`Delete note\`, if deletion is later approved

Editing must rewrite the source Markdown file safely rather than append a duplicate. For Obsidian daily files, preserve unrelated content and update only the selected note entry.

## Main-window reporting

Change the notes list from \`ObservableCollection<string>\` to \`ObservableCollection<NoteItem>\`.

Add:

- Tag chips beside each note.
- Tag filtering.
- A tag summary or sidebar showing tag counts.
- Sorting/filtering by date and tag.
- Search across note text and tags.

The initial implementation should use in-memory indexing from the existing daily Markdown file. A separate tag database is unnecessary at first.

## Compatibility strategy

The parser should accept all existing forms:

\`\`\`text
20260910 17:30:00 - work - Note text
20260910 17:30:00 - #work #urgent - Note text
- 20260910 17:30:00 - work - Note text
\`\`\`

When an old note is edited, rewrite it in the canonical Markdown-tag format. Do not rewrite every existing note automatically.

The current escaped newline format should remain supported so existing multi-line notes continue to load correctly.

## Suggested implementation phases

1. Create shared \`NoteItem\`, tag parser, and serializer.
2. Add multi-tag capture to quick edit.
3. Add Markdown tag synchronization and backward-compatible parsing.
4. Replace the main list’s string rendering with structured note rendering.
5. Add tag filtering and tag counts.
6. Add note editing and tag-association changes.
7. Add tests for legacy notes, multiple tags, nested tags, Markdown bullets, and multi-line content.
8. Update \`features.md\` and use the mandatory build wrapper with a feature description.

## Important future-proofing

Tags should remain independent from reminders, tasks, events, monitoring, and automation. Those future records can later reuse the same tagging model:

\`\`\`csharp
public interface ITaggedItem
{
    IReadOnlyList<string> Tags { get; }
}
\`\`\`

That allows notes, tasks, reminders, events, alerts, and monitoring actions to share tag filtering without forcing those features into the note format prematurely.
