using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class EditTests : TestBase
    {
        [TestMethod]
        public async Task EditText_IsSuccess()
        {
            var action = new Apps.MistralAI.Actions.EditActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.EditTextRequest
            {
                Model = "mistral-large-latest",
                SourceText = "The capital of France is Paris.",
                TargetText = "La capital de Francia es Londres."
            };
            var response = await action.EditText(request);
            Console.WriteLine($"Edited Text: {response.EditedText}");
            Assert.IsNotNull(response);      
        }

        [TestMethod]
        public async Task EditFile_IsSuccess()
        {
            var action = new Apps.MistralAI.Actions.EditActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.EditFileRequest
            {
                Model = "mistral-large-latest",
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "Boost the output and quality of your existing localization tools-en-it-TRA_toReview.mxliff"
                },
                TargetLanguage = "it",
            };
            var response = await action.EditFile(request);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));
            Assert.IsNotNull(response);
        }
    }
}
