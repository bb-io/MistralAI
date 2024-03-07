using Apps.MistralAI.Invocables;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.MistralAI.Actions;

[ActionList]
public class Actions : AppInvocable
{
    public Actions(InvocationContext invocationContext) : base(invocationContext)
    {
    }
}