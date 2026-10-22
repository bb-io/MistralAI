using Apps.MistralAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.MistralAI.Models.Requests
{
    public class ShortenTextRequest
    {
        [Display("Text")]
        public string Text { get; set; } = string.Empty;

        [Display("Maximum length", Description = "Maximum number of characters of the shortened text.")]
        public int MaximumLength { get; set; }

        [Display("Model", Description = "This parameter controls which Mistral model answers your request")]
        [DataSource(typeof(ModelsDataHandler))]
        public string Model { get; set; } = string.Empty;

        [Display("Retries", Description = "Number of extra attempts when the text is still too long. Default value: 3")]
        public int? Retries { get; set; }

        [Display("Additional instructions", Description = "Specify additional instructions to apply when shortening.")]
        public string? AdditionalInstructions { get; set; }

        public FileReference? Glossary { get; set; }
    }
}
