using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class AudioTests : TestBase
    {
        [TestMethod]
        public async Task CreateTranscription_IsSuccess()
        {
            var action = new Apps.MistralAI.Actions.AudioActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.TranscriptionRequest
            {
                Model = "voxtral-mini-latest",
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "dialogue.mp3"
                },
            };
            var response = await action.CreateTranscription(request);
            Console.WriteLine($"Transcription: {response.Transcription}");
            Console.WriteLine($"Segments: {response.Segments}");
            Assert.IsNotNull(response);      
        }

        [TestMethod]
        public async Task CreateTranscription_WithWordTimestampGranularities_IsSuccess()
        {
            var action = new Apps.MistralAI.Actions.AudioActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.TranscriptionRequest
            {
                Model = "voxtral-mini-latest",
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "dialogue.mp3"
                },
                TimestampGranularities = "word"
            };
            var response = await action.CreateTranscription(request);
            Console.WriteLine($"Transcription: {response.Transcription}");
            Console.WriteLine($"Segments: {response.Segments}");
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task CreateTranscription_WithSegmentTimestampGranularities_IsSuccess()
        {
            var action = new Apps.MistralAI.Actions.AudioActions(InvocationContext, FileManager);
            var request = new Apps.MistralAI.Models.Requests.TranscriptionRequest
            {
                Model = "voxtral-mini-latest",
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "dialogue.mp3"
                },
                TimestampGranularities = "segment"
            };
            var response = await action.CreateTranscription(request);
            Console.WriteLine($"Transcription: {response.Transcription}");
            Console.WriteLine($"Segments: {response.Segments}");
            Assert.IsNotNull(response);
        }
    }
}