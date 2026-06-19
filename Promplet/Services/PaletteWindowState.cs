using Promplet.Models;

namespace Promplet.Services;

public static class PaletteWindowState
{
    public const double MinimumWidth = 420;
    public const double MaximumWidth = 1200;

    public static PromptWindowState Normalize(
        PromptWindowState state,
        double screenLeft,
        double screenTop,
        double screenWidth,
        double screenHeight)
    {
        var width = Math.Clamp(state.Width, MinimumWidth, MaximumWidth);
        var screenRight = screenLeft + screenWidth;
        var screenBottom = screenTop + screenHeight;
        var hasVisiblePosition = state.Left is double left
            && state.Top is double top
            && left >= screenLeft
            && top >= screenTop
            && left <= screenRight - 80
            && top <= screenBottom - 40;

        return new PromptWindowState
        {
            Left = hasVisiblePosition ? state.Left : null,
            Top = hasVisiblePosition ? state.Top : null,
            Width = width
        };
    }
}
