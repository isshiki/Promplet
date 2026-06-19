using Microsoft.Win32;
using Promplet.Models;
using WpfApplication = System.Windows.Application;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;
using WpfSolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Promplet.Services;

public static class AppearanceService
{
    public static void Apply(string themeMode)
    {
        var useDark = ShouldUseDark(themeMode);

        SetBrush("PrompletPaletteOuterBackgroundBrush", useDark ? "#15171A" : "#F6F7F9");
        SetBrush("PrompletPaletteOuterBorderBrush", useDark ? "#2A2E34" : "#D8DEE6");
        SetBrush("PrompletSurfaceBackgroundBrush", useDark ? "#1A1D21" : "#FBFCFD");
        SetBrush("PrompletSurfaceBorderBrush", useDark ? "#2E333A" : "#D9DFE7");
        SetBrush("PrompletHeaderBackgroundBrush", useDark ? "#191C20" : "#F1F3F6");
        SetBrush("PrompletHeaderBorderBrush", useDark ? "#2A2F36" : "#E1E5EB");
        SetBrush("PrompletChromeForegroundBrush", useDark ? "#767E8B" : "#A5ADBA");
        SetBrush("PrompletChromeMutedBrush", useDark ? "#6C7480" : "#A7AFBB");
        SetBrush("PrompletTabForegroundBrush", useDark ? "#858E9C" : "#778191");
        SetBrush("PrompletTabHoverForegroundBrush", useDark ? "#D7DCE3" : "#374151");
        SetBrush("PrompletTabSelectedBackgroundBrush", useDark ? "#20242A" : "#FFFFFF");
        SetBrush("PrompletTextPrimaryBrush", useDark ? "#E4E8EE" : "#1F2937");
        SetBrush("PrompletTextSecondaryBrush", useDark ? "#8B95A3" : "#8F9AA8");
        SetBrush("PrompletPromptButtonBackgroundBrush", useDark ? "#20242A" : "#FFFFFF");
        SetBrush("PrompletPromptButtonBorderBrush", useDark ? "#343A43" : "#E2E8F0");
        SetBrush("PrompletPromptButtonHoverBackgroundBrush", useDark ? "#252A31" : "#F8FAFC");
        SetBrush("PrompletPromptButtonHoverBorderBrush", useDark ? "#414854" : "#D7DEE8");
        SetBrush("PrompletPromptButtonPressedBackgroundBrush", useDark ? "#2B3038" : "#EEF2F7");
        SetBrush("PrompletDialogBackgroundBrush", useDark ? "#1A1D21" : "#FBFCFD");
        SetBrush("PrompletDialogPanelBackgroundBrush", useDark ? "#20242A" : "#F6F7F9");
        SetBrush("PrompletDialogInputBackgroundBrush", useDark ? "#252A31" : "#FFFFFF");
        SetBrush("PrompletAccentBrush", "#2AA8D6");
    }

    private static bool ShouldUseDark(string themeMode)
    {
        if (string.Equals(themeMode, PromptThemeModes.Dark, StringComparison.Ordinal))
        {
            return true;
        }

        if (string.Equals(themeMode, PromptThemeModes.Light, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            var value = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme",
                1);

            return value is int intValue && intValue == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void SetBrush(string key, string color)
    {
        var application = WpfApplication.Current;

        if (application is null)
        {
            return;
        }

        application.Resources[key] = new WpfSolidColorBrush((WpfColor)WpfColorConverter.ConvertFromString(color));
    }
}
