using System.Windows;
using Dockyra.Domain.Enums;

namespace Dockyra.Presentation.Wpf.DesignTokens;

public interface IThemeService
{
    ThemeKind CurrentTheme { get; }
    void ApplyTheme(ThemeKind theme);
}

public sealed class WpfThemeService : IThemeService
{
    public ThemeKind CurrentTheme { get; private set; } = ThemeKind.Dark;

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
    }
}

