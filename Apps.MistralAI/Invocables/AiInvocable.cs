using Apps.MistralAI.Ai;
using Apps.MistralAI.Models.Responses;
using Blackbird.Ai.Batching.Models;
using Blackbird.Ai.Content.Models;
using Blackbird.Ai.Files;
using Blackbird.Ai.Files.Models;
using Blackbird.Ai.Llm.Abstract;
using Blackbird.Ai.Llm.Models;
using Blackbird.Ai.Prompts.Models;
using Blackbird.Ai.Text;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.MistralAI.Invocables;

public class AiInvocable(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : AppInvocable(invocationContext)
{
    protected IFileManagementClient FileManagementClient { get; } = fileManagementClient;

    protected virtual ILlmClient CreateLlmClient(string model, MistralLlmSettings settings) =>
        new MistralLlmClient(Client, model, settings);

    protected ContentStreamProcessor CreateContentProcessor(ILlmClient client, string model, int? bucketSize = null) =>
        new(client, CreateEngineOptions(model, bucketSize));

    protected TextProcessor CreateTextProcessor(ILlmClient client, string model) =>
        new(client, CreateEngineOptions(model));

    protected async Task<Stream?> DownloadOptionalAsync(FileReference? file) =>
        file is null ? null : await FileManagementClient.DownloadAsync(file);

    protected async Task<FileReference> UploadAsync(ContentStream file)
    {
        await using var content = file.Content;
        return await FileManagementClient.UploadAsync(content, file.MediaType, file.FileName);
    }

    protected static UsageResponse ToUsage(LlmUsage usage) => new()
    {
        PromptTokens = usage.InputTokens,
        CompletionTokens = usage.OutputTokens,
        TotalTokens = usage.TotalTokens
    };

    protected static string UserPrompt(ContentResult content) =>
        string.Join(Environment.NewLine, content.Traces.Select(trace => trace.UserPrompt));

    protected static string? Instructions(string? targetAudience, string? additionalInstructions)
    {
        var audience = string.IsNullOrWhiteSpace(targetAudience)
            ? null
            : $"The target audience is {targetAudience.Trim()}.";

        var instructions = new[] { audience, additionalInstructions }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();

        return instructions.Count > 0 ? string.Join(Environment.NewLine, instructions) : null;
    }

    protected static void ThrowIfUnresolved(ContentResult content)
    {
        if (content.UnresolvedSegments > 0)
        {
            throw new PluginApplicationException(string.Join(Environment.NewLine, content.Warnings));
        }
    }

    private EngineOptions CreateEngineOptions(string model, int? bucketSize = null) => new()
    {
        Logger = AiAdapters.Logger(InvocationContext),
        LogTag = "[Mistral AI]",
        ProvenanceTool = $"Mistral AI/{model}",
        Bucketing = BucketingOptions.FromInputs(bucketSize, null),
        TraceLevel = PromptTraceLevel.Full
    };
}
