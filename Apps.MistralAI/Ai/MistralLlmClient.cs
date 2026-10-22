using Apps.MistralAI.Api;
using Apps.MistralAI.Constants;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Apps.MistralAI.Utils;
using Blackbird.Ai.Llm.Abstract;
using Blackbird.Ai.Llm.Models;
using RestSharp;

namespace Apps.MistralAI.Ai;

public sealed class MistralLlmClient(MistralAiClient client, string model, MistralLlmSettings settings) : ILlmClient
{
    private const int MaxOutputTokens = 16_384;

    private static readonly HashSet<string> TruncatedFinishReasons =
        new(StringComparer.OrdinalIgnoreCase) { "length", "model_length" };

    public LlmCapabilities Capabilities { get; } = CapabilitiesFor(model, settings.MaxTokens);

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = await client.ExecuteWithJson<SendChatCompletionsResponse>(
            ApiEndpoints.Chat + ApiEndpoints.Completions,
            Method.Post,
            new CreateChatCompletionRequest
            {
                Model = model,
                Messages = [new("system", request.SystemPrompt), new("user", request.UserPrompt)],
                MaxTokens = settings.MaxTokens,
                Temperature = settings.Temperature,
                TopP = settings.TopP
            });

        var choice = response.Choices.First();

        return new LlmResponse(
            choice.Message.Content ?? string.Empty,
            new LlmUsage(response.Usage.PromptTokens, response.Usage.CompletionTokens),
            choice.FinishReason is not null && TruncatedFinishReasons.Contains(choice.FinishReason)
                ? FinishReason.Truncated
                : FinishReason.Stop);
    }

    private static LlmCapabilities CapabilitiesFor(string model, int? requestedMaxTokens)
    {
        var contextTokens = ModelTokenService.GetMaxTokensForModel(model);
        var outputLimit = Math.Min(MaxOutputTokens, contextTokens / 2);

        return new LlmCapabilities(
            SupportsStructuredOutput: false,
            ContextTokens: contextTokens,
            MaxOutputTokens: requestedMaxTokens is > 0 ? Math.Min(requestedMaxTokens.Value, outputLimit) : outputLimit);
    }
}
