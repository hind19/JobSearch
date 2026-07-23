// JobSearch.Business/Services/IJobUrlHasher.cs
namespace JobSearch.Business.Services;

// Internal to JobSearch.Business — only JobIngestService calls this.
// Not in Application.Abstractions because no other host (WPF/Worker)
// needs to invoke it directly.
internal interface IJobUrlHasher
{
    string Compute(string url);
}
