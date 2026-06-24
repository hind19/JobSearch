using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.Application.Abstractions.DTOs
{
    public class ClarifyingQuestionDto
    {
        public string QuestionText { get; init; } = string.Empty;
        public AnswerType AnswerType { get; init; }
        public List<string> Options { get; init; } = [];
        public string? SelectedAnswer { get; init; }
        public decimal? RangeFrom { get; init; }
        public decimal? RangeTo { get; init; }
        public string? Currency { get; init; }
        public string? TextAnswer { get; init; }
    }
}
