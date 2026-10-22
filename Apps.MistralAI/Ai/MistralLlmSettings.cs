using System.Globalization;

namespace Apps.MistralAI.Ai;

public sealed record MistralLlmSettings(double? Temperature = null, double? TopP = null, int? MaxTokens = null)
{
    public static MistralLlmSettings Parse(string? temperature, string? topP, int? maxTokens) =>
        new(ParseDouble(temperature), ParseDouble(topP), maxTokens);

    private static double? ParseDouble(string? value) =>
        double.TryParse(value?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;
}
