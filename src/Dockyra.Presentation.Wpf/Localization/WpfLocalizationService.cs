using System.Windows;

namespace Dockyra.Presentation.Wpf.Localization;

public interface ILocalizationService
{
    string CurrentLanguageCode { get; }
    void ApplyLanguage(string languageCode);
    string GetString(string resourceKey);
}

public sealed class WpfLocalizationService : ILocalizationService
{
    private ResourceDictionary _activeDictionary;

    public WpfLocalizationService()
    {
        CurrentLanguageCode = "tr-TR";
        _activeDictionary = LoadDictionary(CurrentLanguageCode);
    }

    public string CurrentLanguageCode { get; private set; }

    public void ApplyLanguage(string languageCode)
    {
        CurrentLanguageCode = string.Equals(languageCode, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "tr-TR";

        _activeDictionary = LoadDictionary(CurrentLanguageCode);

        if (System.Windows.Application.Current is null)
        {
            return;
        }

        var merged = System.Windows.Application.Current.Resources.MergedDictionaries;
        ResourceDictionary? existing = merged.FirstOrDefault(d =>
            d.Source is not null &&
            d.Source.OriginalString.Contains("Localization/Strings.", StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            int idx = merged.IndexOf(existing);
            merged[idx] = _activeDictionary;
        }
        else
        {
            merged.Add(_activeDictionary);
        }
    }

    public string GetString(string resourceKey)
    {
        if (_activeDictionary.Contains(resourceKey) && _activeDictionary[resourceKey] is string val)
        {
            return val;
        }

        return resourceKey;
    }

    private static ResourceDictionary LoadDictionary(string languageCode)
    {
        _ = System.IO.Packaging.PackUriHelper.UriSchemePack;
        string uriPath = $"pack://application:,,,/Dockyra.Presentation.Wpf;component/Localization/Strings.{languageCode}.xaml";
        return new ResourceDictionary { Source = new Uri(uriPath, UriKind.Absolute) };
    }
}

