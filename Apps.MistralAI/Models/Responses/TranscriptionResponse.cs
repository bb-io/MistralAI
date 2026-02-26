using Apps.MistralAI.Models.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.MistralAI.Models.Responses
{
    public class TranscriptionResponse
    {
        [Display("Language")]
        public string Language { get; set; } = string.Empty;

        [Display("Transcription")]
        public string Transcription { get; set; } = string.Empty;

        [Display("Segments (serialized)")]
        public string Segments { get; set; } = string.Empty;

        [Display("Usage")]
        public UsageResponse? Usage { get; set; }
    }

    public record SegmentResponse(SegmentDto Dto)
    {
        [Display("Speaker ID")]
        public string? Id { get; set; }

        public string Text { get; set; } = Dto.Text;

        public double Start { get; set; } = Dto.Start;

        public double End { get; set; } = Dto.End;
    }
}