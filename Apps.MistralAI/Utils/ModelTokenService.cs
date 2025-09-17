namespace Apps.MistralAI.Utils;

public static class ModelTokenService
{
    public static int GetMaxTokensForModel(string? modelName)
    {
        return modelName switch
        {
            string m when m.Contains("mistral-medium") => 128000,
            string m when m.Contains("mistral-large") => 128000,
            string m when m.Contains("ministral-3b") => 128000,
            string m when m.Contains("ministral-8b") => 128000,
            string m when m.Contains("open-mistral-nemo") => 128000,
            string m when m.Contains("mistral-small") => 32000,
            _ => 8000
        };
    }
}
