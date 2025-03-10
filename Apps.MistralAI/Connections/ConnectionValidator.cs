using Apps.MistralAI.Api;
using Apps.MistralAI.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.MistralAI.Connections;

public class ConnectionValidator : IConnectionValidator
{    
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new MistralAiClient(authenticationCredentialsProviders);
            var response = await client.ExecuteRequest(new RestRequest(ApiConstants.BaseUrl + ApiEndpoints.Models, Method.Get));
            
            if (!response.IsSuccessStatusCode)
            {
                return new()
                {
                    IsValid = false,
                    Message = response.ErrorMessage
                };
            }
            return new() { IsValid = true };
        }
        catch (Exception e)
        {            
            return new ConnectionValidationResponse { IsValid = false, Message = e.Message };
        }
    }
}