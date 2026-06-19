namespace Promplet.Services;

[Flags]
public enum GlobalHotKeyModifiers : uint
{
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    NoRepeat = 0x4000
}

public enum GlobalHotKeyVirtualKeys : uint
{
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
    NumPad9 = 0x69
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
    public static IReadOnlyList<GlobalHotKeyDefinition> CreateDefault()
    {
        var definitions = new List<GlobalHotKeyDefinition>
        {
            new(
                Id: 1,
                Modifiers: GlobalHotKeyModifiers.Control | GlobalHotKeyModifiers.Alt | GlobalHotKeyModifiers.NoRepeat,
                VirtualKey: GlobalHotKeyVirtualKeys.Space,
                Action: GlobalHotKeyAction.TogglePalette,
                DisplayText: "Ctrl+Alt+Space")
        };

        var numpadKeys = new[]
        {
            GlobalHotKeyVirtualKeys.NumPad1,
            GlobalHotKeyVirtualKeys.NumPad2,
            GlobalHotKeyVirtualKeys.NumPad3,
            GlobalHotKeyVirtualKeys.NumPad4,
            GlobalHotKeyVirtualKeys.NumPad5,
            GlobalHotKeyVirtualKeys.NumPad6,
            GlobalHotKeyVirtualKeys.NumPad7,
            GlobalHotKeyVirtualKeys.NumPad8,
            GlobalHotKeyVirtualKeys.NumPad9,
            GlobalHotKeyVirtualKeys.NumPad0
        };

        for (var index = 0; index < numpadKeys.Length; index++)
        {
            definitions.Add(new GlobalHotKeyDefinition(
                Id: index + 2,
                Modifiers: GlobalHotKeyModifiers.Control | GlobalHotKeyModifiers.Shift | GlobalHotKeyModifiers.NoRepeat,
                VirtualKey: numpadKeys[index],
                Action: GlobalHotKeyAction.PasteVisibleButton(index),
                DisplayText: $"Ctrl+Shift+NumPad{(index + 1) % 10}"));
        }

        return definitions;
    }
}
