using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Entities;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.MistralAI.Actions
{
    [ActionList("Audio")]
    public class AudioActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AppInvocable(invocationContext)
    {

        [Action("Create transcription", Description = "Generates a transcription given an audio or video file (mp3, mp4, mpeg, mpga, m4a, wav, or webm).")]
        public async Task<TranscriptionResponse> CreateTranscription([ActionParameter] TranscriptionRequest input)
        {
            var request = new RestRequest(ApiConstants.BaseUrl + ApiEndpoints.Audio + ApiEndpoints.Transcriptions, Method.Post);

            var stream = await fileManagementClient.DownloadAsync(input.File);
            var fileBytes = await stream.GetByteData();

            request.AddFile("file", fileBytes, input.File.Name);
            request.AddParameter("model", input.Model);
            request.AddParameter("temperature", input.Temperature ?? 0);
            request.AddParameter("language", input.SourceLanguage);
            request.AddParameter("timestamp_granularities", input.TimestampGranularities);

            if (input.TimestampGranularities == "segment")
            { 
                request.AddParameter("diarize", true);
            }

            var response = await Client.ExecuteRequest(request);
            var deserializedResponse = JsonConvert.DeserializeObject<TranscriptionEntity>(response.Content)
                ?? throw new PluginApplicationException($"Transcription response could not be parsed. Raw response: {response.Content}");

            var segments = deserializedResponse.Segments.Select(x => new SegmentResponse(x)).ToList();

            return new()
            {
                Language = deserializedResponse.Language,
                Transcription = deserializedResponse.Text,
                Segments = JsonConvert.SerializeObject(segments),
                Usage = deserializedResponse.Usage
            };
        }
    }
}