using Blackbird.Applications.Sdk.Common;

namespace Apps.MistralAI.Models.Responses;

public class SendPromptResponse
{
    public string Id { get; set; }

    public string Content { get; set; }

    [Display("Created at")]
    public DateTime CreatedAt { get; set; }

    public SendPromptResponse(SendChatCompletionsResponse model)
    {
        Id = model.Id;
        if (model.Choices.Count == 0)
        {
            throw new Exception("No choices returned from the model");
        }
        
        Content = model.Choices.First().Message.Content;
        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(model.Created).DateTime;
    }
}