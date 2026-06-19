# Global Hotkeys Design

## Status

Approved for implementation on 2026-06-19.

## Scope

This pass adds global hotkeys for showing/hiding the palette and directly pasting visible prompt buttons from the current group.

Included hotkeys:

- `Ctrl + Alt + Space`: show/hide the palette.
- `Ctrl + Shift + NumPad1`: paste visible button 1.
- `Ctrl + Shift + NumPad2`: paste visible button 2.
- `Ctrl + Shift + NumPad3`: paste visible button 3.
- `Ctrl + Shift + NumPad4`: paste visible button 4.
- `Ctrl + Shift + NumPad5`: paste visible button 5.
- `Ctrl + Shift + NumPad6`: paste visible button 6.
- `Ctrl + Shift + NumPad7`: paste visible button 7.
- `Ctrl + Shift + NumPad8`: paste visible button 8.
- `Ctrl + Shift + NumPad9`: paste visible button 9.
- `Ctrl + Shift + NumPad0`: paste visible button 10.

Out of scope:

- User-editable hotkey settings.
- Left/right modifier key distinction.
- Low-level keyboard hooks.
- Hotkey conflict UI.

## Design Notes

Use Win32 `RegisterHotKey` instead of a low-level keyboard hook for the MVP. It is simpler and stable for app-level global shortcuts, but it cannot distinguish left Ctrl from right Ctrl.

Use `VK_NUMPAD0` to `VK_NUMPAD9` so numpad keys are distinct from the top-row number keys. The first pass assumes NumLock is on for numeric numpad input.

Add `MOD_NOREPEAT` to reduce repeated paste while a hotkey is held down.

If a hotkey cannot be registered because another app already owns it, Promplet keeps running and simply ignores that hotkey. A future settings UI can expose conflict state and allow editing.

## Runtime Behavior

`GlobalHotKeyService` owns registration, unregistration, and `WM_HOTKEY` dispatch. `MainWindow` owns the resulting actions:

- Toggle hotkey calls the same show/hide behavior as the tray menu.
- Paste hotkeys use the current `PaletteViewModel.VisibleButtons` list.
- If the requested 1-10 slot has no visible button, Promplet beeps and does nothing.

Hotkeys remain active while the palette window is hidden because the WPF window handle still exists.
