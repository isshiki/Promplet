# Promplet

Promplet is a small Windows desktop prompt and snippet palette for quickly pasting reusable text into AI chats, CLI tools, terminals, Slack, email, and similar apps.

## Status

Early development. The current build is a tray-resident WPF palette that does not steal focus and can paste JSON-backed prompt groups into the active application.

Use the `X` button on the palette to hide it. Use the tray icon menu to show or hide the palette, open Prompt Library, open settings, reload prompt JSON, or exit Promplet.

Promplet allows only one running instance. Starting it again while it is already running exits the second process before creating another tray icon or hotkey registration.

Prompt data and app settings are stored at `%APPDATA%\Promplet\prompts.json`. If the file is missing, Promplet creates it from the default prompts. If it is invalid, Promplet backs it up and recreates defaults.

Default hotkeys:

- `Ctrl+Alt+Space`: show or hide the palette
- `Ctrl+NumPad1` ... `Ctrl+NumPad9`: paste visible buttons 1-9
- `Ctrl+NumPad0`: paste visible button 10

Hotkeys, theme mode, and palette opacity can be changed from the tray menu's settings dialog.

Groups and prompts can be edited in Prompt Library from the tray menu. The editor supports adding, deleting, renaming, enabling/disabling, and reordering prompts.

## Requirements

- Windows
- Visual Studio 18 or later with WPF desktop development
- .NET 10 SDK

## Build

```powershell
dotnet build .\Promplet.slnx
```

## Roadmap

1. Non-activating topmost palette
2. JSON-backed prompt groups
3. Tray resident mode
4. Global hotkeys
5. Settings dialog for appearance and hotkeys
6. Prompt Library refinements
7. Paste mode and newline handling options
