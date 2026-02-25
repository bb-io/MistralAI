using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Responses;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.MistralAI.DataSourceHandlers;

public class AudioModelsDataHandler(InvocationContext invocationContext)
    : AppInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var response = await Client.ExecuteWithJson<GetModelsResponse>(ApiEndpoints.Models, Method.Get, null);

        const string audioModelFilter = "voxtral";

        return response.Data
            .Where(x => x.Id.Contains(audioModelFilter) &&
                        (context.SearchString is null || x.Id.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)))
            .DistinctBy(x => x.Id)
            .Take(20)
            .ToDictionary(x => x.Id.ToString(), x => x.Id);
    }
}