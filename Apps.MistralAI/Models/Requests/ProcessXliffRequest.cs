using Apps.MistralAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.MistralAI.Models.Requests;

public class ProcessXliffRequest
{
    [DataSource(typeof(ModelsDataHandler))]
    public string Model { get; set; } = string.Empty;

    public FileReference File { get; set; } = default!;

    public FileReference? Glossary { get; set; }
    
    [Display("Prompt", Description = "Specify the instruction to be applied to each source tag within a translation unit. For example, 'Translate text'")]
    public string? Prompt { get; set; }

    [Display("Bucket size", Description = "Specify the number of source texts to be translated at once. Default value: 1500. (See our documentation for an explanation)")]
    public int? BucketSize { get; set; }

    [Display("Add missing trailing tags", Description = "If true, missing trailing tags will be added to the target segment.")]
    public bool? AddMissingTrailingTags { get; set; }
    
    [Display("Filter glossary terms")]
    public bool? FilterGlossary { get; set; }

    public int GetBucketSize()
    {
        const int defaultValue = 1500;
        return BucketSize ?? defaultValue;
    }

    public bool GetFilterGlossary()
    {
        return FilterGlossary ?? true;
    }
}