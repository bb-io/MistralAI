using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.MistralAI.Models.Responses;

public class ProcessXliffResponse
{
    [Display("XLIFF file")]
    public FileReference File { get; set; } = default!;

    public UsageResponse Usage { get; set; } = new();
}