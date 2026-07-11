using System.Globalization;
using System.Windows.Data;
using JobSearch.Application.Abstractions.Enums;
using JobSearch.WPF.Localization;

namespace JobSearch.WPF.Converters;

[ValueConversion(typeof(ProficiencyLevel), typeof(string))]
public class ProficiencyLevelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is ProficiencyLevel level ? level switch
        {
            ProficiencyLevel.NotSpecified  => LocalizationManager.Get("ProficiencyLevel_NotSpecified"),
            ProficiencyLevel.Beginner      => LocalizationManager.Get("ProficiencyLevel_Beginner"),
            ProficiencyLevel.Intermediate  => LocalizationManager.Get("ProficiencyLevel_Intermediate"),
            ProficiencyLevel.Advanced      => LocalizationManager.Get("ProficiencyLevel_Advanced"),
            ProficiencyLevel.Expert        => LocalizationManager.Get("ProficiencyLevel_Expert"),
            _                              => value.ToString() ?? string.Empty
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Enum.TryParse<ProficiencyLevel>(value?.ToString(), out var result)
            ? result
            : ProficiencyLevel.NotSpecified;
}
