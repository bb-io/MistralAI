using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.MistralAI.Models.Responses
{
    public class TranslateFileResult : ITranslateFileOutput
    {
        [Display("Translated file")]
        public FileReference File { get; set; } = default!;

        [Display("Total segments")]
        public int TotalSegmentsCount { get; set; }

        [Display("Total translatable segments")]
        public int TotalTranslatable { get; set; }

        [Display("Targets updated")]
        public int TargetsUpdatedCount { get; set; }

        [Display("Processed batches")]
        public int ProcessedBatchesCount { get; set; }

        [Display("System prompt")]
        public string? SystemPrompt { get; set; }

        [Display("Usage")]
        public UsageResponse? Usage { get; set; }
    }
}
