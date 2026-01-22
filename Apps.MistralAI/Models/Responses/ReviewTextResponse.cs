using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.MistralAI.Models.Responses
{
    public class ReviewTextResponse : IReviewTextOutput
    {
        [Display("Score")]
        public float Score { get; set; }

        [Display("System prompt")]
        public string SystemPrompt { get; set; } = string.Empty;

        [Display("User prompt")]
        public string UserPrompt { get; set; } = string.Empty;

        [Display("Usage")]
        public UsageResponse Usage { get; set; } = new();
    }
}
