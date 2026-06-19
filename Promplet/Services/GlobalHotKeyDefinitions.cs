using Promplet.Models;

namespace Promplet.Services;

[Flags]
public enum GlobalHotKeyModifiers : uint
{
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008,
    NoRepeat = 0x4000
}

public enum GlobalHotKeyVirtualKeys : uint
{
    PageUp = 0x21,
    PageDown = 0x22,
    End = 0x23,
    Home = 0x24,
    Left = 0x25,
    Up = 0x26,
    Right = 0x27,
    Down = 0x28,
    Insert = 0x2D,
    Delete = 0x2E,
    D0 = 0x30,
    D1 = 0x31,
    D2 = 0x32,
    D3 = 0x33,
    D4 = 0x34,
    D5 = 0x35,
    D6 = 0x36,
    D7 = 0x37,
    D8 = 0x38,
    D9 = 0x39,
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,
    Space = 0x20,
    NumPad0 = 0x60,
    NumPad1 = 0x61,
    NumPad2 = 0x62,
    NumPad3 = 0x63,
    NumPad4 = 0x64,
    NumPad5 = 0x65,
    NumPad6 = 0x66,
    NumPad7 = 0x67,
    NumPad8 = 0x68,
    NumPad9 = 0x69,
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B
}

public enum GlobalHotKeyActionKind
{
    TogglePalette,
    PasteVisibleButton
}

public readonly record struct GlobalHotKeyAction(
    GlobalHotKeyActionKind Kind,
    int VisibleButtonIndex)
{
    public static GlobalHotKeyAction TogglePalette { get; } = new(GlobalHotKeyActionKind.TogglePalette, -1);

    public static GlobalHotKeyAction PasteVisibleButton(int zeroBasedIndex)
    {
        return new GlobalHotKeyAction(GlobalHotKeyActionKind.PasteVisibleButton, zeroBasedIndex);
    }
}

public sealed record GlobalHotKeyDefinition(
    int Id,
    GlobalHotKeyModifiers Modifiers,
    GlobalHotKeyVirtualKeys VirtualKey,
    GlobalHotKeyAction Action,
    string DisplayText);

