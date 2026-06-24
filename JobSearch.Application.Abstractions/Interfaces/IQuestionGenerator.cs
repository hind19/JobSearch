using JobSearch.Application.Abstractions.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobSearch.Application.Abstractions.Interfaces
{
    public interface IQuestionGenerator
    {
        Task<List<ClarifyingQuestionDto>> GetClarifyingQuestionsAsync(
            CvAnalysisResult cvResult,
            CancellationToken ct = default);
    }
}
