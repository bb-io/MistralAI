using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.MistralAI.Models.Responses
{
    public class ReviewFileResponse : IReviewFileOutput
    {
        public FileReference File { get; set; } = new();

        public UsageResponse Usage { get; set; } = new();

        public int TotalSegmentsProcessed { get; set; }

        public int TotalSegmentsFinalized { get; set; }

        public int TotalSegmentsUnderThreshhold { get; set; }

        public float AverageMetric { get; set; }

        public float PercentageSegmentsUnderThreshhold { get; set; }
    }
}
