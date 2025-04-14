using System.Text.Json.Serialization;

namespace ChatBotGemini.Models.Gemini
{
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }

        // Có thể có thông tin về prompt feedback nếu bị chặn
        // [JsonPropertyName("promptFeedback")]
        // public PromptFeedback PromptFeedback { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }

        // Các trường khác như finishReason, index, safetyRatings có thể thêm nếu cần
    }

    // Bạn có thể cần định nghĩa lại class Content và Part ở đây
    // nếu cấu trúc response khác request, nhưng thường là giống nhau.
    // Nếu Gemini API trả về cấu trúc khác, hãy điều chỉnh class này.

    // --- Các class phụ nếu cần ---
    // public class PromptFeedback { ... }
    // public class SafetyRating { ... }
}
