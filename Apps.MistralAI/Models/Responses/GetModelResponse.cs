using Blackbird.Applications.Sdk.Common;

namespace Apps.MistralAI.Models.Responses;

public class GetModelResponse
{
    [Display("Model ID")]
    public string Id { get; set; }

    public string Object { get; set; }
}