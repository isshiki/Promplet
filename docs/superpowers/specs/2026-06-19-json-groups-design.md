# JSON Groups Design

## Status

Approved for implementation on 2026-06-19 by the user instruction to proceed with JSON support and real group switching without further confirmation unless blocked.

## Scope

This pass replaces the fixed in-code prompt list with a JSON-backed prompt library and connects the main palette tabs to real groups. It also persists lightweight palette state needed for daily use: selected group, window position, and width.

Tray residence, global hotkeys, the button manager UI, per-button paste modes, and advanced newline rules remain out of scope for this pass.

## Storage

Promplet stores editable prompt data under `%APPDATA%\Promplet\prompts.json`.

The file contains:

- `app`: app-level options currently limited to clipboard restore and selected group.
- `window`: palette left, top, and width.
- `groups`: up to 10 prompt groups.
- `buttons`: up to 10 enabled prompt buttons per group.

When `prompts.json` does not exist, Promplet creates it from the current MVP default prompts. When the file exists but cannot be parsed or validated, Promplet copies it to a timestamped `.bak` file and recreates a valid default file.

## Runtime Model

`PromptStore` owns file paths, JSON load/save, default document creation, and validation. `PaletteViewModel` owns the selected group, visible buttons, and tab commands. The WPF window binds tabs and buttons to the view model instead of hard-coded XAML tab labels.

The main palette keeps the current non-activating paste behavior. Left click still pastes and right click still copies.

## UI Behavior

The top tab row shows groups from the JSON file. Selecting a tab changes the prompt buttons below it immediately and records the selected group in memory. On close, Promplet saves selected group, window left/top, and width.

If the saved window position is outside the current virtual desktop, Promplet starts in the center of the primary work area. This avoids losing the palette after monitor changes.

## Testing

Console tests cover:

- Default JSON creation preserves the four MVP prompts.
- Invalid JSON is backed up and replaced with defaults.
- Group selection changes the visible prompt list.
- Window state is clamped to the visible desktop contract at the service level.
- XAML uses data-bound group tabs and prompt buttons rather than static group labels.
