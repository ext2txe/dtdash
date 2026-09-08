# Features

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
