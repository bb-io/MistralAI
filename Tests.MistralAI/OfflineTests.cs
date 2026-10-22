using Apps.MistralAI.Actions;
using Apps.MistralAI.Ai;
using Apps.MistralAI.Constants;
using Apps.MistralAI.Models.Requests;
using Blackbird.Ai.Llm.Abstract;
using Blackbird.Ai.Llm.Models;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class OfflineTests
    {
        private readonly InMemoryFileManager _files = new();
        private readonly List<string> _loggedWarnings = [];

        [TestMethod]
        public async Task TranslateFile_UnusableAnswers_LogsWarningsAndLeavesSegmentsUntouched()
        {
            var file = _files.Add("broken.xlf", InMemoryFileManager.Xliff("en", "de", ("One", null), ("Two", null)));

            var response = await new UnusableAnswerTranslationActions(Context(), _files).TranslateFile(
                new TranslateContentRequest { File = file, Model = "mistral-large-latest", TargetLanguage = "de" },
                null,
                new GlossaryRequest());

            _loggedWarnings.ForEach(Console.WriteLine);
            Assert.AreEqual(0, response.TargetsUpdatedCount);
            Assert.IsTrue(_loggedWarnings.Count > 0 && _loggedWarnings.All(warning => warning.Contains("[Mistral AI]")));
            Assert.IsTrue(_files.Segments(response.File).All(segment => string.IsNullOrEmpty(segment.GetTarget())));
        }

        [TestMethod]
        public async Task TranslateText_UnusableAnswers_Throws()
        {
            var exception = await Assert.ThrowsExceptionAsync<PluginApplicationException>(() =>
                new UnusableAnswerTranslationActions(Context(), _files).TranslateText(
                    new TranslateTextRequest { Model = "mistral-large-latest", Text = "Hello", SourceLanguage = "en", TargetLanguage = "de" },
                    new GlossaryRequest()));

            Console.WriteLine(exception.Message);
            Assert.IsTrue(_loggedWarnings.Count > 0);
        }

        [TestMethod]
        public async Task ShortenText_NeverFits_Throws()
        {
            var exception = await Assert.ThrowsExceptionAsync<PluginApplicationException>(() =>
                new FixedAnswerShortenActions(Context(), _files, """{"translations":[{"translation_id":"1","translated_text":"Still a sentence that is far too long."}]}""").ShortenText(
                    new ShortenTextRequest { Model = "mistral-large-latest", Text = "A sentence that is definitely far too long.", MaximumLength = 10, Retries = 1 }));

            Console.WriteLine(exception.Message);
            Assert.IsTrue(_loggedWarnings.Count > 0);
        }

        [TestMethod]
        public async Task ShortenText_NonPositiveMaximumLength_Throws()
        {
            await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() =>
                new FixedAnswerShortenActions(Context(), _files, "unused").ShortenText(
                    new ShortenTextRequest { Model = "mistral-large-latest", Text = "Text", MaximumLength = 0 }));
        }

        private InvocationContext Context() => new()
        {
            AuthenticationCredentialsProviders = [new AuthenticationCredentialsProvider(CredsNames.ApiKey, "unused")],
            Logger = new Logger("Debug") { LogWarning = (message, _) => _loggedWarnings.Add(message ?? string.Empty) }
        };

        private sealed class UnusableAnswerTranslationActions(InvocationContext context, IFileManagementClient files)
            : TranslationActions(context, files)
        {
            protected override ILlmClient CreateLlmClient(string model, MistralLlmSettings settings) => new FixedAnswerClient("not json");
        }

        private sealed class FixedAnswerShortenActions(InvocationContext context, IFileManagementClient files, string answer)
            : ShortenActions(context, files)
        {
            protected override ILlmClient CreateLlmClient(string model, MistralLlmSettings settings) => new FixedAnswerClient(answer);
        }

        private sealed class FixedAnswerClient(string answer) : ILlmClient
        {
            public LlmCapabilities Capabilities { get; } = new(ContextTokens: 128_000, MaxOutputTokens: 16_384);

            public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken) =>
                Task.FromResult(LlmResponse.Completed(answer, new LlmUsage(10, 2)));
        }
    }
}
