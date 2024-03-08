using Blackbird.Applications.Sdk.Common;

namespace Apps.MistralAI.Models.Requests;

public class SendPromptRequest
{
    public string Model { get; set; }

    public string Message { get; set; }
    
    public string? Temperature { get; set; }

    [Display("Safe prompt")]
    public bool? SafePrompt { get; set; }
}