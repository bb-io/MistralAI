using Blackbird.Applications.Sdk.Common;

namespace Apps.MistralAI;

public class Application : IApplication
{
    public string Name
    {
        get => "App";
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}