using Apps.MistralAI.Actions;
using Apps.MistralAI.Models.Requests;
using Blackbird.Filters.Enums;
using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class TranslationTests : TestBase
    {
        [TestMethod]
        public async Task TranslateText_IsSuccess()
        {
            var action = new TranslationActions(InvocationContext, FileManager);
            var request = new TranslateTextRequest
            {
                Model = "mistral-large-latest",
                Text = "Hello, world!",
                TargetLanguage = "es"
            };
            var response = await action.TranslateText(request, new GlossaryRequest());
            Print(response);
            Assert.AreEqual("¡Hola, mundo!", response.TranslatedText);
            AssertNoWarningsLogged();
        }

        [TestMethod]
        public async Task TranslateContent_IsSuccess()
        {
            var file = InMemoryFiles.Add("translate.xlf", InMemoryFileManager.Xliff("en", "it",
                ("The weather is lovely today.", null),
                ("Please restart the application.", null),
                ("Our office opens at nine.", null)));

            var action = new TranslationActions(InvocationContext, InMemoryFiles);
            var response = await action.TranslateFile(
                new TranslateContentRequest { File = file, Model = "mistral-large-latest", TargetLanguage = "it" },
                null,
                new GlossaryRequest());

            Print(response);
            var segments = InMemoryFiles.Segments(response.File);
            segments.ForEach(segment => Console.WriteLine($"{segment.GetSource()} => {segment.GetTarget()}"));
            Assert.AreEqual(3, response.TargetsUpdatedCount);
            Assert.IsTrue(response.Usage!.TotalTokens > 0);
            Assert.IsTrue(segments.All(segment => segment.State == SegmentState.Translated && segment.GetTarget() != segment.GetSource()));
            AssertNoWarningsLogged();
        }

        [TestMethod]
        public async Task TranslateContent_HtmlWithoutSourceLanguage_ReturnsHtml()
        {
            var file = InMemoryFiles.Add("page.html", "<html><body><h1>Welcome</h1><p>Thank you for your order.</p></body></html>", "text/html");

            var action = new TranslationActions(InvocationContext, InMemoryFiles);
            var response = await action.TranslateFile(
                new TranslateContentRequest { File = file, Model = "mistral-large-latest", TargetLanguage = "fr", OutputFileHandling = "original" },
                null,
                new GlossaryRequest());

            var html = InMemoryFiles.Read(response.File);
            Print(response);
            Console.WriteLine(html);
            Assert.AreEqual(2, response.TargetsUpdatedCount);
            Assert.IsTrue(html.Contains("<h1>") && !html.Contains("Thank you for your order"));
            AssertNoWarningsLogged();
        }
    }
}
