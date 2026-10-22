using Apps.MistralAI.Ai;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Blackbird.Ai.Content.Models;
using Blackbird.Ai.Files.Models;
using Blackbird.Ai.Text.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.MistralAI.Actions
{
    [ActionList("Shortening")]
    public class ShortenActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : AiInvocable(invocationContext, fileManagementClient)
    {
        [Action("Shorten content", Description = "Shorten translated file content to fit the size restrictions of its units or the maximum length.")]
        public async Task<ShortenContentResponse> ShortenContent([ActionParameter] ShortenContentRequest input)
        {
            if (input.MaximumLength is <= 0)
                throw new PluginMisconfigurationException("Maximum length must be greater than zero.");
            if (input.Retries is < 0)
                throw new PluginMisconfigurationException("Retries must be zero or greater.");

            var client = CreateLlmClient(input.Model, new());
            var processor = CreateContentProcessor(client, input.Model, input.BucketSize);

            await using var file = await FileManagementClient.DownloadAsync(input.File);
            await using var glossaryFile = await DownloadOptionalAsync(input.Glossary);

            var result = await AiAdapters.Mapped(() => processor.ShortenAsync(new ShortenStreamOptions
            {
                File = file,
                FileName = input.File.Name,
                MediaType = input.File.ContentType,
                OutputFileHandling = input.OutputFileHandling,
                GlossaryTbx = glossaryFile,
                AdditionalInstructions = input.AdditionalInstructions,
                MaxLength = input.MaximumLength,
                MaxRetries = input.Retries ?? ShortenOptions.DefaultMaxRetries
            }));

            var shorten = result.Content.Shorten!;

            return new ShortenContentResponse
            {
                File = await UploadAsync(result.File),
                UnitsWithLimit = shorten.UnitsWithLimit,
                UnitsOverLimit = shorten.UnitsOverLimit,
                UnitsShortened = shorten.UnitsShortened,
                UnitsStillOverLimit = shorten.UnitsStillOverLimit,
                Usage = ToUsage(result.Content.Usage)
            };
        }

        [Action("Shorten text", Description = "Shorten text to fit the maximum length.")]
        public async Task<ShortenTextResponse> ShortenText([ActionParameter] ShortenTextRequest input)
        {
            if (input.MaximumLength <= 0)
                throw new PluginMisconfigurationException("Maximum length must be greater than zero.");
            if (input.Retries is < 0)
                throw new PluginMisconfigurationException("Retries must be zero or greater.");

            var client = CreateLlmClient(input.Model, new());
            var processor = CreateTextProcessor(client, input.Model);

            await using var glossaryFile = await DownloadOptionalAsync(input.Glossary);

            var result = await AiAdapters.Mapped(() => processor.ShortenAsync(new ShortenTextOptions
            {
                Text = input.Text,
                MaxLength = input.MaximumLength,
                MaxRetries = input.Retries ?? ShortenOptions.DefaultMaxRetries,
                AdditionalInstructions = input.AdditionalInstructions,
                GlossaryTbx = glossaryFile
            }));

            ThrowIfUnresolved(result.Content);

            return new ShortenTextResponse
            {
                ShortenedText = result.Text,
                SystemPrompt = result.Content.SystemPrompt ?? string.Empty,
                Usage = ToUsage(result.Content.Usage)
            };
        }
    }
}
