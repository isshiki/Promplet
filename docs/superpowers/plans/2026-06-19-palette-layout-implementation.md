# Palette Layout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the fixed `A案 v8` palette design in the WPF MVP.

**Architecture:** Keep the current fixed prompt catalog and paste behavior. Replace only the main palette presentation with a vertically stacked prompt list, flat group tabs, subtle whole-row button borders, and no visible internal identifiers. Add a small WPF contract test around the window surface so future changes do not accidentally return to the horizontal toolbar shape.

**Tech Stack:** C# WPF, .NET 10, existing `Promplet.Tests` console harness.

---

## Files

- Modify: `Promplet/MainWindow.xaml`
- Modify: `Promplet.Tests/Program.cs`

## Tasks

### Task 1: Add Palette UI Contract Test

- [x] Rewrite the test harness to run on STA.
- [x] Add a failing XAML contract test that verifies:
  - `ResizeMode` is `CanResize`.
  - `SizeToContent` is `Height`.
  - Initial width is greater than the old compact toolbar width.
  - `PromptButtons` uses a vertical `StackPanel` items panel.
  - The close button remains present.
- [x] Run `dotnet run --project .\Promplet.Tests\Promplet.Tests.csproj`.
- [x] Confirm it fails against the current horizontal toolbar UI.

### Task 2: Implement v8 Palette Layout

- [x] Replace `MainWindow.xaml` with the approved layout:
  - Light, quiet palette surface.
  - Top drag handle, flat tabs, and subtle `X`.
  - Vertical prompt rows.
  - Whole-row prompt button border.
  - Prompt name and faint content preview only.
  - No visible internal identifiers.
  - No left/right guide lines.
- [x] Keep left click paste and right click copy.
- [x] Keep `X` as MVP app exit.
- [x] Use `SizeToContent="Height"` and `ResizeMode="CanResize"` so height follows content while the width can be resized.

### Task 3: Verify and Commit

- [x] Run `dotnet build .\Promplet.slnx --configuration Release`.
- [x] Run `dotnet run --project .\Promplet.Tests\Promplet.Tests.csproj --configuration Release --no-build`.
- [ ] Commit the plan, tests, and WPF layout.
- [ ] Push `main` to GitHub.

## Self-Review

- Spec coverage: Implements the main palette portion of `docs/superpowers/specs/2026-06-19-palette-layout-design.md`.
- Deferred intentionally: Prompt Library UI, JSON persistence, tray behavior, global hotkeys, and real group switching.
- Placeholder scan: no placeholders remain.
- Type consistency: uses existing `PromptButton`, `PromptCatalog`, and `MainWindow` names.
