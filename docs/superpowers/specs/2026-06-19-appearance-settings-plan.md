# Appearance Settings Plan

## Status

Implemented on 2026-06-19 as part of the settings dialog and configurable hotkey pass.

## Scope

Appearance settings are planned for the same phase as opacity settings, not for the immediate global hotkey pass.

## Planned Behavior

- Support three theme modes: `System`, `Light`, and `Dark`.
- Default to `System`.
- Add theme switching through a settings dialog opened from the tray icon right-click menu.
- Add the same settings entry point somewhere in the Prompt Library MVP.
- Avoid putting permanent appearance settings controls in normal app windows.
- Share the same theme setting between the palette and Prompt Library.
- Implement opacity and theme persistence together in the app settings JSON.

## Follow-up Scope

- Building the Prompt Library UI.
- Direct theme submenu items in the tray menu if they are still useful after settings dialog testing.
