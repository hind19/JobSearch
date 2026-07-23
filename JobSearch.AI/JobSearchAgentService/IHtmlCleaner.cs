// JobSearch.AI/JobSearchAgentService/IHtmlCleaner.cs
namespace JobSearch.AI.JobSearchAgentService;

internal interface IHtmlCleaner
{
    string StripToReadableText(string html);
}
