using Apps.MistralAI.Actions;
using FluentAssertions;
using Tests.MistralAI.Base;

namespace Tests.MistralAI;

[TestClass]
public class Chat : TestBase
{
    [TestMethod]
    public async Task ChatReturnsNotNullResponse()
    {
        var actions = new ChatActions(InvocationContext);
        var result = await actions.SendPrompt(new()
        {
            Model = "mistral-large-latest",
            Message = "Hello, world!"
        });

        
        Console.WriteLine(result.Content);
        
        result.Content.Should().NotBeNullOrEmpty();
        result.Usage.Should().NotBeNull();
    }
}