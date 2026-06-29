using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.Application.Abstractions.DTOs;

public class ClarifyingQuestionDto
{
    public string QuestionText { get; }
    public AnswerType AnswerType { get; }
    public List<string> Options { get; }
    public string? SelectedAnswer { get; }
    public decimal? RangeFrom { get; }
    public decimal? RangeTo { get; }
    public string? Currency { get; }
    public string? TextAnswer { get; }

    public ClarifyingQuestionDto(
        string questionText,
        AnswerType answerType,
        List<string> options,
        string? selectedAnswer,
        decimal? rangeFrom,
        decimal? rangeTo,
        string? currency,
        string? textAnswer)
    {
        QuestionText = questionText;
        AnswerType = answerType;
        Options = options;
        SelectedAnswer = selectedAnswer;
        RangeFrom = rangeFrom;
        RangeTo = rangeTo;
        Currency = currency;
        TextAnswer = textAnswer;
    }
}