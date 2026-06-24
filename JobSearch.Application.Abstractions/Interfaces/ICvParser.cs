using System.Threading.Tasks;

namespace JobSearch.Application.Abstractions.Interfaces
{
    public interface ICvParser
    {
        Task<CvAnalysisResult> ParseCvAsync(byte[] pdfBytes, CancellationToken ct);
    }
}
