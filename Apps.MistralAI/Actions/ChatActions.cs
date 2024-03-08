using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.MistralAI.Actions;

[ActionList]
public class ChatActions(InvocationContext invocationContext) : AppInvocable(invocationContext)
{
    [Action("Send prompt", Description = "Send a prompt to the ai model and return the response")]
    public async Task<SendPromptResponse> SendPrompt([ActionParameter]SendPromptRequest request)
    {
        var apiRequest = new CreateChatCompletionRequest(request);
        var response = await Client.ExecuteWithJson<SendChatCompletionsResponse>(ApiEndpoints.Chat + ApiEndpoints.Completions, Method.Post, apiRequest, creds);
       
        return new SendPromptResponse(response);
    }
}