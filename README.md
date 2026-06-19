# Promplet

Promplet is a small Windows desktop prompt and snippet palette for quickly pasting reusable text into AI chats, CLI tools, terminals, Slack, email, and similar apps.

## Status

Early development. The current build is a Phase 2 MVP: a topmost WPF palette that does not steal focus and can paste JSON-backed prompt groups into the active application.

The app is not tray-resident yet. Use the `X` button on the palette to exit the MVP build.

Prompt data is stored at `%APPDATA%\Promplet\prompts.json`. If the file is missing, Promplet creates it from the default prompts. If it is invalid, Promplet backs it up and recreates defaults.

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
3. Tray resident mode and global hotkey
4. Paste mode and newline handling options
5. Button manager UI
