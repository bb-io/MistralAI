using Apps.MistralAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.MistralAI.Models.Requests
{
    public class TranslateContentRequest : ITranslateFileInput
    {
        [Display("File")]
        public FileReference File { get; set; } = default!;

        [Display("Target language")]
        public string TargetLanguage { get; set; } = string.Empty;

        [Display("Model")]
        [DataSource(typeof(ModelsDataHandler))]
        public string Model { get; set; }

        [Display("Source language", Description = "Optional. If not set, Mistral will infer it from the content.")]
        public string? SourceLanguage { get; set; }

        [Display("Output file handling")]
        [StaticDataSource(typeof(ProcessFileFormatHandler))]
        public string? OutputFileHandling { get; set; }

        [Display("Bucket size", Description = "Specify the maximum number of source texts to be translated at once. Default value: 250. Buckets are also sized to fit the model's output limit. (See our documentation for an explanation)")]
        public int? BucketSize { get; set; }

        [Display("Generate document summary", Description = "By default, a short summary of the document is generated and given to the model as context for files of 20 or more segments. This costs one extra request. Set to false to skip it.")]
        public bool? GenerateDocumentSummary { get; set; }
    }
}
