// JobSearch.Application.Abstractions/DTOs/EmailSendResult.cs
namespace JobSearch.Application.Abstractions.DTOs;

// ADR-0005 §5: WorkerRun only calls MarkAsNotifiedAsync when Sent is
// true. A failed send (even after all Polly retries) leaves matches
// unnotified, so the next Worker run picks them up automatically — no
// separate retry queue needed.
public class EmailSendResult(
    bool sent,
    Guid sentEmailId,
    string? errorMessage)
{
    public bool Sent { get; } = sent;
    public Guid SentEmailId { get; } = sentEmailId;
    public string? ErrorMessage { get; } = errorMessage;
}
