# Development

## Build Environment

Promplet targets `net10.0-windows` and uses WPF.

Use:

```powershell
dotnet build .\Promplet.slnx
```

If `dotnet --version` reports an SDK older than 10.0, install or repair the .NET 10 SDK.

## Visual Studio

Open `Promplet.slnx` in Visual Studio 18 or later.

## MVP Verification

The core manual checks are:

1. The palette stays topmost.
2. Clicking a palette button does not move keyboard focus away from the target app.
3. The selected prompt text is pasted into ChatGPT, Claude, Notepad, and Windows Terminal.
4. The palette can be dragged without losing topmost/no-activate behavior.
5. Switching a group tab changes the visible prompt buttons.
6. The `X` button exits the app.

Prompt data is loaded from `%APPDATA%\Promplet\prompts.json`. Delete that file to regenerate the default MVP prompt group.

## MVP Limitations

- Prompt groups and buttons are JSON-backed, but there is no editor UI yet.
- Only text clipboard restore is attempted.
- Tray, hotkey, advanced paste settings, and manager UI are deferred.
- The current MVP exits from the palette `X` button instead of hiding to a tray icon.
- Full no-activate paste behavior must be tested manually against target applications.
