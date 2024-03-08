using RestSharp;

namespace Apps.MistralAI.Api;

public class MistralAiRequestParameters
{
    public string Url { get; set; }
    
    public Method Method { get; init; }
}