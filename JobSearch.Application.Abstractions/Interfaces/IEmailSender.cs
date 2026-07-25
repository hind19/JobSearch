using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces
{
    public interface IEmailSender
    {
        // toAddress is passed explicitly (not looked up internally via
        // IUserRepository) so EmailSender stays a simple, independently
        // testable unit — the caller (WorkerRun) already has the user's
        // email from earlier in the pipeline.
        Task<EmailSendResult> SendJobDigestAsync(
            Guid userId,
            string toAddress,
            List<UserJobMatchDto> matches,
            CancellationToken ct = default);
    }
}
