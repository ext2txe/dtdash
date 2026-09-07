# dtdash

Desktop Dashboard proof of concept using .NET 10 and Avalonia.

Build on Windows with `.uild.ps1`. The build starts at version `0.1.1` and creates a desktop shortcut named `DTDash.lnk` for the generated executable.

The default configuration is `<executable directory>/.dtdash/config.json`. A configuration file can be supplied as the first startup argument, for example `dtdash.exe C:\path\config.json`. Window geometry is stored beside the active configuration as `.dtdash/window.json`.


