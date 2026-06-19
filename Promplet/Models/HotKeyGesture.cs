namespace Promplet.Models;

public sealed class HotKeySettings
{
    public HotKeyGesture TogglePalette { get; set; } = HotKeyGesture.CreateDefaultTogglePalette();

    public List<HotKeyGesture> PasteButtons { get; set; } = HotKeyGesture.CreateDefaultPasteButtons();

    public static HotKeySettings CreateDefault()
    {
        return new HotKeySettings
        {
            TogglePalette = HotKeyGesture.CreateDefaultTogglePalette(),
            PasteButtons = HotKeyGesture.CreateDefaultPasteButtons()
        };
    }

    public HotKeySettings Clone()
    {
        return new HotKeySettings
        {
            TogglePalette = TogglePalette.Clone(),
            PasteButtons = PasteButtons.Select(gesture => gesture.Clone()).ToList()
        };
    }
}

public sealed class HotKeyGesture
{
    public bool Enabled { get; set; } = true;

    public bool Control { get; set; }

    public bool Alt { get; set; }

    public bool Shift { get; set; }

    public bool Windows { get; set; }

    public string Key { get; set; } = "Space";

    public HotKeyGesture Clone()
    {
        return new HotKeyGesture
        {
            Enabled = Enabled,
            Control = Control,
            Alt = Alt,
            Shift = Shift,
            Windows = Windows,
            Key = Key
        };
    }

    public static HotKeyGesture CreateDefaultTogglePalette()
    {
        return new HotKeyGesture
        {
            Enabled = true,
            Control = true,
            Alt = true,
            Key = "Space"
        };
    }

    public static List<HotKeyGesture> CreateDefaultPasteButtons()
    {
        return Enumerable.Range(0, 10)
            .Select(CreateDefaultPasteButton)
            .ToList();
    }

    public static HotKeyGesture CreateDefaultPasteButton(int zeroBasedIndex)
    {
        var number = (zeroBasedIndex + 1) % 10;

        return new HotKeyGesture
        {
            Enabled = true,
            Control = true,
            Key = $"NumPad{number}"
        };
    }
}

public static class PromptThemeModes
{
    public const string System = "System";
    public const string Light = "Light";
    public const string Dark = "Dark";

    public static bool IsSupported(string? themeMode)
    {
        return string.Equals(themeMode, System, StringComparison.Ordinal)
            || string.Equals(themeMode, Light, StringComparison.Ordinal)
            || string.Equals(themeMode, Dark, StringComparison.Ordinal);
    }
}
