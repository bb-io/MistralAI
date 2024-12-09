using Blackbird.Applications.Sdk.Common;

namespace Apps.MistralAI.Models.Responses;

public class ScoreXliffResponse : ProcessXliffResponse
{
    [Display("Average score")]
    public double AverageScore { get; set; }
}