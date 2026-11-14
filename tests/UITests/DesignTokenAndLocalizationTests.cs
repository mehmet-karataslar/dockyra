using System.Windows;
using Xunit;

namespace Dockyra.UITests;

public sealed class DesignTokenAndLocalizationTests
{
    static DesignTokenAndLocalizationTests()
    {
        EnsureWpfApplicationInitialized();
    }

    private static void EnsureWpfApplicationInitialized()
    {
        if (System.Windows.Application.Current is null)
        {
            // Initializing System.Windows.Application registers the pack:// WebRequest prefix for ResourceDictionary loading
            _ = new System.Windows.Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
        }
    }

    [Fact]
    public void LocalizationDictionaries_TurkishAndEnglish_ContainIdenticalKeysAndNonEmptyStrings()
    {
        var trDict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Dockyra.Presentation.Wpf;component/Localization/Strings.tr-TR.xaml", UriKind.Absolute)
        };

        var enDict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Dockyra.Presentation.Wpf;component/Localization/Strings.en-US.xaml", UriKind.Absolute)
        };

        HashSet<string> trKeys = trDict.Keys.Cast<object>().Select(k => k.ToString()!).ToHashSet(StringComparer.Ordinal);
        HashSet<string> enKeys = enDict.Keys.Cast<object>().Select(k => k.ToString()!).ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(trKeys);
        Assert.Equal(trKeys, enKeys);

        foreach (string key in trKeys)
        {
            Assert.False(string.IsNullOrWhiteSpace(trDict[key] as string), $"Empty Turkish translation for {key}");
            Assert.False(string.IsNullOrWhiteSpace(enDict[key] as string), $"Empty English translation for {key}");
        }
    }

    [Fact]
    public void DesignTokens_DarkAndLightThemes_ContainRequiredBrushKeys()
    {
        var darkDict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Dockyra.Presentation.Wpf;component/DesignTokens/FluentDarkTheme.xaml", UriKind.Absolute)
        };

        var lightDict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Dockyra.Presentation.Wpf;component/DesignTokens/FluentLightTheme.xaml", UriKind.Absolute)
        };

        string[] requiredKeys =
        [
            "Theme.Brush.Surface",
            "Theme.Brush.Card",
            "Theme.Brush.Border",
            "Theme.Brush.TextPrimary",
            "Theme.Brush.TextSecondary"
        ];

        foreach (string key in requiredKeys)
        {
            Assert.True(darkDict.Contains(key), $"Dark theme missing key: {key}");
            Assert.True(lightDict.Contains(key), $"Light theme missing key: {key}");
        }
    }
}
