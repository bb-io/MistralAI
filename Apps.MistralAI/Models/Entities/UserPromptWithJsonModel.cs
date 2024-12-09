namespace Apps.MistralAI.Models.Entities;

public record UserPromptWithJsonModel(string UserPrompt, string Json)
{
    public string UserPrompt { get; set; } = UserPrompt;

    public string Json { get; set; } = Json;
}