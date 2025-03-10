using Apps.MistralAI.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace Apps.MistralAI.Api;

public class MistralAiClient : RestClient
{
    public MistralAiClient(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var token = creds.First(x => x.KeyName == CredsNames.ApiKey).Value;
        this.AddDefaultHeader("Authorization", $"Bearer {token}");
    }

    public async Task<T> ExecuteWithJson<T>(string endpoint, Method method, object? bodyObj)
    {
        var response = await ExecuteWithJson(endpoint, method, bodyObj);
        return JsonConvert.DeserializeObject<T>(response.Content);
    }

    private async Task<RestResponse> ExecuteWithJson(string endpoint, Method method, object? bodyObj)
    {
        var request = new RestRequest(ApiConstants.BaseUrl + endpoint, method);
        if (bodyObj is not null)
            request.WithJsonBody(bodyObj, new()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            });

        return await ExecuteRequest(request);
    }
    
    public async Task<RestResponse> ExecuteRequest(RestRequest request)
    {
        var response = await ExecuteAsync(request);

        if (!response.IsSuccessStatusCode)
            throw GetError(response);

        return response;
    }
    
    private Exception GetError(RestResponse response)
    {
        return new PluginApplicationException($"Status code: {response.StatusCode}, Content: {response.Content}");
    }
}