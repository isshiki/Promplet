# Tray Resident Icon Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the supplied Promplet icon assets and make the app tray-resident with show, hide, reload, and exit commands.

**Architecture:** Store supplied assets under `Promplet/Assets`, generate an `.ico`, enable WinForms for `NotifyIcon`, and add a `TrayIconService` owned by `MainWindow`. Keep JSON loading in `PromptStore`; refresh the UI through `PaletteViewModel.LoadDocument`.

**Tech Stack:** C# WPF, .NET 10, Windows Forms `NotifyIcon`, `System.Text.Json`, existing console test harness, Python/Pillow for one-time ICO generation.

---

## Files

- Create: `Promplet/Assets/promplet_icon.svg`
- Create: `Promplet/Assets/promplet_icon.png`
- Create: `Promplet/Assets/promplet_icon.ico`
- Create: `Promplet/Services/TrayIconService.cs`
- Modify: `Promplet/Promplet.csproj`
- Modify: `Promplet/MainWindow.xaml`
- Modify: `Promplet/MainWindow.xaml.cs`
- Modify: `Promplet/ViewModels/PaletteViewModel.cs`
- Modify: `Promplet.Tests/Program.cs`
- Modify: `README.md`
- Modify: `docs/development.md`
- Modify: `docs/spec-v0.1.md`

## Tasks

### Task 1: Icon Assets

- [x] Add failing tests that assert `Promplet/Assets/promplet_icon.svg`, `.png`, and `.ico` exist and the ICO header is valid.
- [x] Run `dotnet run --project .\Promplet.Tests\Promplet.Tests.csproj` and confirm the icon asset test fails.
- [x] Copy the supplied SVG and PNG into `Promplet/Assets`.
- [x] Generate `promplet_icon.ico` from the PNG with common Windows icon sizes.
- [x] Update `Promplet.csproj` with `ApplicationIcon` and `UseWindowsForms`.
- [x] Run the test command again and confirm the icon asset and project contract pass.

### Task 2: Reloadable View Model

- [x] Add a failing test for `PaletteViewModel.LoadDocument` replacing groups and visible buttons.
- [x] Run the test command and confirm it fails.
- [x] Implement `LoadDocument` and remove readonly state that prevents reload.
- [x] Run the test command again and confirm the reload test passes.

### Task 3: Tray Resident UI

- [x] Add contract tests for a hide-oriented close button and tray menu command source.
- [x] Run the test command and confirm the new tray tests fail.
- [x] Implement `TrayIconService` with Show, Hide, Reload prompts, and Exit menu items.
- [x] Update `MainWindow` so `X` hides, tray reload refreshes JSON-backed groups, and exit disposes the tray icon before shutdown.
- [x] Run the test command again and confirm all tests pass.

### Task 4: Docs, Build, Commit

- [x] Update README, development notes, and spec to reflect tray resident behavior and icon assets.
- [x] Run `dotnet build .\Promplet.slnx --configuration Release`.
- [x] Run `dotnet run --project .\Promplet.Tests\Promplet.Tests.csproj --configuration Release --no-build`.
- [x] Commit the implementation and docs.
- [x] Push `main` to GitHub.

## Self-Review

- Spec coverage: Covers provided icon use, tray resident behavior, explicit exit, prompt reload, and close-to-hide behavior.
- Deferred intentionally: global hotkey, manager UI, installer, and custom tray menu styling.
- Placeholder scan: no placeholders remain.
