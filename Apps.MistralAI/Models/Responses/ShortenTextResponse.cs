using Blackbird.Applications.Sdk.Common;

namespace Apps.MistralAI.Models.Responses
{
    public class ShortenTextResponse
    {
        [Display("Shortened text")]
        public string ShortenedText { get; set; } = string.Empty;

        [Display("System prompt")]
        public string SystemPrompt { get; set; } = string.Empty;

        [Display("Usage")]
        public UsageResponse Usage { get; set; } = new();
    }
}
