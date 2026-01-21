using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Apps.MistralAI.Utils
{
    public static class ReviewResponseDeserializer
    {
        public static float DeserializeScore(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return 0;

            try
            {
                var token = JToken.Parse(content);

                if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                    return Clamp(Convert.ToSingle(((JValue)token).Value, CultureInfo.InvariantCulture));

                if (token.Type == JTokenType.Object)
                {
                    var obj = (JObject)token;

                    var scoreToken =
                        obj["score"] ??
                        obj["qualityScore"] ??
                        obj["quality_score"] ??
                        obj["result"]?["score"];

                    if (scoreToken != null && (scoreToken.Type == JTokenType.Float || scoreToken.Type == JTokenType.Integer))
                        return Clamp(Convert.ToSingle(((JValue)scoreToken).Value, CultureInfo.InvariantCulture));

                    if (scoreToken != null && scoreToken.Type == JTokenType.String &&
                        float.TryParse(scoreToken.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                        return Clamp(parsed);
                }
            }
            catch
            {
            }

            if (TryExtractFloat(content, out var val))
                return Clamp(val);

            return 0;
        }

        private static bool TryExtractFloat(string text, out float value)
        {
            value = 0;
            var chars = text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray();
            var cleaned = new string(chars).Replace(',', '.');
            return float.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static float Clamp(float v) => Math.Max(0, Math.Min(1, v));
    }
}
