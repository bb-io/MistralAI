using Blackbird.Filters.Transformations;
using System.Text;

namespace Apps.MistralAI.Utils
{
    public static class EditPromptBuilder
    {
        public static string BuildEditSystemPrompt()
        {
            return
                "You are a translation editor. " +
                "You will receive source and existing target translations. " +
                "Your job: improve the TARGET text while preserving meaning, formatting, tags/markup, placeholders and structure. " +
                "Return ONLY JSON object in format: {\"translations\":[{\"translationId\":\"1\",\"translatedText\":\"...\"}]} " +
                "No explanations.";
        }

        public static string BuildEditUserPrompt(string? additionalInstructions, Transformation content, string batchJson)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Edit the following translations (JSON array). Each item has Id, SourceText, TargetText:");
            sb.AppendLine(batchJson);
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(content.SourceLanguage))
                sb.AppendLine($"Source language: {content.SourceLanguage}");
            if (!string.IsNullOrWhiteSpace(content.TargetLanguage))
                sb.AppendLine($"Target language: {content.TargetLanguage}");

            if (!string.IsNullOrWhiteSpace(additionalInstructions))
            {
                sb.AppendLine();
                sb.AppendLine("Additional instructions:");
                sb.AppendLine(additionalInstructions);
            }

            sb.AppendLine();
            sb.AppendLine("Return JSON only in the specified format.");

            return sb.ToString();
        }
    }
}
