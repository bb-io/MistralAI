using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Entities;

public class TranslationEntities
{
    [JsonProperty("translations")]
    public List<TranslationEntity> Translations { get; set; } = new();
}

/*
{
    "translations": [
        {
            "translation_id": "21",
            "translated_text": "Some translated text",
            "quality_score": 8.5
        }
    ]
}
 */