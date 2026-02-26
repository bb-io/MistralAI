using Apps.MistralAI.Models.Responses;
using Newtonsoft.Json;

namespace Apps.MistralAI.Models.Entities;

public record TranscriptionEntity(
    string Language,
    SegmentDto[] Segments,
    string Text,
    UsageResponse Usage
);

public record SegmentDto(
    double Start, 
    double End, 
    string Text,
    double? Score,
    [JsonProperty("speaker_id")]
    string? SpeakerId
);