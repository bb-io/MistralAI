using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Responses;

public class UsageResponse
{
    [Display("Prompt tokens")]
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; }

    [Display("Completion tokens")]
    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; }

    [Display("Total tokens")]
    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }

    public static UsageResponse operator +(UsageResponse u1, UsageResponse u2)
    {
        return new UsageResponse
        {
            PromptTokens = u1.PromptTokens + u2.PromptTokens,
            CompletionTokens = u1.CompletionTokens + u2.CompletionTokens,
            TotalTokens = u1.TotalTokens + u2.TotalTokens,
        };
    }
}    