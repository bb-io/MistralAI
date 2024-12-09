using System.Globalization;
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
using Blackbird.Xliff.Utils.Models;
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
        return await ProcessXliffInternalAsync(request, ProcessType.Process);
    }

    [Action("Post-edit XLIFF file", Description = "Updates the targets of XLIFF file")]
    public async Task<ProcessXliffResponse> PostEditXliffAsync([ActionParameter] ProcessXliffRequest request)
    {
        return await ProcessXliffInternalAsync(request, ProcessType.PostEdit);
    }
    
    [Action("Get quality scores for XLIFF file", Description = "Get quality scores for XLIFF file")]
    public async Task<ProcessXliffResponse> GetQualityScoreForXliff([ActionParameter] ScoreXliffRequest request)
    {
        var xliff = await DownloadXliffDocumentAsync(request.File);
        
        var translationUnits = await ProcessTranslationUnitsAsync(new()
        {
            Glossary = request.Glossary,
            Model = request.Model,
            Prompt = request.Prompt,
            BucketSize = request.BucketSize,
            FilterGlossary = true
        }, xliff, ProcessType.Score);
        
        UpdateTranslationUnitTargets(translationUnits, xliff, false, ProcessType.Score);
        
        if (request.Threshold != null && request.Condition != null && request.State != null)
        {
            var filteredTUs = new List<string>();
            switch (request.Condition)
            {
                case ">":
                    filteredTUs = translationUnits.Where(x => x.QualityScore > request.Threshold).Select(x => x.TranslationId)
                        .ToList();
                    break;
                case ">=":
                    filteredTUs = translationUnits.Where(x => x.QualityScore >= request.Threshold).Select(x => x.TranslationId)
                        .ToList();
                    break;
                case "=":
                    filteredTUs = translationUnits.Where(x => x.QualityScore == request.Threshold).Select(x => x.TranslationId)
                        .ToList();
                    break;
                case "<":
                    filteredTUs = translationUnits.Where(x => x.QualityScore < request.Threshold).Select(x => x.TranslationId)
                        .ToList();
                    break;
                case "<=":
                    filteredTUs = translationUnits.Where(x => x.QualityScore <= request.Threshold).Select(x => x.TranslationId)
                        .ToList();
                    break;
            }

            filteredTUs.ForEach(x =>
            {
                var translationUnit = xliff.TranslationUnits.FirstOrDefault(tu => tu.Id == x);
                if (translationUnit != null)
                {
                    var stateAttribute = translationUnit.Attributes.FirstOrDefault(x => x.Key == "state");
                    if (!string.IsNullOrEmpty(stateAttribute.Key))
                    {
                        translationUnit.Attributes.Remove(stateAttribute.Key);
                        translationUnit.Attributes.Add("state", request.State);
                    }
                    else
                    {
                        translationUnit.Attributes.Add("state", request.State);
                    }
                }
            });
        }
        
        await using var stream = xliff.ToStream();
        var fileReference = await fileManagementClient.UploadAsync(stream, request.File.ContentType, request.File.Name);
        return new ProcessXliffResponse { File = fileReference };
    }

    private async Task<ProcessXliffResponse> ProcessXliffInternalAsync(ProcessXliffRequest request,
        ProcessType processType)
    {
        var xliffDocument = await DownloadXliffDocumentAsync(request.File);
        var translationUnits = await ProcessTranslationUnitsAsync(request, xliffDocument, processType);

        UpdateTranslationUnitTargets(translationUnits, xliffDocument, request.AddMissingTrailingTags, processType);

        await using var stream = xliffDocument.ToStream();
        var fileReference = await fileManagementClient.UploadAsync(stream, request.File.ContentType, request.File.Name);
        return new ProcessXliffResponse { File = fileReference };
    }

    private void UpdateTranslationUnitTargets(IEnumerable<TranslationEntity> translationEntities,
        XliffDocument xliffDocument, bool? addMissingTrailingTags, ProcessType processType)
    {
        foreach (var translationEntity in translationEntities)
        {
            var translationUnit =
                xliffDocument.TranslationUnits.FirstOrDefault(x => translationEntity.TranslationId == x.Id);
            if (translationUnit == null)
            {
                continue;
            }

            if (processType == ProcessType.Score)
            {
                var attribute = translationUnit.Attributes.FirstOrDefault(x => x.Key == "extradata");
                if (!string.IsNullOrEmpty(attribute.Key))
                {
                    translationUnit.Attributes.Remove(attribute.Key);
                    translationUnit.Attributes.Add("extradata", translationEntity.QualityScore.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    translationUnit.Attributes.Add("extradata", translationEntity.QualityScore.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                if (addMissingTrailingTags == true)
                {
                    UpdateTranslationUnitWithTrailingTags(translationUnit, translationEntity.TranslatedText);
                }
                else
                {
                    translationUnit.Target = translationEntity.TranslatedText;
                }
            }
        }
    }

    private void UpdateTranslationUnitWithTrailingTags(TranslationUnit translationUnit, string fallbackTranslation)
    {
        var sourceContent = translationUnit.Source;
        var targetContent = translationUnit.Target;

        if (sourceContent == null)
        {
            translationUnit.Target = fallbackTranslation;
            return;
        }

        var tagPattern = @"<(?<tag>\w+)([^>]*)>(?<content>.*)</\k<tag>>";
        var sourceMatch = Regex.Match(sourceContent, tagPattern, RegexOptions.Singleline);
        if (sourceMatch.Success)
        {
            var tagName = sourceMatch.Groups["tag"].Value;
            var tagAttributes = sourceMatch.Groups[2].Value;
            var openingTag = $"<{tagName}{tagAttributes}>";
            var closingTag = $"</{tagName}>";

            if (targetContent != null && !targetContent.Contains(openingTag) && !targetContent.Contains(closingTag))
            {
                translationUnit.Target = openingTag + targetContent + closingTag;
            }
        }
        else
        {
            translationUnit.Target = fallbackTranslation;
        }
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

    private string BuildSystemPrompt(string? prompt, ProcessType processType)
    {
        if (processType == ProcessType.Process)
        {
            return PromptBuilder.BuildSystemPrompt(string.IsNullOrEmpty(prompt));
        }

        if (processType == ProcessType.PostEdit)
        {
            return PromptBuilder.GetPostEditPrompt();
        }

        if (processType == ProcessType.Score)
        {
            return PromptBuilder.DefaultSystemPrompt;
        }

        throw new Exception($"Unexpected operation occured. Process type: {processType.ToString()}");
    }

    private UserPromptWithJsonModel BuildUserPrompt(string? prompt, string sourceLanguage, string targetLanguage,
        List<TranslationUnit> translationUnits, ProcessType processType)
    {
        if (processType == ProcessType.Process)
        {
            var list = translationUnits.Select(x => new { x.Id, x.Source }).ToList();
            var json = JsonConvert.SerializeObject(list);
            var userPrompt = PromptBuilder.BuildUserPrompt(prompt, sourceLanguage, targetLanguage, json);
            return new (userPrompt, json);
        }

        if (processType == ProcessType.PostEdit)
        {
            var list = translationUnits.Select(x => new { x.Id, x.Source, x.Target }).ToList();
            var json = JsonConvert.SerializeObject(list);
            var userPrompt =  PromptBuilder.BuildPostEditUserPrompt(prompt, sourceLanguage, targetLanguage, json);
            return new (userPrompt, json);
        }

        if (processType == ProcessType.Score)
        {
            var list = translationUnits.Select(x => new { x.Id, x.Source, x.Target }).ToList();
            var json = JsonConvert.SerializeObject(list);
            var userPrompt =  PromptBuilder.BuildQualityScorePrompt(sourceLanguage, targetLanguage, prompt, json);
            return new (userPrompt, json);
        }

        throw new Exception($"Unexpected operation occured. Process type: {processType.ToString()}");
    }

    private async Task<List<TranslationEntity>> ProcessTranslationUnitsAsync(ProcessXliffRequest request,
        XliffDocument xliff, ProcessType processType)
    {
        var systemPrompt = BuildSystemPrompt(request.Prompt, processType);

        var results = new List<TranslationEntity>();
        var batches = xliff.TranslationUnits.Batch(request.GetBucketSize());
        foreach (var batch in batches)
        {
            var userPrompt = BuildUserPrompt(request.Prompt, xliff.SourceLanguage, xliff.TargetLanguage,
                batch.ToList(), processType);

            if (request.Glossary != null)
            {
                var glossaryPromptPart =
                    await GetGlossaryPromptPart(request.Glossary, userPrompt.Json, request.GetFilterGlossary());
                if (glossaryPromptPart != null)
                {
                    var glossaryPrompt =
                        "Enhance the target text by incorporating relevant terms from our glossary where applicable. " +
                        "Ensure that the translation aligns with the glossary entries for the respective languages. " +
                        "If a term has variations or synonyms, consider them and choose the most appropriate " +
                        "translation to maintain consistency and precision. ";
                    glossaryPrompt += glossaryPromptPart;
                    userPrompt.UserPrompt += glossaryPrompt;
                }
            }

            var apiRequest = new CreateChatCompletionRequest
            {
                Model = request.Model,
                Messages = [new("assistant", systemPrompt), new("user", userPrompt.UserPrompt)],
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
                }, $"Failed to deserialize the response from Mistral AI, try again later. Response: {content}");
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