public static class GlobalHotKeyDefinitions
{
    private static readonly Dictionary<string, (GlobalHotKeyVirtualKeys VirtualKey, string DisplayText)> SupportedKeys =
        new Dictionary<string, (GlobalHotKeyVirtualKeys VirtualKey, string DisplayText)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Space"] = (GlobalHotKeyVirtualKeys.Space, "Space"),
            ["PageUp"] = (GlobalHotKeyVirtualKeys.PageUp, "PageUp"),
            ["PageDown"] = (GlobalHotKeyVirtualKeys.PageDown, "PageDown"),
            ["End"] = (GlobalHotKeyVirtualKeys.End, "End"),
            ["Home"] = (GlobalHotKeyVirtualKeys.Home, "Home"),
            ["Left"] = (GlobalHotKeyVirtualKeys.Left, "Left"),
            ["Up"] = (GlobalHotKeyVirtualKeys.Up, "Up"),
            ["Right"] = (GlobalHotKeyVirtualKeys.Right, "Right"),
            ["Down"] = (GlobalHotKeyVirtualKeys.Down, "Down"),
            ["Insert"] = (GlobalHotKeyVirtualKeys.Insert, "Insert"),
            ["Delete"] = (GlobalHotKeyVirtualKeys.Delete, "Delete"),
            ["D0"] = (GlobalHotKeyVirtualKeys.D0, "0"),
            ["D1"] = (GlobalHotKeyVirtualKeys.D1, "1"),
            ["D2"] = (GlobalHotKeyVirtualKeys.D2, "2"),
            ["D3"] = (GlobalHotKeyVirtualKeys.D3, "3"),
            ["D4"] = (GlobalHotKeyVirtualKeys.D4, "4"),
            ["D5"] = (GlobalHotKeyVirtualKeys.D5, "5"),
            ["D6"] = (GlobalHotKeyVirtualKeys.D6, "6"),
            ["D7"] = (GlobalHotKeyVirtualKeys.D7, "7"),
            ["D8"] = (GlobalHotKeyVirtualKeys.D8, "8"),
            ["D9"] = (GlobalHotKeyVirtualKeys.D9, "9"),
            ["NumPad0"] = (GlobalHotKeyVirtualKeys.NumPad0, "NumPad0"),
            ["NumPad1"] = (GlobalHotKeyVirtualKeys.NumPad1, "NumPad1"),
            ["NumPad2"] = (GlobalHotKeyVirtualKeys.NumPad2, "NumPad2"),
            ["NumPad3"] = (GlobalHotKeyVirtualKeys.NumPad3, "NumPad3"),
            ["NumPad4"] = (GlobalHotKeyVirtualKeys.NumPad4, "NumPad4"),
            ["NumPad5"] = (GlobalHotKeyVirtualKeys.NumPad5, "NumPad5"),
            ["NumPad6"] = (GlobalHotKeyVirtualKeys.NumPad6, "NumPad6"),
            ["NumPad7"] = (GlobalHotKeyVirtualKeys.NumPad7, "NumPad7"),
            ["NumPad8"] = (GlobalHotKeyVirtualKeys.NumPad8, "NumPad8"),
            ["NumPad9"] = (GlobalHotKeyVirtualKeys.NumPad9, "NumPad9")
        };

    static GlobalHotKeyDefinitions()
    {
        foreach (var letter in Enumerable.Range('A', 26).Select(value => (char)value))
        {
            var key = letter.ToString();
            SupportedKeys[key] = ((GlobalHotKeyVirtualKeys)letter, key);
        }

        for (var index = 1; index <= 11; index++)
        {
            var key = $"F{index}";
            SupportedKeys[key] = ((GlobalHotKeyVirtualKeys)(0x6F + index), key);
        }
    }

    public static IReadOnlyList<GlobalHotKeyDefinition> CreateDefault()
    {
        return Create(HotKeySettings.CreateDefault());
    }

    public static IReadOnlyList<GlobalHotKeyDefinition> Create(HotKeySettings settings)
    {
        var definitions = new List<GlobalHotKeyDefinition>();

        if (TryCreateDefinition(1, settings.TogglePalette, GlobalHotKeyAction.TogglePalette, out var toggleDefinition))
        {
            definitions.Add(toggleDefinition);
        }

        for (var index = 0; index < Math.Min(settings.PasteButtons.Count, PromptStore.MaximumButtonsPerGroup); index++)
        {
            if (TryCreateDefinition(
                    index + 2,
                    settings.PasteButtons[index],
                    GlobalHotKeyAction.PasteVisibleButton(index),
                    out var pasteDefinition))
            {
                definitions.Add(pasteDefinition);
            }
        }

        return definitions;
    }

    public static bool IsSupportedKey(string? key)
    {
        return key is not null && SupportedKeys.ContainsKey(key);
    }

    public static string NormalizeKeyName(string key)
    {
        return SupportedKeys.TryGetValue(key, out var keyDefinition)
            ? keyDefinition.VirtualKey.ToString()
            : key;
    }

    public static string FormatGesture(HotKeyGesture gesture)
    {
        if (!gesture.Enabled)
        {
            return "Disabled";
        }

        if (!TryGetKeyDefinition(gesture.Key, out _, out var keyDisplayText))
        {
            return "Invalid";
        }

        var parts = new List<string>();
        if (gesture.Control)
        {
            parts.Add("Ctrl");
        }

        if (gesture.Alt)
        {
            parts.Add("Alt");
        }

        if (gesture.Shift)
        {
            parts.Add("Shift");
        }

        if (gesture.Windows)
        {
            parts.Add("Win");
        }

        parts.Add(keyDisplayText);
        return string.Join("+", parts);
    }

    private static bool TryCreateDefinition(
        int id,
        HotKeyGesture gesture,
        GlobalHotKeyAction action,
        out GlobalHotKeyDefinition definition)
    {
        definition = default!;

        if (!gesture.Enabled || !TryGetKeyDefinition(gesture.Key, out var virtualKey, out _))
        {
            return false;
        }

        var modifiers = GlobalHotKeyModifiers.NoRepeat;
        if (gesture.Control)
        {
            modifiers |= GlobalHotKeyModifiers.Control;
        }

        if (gesture.Alt)
        {
            modifiers |= GlobalHotKeyModifiers.Alt;
        }

        if (gesture.Shift)
        {
            modifiers |= GlobalHotKeyModifiers.Shift;
        }

        if (gesture.Windows)
        {
            modifiers |= GlobalHotKeyModifiers.Windows;
        }

        if (modifiers == GlobalHotKeyModifiers.NoRepeat)
        {
            return false;
        }

        definition = new GlobalHotKeyDefinition(
            Id: id,
            Modifiers: modifiers,
            VirtualKey: virtualKey,
            Action: action,
            DisplayText: FormatGesture(gesture));
        return true;
    }

    private static bool TryGetKeyDefinition(
        string key,
        out GlobalHotKeyVirtualKeys virtualKey,
        out string displayText)
    {
        if (SupportedKeys.TryGetValue(key, out var keyDefinition))
        {
            virtualKey = keyDefinition.VirtualKey;
            displayText = keyDefinition.DisplayText;
            return true;
        }

        virtualKey = default;
        displayText = string.Empty;
        return false;
    }
}
