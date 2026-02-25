using Apps.MistralAI.DataSourceHandlers;
using Apps.MistralAI.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.MistralAI.Models.Requests
{
    public class TranscriptionRequest
    {
        [Display("File")]
        public required FileReference File { get; set; }
        
        [Display("Model")]
        [DataSource(typeof(AudioModelsDataHandler))]
        public required string Model { get; set; }      
        
        [Display("Source language (ISO 639-1)", Description = "Optional. If not set, Mistral will infer it from the content.")]
        [StaticDataSource(typeof(IsoLanguageDataSourceHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Temperature")]
        [StaticDataSource(typeof(TemperatureDataSourceHandler))]
        public float? Temperature { get; set; }
        
        [Display("Timestamp granularities")]
        [StaticDataSource(typeof(TimestampGranularitiesSourceHandler))]
        public string? TimestampGranularities { get; set; }
    }
}