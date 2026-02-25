using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Responses;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.MistralAI.DataSourceHandlers;

public class ModelsDataHandler(InvocationContext invocationContext)
    : AppInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var response = await Client.ExecuteWithJson<GetModelsResponse>(ApiEndpoints.Models, Method.Get, null);

        return response.Data
            .Where(x => context.SearchString is null ||
                        x.Id.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(x => x.Id)
            .Take(20)
            .Select(x => new DataSourceItem(x.Id, x.Id));
    }
}