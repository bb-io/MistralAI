using System.Globalization;
using Apps.MistralAI.Actions;
using Apps.MistralAI.Models.Requests;
using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class ShortenTests : TestBase
    {
        [TestMethod]
        public async Task ShortenText_IsSuccess()
        {
            const string text = "Thank you very much for your order. We have received it and our team will start processing it as soon as possible, usually within one business day.";

            var action = new ShortenActions(InvocationContext, FileManager);
            var response = await action.ShortenText(new ShortenTextRequest
            {
                Model = "mistral-large-latest",
                Text = text,
                MaximumLength = 70
            });

            Print(response);
            Console.WriteLine($"{text.Length} => {response.ShortenedText.Length}");
            Assert.IsTrue(new StringInfo(response.ShortenedText).LengthInTextElements <= 70);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.ShortenedText));
            AssertNoWarningsLogged();
        }

        [TestMethod]
        public async Task ShortenContent_IsSuccess()
        {
            var file = InMemoryFiles.Add("shorten.xlf", """
                <?xml version="1.0" encoding="utf-8"?>
                <xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" xmlns:slr="urn:oasis:names:tc:xliff:sizerestriction:2.0" version="2.0" srcLang="en" trgLang="de">
                  <file id="f1" original="test.xlf">
                    <unit id="u1" slr:sizeRestriction="40">
                      <segment id="s1" state="translated">
                        <source>Free shipping on all orders over fifty euros.</source>
                        <target>Kostenloser Versand für alle Bestellungen mit einem Warenwert von über fünfzig Euro.</target>
                      </segment>
                    </unit>
                    <unit id="u2">
                      <segment id="s2" state="translated">
                        <source>Our customer service team is available every day from eight in the morning until eight in the evening.</source>
                        <target>Unser Kundenservice-Team steht Ihnen jeden Tag von acht Uhr morgens bis acht Uhr abends gerne zur Verfügung.</target>
                      </segment>
                    </unit>
                    <unit id="u3">
                      <segment id="s3" state="translated">
                        <source>Hello.</source>
                        <target>Hallo.</target>
                      </segment>
                    </unit>
                  </file>
                </xliff>
                """);

            var action = new ShortenActions(InvocationContext, InMemoryFiles);
            var response = await action.ShortenContent(new ShortenContentRequest { File = file, Model = "mistral-large-latest", MaximumLength = 70 });

            Print(response);
            var targets = InMemoryFiles.Segments(response.File).Select(segment => segment.GetTarget()).ToList();
            targets.ForEach(target => Console.WriteLine($"{new StringInfo(target).LengthInTextElements,3} {target}"));
            Assert.AreEqual(3, response.UnitsWithLimit);
            Assert.AreEqual(2, response.UnitsOverLimit);
            Assert.AreEqual(2, response.UnitsShortened);
            Assert.AreEqual(0, response.UnitsStillOverLimit);
            Assert.IsTrue(new StringInfo(targets[0]).LengthInTextElements <= 40);
            Assert.IsTrue(new StringInfo(targets[1]).LengthInTextElements <= 70);
            Assert.AreEqual("Hallo.", targets[2]);
            AssertNoWarningsLogged();
        }
    }
}
