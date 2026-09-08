# Features

## 0.1.8 — 2026-09-07

- Replaces the shortcut ICO with a transparent multi-size icon containing 16–256 px frames.

## 0.1.7 — 2026-09-07

- Desktop shortcut targets the current Debug executable so the displayed version matches the rebuilt app.

## 0.1.6 — 2026-09-07

- Makes the white background of `redicon.png` transparent for the application and shortcut icons.

## 0.1.5 — 2026-09-07

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
