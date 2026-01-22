using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Entities;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Apps.MistralAI.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Constants;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Apps.MistralAI.Actions
{
    [ActionList("Editing")]
    public class EditActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AppInvocable(invocationContext)
    {
        [BlueprintActionDefinition(BlueprintAction.EditFile)]
        [Action("Edit", Description = "Edit a translation. Assumes you have previously translated content in Blackbird through any translation action.")]
        public async Task<EditFileResponse> EditFile([ActionParameter] EditFileRequest input)
        {
            var result = new EditFileResponse();

            var stream = await fileManagementClient.DownloadAsync(input.File);
            var content = await Transformation.Parse(stream, input.File.Name);

            content.SourceLanguage ??= input.SourceLanguage;
            content.TargetLanguage ??= input.TargetLanguage;

            if (string.IsNullOrWhiteSpace(content.TargetLanguage))
                throw new PluginMisconfigurationException(
                    "The target language is not defined yet. Please assign the target language in this action.");

            var segments = content.GetUnits()
                .SelectMany(u => u.Segments)
                .Where(s => s.State == SegmentState.Translated)
                .ToList();

            result.TotalSegmentsReviewed = segments.Count;

            if (!segments.Any())
            {
                result.File = await UploadResultFile(content, input.OutputFileHandling);
                return result;
            }

            var batchSize = input.GetBucketSize();
            var updatedCount = 0;

            var systemPrompt = EditPromptBuilder.BuildEditSystemPrompt();

            for (var offset = 0; offset < segments.Count; offset += batchSize)
            {
                var batch = segments.Skip(offset).Take(batchSize).ToList();

                var idToSegment = batch
                    .Select((seg, i) => new { Id = (i + 1).ToString(), Segment = seg })
                    .ToDictionary(x => x.Id, x => x.Segment);

                var batchForJson = idToSegment.Select(kvp => new
                {
                    Id = kvp.Key,
                    SourceText = kvp.Value.GetSource(),
                    TargetText = kvp.Value.GetTarget()
                }).ToList();

                var batchJson = JsonConvert.SerializeObject(batchForJson);

                var userPrompt = EditPromptBuilder.BuildEditUserPrompt(input.AdditionalInstructions, content, batchJson);

                if (input.Glossary != null)
                {
                    var combinedSource = string.Join(" ", batch.Select(s => s.GetSource()));
                    var glossaryPart = await GetGlossaryPromptPart(input.Glossary, combinedSource, filter: true);
                    if (!string.IsNullOrWhiteSpace(glossaryPart))
                    {
                        userPrompt +=
                            "\nUse relevant terms from the glossary where applicable, ensuring the edited translation aligns with glossary entries.\n" +
                            glossaryPart;
                    }
                }

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

                List<TranslationEntity> translations;
                try
                {
                    translations = DeserializeTranslations(raw);
                }
                catch when (input.IgnoreDeserializationErrors == true)
                {
                    continue;
                }

                foreach (var t in translations)
                {
                    if (!idToSegment.TryGetValue(t.TranslationId, out var seg))
                        continue;

                    if (string.IsNullOrWhiteSpace(t.TranslatedText))
                        continue;

                    if (seg.GetTarget() != t.TranslatedText)
                    {
                        updatedCount++;
                        seg.SetTarget(t.TranslatedText);
                        seg.State = SegmentState.Reviewed;
                    }
                }
            }

            result.TotalSegmentsUpdated = updatedCount;
            result.File = await UploadResultFile(content, input.OutputFileHandling);
            return result;
        }

        [BlueprintActionDefinition(BlueprintAction.EditText)]
        [Action("Edit text", Description = "Review translated text and generate an edited version")]
        public async Task<EditTextResponse> EditText([ActionParameter] EditTextRequest input)
        {
            var systemPrompt =
                $"You are receiving a source text{(input.SourceLanguage != null ? $" written in {input.SourceLanguage} " : "")}" +
                $"that was translated into target text{(input.TargetLanguage != null ? $" written in {input.TargetLanguage}" : "")}. " +
                "Review the target text and respond ONLY with the edited version of the target text. " +
                "If no edits are required, respond with the original target text unchanged. " +
                "Do not include any explanations, comments, or additional text in your response. " +
                $"{(input.TargetAudience != null ? $"The target audience is {input.TargetAudience}." : string.Empty)}";

            if (input.Glossary != null)
                systemPrompt += " Use relevant terms from the glossary where applicable, ensuring terminology consistency.";

            if (!string.IsNullOrWhiteSpace(input.AdditionalInstructions))
                systemPrompt += $" {input.AdditionalInstructions}";

            var userPrompt = $@"
                Source text:
                {input.SourceText}

                Target text:
                {input.TargetText}

                Important: Your response must contain ONLY the edited text, with no explanations or comments.
                ";

            if (input.Glossary != null)
            {
                var glossaryPart = await GetGlossaryPromptPart(input.Glossary, input.SourceText, filter: true);
                if (!string.IsNullOrWhiteSpace(glossaryPart))
                    userPrompt += glossaryPart;
            }

            var apiRequest = new CreateChatCompletionRequest
            {
                Model = input.Model,
                Messages =
                [
                    new("assistant", systemPrompt),
                    new("user", userPrompt)
                ],
                MaxTokens = input.MaxTokens,
                Temperature = ParseNullableFloat(input.Temperature),
                TopP = ParseNullableFloat(input.TopP)
            };

            var response = await Client.ExecuteWithJson<SendChatCompletionsResponse>(
                ApiEndpoints.Chat + ApiEndpoints.Completions,
                Method.Post,
                apiRequest);

            return new EditTextResponse
            {
                SystemPrompt = systemPrompt,
                UserPrompt = userPrompt,
                EditedText = response.Choices.First().Message.Content,
                Usage = response.Usage
            };
        }

        //helpers
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

        private static List<TranslationEntity> DeserializeTranslations(string content)
        {
            var token = JToken.Parse(content);

            JArray array;
            if (token.Type == JTokenType.Array)
                array = (JArray)token;
            else if (token.Type == JTokenType.Object && token["translations"] is JArray arr)
                array = arr;
            else
                throw new PluginApplicationException("Unknown format response from Mistral AI: " + content);

            return array.ToObject<List<TranslationEntity>>() ?? new List<TranslationEntity>();
        }

        private async Task<string?> GetGlossaryPromptPart(FileReference glossary, string sourceContent, bool filter)
        {
            if (glossary == null || string.IsNullOrWhiteSpace(glossary.Name))
                return null;

            if (!glossary.Name.EndsWith(".tbx", StringComparison.OrdinalIgnoreCase))
            {
                var ext = Path.GetExtension(glossary.Name);
                throw new PluginMisconfigurationException($"Glossary file must be TBX. Provided: {ext}");
            }

            var glossaryStream = await fileManagementClient.DownloadAsync(glossary);
            var blackbirdGlossary = await glossaryStream.ConvertFromTbx();

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Glossary entries (each entry includes terms in different language; synonyms separated by ;;):");

            var included = false;
            foreach (var entry in blackbirdGlossary.ConceptEntries)
            {
                var allTerms = entry.LanguageSections.SelectMany(x => x.Terms.Select(y => y.Term));
                if (filter && !allTerms.Any(t => Regex.IsMatch(sourceContent, $@"\b{Regex.Escape(t)}\b", RegexOptions.IgnoreCase)))
                    continue;

                included = true;
                sb.AppendLine();
                sb.AppendLine("\tEntry:");
                foreach (var section in entry.LanguageSections)
                    sb.AppendLine($"\t\t{section.LanguageCode}: {string.Join(";; ", section.Terms.Select(t => t.Term))}");
            }

            return included ? sb.ToString() : null;
        }
    }
}
