using System.Text;
using System.Text.RegularExpressions;
using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Entities;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Apps.MistralAI.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Extensions;
using MoreLinq;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.MistralAI.Actions;

[ActionList]
public class XliffActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : AppInvocable(invocationContext)
{
    [Action("Process XLIFF file",
        Description =
            "Processes each translation unit in the XLIFF file according to the provided instructions (by default it just translates the source tags) and updates the target text for each unit.")]
    public async Task<ProcessXliffResponse> ProcessXliffAsync([ActionParameter] ProcessXliffRequest request)
    {
        var xliffDocument = await DownloadXliffDocumentAsync(request.File);
        var translationUnits = await ProcessTranslationUnitsAsync(request, xliffDocument, ProcessType.Process);

        foreach (var translationEntity in translationUnits)
        {
            var translationUnit =
                xliffDocument.TranslationUnits.FirstOrDefault(x => translationEntity.TranslationId == x.Id);
            if (translationUnit != null)
            {
                if (request.AddMissingTrailingTags is true)
                {
                    var sourceContent = translationUnit.Source;
                    var targetContent = translationUnit.Target;

                    if (sourceContent == null)
                    {
                        continue;
                    }

                    var tagPattern = @"<(?<tag>\w+)([^>]*)>(?<content>.*)</\k<tag>>";
                    var sourceMatch = Regex.Match(sourceContent, tagPattern, RegexOptions.Singleline);
                    if (sourceMatch.Success)
                    {
                        var tagName = sourceMatch.Groups["tag"].Value;
                        var tagAttributes = sourceMatch.Groups[2].Value;
                        var openingTag = $"<{tagName}{tagAttributes}>";
                        var closingTag = $"</{tagName}>";

                        if (targetContent != null && !targetContent.Contains(openingTag) &&
                            !targetContent.Contains(closingTag))
                        {
                            translationUnit.Target = openingTag + targetContent + closingTag;
                        }
                    }
                    else
                    {
                        translationUnit.Target = translationEntity.TranslatedText;
                    }
                }
                else
                {
                    translationUnit.Target = translationEntity.TranslatedText;
                }
            }
        }

        var stream = xliffDocument.ToStream();
        var fileReference = await fileManagementClient.UploadAsync(stream, request.File.ContentType, request.File.Name);
        return new ProcessXliffResponse { File = fileReference };
    }

    [Action("Post-edit XLIFF file", Description = "Updates the targets of XLIFF file")]
    public async Task<ProcessXliffResponse> PostEditXliffAsync([ActionParameter] ProcessXliffRequest request)
    {
        var xliffDocument = await DownloadXliffDocumentAsync(request.File);
        var translationUnits = await ProcessTranslationUnitsAsync(request, xliffDocument, ProcessType.PostEdit);

        foreach (var translationEntity in translationUnits)
        {
            var translationUnit =
                xliffDocument.TranslationUnits.FirstOrDefault(x => translationEntity.TranslationId == x.Id);
            if (translationUnit != null)
            {
                if (request.AddMissingTrailingTags is true)
                {
                    var sourceContent = translationUnit.Source;
                    var targetContent = translationUnit.Target;

                    if (sourceContent == null)
                    {
                        continue;
                    }

                    var tagPattern = @"<(?<tag>\w+)([^>]*)>(?<content>.*)</\k<tag>>";
                    var sourceMatch = Regex.Match(sourceContent, tagPattern, RegexOptions.Singleline);
                    if (sourceMatch.Success)
                    {
                        var tagName = sourceMatch.Groups["tag"].Value;
                        var tagAttributes = sourceMatch.Groups[2].Value;
                        var openingTag = $"<{tagName}{tagAttributes}>";
                        var closingTag = $"</{tagName}>";

                        if (targetContent != null && !targetContent.Contains(openingTag) &&
                            !targetContent.Contains(closingTag))
                        {
                            translationUnit.Target = openingTag + targetContent + closingTag;
                        }
                    }
                    else
                    {
                        translationUnit.Target = translationEntity.TranslatedText;
                    }
                }
                else
                {
                    translationUnit.Target = translationEntity.TranslatedText;
                }
            }
        }

        var stream = xliffDocument.ToStream();
        var fileReference = await fileManagementClient.UploadAsync(stream, request.File.ContentType, request.File.Name);
        return new ProcessXliffResponse { File = fileReference };
    }


