namespace Apps.MistralAI.Models.Requests;

public class CreateChatCompletionRequest
{
    public string Model { get; set; }

    public List<MessageRequest> Messages { get; set; }

    public CreateChatCompletionRequest(SendPromptRequest request)
    {
        Model = request.Model;
        Messages = new List<MessageRequest>
        {
            new MessageRequest("user", request.Message)
        };
    }
}