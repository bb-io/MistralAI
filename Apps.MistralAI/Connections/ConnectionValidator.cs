using Apps.MistralAI.Api;
using Apps.MistralAI.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.MistralAI.Connections;

public class ConnectionValidator : IConnectionValidator
{
    private readonly MistralAiClient _client = new();
    
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            await _client.ExecuteRequest(new MistralAiRequest(new()
            {
                Url = ApiConstants.BaseUrl + ApiEndpoints.Models,
                Method = Method.Get
            }, authenticationCredentialsProviders.ToArray()));
            
            return new() { IsValid = true };
        }
        catch (Exception e)
        {            
            return new ConnectionValidationResponse { IsValid = false, Message = e.Message };
        }
    }
}