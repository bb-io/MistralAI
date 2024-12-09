using Apps.MistralAI.DataSourceHandlers;
using Apps.MistralAI.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.MistralAI.Models.Requests;

public class ScoreXliffRequest
{
    [DataSource(typeof(ModelsDataHandler))]
    public string Model { get; set; } = string.Empty;

    public FileReference File { get; set; } = default!;

    public FileReference? Glossary { get; set; }
    
    [Display("Prompt", Description = "Add any linguistic criteria for quality evaluation")]
    public string? Prompt { get; set; }

    [Display("Bucket size", Description = "Specify the number of source texts to be translated at once. Default value: 1500. (See our documentation for an explanation)")]
    public int? BucketSize { get; set; }
    
    public float? Threshold { get; set; }

    [StaticDataSource(typeof(ConditionDataSourceHandler))]
    public string? Condition { get; set; }

    [Display("New target state"), StaticDataSource(typeof(XliffStateDataSourceHandler))]
    public string? State { get; set; }

    public int GetBucketSize()
    {
        const int defaultValue = 1500;
        return BucketSize ?? defaultValue;
    }
}