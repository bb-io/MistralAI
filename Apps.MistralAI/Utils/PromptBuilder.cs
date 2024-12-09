namespace Apps.MistralAI.Utils;

public class PromptBuilder
{
    private const string TranslationPrompt =
        "Translate the following texts from {source_language} language to {target_language} language";

    private const string CustomInstructionPrompt =
        "Process the following texts as per the custom instructions: {prompt}. The source language is {source_language} and the target language is {target_language}. This information might be useful for the custom instructions";

    private const string ProcessXliffSummary =
        "Please provide a translation for each individual text, even if similar texts have been provided more than once. " +
        "{custom_instruction_prompt}. Original texts (in serialized array format): {json}";

    private const string PostEditUserPrompt = 
        "Your input consists of sentences in {source_language} with their translations into {target_language}. " +
        "Review and edit the translated target text as necessary to ensure it is a correct and accurate translation of the source text. " +
        "Fix all grammar and syntax mistakes. " +
        "If you see XML tags in the source, include them in the target text without deleting or modifying them. " +
        "{instructions} Sentences: {json}.";

    public static string BuildUserPrompt(string? prompt, string sourceLanguage, string targetLanguage, string json)
    {
        var instruction = string.IsNullOrEmpty(prompt)
            ? TranslationPrompt.Replace("{source_language}", sourceLanguage)
                .Replace("{target_language}", targetLanguage)
            : CustomInstructionPrompt.Replace("{prompt}", prompt).Replace("{source_language}", sourceLanguage)
                .Replace("{target_language}", targetLanguage);

        return ProcessXliffSummary.Replace("{custom_instruction_prompt}", instruction).Replace("{json}", json);
    }

    public static string BuildPostEditUserPrompt(string? instructions, string sourceLanguage, string targetLanguage,
        string json)
    {
        var result = PostEditUserPrompt
            .Replace("{source_language}", sourceLanguage)
            .Replace("{target_language}", targetLanguage)
            .Replace("{json}", json);

        if (!string.IsNullOrEmpty(instructions))
        {
            result = result.Replace("{instructions}", instructions);
        }
        else
        {
            result = result.Replace("{instructions}", string.Empty);
        }

        return result;
    }

    private const string TranslatorSystemPrompt =
        "You are tasked with localizing the provided text. Consider cultural nuances, idiomatic expressions, and locale-specific references to make the text feel natural in the target language. Ensure the structure of the original text is preserved. Respond with the localized text.";

    private const string CustomInstructionSystemPrompt =
        "You are tasked with processing the provided text according to the custom instructions. Consider the specific requirements outlined in the instructions and adapt the text accordingly. Respond with the processed text.";

    private const string SystemPromptSummary =
        "Each text is treated as an individual item for translation. Even if some entries are identical or similar, they must be processed separately. Please provide the output in JSON format. Json model must be the next: {\"translations\": [{\"translation_id\": \"21\",\"translated_text\":\"Some translated text\",\"quality_score\": 8.5}]}. The output will be used programmatically, so it is important to maintain the same number of elements in the array and return only valid JSON";

    public static string BuildSystemPrompt(bool translator)
    {
        return (translator ? TranslatorSystemPrompt : CustomInstructionSystemPrompt) + SystemPromptSummary;
    }

    public static string GetPostEditPrompt()
    {
        return DefaultSystemPrompt + SystemPromptSummary;
    }
    
    public const string DefaultSystemPrompt =
        "You are a linguistic expert that should process the following texts according to the given instructions. ";

    private const string QualityScorePrompt =
        "Your input is going to be a group of sentences in {source_language} and their translation into {target_language}. " +
        "Please provide the output in JSON format. Json model must be the next: {\"translations\": [{\"translation_id\": \"21\",\"quality_score\": 8.5}]}. The output will be used programmatically, so it is important to maintain the same number of elements in the array and return only valid JSON" +
        "The score number is a score from 1 to 10 assessing the quality of the translation, considering the following criteria: {criteria}. Sentences: {json}.";

    public static string BuildQualityScorePrompt(string sourceLanguage, string targetLanguage, string? criteria,
        string json)
    {
        criteria ??= "accuracy, fluency, consistency, style, grammar and spelling";
        return QualityScorePrompt.Replace("{source_language}", sourceLanguage)
            .Replace("{target_language}", targetLanguage).Replace("{criteria}", criteria).Replace("{json}", json);
    }
}