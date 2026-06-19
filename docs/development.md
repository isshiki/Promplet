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
6. The `X` button hides the palette without ending the process.
7. The tray icon menu can show or hide the palette, open Prompt Library, open settings, reload prompt JSON, and exit.
8. `Ctrl+Alt+Space` shows or hides the palette.
9. `Ctrl+NumPad1` through `Ctrl+NumPad0` paste visible buttons 1 through 10.
10. The settings dialog can change theme mode, palette opacity, and hotkeys.
11. The settings dialog shows hotkey registration failures if Windows rejects a shortcut.
12. Prompt Library can add, delete, rename, enable/disable, and reorder prompts, then save or cancel changes.
13. Starting Promplet again while it is already running exits the second process without creating another tray icon or exception dialog.

Prompt data is loaded from `%APPDATA%\Promplet\prompts.json`. Delete that file to regenerate the default MVP prompt group.

## MVP Limitations

- Only text clipboard restore is attempted.
- Advanced paste settings are deferred.
- Full no-activate paste behavior must be tested manually against target applications.
- `NumPad` hotkeys require the key to be reported as a numpad virtual key; with NumLock off, Windows may report the physical key as a navigation key such as `End`. Avoid adding `Shift` to the default numpad shortcuts because Windows may stop reporting the physical key as `NumPad`.
