using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.Application.Abstractions.DTOs;

public class ClarifyingQuestionDto(
    string questionText,
    AnswerType answerType,
    List<string> options,
    string? selectedAnswer,
    decimal? rangeFrom,
    decimal? rangeTo,
    string? currency,
    string? textAnswer)
{
    public string QuestionText { get; } = questionText;
    public AnswerType AnswerType { get; } = answerType;
    public List<string> Options { get; } = options;
    public string? SelectedAnswer { get; } = selectedAnswer;
    public decimal? RangeFrom { get; } = rangeFrom;
    public decimal? RangeTo { get; } = rangeTo;
    public string? Currency { get; } = currency;
    public string? TextAnswer { get; } = textAnswer;
}
