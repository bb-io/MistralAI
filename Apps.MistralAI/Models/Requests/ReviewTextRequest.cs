using Apps.MistralAI.DataSourceHandlers;
using Apps.MistralAI.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;
using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Requests
{
    public class ReviewTextRequest : IReviewTextInput
    {
        [Display("Source text")]
        public string SourceText { get; set; } = string.Empty;

        [Display("Target text")]
        public string TargetText { get; set; } = string.Empty;

        [Display("Source language")]
        [DataSource(typeof(LocaleDataSourceHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Target language")]
        [DataSource(typeof(LocaleDataSourceHandler))]
        public string TargetLanguage { get; set; } = string.Empty;

        [Display("Model", Description = "This parameter controls which Mistral model answers your request")]
        [DataSource(typeof(ModelsDataHandler))]
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [Display("Additional instructions",
            Description = "Additional instructions applied to the review.\nFor example: 'Focus on technical terminology accuracy.'")]
        public string? AdditionalInstructions { get; set; }

        [Display("Max tokens", Description = "Maximum number of tokens to generate before stopping.")]
        [JsonProperty("max_tokens")]
        public int? MaxTokens { get; set; }

        [Display("Temperature", Description = "Amount of randomness injected into the response.")]
        [StaticDataSource(typeof(TemperatureDataSourceHandler))]
        [JsonProperty("temperature")]
        public string? Temperature { get; set; }

        [Display("top_p", Description = "Use nucleus sampling.")]
        [DataSource(typeof(TopPDataSourceHandler))]
        [JsonProperty("top_p")]
        public string? TopP { get; set; }

        public FileReference? Glossary { get; set; }
    }
}
