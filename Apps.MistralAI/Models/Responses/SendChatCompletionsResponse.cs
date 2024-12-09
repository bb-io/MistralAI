namespace Apps.MistralAI.Models.Responses;

public class SendChatCompletionsResponse
{
    public string Id { get; set; } = string.Empty;

    public string Object { get; set; } = string.Empty;

    public UsageResponse Usage { get; set; } = new();

    public long Created { get; set; }

    public string Model { get; set; } = string.Empty;

    public List<ChoicesResponse> Choices { get; set; } = new();
}