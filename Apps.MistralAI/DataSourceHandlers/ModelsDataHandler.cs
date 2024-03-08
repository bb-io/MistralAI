using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Responses;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.MistralAI.DataSourceHandlers;

public class ModelsDataHandler(InvocationContext invocationContext)
    : AppInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        string endpoint = ApiEndpoints.Models;
        var response = await Client.ExecuteWithJson<GetModelsResponse>(endpoint, Method.Get, null, Creds);
        
        return response.Data
            .Where(x => context.SearchString == null ||
                        x.Id.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(20)
            .ToDictionary(x => x.Id.ToString(), x => x.Id);
    }
}