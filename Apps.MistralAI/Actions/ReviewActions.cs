using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Apps.MistralAI.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Constants;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff1;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.Xml.Linq;

namespace Apps.MistralAI.Actions
{
    [ActionList("Review")]
    public class ReviewActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AppInvocable(invocationContext)
    {
        [BlueprintActionDefinition(BlueprintAction.ReviewText)]
        [Action("Review text", Description = "Review the quality of translated text.")]
        public async Task<ReviewTextResponse> ReviewText([ActionParameter] ReviewTextRequest input)
        {
            var json = JsonConvert.SerializeObject(new
            {
                translation_id = "1",
                source_text = input.SourceText,
                target_text = input.TargetText
            });

            var systemPrompt = ReviewPromptBuilder.BuildReviewSystemPrompt();
            var userPrompt = ReviewPromptBuilder.BuildReviewUserPrompt(
                input.AdditionalInstructions,
                input.SourceLanguage,
                input.TargetLanguage,
                json);

            var apiRequest = new CreateChatCompletionRequest
            {
                Model = input.Model,
                Messages =
                [
                    new("assistant", systemPrompt),
                new("user", userPrompt)
                ],
                ResponseFormat = new() { Type = "json_object" },
                Temperature = ParseNullableFloat(input.Temperature),
                TopP = ParseNullableFloat(input.TopP),
                MaxTokens = input.MaxTokens
            };

            var response = await Client.ExecuteWithJson<SendChatCompletionsResponse>(
                ApiEndpoints.Chat + ApiEndpoints.Completions,
                Method.Post,
                apiRequest);

            var raw = response.Choices.First().Message.Content;
            var score = ReviewResponseDeserializer.DeserializeScore(raw);

            return new ReviewTextResponse
            {
                Score = score,
                SystemPrompt = systemPrompt,
                UserPrompt = userPrompt,
                Usage = response.Usage
            };
        }

        [BlueprintActionDefinition(BlueprintAction.ReviewFile)]
        [Action("Review", Description = "Review translation. Assumes you have previously translated content in Blackbird through any translation action.")]
        public async Task<ReviewFileResponse> ReviewFile([ActionParameter] ReviewFileRequest input)
        {
            var result = new ReviewFileResponse();

            var stream = await fileManagementClient.DownloadAsync(input.File);
            var content = await Transformation.Parse(stream, input.File.Name);

            content.SourceLanguage ??= input.SourceLanguage;
            content.TargetLanguage ??= input.TargetLanguage;

            var segments = content.GetUnits()
                .SelectMany(u => u.Segments)
                .Where(s => s.State != SegmentState.Final)
                .ToList();


            result.TotalSegmentsProcessed = segments.Count;

            float totalScore = 0;
            var finalizedSegments = 0;
            var underThresholdSegments = 0;

            foreach (var segment in segments)
            {
                var json = JsonConvert.SerializeObject(new
                {
                    translation_id = segment.Id,
                    source_text = segment.GetSource(),
                    target_text = segment.GetTarget()
                });

                var userPrompt = ReviewPromptBuilder.BuildReviewUserPrompt(
                    input.AdditionalInstructions,
                    content.SourceLanguage,
                    content.TargetLanguage,
                    json);

                var systemPrompt = ReviewPromptBuilder.BuildReviewSystemPrompt();

                var apiRequest = new CreateChatCompletionRequest
                {
                    Model = input.Model,
                    Messages =
                    [
                        new("assistant", systemPrompt),
                        new("user", userPrompt)
                    ],
                    ResponseFormat = new() { Type = "json_object" },
                    MaxTokens = input.MaxTokens,
                    Temperature = ParseNullableFloat(input.Temperature),
                    TopP = ParseNullableFloat(input.TopP)
                };

                var response = await Client.ExecuteWithJson<SendChatCompletionsResponse>(
                    ApiEndpoints.Chat + ApiEndpoints.Completions,
                    Method.Post,
                    apiRequest);

                result.Usage += response.Usage;

                var raw = response.Choices.First().Message.Content;
                var qualityScore = ReviewResponseDeserializer.DeserializeScore(raw);

                segment.TargetAttributes.RemoveAll(attr => attr.Name == "extradata");
                segment.TargetAttributes.Add(new XAttribute(
                    "extradata",
                    qualityScore.ToString("0.###", CultureInfo.InvariantCulture)));

                totalScore += qualityScore;

                if (qualityScore >= 0.8f)
                {
                    finalizedSegments++;
                    segment.State = SegmentState.Final;
                }

                if (qualityScore < 0.6f)
                    underThresholdSegments++;
            }

            result.TotalSegmentsFinalized = finalizedSegments;
            result.TotalSegmentsUnderThreshhold = underThresholdSegments;
            result.AverageMetric = segments.Count > 0 ? totalScore / segments.Count : 0;
            result.PercentageSegmentsUnderThreshhold =
                segments.Count > 0 ? (float)underThresholdSegments / segments.Count * 100 : 0;

            result.File = await UploadResultFile(content, input.OutputFileHandling);
            return result;
        }

        private async Task<FileReference> UploadResultFile(Transformation content, string? outputFileHandling)
        {
            if (outputFileHandling == "original")
            {
                var targetContent = content.Target();
                return await fileManagementClient.UploadAsync(
                    targetContent.Serialize().ToStream(),
                    targetContent.OriginalMediaType,
                    targetContent.OriginalName);
            }

            if (outputFileHandling == "xliff1")
            {
                var xliff1String = Xliff1Serializer.Serialize(content);
                return await fileManagementClient.UploadAsync(
                    xliff1String.ToStream(),
                    MediaTypes.Xliff,
                    content.XliffFileName);
            }

            return await fileManagementClient.UploadAsync(
                content.Serialize().ToStream(),
                MediaTypes.Xliff,
                content.XliffFileName);
        }

        private static float? ParseNullableFloat(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Replace(',', '.');
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : null;
        }
    }
}
