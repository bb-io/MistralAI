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
using System.Text;
using System.Text.RegularExpressions;

namespace Apps.MistralAI.Actions
{
    [ActionList("Translation")]
    public class TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AppInvocable(invocationContext)
    {

        [BlueprintActionDefinition(BlueprintAction.TranslateText)]
        [Action("Translate text", Description = "Localize the text provided.")]
        public async Task<TranslateTextResponse> TranslateText(
        [ActionParameter] TranslateTextRequest input,
        [ActionParameter] GlossaryRequest glossary)
        {
            var systemPrompt =
                "You are a text localizer. Localize the provided text for the specified locale while preserving the " +
                "original text structure and any markup/tags. Respond with localized text and only localized text, nothing else.";

            var userPrompt = $@"
                Original text: {input.Text}
                Source language: {input.SourceLanguage ?? "auto-detect"}
                Target language / locale: {input.TargetLanguage}
                ";

            if (glossary != null)
            {
                var glossaryPromptPart = await GetGlossaryPromptPart(glossary.Glossary, input.Text, filter: true);
                if (glossaryPromptPart != null)
                {
                    userPrompt +=
                        "\nEnhance the localized text by incorporating relevant terms from our glossary where applicable. " +
                        "If you encounter glossary terms in the text, ensure that the localized text aligns with these entries. " +
                        "If a term has variations or synonyms, choose the most appropriate one to maintain consistency.\n" +
                        glossaryPromptPart;
                }
            }

            userPrompt += "\nLocalized text: ";

            var apiRequest = new CreateChatCompletionRequest
            {
                Model = input.Model,
                Messages =
                [
                    new("assistant", systemPrompt),
                    new("user", userPrompt)
                ]
            };

            var response =
                await Client.ExecuteWithJson<SendChatCompletionsResponse>(
                    ApiEndpoints.Chat + ApiEndpoints.Completions,
                    Method.Post,
                    apiRequest);

            var translated = response.Choices.First().Message.Content;

            return new TranslateTextResponse
            {
                SystemPrompt = systemPrompt,
                UserPrompt = userPrompt,
                TranslatedText = translated,
                Usage = response.Usage
            };
        }


        [BlueprintActionDefinition(BlueprintAction.TranslateFile)]
        [Action("Translate", Description = "Translate file content retrieved from a CMS or file storage")]
        public async Task<TranslateFileResult> TranslateFile(
        [ActionParameter] TranslateContentRequest input,
        [ActionParameter, Display("Additional instructions", Description = "Specify additional instructions to be applied to the translation")] string? prompt,
        [ActionParameter] GlossaryRequest glossary)
        {
            var stream = await fileManagementClient.DownloadAsync(input.File);
            var content = await Transformation.Parse(stream, input.File.Name);

            content.TargetLanguage ??= input.TargetLanguage;
            content.SourceLanguage ??= input.SourceLanguage;

            if (string.IsNullOrWhiteSpace(content.TargetLanguage))
                throw new PluginMisconfigurationException("The target language is not defined yet. Please assign the target language in this action.");

            var units = content.GetUnits();
            var segments = units.SelectMany(x => x.Segments).ToList();

            var result = new TranslateFileResult
            {
                TotalSegmentsCount = segments.Count
            };

            var translatableSegments = segments
                .Where(s => s.State == null || s.State == SegmentState.Initial)
                .ToList();

            result.TotalTranslatable = translatableSegments.Count;

            if (!translatableSegments.Any())
            {
                result.File = await UploadResultFile(content, input.OutputFileHandling);
                return result;
            }

            var usage = new UsageResponse();
            var batchSize = input.GetBucketSize();
            var systemPromptBase = BuildSystemPromptForFile(content.SourceLanguage, content.TargetLanguage, prompt);

            int batchCounter = 0;
            int updatedCount = 0;

            for (var offset = 0; offset < translatableSegments.Count; offset += batchSize)
            {
                batchCounter++;
                var batchList = translatableSegments
                    .Skip(offset)
                    .Take(batchSize)
                    .ToList();

                var idToSegment = batchList
                    .Select((segment, index) => new { Id = offset + index + 1, Segment = segment })
                    .ToDictionary(x => x.Id.ToString(), x => x.Segment);

                var jsonPayload = JsonConvert.SerializeObject(
                    idToSegment.Select(kvp => new
                    {
                        Id = kvp.Key,
                        Text = kvp.Value.GetSource()
                    }));

                var userPrompt = BuildUserPromptForFile(jsonPayload, content.TargetLanguage, prompt);

                if (glossary?.Glossary != null)
                {
                    var combinedSource = string.Join(" ", idToSegment.Values.Select(s => s.GetSource()));
                    var glossaryPromptPart = await GetGlossaryPromptPart(glossary.Glossary, combinedSource, filter: true);
                    if (glossaryPromptPart != null)
                    {
                        userPrompt +=
                            "\nEnhance the translation by incorporating relevant terms from our glossary where applicable. " +
                            "Ensure consistency with the glossary entries for each language. " +
                            glossaryPromptPart;
                    }
                }

                var apiRequest = new CreateChatCompletionRequest
                {
                    Model = input.Model,
                    Messages =
                    [
                        new("assistant", systemPromptBase),
                        new("user", userPrompt)
                    ],
                    ResponseFormat = new() { Type = "json_object" }
                };

                var response =
                    await Client.ExecuteWithJson<SendChatCompletionsResponse>(
                        ApiEndpoints.Chat + ApiEndpoints.Completions,
                        Method.Post,
                        apiRequest);

                usage += response.Usage;

                var contentStr = response.Choices.First().Message.Content;
                var translations = DeserializeTranslations(contentStr);

                foreach (var t in translations)
                {
                    if (!idToSegment.TryGetValue(t.TranslationId, out var segment))
                        continue;

                    if (string.IsNullOrWhiteSpace(t.TranslatedText))
                        continue;

                    if (segment.GetTarget() != t.TranslatedText)
                    {
                        updatedCount++;
                        segment.SetTarget(t.TranslatedText);
                        segment.State = SegmentState.Translated;
                    }
                }
            }

            result.ProcessedBatchesCount = batchCounter;
            result.TargetsUpdatedCount = updatedCount;
            result.SystemPrompt = systemPromptBase;
            result.Usage = usage;

            result.File = await UploadResultFile(content, input.OutputFileHandling);

            return result;
        }

