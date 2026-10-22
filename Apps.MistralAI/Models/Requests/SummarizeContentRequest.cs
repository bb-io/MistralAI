using Apps.MistralAI.DataSourceHandlers;
using Apps.MistralAI.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.MistralAI.Models.Requests
{
    public class SummarizeContentRequest
    {
        public FileReference File { get; set; } = new();

        [Display("Model", Description = "This parameter controls which Mistral model answers your request")]
        [DataSource(typeof(ModelsDataHandler))]
        public string Model { get; set; } = string.Empty;

        [Display("Language", Description = "Language of the summary. By default, the language of the content.")]
        [DataSource(typeof(LocaleDataSourceHandler))]
        public string? Language { get; set; }

        [Display("Maximum words", Description = "Maximum number of words of the summary.")]
        public int? MaximumWords { get; set; }

        [Display("Target audience")]
        public string? TargetAudience { get; set; }

        [Display("Additional instructions", Description = "Specify additional instructions for the summary. For example: 'Use bullet points.'")]
        public string? AdditionalInstructions { get; set; }

        public FileReference? Glossary { get; set; }
    }
}
