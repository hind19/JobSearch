using System.Globalization;
using System.Windows.Data;
using JobSearch.WPF.Models;

namespace JobSearch.WPF.Converters;

[ValueConversion(typeof(ProficiencyLevel), typeof(string))]
public class ProficiencyLevelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is ProficiencyLevel level ? level switch
        {
            ProficiencyLevel.NotSpecified => "—",
            ProficiencyLevel.Beginner     => "Beginner",
            ProficiencyLevel.Intermediate => "Intermediate",
            ProficiencyLevel.Advanced     => "Advanced",
            ProficiencyLevel.Expert       => "Expert",
            _                             => value.ToString() ?? string.Empty
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Enum.TryParse<ProficiencyLevel>(value?.ToString(), out var result)
            ? result
            : ProficiencyLevel.NotSpecified;
}
