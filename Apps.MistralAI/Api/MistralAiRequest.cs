using Apps.MistralAI.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.MistralAI.Api;

public class MistralAiRequest : RestRequest
{
    public MistralAiRequest(MistralAiRequestParameters requestParameters,
        IEnumerable<AuthenticationCredentialsProvider> creds) : base(requestParameters.Url, requestParameters.Method)
    {
        var token = creds.First(x => x.KeyName == CredsNames.ApiKey).Value;
        this.AddHeader("Authorization", $"Bearer {token}");
    }
}