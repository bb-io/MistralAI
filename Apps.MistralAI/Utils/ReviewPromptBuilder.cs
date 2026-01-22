using System.Text;

namespace Apps.MistralAI.Utils
{
    public static class ReviewPromptBuilder
    {
        public static string BuildReviewSystemPrompt()
        {
            return
                "You are a translation quality reviewer. " +
                "Given source and target text, output ONLY a JSON object with numeric field 'score' from 0 to 1. " +
                "Score meaning: 1 = perfect, 0 = completely wrong. " +
                "Consider adequacy, fluency, terminology consistency, and formatting/markup preservation.";
        }

        public static string BuildReviewUserPrompt(string? additionalInstructions, string? sourceLang, string? targetLang, string json)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Evaluate translation quality for this item:");
            sb.AppendLine(json);

            if (!string.IsNullOrWhiteSpace(sourceLang))
                sb.AppendLine($"Source language: {sourceLang}");
            if (!string.IsNullOrWhiteSpace(targetLang))
                sb.AppendLine($"Target language: {targetLang}");

            if (!string.IsNullOrWhiteSpace(additionalInstructions))
            {
                sb.AppendLine();
                sb.AppendLine("Additional instructions:");
                sb.AppendLine(additionalInstructions);
            }

            sb.AppendLine();
            sb.AppendLine("Return JSON ONLY, example: {\"score\": 0.82}");

            return sb.ToString();
        }
    }
}
