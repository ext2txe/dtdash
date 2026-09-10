# dtdash

Desktop Dashboard proof of concept using .NET 10 and Avalonia.

Build on Windows with `.uild.ps1`. The build starts at version `0.1.1` and creates a desktop shortcut named `DTDash.lnk` for the generated executable.

The default configuration is `<user home directory>/.dtdash/config.json`. A configuration file can be supplied as the first startup argument, for example `dtdash.exe C:\path\config.json`. Window geometry is stored beside the active configuration as `.dtdash/window.json`. Daily logs are stored in the sibling `logs` folder as `yyyyMMdd_dtdash.log`.

## macOS Notes

When DTDash is launched from a removable volume, macOS may show a prompt that “applet” would like to access files on a removable volume. This is macOS privacy protection. Click **Allow**, or copy `dtdash.app` to `/Applications` and launch it there. Shortcuts should point to the copied app.
