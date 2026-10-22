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

        [Display("Bucket size", Description = "Specify the maximum number of source texts to be edited at once. Default value: 250. Buckets are also sized to fit the model's output limit. (See our documentation for an explanation)")]
        public int? BucketSize { get; set; }

        [Display("Generate document summary", Description = "By default, a short summary of the document is generated and given to the model as context for files of 20 or more segments. This costs one extra request. Set to false to skip it.")]
        public bool? GenerateDocumentSummary { get; set; }

        public FileReference? Glossary { get; set; }
    }
}
