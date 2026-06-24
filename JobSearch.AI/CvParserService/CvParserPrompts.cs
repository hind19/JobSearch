namespace JobSearch.AI.CvParserService;

internal static class CvParserPrompts
{
    internal const string System = """
        You are a CV analysis expert. Extract structured information 
        from the provided resume PDF.
        
        Return ONLY a valid JSON object. No markdown, no backticks, 
        no preamble. The JSON must strictly follow this schema:
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
            "detectedLanguages": ["string"]
        }
        """;

    internal const string User = """
        Analyze this CV and extract all information according 
        to the provided schema.
        """;
}