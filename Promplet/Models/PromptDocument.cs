namespace Promplet.Models;

public sealed class PromptDocument
{
    public PromptAppSettings App { get; set; } = new();

    public PromptWindowState Window { get; set; } = new();

    public List<PromptGroup> Groups { get; set; } = [];
}

public sealed class PromptAppSettings
{
    public bool RestoreClipboard { get; set; } = true;

    public string SelectedGroupId { get; set; } = "ai-chat";

    public string ThemeMode { get; set; } = PromptThemeModes.System;

    public double Opacity { get; set; } = 1;

    public HotKeySettings HotKeys { get; set; } = HotKeySettings.CreateDefault();

    public PromptAppSettings Clone()
    {
        return new PromptAppSettings
        {
            RestoreClipboard = RestoreClipboard,
            SelectedGroupId = SelectedGroupId,
            ThemeMode = ThemeMode,
            Opacity = Opacity,
            HotKeys = HotKeys.Clone()
        };
    }

    public void ResetConfigurableSettings()
    {
        ThemeMode = PromptThemeModes.System;
        Opacity = 1;
        HotKeys = HotKeySettings.CreateDefault();
    }
}

public sealed class PromptWindowState
{
    public double? Left { get; set; }

    public double? Top { get; set; }

    public double Width { get; set; } = 620;
}

public sealed class PromptGroup
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<PromptButton> Buttons { get; set; } = [];
}
