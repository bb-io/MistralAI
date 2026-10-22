using Apps.MistralAI.Actions;
using Apps.MistralAI.Models.Requests;
using Blackbird.Filters.Enums;
using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class EditTests : TestBase
    {
        [TestMethod]
        public async Task EditText_IsSuccess()
        {
            var action = new EditActions(InvocationContext, FileManager);
            var request = new EditTextRequest
            {
                Model = "mistral-large-latest",
                SourceText = "The capital of France is Paris.",
                TargetText = "La capital de Francia es Londres.",
                SourceLanguage = "en",
                TargetLanguage = "es"
            };
            var response = await action.EditText(request);
            Print(response);
            StringAssert.Contains(response.EditedText, "París");
            AssertNoWarningsLogged();
        }

        [TestMethod]
        public async Task EditFile_IsSuccess()
        {
            var file = InMemoryFiles.Add("edit.xlf", InMemoryFileManager.Xliff("en", "it",
                ("The cat is sleeping on the sofa.", "Il cane dorme sul divano."),
                ("Good morning.", "Buongiorno.")));

            var action = new EditActions(InvocationContext, InMemoryFiles);
            var response = await action.EditFile(new EditFileRequest { File = file, Model = "mistral-large-latest", TargetLanguage = "it" });

            Print(response);
            var segments = InMemoryFiles.Segments(response.File);
            segments.ForEach(segment => Console.WriteLine($"{segment.GetSource()} => {segment.GetTarget()}"));
            Assert.AreEqual(2, response.TotalSegmentsReviewed);
            StringAssert.Contains(segments[0].GetTarget(), "gatto");
            Assert.IsTrue(segments.All(segment => segment.State == SegmentState.Reviewed));
            AssertNoWarningsLogged();
        }
    }
}
