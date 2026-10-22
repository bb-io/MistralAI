using Apps.MistralAI.Actions;
using Apps.MistralAI.Models.Requests;
using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class SummarizeTests : TestBase
    {
        private const string Article =
            "Our new mobile app lets customers track their orders in real time, change the delivery address until the parcel leaves the warehouse, " +
            "and chat with support directly from the order page. The app is available for iOS and Android and replaces the old order tracking website, " +
            "which will be switched off at the end of the year. Customers who sign in with their existing account keep their full order history.";

        [TestMethod]
        public async Task SummarizeText_IsSuccess()
        {
            var action = new SummarizeActions(InvocationContext, FileManager);
            var response = await action.SummarizeText(new SummarizeTextRequest
            {
                Text = Article,
                Model = "mistral-large-latest",
                Language = "fr-FR",
                MaximumWords = 30,
                TargetAudience = "existing customers"
            });

            Print(response);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Summary));
            Assert.IsTrue(response.Summary.Length < Article.Length);
            Assert.IsTrue(response.Usage.TotalTokens > 0);
            AssertNoWarningsLogged();
        }

        [TestMethod]
        public async Task Summarize_IsSuccess()
        {
            var file = InMemoryFiles.Add("summarize.xlf", InMemoryFileManager.Xliff("en", "it",
                ("Our new mobile app lets customers track their orders in real time.", "La nostra nuova app consente ai clienti di seguire i propri ordini in tempo reale."),
                ("The old order tracking website will be switched off at the end of the year.", "Il vecchio sito di tracciamento degli ordini sarà disattivato alla fine dell'anno."),
                ("Customers keep their full order history.", "I clienti conservano l'intera cronologia degli ordini.")));

            var action = new SummarizeActions(InvocationContext, InMemoryFiles);
            var response = await action.Summarize(new SummarizeContentRequest { File = file, Model = "mistral-large-latest", MaximumWords = 25 });

            Print(response);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Summary));
            Assert.IsTrue(response.Usage.TotalTokens > 0);
            AssertNoWarningsLogged();
        }
    }
}
