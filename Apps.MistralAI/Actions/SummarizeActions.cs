using Apps.MistralAI.Ai;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Blackbird.Ai.Summarizing.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.MistralAI.Actions
{
    [ActionList("Summarizing")]
    public class SummarizeActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : AiInvocable(invocationContext, fileManagementClient)
    {
        [Action("Summarize", Description = "Summarize file content for a target audience and language. Translated content is summarized from its translation.")]
        public async Task<SummarizeResponse> Summarize(
            [ActionParameter, Display("File")] FileReference file,
            [ActionParameter] SummarizeRequest input)
        {
            var client = CreateLlmClient(input.Model, new());
            var processor = CreateContentProcessor(client, input.Model);

            await using var content = await FileManagementClient.DownloadAsync(file);
            await using var glossaryFile = await DownloadOptionalAsync(input.Glossary);

            var result = await AiAdapters.Mapped(() => processor.SummarizeAsync(new SummarizeStreamOptions
            {
                File = content,
                FileName = file.Name,
                MediaType = file.ContentType,
                Language = input.Language,
                MaxWords = input.MaximumWords,
                AdditionalInstructions = Instructions(input.TargetAudience, input.AdditionalInstructions),
                GlossaryTbx = glossaryFile
            }));

            return ToResponse(result);
        }

        [Action("Summarize text", Description = "Summarize text for a target audience and language.")]
        public async Task<SummarizeResponse> SummarizeText(
            [ActionParameter, Display("Text")] string text,
            [ActionParameter] SummarizeRequest input)
        {
            var client = CreateLlmClient(input.Model, new());
            var processor = CreateTextProcessor(client, input.Model);

            await using var glossaryFile = await DownloadOptionalAsync(input.Glossary);

            var result = await AiAdapters.Mapped(() => processor.SummarizeAsync(new SummarizeTextOptions
            {
                Text = text,
                Language = input.Language,
                MaxWords = input.MaximumWords,
                AdditionalInstructions = Instructions(input.TargetAudience, input.AdditionalInstructions),
                GlossaryTbx = glossaryFile
            }));

            if (string.IsNullOrWhiteSpace(result.Summary) && !string.IsNullOrWhiteSpace(text))
                throw new PluginApplicationException(string.Join(Environment.NewLine, result.Warnings));

            return ToResponse(result);
        }

        private static SummarizeResponse ToResponse(SummarizeResult result) => new()
        {
            Summary = result.Summary,
            SystemPrompt = result.SystemPrompt ?? string.Empty,
            Usage = ToUsage(result.Usage)
        };
    }
}
