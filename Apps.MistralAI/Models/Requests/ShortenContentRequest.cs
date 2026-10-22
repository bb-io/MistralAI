using Apps.MistralAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;

namespace Apps.MistralAI.Models.Requests
{
    public class ShortenContentRequest
    {
        public FileReference File { get; set; } = new();

        [Display("Model", Description = "This parameter controls which Mistral model answers your request")]
        [DataSource(typeof(ModelsDataHandler))]
        public string Model { get; set; } = string.Empty;

        [Display("Maximum length", Description = "Maximum number of characters for units that have no size restriction in the file. Units with a size restriction always use their own limit.")]
        public int? MaximumLength { get; set; }

        [Display("Retries", Description = "Number of extra attempts for texts that are still too long. Default value: 3")]
        public int? Retries { get; set; }

        [Display("Additional instructions", Description = "Specify additional instructions to apply when shortening.")]
        public string? AdditionalInstructions { get; set; }

        [Display("Output file handling",
            Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps.")]
        [StaticDataSource(typeof(ProcessFileFormatHandler))]
        public string? OutputFileHandling { get; set; }

        [Display("Bucket size", Description = "Specify the maximum number of source texts to be shortened at once. Default value: 250. Buckets are also sized to fit the model's output limit. (See our documentation for an explanation)")]
        public int? BucketSize { get; set; }

        public FileReference? Glossary { get; set; }
    }
}
