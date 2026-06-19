# Tray Resident Icon Design

## Status

Approved for implementation on 2026-06-19 by the user's instruction to use the provided icon assets and continue with the planned tray resident work.

## Scope

This pass adds the provided Promplet icon as a durable project asset and changes the MVP from an exit-on-close palette into a tray-resident app.

Included:

- Store the supplied SVG and PNG under `Promplet/Assets`.
- Generate a Windows `.ico` from the supplied PNG and use it for the executable and tray icon.
- Add a task tray icon.
- Change the palette `X` button from exit to hide.
- Add tray menu commands: show palette, hide palette, reload prompts, exit.
- Reload prompts from `%APPDATA%\Promplet\prompts.json` without restarting the app.

Out of scope:

- Global hotkeys.
- Button manager UI.
- Installer/package work.
- Custom tray menu styling.

## Behavior

Promplet starts by showing the palette and creating a tray icon. Closing the palette with `X` hides it but keeps the process running. The tray icon menu can show or hide the palette. Exiting is explicit through the tray menu.

The reload command reuses `PromptStore.LoadOrCreate()` and replaces the palette view model content. If JSON is missing or invalid, the existing backup/default behavior applies.

## Architecture

`TrayIconService` owns the WinForms `NotifyIcon`, its context menu, and disposal. `MainWindow` remains the owner of palette behavior and supplies callbacks to the tray service.

`PaletteViewModel` gains a `LoadDocument` method so the current WPF bindings can refresh groups and visible buttons after JSON reload without rebuilding the window.

## Icon Assets

The SVG is stored as the source asset. The PNG is stored as the supplied raster preview/source. The `.ico` is generated from the PNG at common Windows icon sizes and referenced from `Promplet.csproj` through `ApplicationIcon`.

## Testing

Automated tests cover:

- Icon assets exist and the `.ico` has a valid ICO header.
- Project file enables Windows Forms and references the app icon.
- XAML and code contract: the close button is hide-oriented, not exit-oriented.
- `PaletteViewModel.LoadDocument` replaces groups and selected buttons.
- Tray service source includes the expected menu command labels and disposal path.
