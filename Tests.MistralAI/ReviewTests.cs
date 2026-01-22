using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class ReviewTests : TestBase
    {
        [TestMethod]
        public async Task ReviewText_IsSuccess()
        {
            var action = new Apps.MistralAI.Actions.ReviewActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.ReviewTextRequest
            {
                Model = "mistral-large-latest",
                SourceText = "This is a sample text to be reviewed for quality and accuracy.",
                TargetText = "Ceci est un texte d'exemple à examiner pour la qualité et l'exactitude.",
                TargetLanguage = "fr",
            };
            var response = await action.ReviewText(request);
            Assert.IsNotNull(response);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));
        }

        [TestMethod]
        public async Task ReviewContent_IsSuccess()
        {
            var action = new Apps.MistralAI.Actions.ReviewActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.ReviewFileRequest
            {
                Model = "mistral-large-latest",
                TargetLanguage = "it",
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "Boost the output and quality of your existing localization tools-en-it-TRA_toReview.mxliff"
                }
            };
            var response = await action.ReviewFile(request);
            Assert.IsNotNull(response);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));
        }
    }
}
