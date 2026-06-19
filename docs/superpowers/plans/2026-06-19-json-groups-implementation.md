# JSON Groups Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the fixed prompt list with JSON-backed groups and make the palette tabs switch real prompt groups.

**Architecture:** Add serializable document models for prompt storage, a `PromptStore` service for `%APPDATA%\Promplet\prompts.json`, and a `PaletteViewModel` for selected-group state. Keep paste behavior in `MainWindow` and bind the existing approved layout to the view model.

**Tech Stack:** C# WPF, .NET 10, `System.Text.Json`, existing console test harness.

---

## Files

- Create: `Promplet/Models/PromptDocument.cs`
- Create: `Promplet/Services/PromptStore.cs`
- Create: `Promplet/ViewModels/PaletteViewModel.cs`
- Modify: `Promplet/Services/PromptCatalog.cs`
- Modify: `Promplet/MainWindow.xaml`
- Modify: `Promplet/MainWindow.xaml.cs`
- Modify: `Promplet.Tests/Program.cs`
- Modify: `docs/spec-v0.1.md`
- Modify: `README.md`

## Tasks

### Task 1: JSON Storage

- [x] Add failing tests in `Promplet.Tests/Program.cs` for default JSON creation and invalid JSON backup.
- [x] Run `dotnet run --project .\Promplet.Tests\Promplet.Tests.csproj` and confirm the new tests fail because `PromptStore` does not exist.
- [x] Add `PromptDocument` model classes and `PromptStore` with `LoadOrCreate`, `Save`, default creation, validation, and backup behavior.
- [x] Run the test command again and confirm the JSON storage tests pass.

### Task 2: Palette View Model

- [x] Add a failing test that selecting a second group changes the visible prompt buttons and stores the selected group id.
- [x] Run the test command and confirm it fails because `PaletteViewModel` does not exist.
- [x] Add `PaletteViewModel` with observable groups, selected group, visible buttons, and selection command.
- [x] Run the test command again and confirm the view model test passes.

### Task 3: WPF Binding

- [x] Add or update the XAML contract test so `GroupTabs` is an `ItemsControl`, static group labels are gone, and prompt buttons remain vertical.
- [x] Run the test command and confirm it fails against the current static tab XAML.
- [x] Update `MainWindow.xaml` to bind group tabs and prompt buttons to `PaletteViewModel`.
- [x] Update `MainWindow.xaml.cs` to load JSON, create the view model, save window state on close, and keep left-click paste/right-click copy.
- [x] Run the test command again and confirm all tests pass.

### Task 4: Docs, Build, Commit

- [x] Update `README.md` and `docs/spec-v0.1.md` to reflect JSON-backed groups and current MVP limits.
- [x] Run `dotnet build .\Promplet.slnx --configuration Release`.
- [x] Run `dotnet run --project .\Promplet.Tests\Promplet.Tests.csproj --configuration Release --no-build`.
- [ ] Commit the implementation and docs.
- [ ] Push `main` to GitHub.

## Self-Review

- Spec coverage: Covers JSON definition loading, group switching, selected group persistence, and window state persistence. It deliberately leaves tray, hotkey, manager UI, and advanced paste settings for later phases.
- Placeholder scan: no placeholders remain.
- Type consistency: `PromptDocument`, `PromptStore`, and `PaletteViewModel` are the only new production entry points used by the plan.
