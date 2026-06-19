# Promplet Palette Layout Design

## Status

Fixed on 2026-06-19 from the visual mockup direction `A案 v8`.

## Scope

This design covers the main Promplet palette layout and the near-term direction for the future button manager. It does not implement JSON persistence, tray behavior, global hotkeys, or the full manager UI yet.

## Main Palette

The palette uses a compact floating window with a quiet, low-contrast appearance so it does not distract while the user works in another app.

- The top area contains a subtle drag handle, group tabs, and a close button.
- Group tabs are visually flat. Only the selected tab receives a minimal active background.
- Group count is capped at 10. Horizontal tab overflow/scroll can be added later.
- The palette body shows 1 to 10 prompt buttons vertically.
- Each prompt button fills the available window width.
- Each prompt row has a fixed height, with the window height calculated from the number of visible rows.
- Prompt rows have a subtle whole-row border and white/light background.
- The prompt name area does not have its own separate border or color block.
- Left and right vertical guide lines are not part of the design.
- The window should support horizontal resizing from the left and right edges.
- The prompt row width follows the current window width.

## Prompt Button Content

User-facing prompt buttons show only the prompt name and a faint preview of the prompt content.

- Prompt name is the only visible label.
- Internal identifiers can exist in data files or code, but are not shown or edited in the UI.
- Suggested prompt name display limit is 10 ASCII characters or 5 full-width characters; overflow is ellipsized.
- Prompt preview is faint gray text.
- Prompt preview can use up to 3 lines.
- Overflowing preview text is clipped and must not increase row height.
- Left click pastes the prompt.
- Right click copies the prompt without pasting.

## Button Manager Direction

The manager should feel close to a bookmark manager.

- Left side lists prompt groups.
- Right side lists prompt buttons in the selected group.
- Prompt items in the manager use the same subtle whole-row border treatment as the palette buttons.
- The normal manager state is a saved/list view, not an always-open edit form.
- Adding or editing a prompt opens a simple dialog.
- The basic prompt edit dialog contains only:
  - Prompt name
  - Prompt content
  - Save
  - Cancel
- Saving closes the dialog and returns to the list state.
- Internal prompt IDs are generated automatically and are not shown to the user.

## Visual Tone

The target visual style is flat, quiet, and utilitarian.

- Prefer light gray, white, and muted text colors for the first implementation.
- Avoid strong color blocks, heavy borders, or visually dominant controls.
- Hover states can be slightly visible but should remain restrained.
- The palette should read as a tool surface rather than a decorative panel.

## Implementation Notes

For the first WPF pass, implement only the palette layout changes. The manager direction should be kept as design guidance for a later phase unless the next implementation plan explicitly includes manager work.
