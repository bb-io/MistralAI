using Apps.MistralAI.Models.Responses;

namespace Apps.MistralAI.Models.Entities;

public class ProcessTranslationUnitsResult
{
    public List<TranslationEntity> TranslationEntities { get; set; } = new();

    public UsageResponse Usage { get; set; } = new();
}