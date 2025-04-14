using System.Text.Json.Serialization;

namespace ChatBotGemini.Models.Gemini
{
    public class GeminiRequest
    {
        // Danh sách các tin nhắn trong cuộc hội thoại
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; } = new List<Content>();

        // Có thể thêm các cài đặt an toàn, cấu hình generate khác ở đây nếu cần
        // public GenerationConfig GenerationConfig { get; set; }
        // public List<SafetySetting> SafetySettings { get; set; }
    }

    public class Content
    {
        // Vai trò: "user" hoặc "model"
        [JsonPropertyName("role")]
        public string Role { get; set; }

        // Phần nội dung tin nhắn
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new List<Part>();
    }

    public class Part
    {
        // Nội dung text của tin nhắn
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
