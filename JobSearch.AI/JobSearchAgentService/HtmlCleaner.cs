// JobSearch.AI/JobSearchAgentService/HtmlCleaner.cs
using System.Net;
using HtmlAgilityPack;

namespace JobSearch.AI.JobSearchAgentService;

internal sealed class HtmlCleaner : IHtmlCleaner
{
    public string StripToReadableText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var junk = doc.DocumentNode
            .SelectNodes("//script|//style|//nav|//footer|//header");

        if (junk is not null)
            foreach (var node in junk.ToList())
                node.Remove();

        return WebUtility.HtmlDecode(doc.DocumentNode.InnerText);
    }
}
