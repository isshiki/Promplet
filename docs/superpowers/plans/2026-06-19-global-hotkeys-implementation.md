# Global Hotkeys Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add global hotkeys for palette show/hide and direct paste of visible prompt buttons 1-10.

**Architecture:** Add testable hotkey definition classes for shortcut mapping, a Win32-backed `GlobalHotKeyService` for registration and dispatch, and `MainWindow` action handlers that reuse existing show/hide and paste behavior.

**Tech Stack:** C# WPF, .NET 10, Win32 `RegisterHotKey`, existing console test harness.

---

## Files

- Create: `Promplet/Services/GlobalHotKeyDefinitions.cs`
- Create: `Promplet/Services/GlobalHotKeyService.cs`
- Modify: `Promplet/Win32/NativeMethods.cs`
- Modify: `Promplet/MainWindow.xaml.cs`
- Modify: `Promplet.Tests/Program.cs`
- Modify: `README.md`
- Modify: `docs/development.md`
- Modify: `docs/spec-v0.1.md`

## Tasks

### Task 1: Hotkey Definitions

- [x] Add failing tests that verify the default hotkey map contains `Ctrl+Alt+Space` for toggle and `Ctrl+NumPad1..0` for visible button slots 1-10.
- [x] Run `dotnet run --project .\Promplet.Tests\Promplet.Tests.csproj` and confirm the tests fail because hotkey definitions do not exist.
- [x] Add `GlobalHotKeyDefinitions`, `GlobalHotKeyDefinition`, and `GlobalHotKeyAction`.
- [x] Run the test command again and confirm the hotkey definition tests pass.

### Task 2: Win32 Registration Service

- [x] Add source-contract tests that `NativeMethods` exposes `RegisterHotKey`, `UnregisterHotKey`, and `WM_HOTKEY`, and that hotkey definitions expose numpad virtual keys and modifier constants.
- [x] Run the test command and confirm the tests fail.
- [x] Add Win32 constants and P/Invoke declarations.
- [x] Add `GlobalHotKeyService` with registration, unregistration, `WM_HOTKEY` dispatch, and non-throwing registration failure behavior.
- [x] Run the test command again and confirm the registration service tests pass.

### Task 3: MainWindow Integration

- [x] Add source-contract tests that `MainWindow` owns `GlobalHotKeyService`, handles toggle hotkeys, and pastes visible button slots.
- [x] Run the test command and confirm the tests fail.
- [x] Refactor prompt paste into a shared method.
- [x] Wire hotkey events to show/hide and visible-button paste actions.
- [x] Dispose hotkey service when the window closes.
- [x] Run the test command again and confirm all tests pass.

### Task 4: Docs, Build, Commit

- [x] Update README, development notes, and spec for the accepted hotkeys and current limitations.
- [x] Run `dotnet build .\Promplet.slnx --configuration Release`.
- [x] Run `dotnet run --project .\Promplet.Tests\Promplet.Tests.csproj --configuration Release --no-build`.
- [x] Commit the implementation and docs.
- [x] Push `main` to GitHub.

## Self-Review

- Spec coverage: Covers show/hide and 1-10 visible button paste hotkeys.
- Deferred intentionally: editable hotkey settings, left/right modifier distinction, low-level keyboard hooks, and conflict UI.
- Placeholder scan: no placeholders remain.
