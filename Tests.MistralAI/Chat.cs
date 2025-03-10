using Apps.MistralAI.Actions;
using Apps.MistralAI.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
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


    [TestMethod]
    public async Task ExtractTextReturnsNotNullResponse()
    {
        var actions = new OCRActions(InvocationContext,FileManager);
        //var file = new FileReference { Name = "test1.jpg", ContentType= "image/jpeg" };
        var file = new FileReference { Name = "test.pdf", ContentType = "application/pdf" };
        var input = new FileInput 
        { 
           File = file
        };
        var result = await actions.ExtractTextFromImage(input);

        Assert.IsNotNull(result);
        //Console.WriteLine(result.RawJson);

    }
}