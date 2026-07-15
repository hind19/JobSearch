// JobSearch.Business/Mapping/ScrapeConfigMapper.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Business.Mapping;

internal static class ScrapeConfigMapper
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // TODO: рассмотреть вопрос с изменением БД под объект вместо строки
    //       с добавлением новой таблицы ScrapeConfigs (Id, JobSiteId, поля конфига)
    internal static string ToJson(ScrapeConfigDto dto) =>
        JsonSerializer.Serialize(dto, _options);

    internal static ScrapeConfigDto FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return ScrapeConfigDto.Empty;

        try
        {
            return JsonSerializer.Deserialize<ScrapeConfigDto>(json, _options)
                ?? ScrapeConfigDto.Empty;
        }
        catch
        {
            return ScrapeConfigDto.Empty;
        }
    }
}