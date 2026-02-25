using Apps.MistralAI.DataSourceHandlers;
using Apps.MistralAI.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Edit;
using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Requests
{
    public class EditFileRequest : IEditFileInput
    {
        public FileReference File { get; set; } = new();

        [Display("Source language")]
        [DataSource(typeof(LocaleDataSourceHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Target language")]
        [DataSource(typeof(LocaleDataSourceHandler))]
        public string TargetLanguage { get; set; } = string.Empty;

        [Display("Output file handling",
            Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps.")]
        [StaticDataSource(typeof(ProcessFileFormatHandler))]
        public string? OutputFileHandling { get; set; }

        [Display("Model", Description = "This parameter controls which Mistral model answers your request")]
        [DataSource(typeof(ModelsDataHandler))]
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [Display("Additional instructions",
            Description = "Additional instructions you want to apply to the editing.\nFor example: 'Prefer a more formal tone.'")]
        [JsonProperty("prompt")]
        public string? AdditionalInstructions { get; set; }

        [Display("Max tokens", Description = "The maximum number of tokens to generate before stopping.")]
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

        [Display("Bucket size", Description = "Number of segments to edit per request. Default: 50")]
        public int? BucketSize { get; set; }

        [Display("Ignore deserialization errors",
            Description = "If enabled, the action will skip batches that cannot be deserialized instead of failing.")]
        public bool? IgnoreDeserializationErrors { get; set; }

        public FileReference? Glossary { get; set; }

        public int GetBucketSize() => BucketSize ?? 1500;
    }
}
