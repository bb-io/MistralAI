using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Entities;

public class TranslationEntities
{
    [JsonProperty("translations")]
    public List<TranslationEntity> Translations { get; set; } = new();
}
