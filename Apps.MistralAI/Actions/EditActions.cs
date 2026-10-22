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
    [ActionList("Editing")]
    public class EditActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : AiInvocable(invocationContext, fileManagementClient)
    {
        [BlueprintActionDefinition(BlueprintAction.EditFile)]
        [Action("Edit", Description = "Edit a translation. Assumes you have previously translated content in Blackbird through any translation action.")]
        public async Task<EditFileResponse> EditFile([ActionParameter] EditFileRequest input)
        {
            var client = CreateLlmClient(input.Model, MistralLlmSettings.Parse(input.Temperature, input.TopP, input.MaxTokens));
            var processor = CreateContentProcessor(client, input.Model, input.BucketSize);

            await using var file = await FileManagementClient.DownloadAsync(input.File);
            await using var glossaryFile = await DownloadOptionalAsync(input.Glossary);

            var result = await AiAdapters.Mapped(() => processor.EditAsync(new EditStreamOptions
            {
                File = file,
                FileName = input.File.Name,
                MediaType = input.File.ContentType,
                SourceLanguage = input.SourceLanguage,
                TargetLanguage = input.TargetLanguage,
                OutputFileHandling = input.OutputFileHandling,
                GlossaryTbx = glossaryFile,
                AdditionalInstructions = input.AdditionalInstructions,
                Summary = SummaryOptions.FromToggle(input.GenerateDocumentSummary)
            }));

            return new EditFileResponse
            {
                File = await UploadAsync(result.File),
                Usage = ToUsage(result.Content.Usage),
                TotalSegmentsReviewed = result.Content.ProcessableSegments,
                TotalSegmentsUpdated = result.Content.UpdatedSegments
            };
        }

        [BlueprintActionDefinition(BlueprintAction.EditText)]
        [Action("Edit text", Description = "Review translated text and generate an edited version")]
        public async Task<EditTextResponse> EditText([ActionParameter] EditTextRequest input)
        {
            var client = CreateLlmClient(input.Model, MistralLlmSettings.Parse(input.Temperature, input.TopP, input.MaxTokens));
            var processor = CreateTextProcessor(client, input.Model);

            await using var glossaryFile = await DownloadOptionalAsync(input.Glossary);

            var result = await AiAdapters.Mapped(() => processor.EditAsync(new EditTextOptions
            {
                SourceText = input.SourceText,
                TargetText = input.TargetText,
                SourceLanguage = input.SourceLanguage,
                TargetLanguage = input.TargetLanguage,
                AdditionalInstructions = Instructions(input),
                GlossaryTbx = glossaryFile
            }));

            ThrowIfUnresolved(result.Content);

            return new EditTextResponse
            {
                EditedText = result.Text,
                SystemPrompt = result.Content.SystemPrompt ?? string.Empty,
                UserPrompt = UserPrompt(result.Content),
                Usage = ToUsage(result.Content.Usage)
            };
        }

        private static string? Instructions(EditTextRequest input)
        {
            var audience = string.IsNullOrWhiteSpace(input.TargetAudience)
                ? null
                : $"The target audience is {input.TargetAudience.Trim()}.";

            var instructions = new[] { audience, input.AdditionalInstructions }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            return instructions.Count > 0 ? string.Join(Environment.NewLine, instructions) : null;
        }
    }
}
