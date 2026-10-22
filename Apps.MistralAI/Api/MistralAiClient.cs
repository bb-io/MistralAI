using System.Globalization;
using System.Net;
using Apps.MistralAI.Constants;
using Apps.MistralAI.Models.Entities;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.MistralAI.Api;

public class MistralAiClient : RestClient
{
    private static readonly ResiliencePipeline<RestResponse> RateLimitPipeline = new ResiliencePipelineBuilder<RestResponse>()
        .AddRetry(new RetryStrategyOptions<RestResponse>
        {
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = new PredicateBuilder<RestResponse>()
                .HandleResult(response => response.StatusCode == HttpStatusCode.TooManyRequests),
            DelayGenerator = args => ValueTask.FromResult(RetryAfter(args.Outcome.Result))
        })
        .Build();

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
        var response = await RateLimitPipeline.ExecuteAsync(
            async cancellationToken => await ExecuteAsync(request, cancellationToken));

        if (!response.IsSuccessStatusCode)
            throw GetError(response);

        return response;
    }

    private static TimeSpan? RetryAfter(RestResponse? response) =>
        response?.Headers?.FirstOrDefault(header => string.Equals(header.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))?.Value?.ToString() is { } value
        && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            ? TimeSpan.FromSeconds(Math.Min(seconds, 60))
            : null;

    private Exception GetError(RestResponse response)
    {
        try
        {
            var errorModel = JsonConvert.DeserializeObject<ErrorModel>(response.Content);
            if (errorModel?.Message is not null)
            {
                return new PluginApplicationException(errorModel?.Message!);
            }
        }
        catch
        {
            return new PluginApplicationException($"Status code: {response.StatusCode}, Content: {response.Content}");
        }

        return new PluginApplicationException($"Status code: {response.StatusCode}, Content: {response.Content}");
    }
}
