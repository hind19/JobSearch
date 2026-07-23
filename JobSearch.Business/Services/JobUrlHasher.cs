// JobSearch.Business/Services/JobUrlHasher.cs
using System.Security.Cryptography;
using System.Text;

namespace JobSearch.Business.Services;

// ADR-0004 guardrail #2: UrlHash is always computed here, server-side —
// never accepted as a pre-computed value from a caller (in particular,
// never from the Worker agent loop's save_job tool input, which doesn't
// even expose a urlHash field).
internal sealed class JobUrlHasher : IJobUrlHasher
{
    public string Compute(string url)
    {
        var bytes = Encoding.UTF8.GetBytes(url.Trim());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
