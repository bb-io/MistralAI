using Apps.MistralAI.Actions;
using Apps.MistralAI.Ai;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Constants;
using Blackbird.Ai.Llm.Abstract;
using Blackbird.Ai.Llm.Models;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json.Linq;
using Tests.MistralAI.Base;

namespace Tests.MistralAI;

[TestClass]
public class SegmentNotesOfflineTests
{
    private const string Model = "mistral-large-latest";

    private const string FileWithNotes = """
        <xliff version="2.0" srcLang="en" trgLang="de" xmlns="urn:oasis:names:tc:xliff:document:2.0">
          <file id="f1" original="labels.xlf">
            <unit id="u1">
              <notes><note>Label for missing job type</note></notes>
              <segment id="s1" state="translated"><source>Add job type</source><target>Jobart hinzufügen</target></segment>
            </unit>
            <unit id="u2">
              <notes><note>Add button</note></notes>
              <segment id="s2" state="translated"><source>Add job types</source><target>Jobarten hinzufügen</target></segment>
            </unit>
          </file>
        </xliff>
        """;

    private readonly InMemoryFileManager _files = new();
    private readonly RecordingClient _client = new();

    [TestMethod]
    public async Task Translate_IncludeSegmentNotes_SendsEachUnitsNotesToTheModel()
    {
        await new RecordingTranslationActions(_files, _client).TranslateFile(
            new TranslateContentRequest { File = UntranslatedFileWithNotes(), Model = Model, TargetLanguage = "de", IncludeSegmentNotes = true }, null, new GlossaryRequest());

        CollectionAssert.AreEquivalent(new[] { "Label for missing job type", "Add button" }, _client.SentNotes.ToArray());
        StringAssert.Contains(_client.Requests.Single().SystemPrompt, "### SEGMENT NOTES");
    }

    [TestMethod]
    public async Task Translate_IncludeSegmentNotesNotSet_SendsNoNotes()
    {
        await new RecordingTranslationActions(_files, _client).TranslateFile(
            new TranslateContentRequest { File = UntranslatedFileWithNotes(), Model = Model, TargetLanguage = "de" }, null, new GlossaryRequest());

        Assert.AreEqual(0, _client.SentNotes.Count);
        Assert.IsFalse(_client.Requests.Single().SystemPrompt.Contains("### SEGMENT NOTES"));
    }

    [TestMethod]
    public async Task ReviewFile_IncludeSegmentNotes_SendsEachUnitsNotesToTheModel()
    {
        await new RecordingReviewActions(_files, _client).ReviewFile(
            new ReviewFileRequest { File = _files.Add("labels.xlf", FileWithNotes), Model = Model, TargetLanguage = "de", IncludeSegmentNotes = true });

        CollectionAssert.AreEquivalent(new[] { "Label for missing job type", "Add button" }, _client.SentNotes.ToArray());
    }

    private Blackbird.Applications.Sdk.Common.Files.FileReference UntranslatedFileWithNotes() =>
        _files.Add("labels.xlf", FileWithNotes
            .Replace(" state=\"translated\"", string.Empty)
            .Replace("<target>Jobart hinzufügen</target>", string.Empty)
            .Replace("<target>Jobarten hinzufügen</target>", string.Empty));

    private static InvocationContext Context() => new()
    {
        AuthenticationCredentialsProviders = [new AuthenticationCredentialsProvider(CredsNames.ApiKey, "unused")]
    };

    private sealed class RecordingTranslationActions(IFileManagementClient files, ILlmClient client)
        : TranslationActions(Context(), files)
    {
        protected override ILlmClient CreateLlmClient(string model, MistralLlmSettings settings) => client;
    }

    private sealed class RecordingReviewActions(IFileManagementClient files, ILlmClient client)
        : ReviewActions(Context(), files)
    {
        protected override ILlmClient CreateLlmClient(string model, MistralLlmSettings settings) => client;
    }

    private sealed class RecordingClient : ILlmClient
    {
        public List<LlmRequest> Requests { get; } = [];

        public List<string> SentNotes { get; } = [];

        public LlmCapabilities Capabilities { get; } = new(ContextTokens: 128_000, MaxOutputTokens: 16_384);

        public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var start = request.UserPrompt.IndexOf('[');
            var end = request.UserPrompt.LastIndexOf(']');
            var items = JArray.Parse(request.UserPrompt[start..(end + 1)]).Cast<JObject>().ToList();

            SentNotes.AddRange(items.SelectMany(item => item["notes"]?.Values<string>() ?? []).OfType<string>());

            var answers = items.Select(item => new JObject
            {
                ["translation_id"] = item.Value<string>("id"),
                ["translated_text"] = "Übersetzt",
                ["quality_score"] = 0.9
            });

            var response = new JObject { ["translations"] = new JArray(answers) };
            return Task.FromResult(LlmResponse.Completed(response.ToString(), new LlmUsage(10, 5)));
        }
    }
}
