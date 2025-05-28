using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.MistralAI.Utils;

public class TryCatchHelper
{
    public static void TryCatch(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException($"Exception message: {ex.Message}. {message}");
        }
    }
}