using Apps.MistralAI.DataSourceHandlers;
using Apps.MistralAI.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MistralAI.Models.Requests;

public class SendPromptRequest
{
    [DataSource(typeof(ModelsDataHandler))]
    public string Model { get; set; }

    public string Message { get; set; }
    
    public string? Temperature { get; set; }

    [Display("Safe prompt")]
    public bool? SafePrompt { get; set; }

    [Display("Message history")]
    public List<string>? MessageHistory { get; set; }
}