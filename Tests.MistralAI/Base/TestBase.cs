using Apps.MistralAI.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Tests.MistralAI.Base;

public class TestBase
{
    protected TestBase()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        
        var connectionApiKey = config["ConnectionDefinition:API key"]
                               ?? throw new InvalidOperationException("API key is missing in appsettings.json");
        Creds = new List<AuthenticationCredentialsProvider>
        {
            new (CredsNames.ApiKey, connectionApiKey)
        };
        
        var folderLocation = config.GetSection("TestFolder").Value!;

        InvocationContext = new InvocationContext
        {
            AuthenticationCredentialsProviders = Creds,
            Logger = new Logger("Debug") { LogWarning = (message, _) => LoggedWarnings.Add(message ?? string.Empty) }
        };

        FileManager = new FileManager(folderLocation);
    }

    protected IEnumerable<AuthenticationCredentialsProvider> Creds { get; set; }

    public InvocationContext InvocationContext { get; set; }

    public FileManager FileManager { get; set; }

    protected InMemoryFileManager InMemoryFiles { get; } = new();

    protected List<string> LoggedWarnings { get; } = [];

    protected void AssertNoWarningsLogged() =>
        Assert.AreEqual(0, LoggedWarnings.Count, string.Join(Environment.NewLine, LoggedWarnings));

    protected static void Print(object result) =>
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
}