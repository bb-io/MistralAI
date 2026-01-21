using Apps.MistralAI.DataSourceHandlers;
using Apps.MistralAI.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.MistralAI.Models.Requests
{
    public class TranslateTextRequest : ITranslateTextInput
    {
        [Display("Text")]
        public string Text { get; set; } = string.Empty;

        [Display("Target language")]
        [DataSource(typeof(LocaleDataSourceHandler))]
        public string TargetLanguage { get; set; } = string.Empty;

        [Display("Source language", Description = "Optional. If not set, Mistral will infer it from the content.")]
        [DataSource(typeof(LocaleDataSourceHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Model")]
        [DataSource(typeof(ModelsDataHandler))]
        public string Model { get; set; }
    }
}
