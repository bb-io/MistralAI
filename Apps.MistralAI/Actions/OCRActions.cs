using Apps.MistralAI.Api;
using Apps.MistralAI.Constants;
using Apps.MistralAI.Invocables;
using Apps.MistralAI.Models.Requests;
using Apps.MistralAI.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.MistralAI.Actions
{
    [ActionList]
    public class OCRActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : AppInvocable(invocationContext)
    {

        [Action("Extract text from image or PDF",
            Description = "Extracts text from PDF or image using Mistral AI OCR service and returns the raw JSON response.")]
        public async Task<ExtractTextFromImageResult> ExtractTextFromImage(
            [ActionParameter] FileInput file)
        {

            using var fileStream = await fileManagementClient.DownloadAsync(file.File);
            var fileBytes = await fileStream.GetByteData();
            var base64Data = Convert.ToBase64String(fileBytes);

            var isPdf = file.File.ContentType?.Contains("pdf", StringComparison.OrdinalIgnoreCase) ?? false;

            string mimeType = isPdf
                ? "application/pdf"
                : string.IsNullOrEmpty(file.File.ContentType) ? "image/jpeg" : file.File.ContentType;

            var documentObj = new
            {
                type = isPdf ? "document_url" : "image_url",
                document_url = isPdf ? $"data:{mimeType};base64,{base64Data}" : null,
                image_url = !isPdf ? $"data:{mimeType};base64,{base64Data}" : null
            };

            var requestBody = new
            {
                model = "mistral-ocr-latest",
                document = documentObj
            };

            var response = await Client.ExecuteWithJson<object>(
                ApiEndpoints.Document,
                Method.Post,
                requestBody
            );

            var rawJson = JsonConvert.SerializeObject(response);

            // "JSON data file"
            // Content file (.md) -> get all the different markdown segments and 

            return new ExtractTextFromImageResult
            {
                RawJson = rawJson
            };
        }
    }
}

