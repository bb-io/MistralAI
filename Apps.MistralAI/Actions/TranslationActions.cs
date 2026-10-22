using Apps.MistralAI.Ai;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Blackbird.Ai.Files.Models;
using Blackbird.Ai.Summary.Models;
using Blackbird.Ai.Text.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.MistralAI.Actions
{
    [ActionList("Translation")]
    public class TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : AiInvocable(invocationContext, fileManagementClient)
    {
        [BlueprintActionDefinition(BlueprintAction.TranslateText)]
        [Action("Translate text", Description = "Localize the text provided.")]
        public async Task<TranslateTextResponse> TranslateText(
            [ActionParameter] TranslateTextRequest input,
            [ActionParameter] GlossaryRequest glossary)
        {
            var client = CreateLlmClient(input.Model, new());
            var processor = CreateTextProcessor(client, input.Model);

            await using var glossaryFile = await DownloadOptionalAsync(glossary.Glossary);

            var result = await AiAdapters.Mapped(() => processor.TranslateAsync(new TranslateTextOptions
            {
                Text = input.Text,
                SourceLanguage = input.SourceLanguage,
                TargetLanguage = input.TargetLanguage,
                GlossaryTbx = glossaryFile
            }));

            ThrowIfUnresolved(result.Content);

            return new TranslateTextResponse
            {
                TranslatedText = result.Text,
                SystemPrompt = result.Content.SystemPrompt ?? string.Empty,
                UserPrompt = UserPrompt(result.Content),
                Usage = ToUsage(result.Content.Usage)
            };
        }

        [BlueprintActionDefinition(BlueprintAction.TranslateFile)]
        [Action("Translate", Description = "Translate file content retrieved from a CMS or file storage")]
        public async Task<TranslateFileResult> TranslateFile(
            [ActionParameter] TranslateContentRequest input,
            [ActionParameter, Display("Additional instructions", Description = "Specify additional instructions to be applied to the translation")] string? prompt,
            [ActionParameter] GlossaryRequest glossary)
        {
            var client = CreateLlmClient(input.Model, new());
            var processor = CreateContentProcessor(client, input.Model, input.BucketSize);

            await using var file = await FileManagementClient.DownloadAsync(input.File);
            await using var glossaryFile = await DownloadOptionalAsync(glossary.Glossary);

            var result = await AiAdapters.Mapped(() => processor.TranslateAsync(new TranslateStreamOptions
            {
                File = file,
                FileName = input.File.Name,
                MediaType = input.File.ContentType,
                SourceLanguage = input.SourceLanguage,
                TargetLanguage = input.TargetLanguage,
                OutputFileHandling = input.OutputFileHandling,
                GlossaryTbx = glossaryFile,
                AdditionalInstructions = prompt,
                Summary = SummaryOptions.FromToggle(input.GenerateDocumentSummary)
            }));

            var content = result.Content;

            return new TranslateFileResult
            {
                File = await UploadAsync(result.File),
                TotalSegmentsCount = content.TotalSegments,
                TotalTranslatable = content.ProcessableSegments,
                TargetsUpdatedCount = content.UpdatedSegments,
                ProcessedBatchesCount = content.BucketsProcessed,
                SystemPrompt = content.SystemPrompt,
                Usage = ToUsage(content.Usage)
            };
        }
    }
}
