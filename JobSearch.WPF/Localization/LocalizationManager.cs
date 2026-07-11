using System.Globalization;
using System.Windows;
using WpfApplication = System.Windows.Application;

namespace JobSearch.WPF.Localization;

public static class LocalizationManager
{
    private static readonly HashSet<string> _supported = ["ru", "uk", "en"];
    private const string Default = "ru";

    /// <summary>
    /// Detects the current Windows UI culture and applies the matching dictionary.
    /// Falls back to Russian for unsupported cultures.
    /// </summary>
    public static void ApplyCurrentCulture()
    {
        var code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        Apply(_supported.Contains(code) ? code : Default);
    }

    /// <summary>
    /// Loads and activates the ResourceDictionary for the given two-letter culture code.
    /// </summary>
    public static void Apply(string twoLetterCode)
    {
        var uri = new Uri(
            $"pack://application:,,,/Localization/{twoLetterCode}.xaml",
            UriKind.Absolute);

        var next = new ResourceDictionary { Source = uri };

        var dicts = WpfApplication.Current.Resources.MergedDictionaries;
        var existing = dicts.FirstOrDefault(
            d => d.Source?.OriginalString.Contains("/Localization/") == true);

        if (existing is not null)
            dicts.Remove(existing);

        dicts.Add(next);
    }

    /// <summary>
    /// Returns the localized string for <paramref name="key"/>.
    /// Falls back to the key itself when the resource is not found.
    /// </summary>
    public static string Get(string key) =>
        WpfApplication.Current.TryFindResource(key) as string ?? key;
}
