using Blackbird.Applications.Sdk.Common;

namespace Apps.MistralAI.Models.Responses
{
    public class SummarizeResponse
    {
        [Display("Summary")]
        public string Summary { get; set; } = string.Empty;

        [Display("System prompt")]
        public string SystemPrompt { get; set; } = string.Empty;

        [Display("Usage")]
        public UsageResponse Usage { get; set; } = new();
    }
}
