using Apps.MistralAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MistralAI.Models.Requests;

public class SendPromptRequest
{
    [DataSource(typeof(ModelsDataHandler))]
    public string Model { get; set; }

    public string Message { get; set; }
    
    public double? Temperature { get; set; }

    [Display("Max tokens")]
    public int? MaxTokens { get; set; }
    
    [Display("Top p")]
    public double? TopP { get; set; }

    [Display("Safe prompt")]
    public bool? SafePrompt { get; set; }
    
    [Display("Random seed")]
    public int? RandomSeed { get; set; }

    [Display("Message history")]
    public List<string>? MessageHistory { get; set; }
}