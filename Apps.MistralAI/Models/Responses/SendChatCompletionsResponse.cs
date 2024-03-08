namespace Apps.MistralAI.Models.Responses;

public class SendChatCompletionsResponse
{
    public string Id { get; set; }

    public string Object { get; set; }

    public long Created { get; set; }

    public string Model { get; set; }

    public List<ChoicesResponse> Choices { get; set; }
}