    private async Task<XliffDocument> DownloadXliffDocumentAsync(FileReference file)
    {
        var fileStream = await fileManagementClient.DownloadAsync(file);
        var xliffMemoryStream = new MemoryStream();
        await fileStream.CopyToAsync(xliffMemoryStream);
        xliffMemoryStream.Position = 0;

        var xliffDocument = xliffMemoryStream.ToXliffDocument();
        if (xliffDocument.TranslationUnits.Count == 0)
        {
            throw new InvalidOperationException("The XLIFF file does not contain any translation units.");
        }

        return xliffDocument;
    }

    private async Task<List<TranslationEntity>> ProcessTranslationUnitsAsync(ProcessXliffRequest request,
        XliffDocument xliff, ProcessType processType)
    {
        var systemPrompt = processType == ProcessType.Process
            ? PromptBuilder.BuildSystemPrompt(string.IsNullOrEmpty(request.Prompt))
            : PromptBuilder.GetPostEditPrompt();

        var results = new List<TranslationEntity>();
        var batches = xliff.TranslationUnits.Batch(request.GetBucketSize());

        foreach (var batch in batches)
        {
            IEnumerable<object> selectedValues = processType == ProcessType.Process
                ? batch.Select(x => new { x.Id, x.Source }).ToList()
                : batch.Select(x => new { x.Id, x.Source, x.Target }).ToList();

            var json = JsonConvert.SerializeObject(selectedValues);
            var userPrompt = processType == ProcessType.Process
                ? PromptBuilder.BuildUserPrompt(request.Prompt, xliff.SourceLanguage, xliff.TargetLanguage, json)
                : PromptBuilder.BuildPostEditUserPrompt(request.Prompt, xliff.SourceLanguage, xliff.TargetLanguage,
                    json);

            if (request.Glossary != null)
            {
                var glossaryPromptPart =
                    await GetGlossaryPromptPart(request.Glossary, json, request.GetFilterGlossary());
                if (glossaryPromptPart != null)
                {
                    var glossaryPrompt =
                        "Enhance the target text by incorporating relevant terms from our glossary where applicable. " +
                        "Ensure that the translation aligns with the glossary entries for the respective languages. " +
                        "If a term has variations or synonyms, consider them and choose the most appropriate " +
                        "translation to maintain consistency and precision. ";
                    glossaryPrompt += glossaryPromptPart;
                    userPrompt += glossaryPrompt;
                }
            }

            var apiRequest = new CreateChatCompletionRequest
            {
                Model = request.Model,
                Messages = [new("assistant", systemPrompt), new("user", userPrompt)],
                ResponseFormat = new() { Type = "json_object" }
            };

            var response =
                await Client.ExecuteWithJson<SendChatCompletionsResponse>(ApiEndpoints.Chat + ApiEndpoints.Completions,
                    Method.Post, apiRequest, Creds);

            var content = response.Choices.First().Message.Content;
            TryCatchHelper.TryCatch(() =>
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<TranslationEntities>(content)!;
                    results.AddRange(deserializedResponse.Translations);
                }, $"Failed to deserialize the response from OpenAI, try again later. Response: {content}");
        }

        return results;
    }

    protected async Task<string?> GetGlossaryPromptPart(FileReference glossary, string sourceContent, bool filter)
    {
        var glossaryStream = await fileManagementClient.DownloadAsync(glossary);
        var blackbirdGlossary = await glossaryStream.ConvertFromTbx();

        var glossaryPromptPart = new StringBuilder();
        glossaryPromptPart.AppendLine();
        glossaryPromptPart.AppendLine();
        glossaryPromptPart.AppendLine("Glossary entries (each entry includes terms in different language. Each " +
                                      "language may have a few synonymous variations which are separated by ;;):");

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