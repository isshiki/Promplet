# Appearance Settings Plan

## Status

Recorded on 2026-06-19 from the user's request before global hotkey work.

## Scope

Appearance settings are planned for the same phase as opacity settings, not for the immediate global hotkey pass.

## Planned Behavior

- Support three theme modes: `System`, `Light`, and `Dark`.
- Default to `System`.
- Add theme switching to the tray icon right-click menu.
- Add the same theme switching entry point somewhere in the button manager MVP.
- Avoid putting permanent appearance settings controls in normal app windows.
- Share the same theme setting between the palette and button manager.
- Implement opacity and theme persistence together in the app settings JSON.

## Out of Scope For Next Pass

- Implementing theme resources.
- Building the button manager UI.
- Adding opacity controls.

The next implementation pass remains global hotkey support.
