using CommunityToolkit.Mvvm.ComponentModel;

namespace JobSearch.WPF.Models;

public partial class ClarifyingQuestionItem : ObservableObject
{
    public string QuestionText { get; set; } = string.Empty;
    public AnswerType AnswerType { get; set; }
    public List<string> Options { get; set; } = new();
    public List<string> Currencies { get; } = new() { "USD", "EUR", "GBP", "RUB" };

    [ObservableProperty]
    private string _selectedAnswer = string.Empty;

    [ObservableProperty]
    private string _textAnswer = string.Empty;

    [ObservableProperty]
    private decimal? _rangeFrom;

    [ObservableProperty]
    private decimal? _rangeTo;

    [ObservableProperty]
    private string _selectedCurrency = "USD";
}
