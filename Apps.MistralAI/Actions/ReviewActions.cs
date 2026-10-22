using Apps.MistralAI.Ai;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Blackbird.Ai.Files.Models;
using Blackbird.Ai.Summary.Models;
using Blackbird.Ai.Text.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Enums;

namespace Apps.MistralAI.Actions
{
    [ActionList("Review")]
    public class ReviewActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : AiInvocable(invocationContext, fileManagementClient)
    {
        [BlueprintActionDefinition(BlueprintAction.ReviewText)]
        [Action("Review text", Description = "Review the quality of translated text.")]
        public async Task<ReviewTextResponse> ReviewText([ActionParameter] ReviewTextRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.SourceText) || string.IsNullOrWhiteSpace(input.TargetText))
                throw new PluginMisconfigurationException("Both the source text and the target text are required for a review.");

            var client = CreateLlmClient(input.Model, MistralLlmSettings.Parse(input.Temperature, input.TopP, input.MaxTokens));
            var processor = CreateTextProcessor(client, input.Model);

            await using var glossaryFile = await DownloadOptionalAsync(input.Glossary);

            var result = await AiAdapters.Mapped(() => processor.ReviewAsync(new ReviewTextOptions
            {
                SourceText = input.SourceText,
                TargetText = input.TargetText,
                SourceLanguage = input.SourceLanguage,
                TargetLanguage = input.TargetLanguage,
                AdditionalInstructions = input.AdditionalInstructions,
                GlossaryTbx = glossaryFile
            }));

            ThrowIfUnresolved(result.Content);

            return new ReviewTextResponse
            {
                Score = (float)(result.Score ?? throw new PluginApplicationException("The model returned no quality score.")),
                SystemPrompt = result.Content.SystemPrompt ?? string.Empty,
                UserPrompt = UserPrompt(result.Content),
                Usage = ToUsage(result.Content.Usage)
            };
        }

        [BlueprintActionDefinition(BlueprintAction.ReviewFile)]
        [Action("Review", Description = "Review translation. Assumes you have previously translated content in Blackbird through any translation action.")]
        public async Task<ReviewFileResponse> ReviewFile([ActionParameter] ReviewFileRequest input)
        {
            var threshold = input.Threshold ?? 0.8;
            if (threshold is < 0 or > 1)
                throw new PluginMisconfigurationException("Score threshold must be in range 0..1.");

            var client = CreateLlmClient(input.Model, MistralLlmSettings.Parse(input.Temperature, input.TopP, input.MaxTokens));
            var processor = CreateContentProcessor(client, input.Model);

            await using var file = await FileManagementClient.DownloadAsync(input.File);
            await using var glossaryFile = await DownloadOptionalAsync(input.Glossary);

            var result = await AiAdapters.Mapped(() => processor.ReviewAsync(new ReviewStreamOptions
            {
                File = file,
                FileName = input.File.Name,
                MediaType = input.File.ContentType,
                SourceLanguage = input.SourceLanguage,
                TargetLanguage = input.TargetLanguage,
                OutputFileHandling = input.OutputFileHandling,
                GlossaryTbx = glossaryFile,
                AdditionalInstructions = input.AdditionalInstructions,
                Threshold = threshold,
                LowScoreThreshold = threshold,
                StateAboveThreshold = SegmentState.Final,
                Summary = SummaryOptions.FromToggle(input.GenerateDocumentSummary)
            }));

            var review = result.Content.Review!;

            return new ReviewFileResponse
            {
                File = await UploadAsync(result.File),
                Usage = ToUsage(result.Content.Usage),
                TotalSegmentsProcessed = result.Content.ProcessableSegments,
                TotalSegmentsFinalized = review.Finalized,
                TotalSegmentsUnderThreshhold = review.UnderThreshold,
                AverageMetric = (float)review.AverageScore,
                PercentageSegmentsUnderThreshhold = (float)review.PercentageUnderThreshold
            };
        }
    }
}
