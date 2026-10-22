using Blackbird.Ai.Diagnostics;
using Blackbird.Ai.Diagnostics.Abstract;
using Blackbird.Ai.Exceptions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.MistralAI.Ai;

public static class AiAdapters
{
    public static IAiLogger Logger(InvocationContext context) => new DelegateAiLogger(
        debug: message => context.Logger?.LogInformation?.Invoke(message, []),
        warn: message => context.Logger?.LogWarning?.Invoke(message, []),
        error: (message, exception) => context.Logger?.LogError?.Invoke($"{message} {exception}", []));

    public static async Task<T> Mapped<T>(Func<Task<T>> run)
    {
        try
        {
            return await run();
        }
        catch (AiConfigurationException exception)
        {
            throw new PluginMisconfigurationException(exception.Message, exception);
        }
    }
}
