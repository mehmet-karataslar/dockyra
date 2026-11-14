using System.Windows;
using System.Windows.Media;
using Dockyra.Domain.Enums;

namespace Dockyra.Presentation.Wpf.DesignTokens;

public interface IThemeService
{
    ThemeKind CurrentTheme { get; }
    string CurrentAccentHex { get; }
    string CurrentSurfaceHex { get; }
    void ApplyTheme(ThemeKind theme);
    void ApplyColorPalette(string accentHex, string? surfaceHex = null);
}

public sealed class WpfThemeService : IThemeService
{
    public ThemeKind CurrentTheme { get; private set; } = ThemeKind.Dark;
    public string CurrentAccentHex { get; private set; } = "#3B82F6";
    public string CurrentSurfaceHex { get; private set; } = "#EA18181B";

    public void ApplyTheme(ThemeKind theme)
    {
        CurrentTheme = theme;
        if (System.Windows.Application.Current is null)
        {
            return;
        }

        string uriPath = theme == ThemeKind.Light
            ? "pack://application:,,,/Dockyra.Presentation.Wpf;component/DesignTokens/FluentLightTheme.xaml"
            : "pack://application:,,,/Dockyra.Presentation.Wpf;component/DesignTokens/FluentDarkTheme.xaml";

        var dict = new ResourceDictionary { Source = new Uri(uriPath, UriKind.Absolute) };
        var merged = System.Windows.Application.Current.Resources.MergedDictionaries;

        ResourceDictionary? existing = merged.FirstOrDefault(d =>
            d.Source is not null &&
            (d.Source.OriginalString.Contains("FluentDarkTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
             d.Source.OriginalString.Contains("FluentLightTheme.xaml", StringComparison.OrdinalIgnoreCase)));

        if (existing is not null)
        {
            int idx = merged.IndexOf(existing);
            merged[idx] = dict;
        }
        else
        {
            merged.Add(dict);
        }

        ApplyColorPalette(CurrentAccentHex, theme == ThemeKind.Light ? "#F2F4F4F5" : CurrentSurfaceHex);
    }

    public void ApplyColorPalette(string accentHex, string? surfaceHex = null)
    {
        if (!string.IsNullOrWhiteSpace(accentHex))
        {
            CurrentAccentHex = accentHex.Trim();
        }

        if (!string.IsNullOrWhiteSpace(surfaceHex))
        {
            CurrentSurfaceHex = surfaceHex.Trim();
        }

        if (System.Windows.Application.Current is null)
        {
            return;
        }

        try
        {
            if (ColorConverter.ConvertFromString(CurrentAccentHex) is Color accentColor)
            {
                System.Windows.Application.Current.Resources["Token.Color.Accent.Primary"] = accentColor;
                System.Windows.Application.Current.Resources["Token.Brush.Accent"] = new SolidColorBrush(accentColor);
            }
        }
        catch
        {
            // Keep existing brush if invalid hex input
        }

        if (!string.IsNullOrWhiteSpace(surfaceHex))
        {
            try
            {
                if (ColorConverter.ConvertFromString(CurrentSurfaceHex) is Color surfaceColor)
                {
                    System.Windows.Application.Current.Resources["Theme.Brush.Surface"] = new SolidColorBrush(surfaceColor);
                }
            }
            catch
            {
                // Keep existing surface brush if invalid hex input
            }
        }
    }
}
