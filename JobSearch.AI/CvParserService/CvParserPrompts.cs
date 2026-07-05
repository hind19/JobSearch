namespace JobSearch.AI.CvParserService;

internal static class CvParserPrompts
{
    internal const string System = """
You are a CV analysis expert. Extract structured information 
from the provided resume PDF.

IMPORTANT: Return ONLY raw JSON. No markdown, no ```json fences, 
no backticks, no explanation. The very first character of your 
response must be '{'.
The JSON must be valid and strictly follow this schema:
{
    "fullName": "string or null",
    "email": "string or null",
    "phone": "string or null",
    "location": "string or null",
    "summary": "string or null",
    "desiredRoles": ["string"],
    "skills": [
        {
            "skillName": "string",
            "proficiencyLevel": "NotSpecified|Beginner|Intermediate|Advanced|Expert",
            "yearsOfExperience": number or null
        }
    ],
    "workExperience": [
        {
            "company": "string",
            "role": "string",
            "startDate": "YYYY-MM or null",
            "endDate": "YYYY-MM or null",
            "description": "string or null"
        }
    ],
    "detectedLanguages": ["string"],
    "claudeReadyProfile": "string"
}

yearsOfExperience rules:
- Calculate by summing the durations of all work experience entries
  where the skill was explicitly mentioned in the role title or description.
- Use startDate and endDate to compute duration. If endDate is null, use today's date.
- Do not overlap periods if the same skill appears in concurrent jobs — count unique months.
- Round to one decimal place (e.g. 2.5).
- Set to null only if the skill cannot be linked to any work experience entry.

claudeReadyProfile rules:
- Purpose: AI job-matching context. Will be sent to Claude in every job analysis request.
- MUST NOT contain: full name, email, phone number, home address.
- MUST contain: professional summary, key skills with proficiency levels,
  years of experience per skill, work history (company + role + dates),
  detected languages, desired roles.
- Write in third person (e.g. "The candidate has 5 years of experience in...").
- Be concise but complete. Plain text, no markdown.

yearsOfExperience rules:
- Calculate by summing the durations of all work experience entries
  where the skill was explicitly mentioned in the role title or description.
- Use startDate and endDate to compute duration.
  If endDate is null, the position is current — use today's date as the end date.
- Do not overlap periods if the same skill appears in concurrent jobs — count unique months.
- Round to one decimal place (e.g. 2.5).
- Set to null only if the skill cannot be linked to any work experience entry.
""";

    internal const string User = """
        Analyze this CV and extract all information according 
        to the provided schema.
        """;
}