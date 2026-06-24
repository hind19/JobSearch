namespace JobSearch.AI.QuestionGeneratorService
{
    internal sealed class ClarifyingQuestionRaw
    {
        public string QuestionText { get; init; } = string.Empty;
        public string AnswerType { get; init; } = "Text";
        public List<string> Options { get; init; } = [];
        public decimal? RangeFrom { get; init; }
        public decimal? RangeTo { get; init; }
        public string? Currency { get; init; }
    }
}
