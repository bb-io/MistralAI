using Apps.MistralAI.Actions;
using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class TranslationTests : TestBase
    {
        [TestMethod]
        public async Task TranslateText_Test()
        {
            var action = new TranslationActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.TranslateTextRequest
            {
                Model = "mistral-large-latest",
                Text = "Hello, world!",
                TargetLanguage = "es"
            };
            var response = await action.TranslateText(request, new Apps.MistralAI.Models.Requests.GlossaryRequest { });
            Assert.IsNotNull(response);
            Assert.AreEqual("¡Hola, mundo!", response.TranslatedText);
        }

        [TestMethod]
        public async Task TranslateContent_Test()
        {
            var action = new TranslationActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.TranslateContentRequest
            {
                File= new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "Boost the output and quality of your existing localization tools-en-it-TRA (1).mxliff"
                },
                Model = "mistral-large-latest",
                TargetLanguage = "it"
            };
            var response = await action.TranslateFile(request, "" ,new Apps.MistralAI.Models.Requests.GlossaryRequest
            {});
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));

            Assert.IsNotNull(response);
        }
    }
}
