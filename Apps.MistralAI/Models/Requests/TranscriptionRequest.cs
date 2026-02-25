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
        [DataSource(typeof(ModelsDataHandler))]
        public required string Model { get; set; }      
        
        [Display("Source language", Description = "Optional. If not set, Mistral will infer it from the content.")]
        [DataSource(typeof(LocaleDataSourceHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Temperature")]
        [StaticDataSource(typeof(TemperatureDataSourceHandler))]
        public float? Temperature { get; set; }
        
        [Display("Timestamp granularities")]
        [StaticDataSource(typeof(TimestampGranularitiesSourceHandler))]
        public string? TimestampGranularities { get; set; }
    }
}