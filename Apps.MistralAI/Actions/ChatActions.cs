using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Responses;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.MistralAI.Actions;

[ActionList]
public class ChatActions(InvocationContext invocationContext) : AppInvocable(invocationContext)
{
    [Action("Get models", Description = "Get the list of models available for translation")]
    public async Task<List<GetModelResponse>> GetModels()
    {
        var response = await Client.ExecuteWithJson<GetModelsResponse>(ApiEndpoints.Models, Method.Get, null, Creds);
        return response.Data;
    }
}