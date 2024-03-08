using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Requests;

public class CreateChatCompletionRequest
{
    public string Model { get; set; }

    public List<MessageRequest> Messages { get; set; }

    public double? Temperature { get; set; }

    [JsonProperty("top_p")]
    public double? TopP { get; set; }
    
    [JsonProperty("max_tokens")]
    public int? MaxTokens { get; set; }
    
    [JsonProperty("safe_prompt")]
    public bool? SafePrompt { get; set; }

    public CreateChatCompletionRequest(SendPromptRequest request)
    {
        Model = request.Model;
        Temperature = request.Temperature;
        TopP = request.TopP;
        MaxTokens = request.MaxTokens;
        SafePrompt = request.SafePrompt;
        
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