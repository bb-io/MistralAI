using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Entities;

public class TranslationEntity
{
    [JsonProperty("translationId")]
    public string TranslationId { get; set; } = string.Empty;

    [JsonProperty("translatedText")]
    public string TranslatedText { get; set; } = string.Empty;

    [JsonProperty("qualityScore")]
    public float QualityScore { get; set; }
}