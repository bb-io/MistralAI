namespace Apps.MistralAI.Models.Responses;

public class GetModelsResponse
{
    public string Object { get; set; }
    
    public List<GetModelResponse> Data { get; set; }
}