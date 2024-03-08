using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Requests;

public class CreateChatCompletionRequest
{
    public string Model { get; set; }

    public List<MessageRequest> Messages { get; set; }

    public CreateChatCompletionRequest(SendPromptRequest request)
    {
        Model = request.Model;
        Messages = new List<MessageRequest>();
        ParseMessageHistory(request.MessageHistory);
        
        Messages.Add(new MessageRequest("user", request.Message));
    }
    
    private void ParseMessageHistory(List<string>? messageHistory)
    {
        if (messageHistory != null)
        {
            foreach (var message in messageHistory)
            {
                var messageRequest = JsonConvert.DeserializeObject<MessageRequest>(message);
                if (messageRequest != null)
                {
                    Messages.Add(messageRequest);
                }
            }
        }
    }
}