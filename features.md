# Features

## 0.1.5 — 2026-09-08

- Holding Shift while starting the application skips restoring saved window geometry and uses the default window size and position.

## 0.1.6 — 2026-09-08

- Updates Avalonia desktop dependencies to the patched 11.3.14 release line, resolving the transitive `Tmds.DBus.Protocol` vulnerability warning.

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
