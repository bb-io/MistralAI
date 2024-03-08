using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Responses;

public class ChoicesResponse
{
    public string Index { get; set; }

    public MessageResponse Message { get; set; }

    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; }

    public string? LogProbs { get; set; }
}