        //helpers
        private static string BuildSystemPromptForFile(string? sourceLang, string? targetLang, string? extra)
        {
            var sb = new StringBuilder();

            sb.Append("You are a professional translation engine. ");
            if (!string.IsNullOrWhiteSpace(sourceLang))
                sb.Append($"Translate from {sourceLang} ");
            else
                sb.Append("Translate from the source language detected in the text ");

            if (!string.IsNullOrWhiteSpace(targetLang))
                sb.Append($"to {targetLang}. ");
            else
                sb.Append("to the requested target language. ");

            sb.Append("Preserve all markup (HTML, XML, placeholders, tags) and structure exactly as in the source. ");
            sb.Append("Return a JSON object with property 'translations' which is an array of objects having 'translationId' and 'translatedText' fields. ");

            if (!string.IsNullOrWhiteSpace(extra))
            {
                sb.Append("Additional instructions: ");
                sb.Append(extra);
            }

            return sb.ToString();
        }

        private static string BuildUserPromptForFile(string jsonPayload, string? targetLang, string? extra)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Translate the following segments represented as JSON array:");
            sb.AppendLine(jsonPayload);
            if (!string.IsNullOrWhiteSpace(targetLang))
            {
                sb.AppendLine();
                sb.AppendLine($"Target language: {targetLang}");
            }

            if (!string.IsNullOrWhiteSpace(extra))
            {
                sb.AppendLine();
                sb.AppendLine($"Additional instructions: {extra}");
            }

            sb.AppendLine();
            sb.AppendLine("Return JSON only in the specified format.");

            return sb.ToString();
        }

        private static List<TranslationEntity> DeserializeTranslations(string content)
        {
            var result = new List<TranslationEntity>();

            TryCatchHelper.TryCatch(() =>
            {
                var jToken = JToken.Parse(content);

                JArray array;
                if (jToken.Type == JTokenType.Array)
                {
                    array = (JArray)jToken;
                }
                else if (jToken.Type == JTokenType.Object && jToken["translations"] is JArray arr)
                {
                    array = arr;
                }
                else
                {
                    throw new InvalidOperationException("Unknown format response from Mistral AI");
                }

                var list = array.ToObject<List<TranslationEntity>>()
                           ?? throw new InvalidOperationException("Empty array of translation units");

                result.AddRange(list);
            }, $"Failed to deserialize the response from Mistral AI, try again later. Response: {content}");

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

        protected async Task<string?> GetGlossaryPromptPart(FileReference glossary, string sourceContent, bool filter)
        {
            if (glossary == null || string.IsNullOrWhiteSpace(glossary.Name))
                return null;

            if (!glossary.Name.EndsWith(".tbx", StringComparison.OrdinalIgnoreCase))
            {
                var extension = Path.GetExtension(glossary.Name);
                throw new PluginMisconfigurationException(
                    $"Glossary file must be in TBX format. But the provided file has {extension} extension.");
            }

            var glossaryStream = await fileManagementClient.DownloadAsync(glossary);
            var blackbirdGlossary = await glossaryStream.ConvertFromTbx();

            var glossaryPromptPart = new StringBuilder();
            glossaryPromptPart.AppendLine();
            glossaryPromptPart.AppendLine();
            glossaryPromptPart.AppendLine("Glossary entries (each entry includes terms in different language. Each language may have a few synonymous variations which are separated by ;;):");

            var entriesIncluded = false;
            foreach (var entry in blackbirdGlossary.ConceptEntries)
            {
                var allTerms = entry.LanguageSections.SelectMany(x => x.Terms.Select(y => y.Term));
                if (filter && !allTerms.Any(x => Regex.IsMatch(sourceContent, $@"\b{x}\b", RegexOptions.IgnoreCase)))
                    continue;

                entriesIncluded = true;

                glossaryPromptPart.AppendLine();
                glossaryPromptPart.AppendLine("\tEntry:");

                foreach (var section in entry.LanguageSections)
                {
                    glossaryPromptPart.AppendLine(
                        $"\t\t{section.LanguageCode}: {string.Join(";; ", section.Terms.Select(term => term.Term))}");
                }
            }

            return entriesIncluded ? glossaryPromptPart.ToString() : null;
        }
    }
}
