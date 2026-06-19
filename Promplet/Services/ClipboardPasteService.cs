using System.Windows;

namespace Promplet.Services;

public sealed class ClipboardPasteService
{
    private readonly KeyboardInputService _keyboardInputService = new();

    public async Task PasteTextAsync(string text)
    {
        var previousText = TryGetClipboardText();

        Clipboard.SetText(text);
        await Task.Delay(50).ConfigureAwait(true);
        _keyboardInputService.SendCtrlV();

        if (previousText is not null)
        {
            await Task.Delay(200).ConfigureAwait(true);
            Clipboard.SetText(previousText);
        }
    }

    public void CopyText(string text)
    {
        Clipboard.SetText(text);
    }

    private static string? TryGetClipboardText()
    {
        try
        {
            return Clipboard.ContainsText() ? Clipboard.GetText() : null;
        }
        catch
        {
            return null;
        }
    }
}
