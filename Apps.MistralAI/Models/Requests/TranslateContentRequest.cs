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

        [Display("Bucket size", Description = "Number of segments per batch. Default: 1500.")]
        public int? BucketSize { get; set; }

        public int GetBucketSize() => BucketSize ?? 1500;
    }
}
