using System.Text;
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Email
{
    // ADR-0005 §7: builds a plain-text digest from an external template
    // file with {{token}} placeholders — no HTML, no templating engine
    // dependency (plain string.Replace is sufficient at this scale).
    internal class EmailTemplateBuilder
    {
        private const string TemplateRelativePath = "Templates/JobDigestEmail.txt";

        public (string Subject, string Body) BuildJobDigest(
            List<UserJobMatchDto> matches)
        {
            var templatePath = Path.Combine(
                AppContext.BaseDirectory, TemplateRelativePath);

            var template = File.ReadAllText(templatePath);

            var listing = new StringBuilder();
            foreach (var match in matches)
            {
                listing.AppendLine($"- {match.Job.Title} at {match.Job.Company}");

                if (!string.IsNullOrWhiteSpace(match.Job.Location))
                    listing.AppendLine($"  Location: {match.Job.Location}");

                if (!string.IsNullOrWhiteSpace(match.Job.SalaryRaw))
                    listing.AppendLine($"  Salary: {match.Job.SalaryRaw}");

                listing.AppendLine(
                    $"  Relevance: {match.RelevanceScore}/100 — {match.RelevanceReason}");
                listing.AppendLine($"  Link: {match.Job.Url}");
                listing.AppendLine();
            }

            var body = template
                .Replace("{{JobCount}}", matches.Count.ToString())
                .Replace("{{JobListing}}", listing.ToString().TrimEnd());

            var subject = $"JobSearch: {matches.Count} new job match(es) found";

            return (subject, body);
        }
    }
}
