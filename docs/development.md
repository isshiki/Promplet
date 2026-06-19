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
