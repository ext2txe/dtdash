# Features

## 0.1.38 — 2026-09-09 08:22

- Applies `StartMinimized` after the main window opens, allowing the startup window to lay out its controls correctly before minimizing.

## 0.1.37 — 2026-09-09 08:20

- Adds the `StartMinimized` configuration setting, defaulting to `true`, and starts the main window minimized when enabled.

## 0.1.36 — 2026-09-09 08:16

- Generates a native `redicon.icns` for the macOS app bundle so Dock entries use the dtdash icon.

## 0.1.35 — 2026-09-09 08:04

- Builds a macOS `dtdash.app` bundle that declares `redicon.png` as the Dock icon.

## 0.1.34 — 2026-09-09 07:58

- Applies the `redicon.png` macOS dock icon after the main application window opens.

## 0.1.33 — 2026-09-09 07:56

- Adds Alt+T to move the quick note caret to the start, an optional `tag - note` entry format, and context-menu actions for adding a tag, copying, pasting, and cutting.

## 0.1.32 — 2026-09-09 07:49

- Makes the quick note window resizable and closes it on Escape when the text is empty; Escape clears non-empty text.

## 0.1.31 — 2026-09-08 23:00

- Replaces Quick Edit Clear and Cancel buttons with context-menu actions, using Esc and Shift+Esc as their keyboard shortcuts, and reduces the window height.

## 0.1.27 — 2026-09-08 22:55

- Resets quick-edit window geometry when Shift is pressed at startup and keeps restored geometry within the display bounds.

## 0.1.26 — 2026-09-08 22:55

- Adds the Windows `Ctrl+Shift+N` global hotkey to open a quick note.

## 0.1.25 — 2026-09-08

- Opens hotkey-triggered notes without owning them from the minimized main window, preventing the main window from being restored.

## 0.1.24 — 2026-09-08

- Preserves the minimized state of the main window after opening a quick note.

## 0.1.23 — 2026-09-08

- Keeps the main window minimized when the global hotkey opens a note.
- Sets the macOS dock icon from `redicon.png`.

## 0.1.22 — 2026-09-08

- Registers `Ctrl+Shift+N` as a global hotkey while dtDash is running; it restores the app and opens the note window, including when minimized.

## 0.1.21 — 2026-09-08

- Persists settings edits when the settings window closes and limits note-window dragging to its non-control surface.

## 0.1.20 — 2026-09-08

- Adds a minimal borderless note-entry window, opened by the `Note` button or `Shift+Alt+N`, with keyboard completion and clearing controls.
- Stores single-line timestamped notes in the editable `PathToNotes` setting.
- Adds a settings grid backed by a separate `settings.json` file.

## 0.1.19 — 2026-09-08

- Appends the application version to the `dtDash` window title.

## 0.1.18 — 2026-09-08

- A second launch activates the existing instance, restores it if minimized, and brings it to the front.

## 0.1.17 — 2026-09-08

- Prevents more than one instance of the application from running at the same time.

## 0.1.16 — 2026-09-08

- Sets the Avalonia application name to `dtDash`.

## 0.1.15 — 2026-09-08

- Sets the application title to `dtDash`.

## 0.1.14 — 2026-09-08

- Closing the main window shuts down the application and closes the settings window while leaving the external log window open.

## 0.1.13 — 2026-09-08

- Closing the main window shuts down the application and closes the settings window.

## 0.1.12 — 2026-09-08

- Logs application startup and shutdown events, including executable, current log, configuration, and settings-file paths at startup.

## 0.1.11 — 2026-09-08

- Adds a `Log` button that opens the daily `yyyyMMdd_dtdash.log` file from the `logs` folder beside the configuration folder.

## 0.1.39 — 2026-09-09 09:55

- Quick edit notes use separate tag and note inputs, default to the last saved tag, support Alt+T to edit the tag, and save as `timestamp - tag - note`.

## 0.1.10 — 2026-09-08

- Stores the default configuration under the user's home directory at `<user home directory>/.dtdash/config.json`.

## 0.1.9 — 2026-09-08

- Uses a transparent multi-size ICO for the Windows application and desktop shortcut icons.
- Updates Avalonia desktop dependencies to 11.3.14.

## 0.1.8 — 2026-09-08

- Adds an “Open settings” tooltip to the main-window settings button and persists/restores the settings window geometry.

## 0.1.7 — 2026-09-08

- Adds a bottom status bar with timestamped status messages, a live HH:MM:SS clock, and a gear button that opens an empty Settings window.

## 0.1.6 — 2026-09-09

- Updates Avalonia desktop dependencies to the patched 11.3.14 release line, resolving the transitive `Tmds.DBus.Protocol` vulnerability warning.

## 0.1.5 — 2026-09-08

- Uses the ICO version of `redicon.png` for Windows desktop shortcut and executable icons.

## 0.1.4 — 2026-09-07

- Desktop shortcut uses `redicon.png` for its icon.

## 0.1.3 — 2026-09-07

- Uses `redicon.png` for the application icon and window icon.

## 0.1.2 — 2026-09-07

- Cross-platform Avalonia desktop application for Windows, macOS, and Linux.
- Window title includes the application version.
- Window location and size persist and restore between runs.
- Optional configuration file path can be supplied at startup.
- Default configuration is located at `<executable directory>/.dtdash/config.json`.
- Initial configuration model includes the named AvBot/AvAutomation settings as a proof of concept.
- Successful builds increment the patch version for the next build.
- Windows builds provide a desktop shortcut to the current executable.
- 2026-09-08: Windows Ctrl+Shift+N opens the quick notes window.
- 2026-09-08: Quick notes window position and geometry persist on Windows and macOS.
