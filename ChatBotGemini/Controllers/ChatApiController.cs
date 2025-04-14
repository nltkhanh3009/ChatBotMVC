using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // Cần để đọc appsettings
using ChatBotGemini.Models.Gemini; // Namespace chứa model Gemini
using System.Collections.Generic; // Cho List
using Microsoft.AspNetCore.Http; // Cho Session

namespace ChatBotGemini.Controllers
{
    [Route("api/chat")] // Định nghĩa route cho API
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl;

        public ChatApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _geminiApiKey = _configuration["Gemini:ApiKey"]; // Đọc API Key từ config

            if (string.IsNullOrEmpty(_geminiApiKey))
            {
                // Nên ghi log lỗi ở đây
                throw new InvalidOperationException("Gemini API Key chưa được cấu hình trong appsettings.");
            }

            // Model 'gemini-pro' là phổ biến cho chat
            _geminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}";
        }

        // --- Helper để quản lý lịch sử chat trong Session ---this

        private List<Content> GetChatHistory()
        {
            var historyJson = HttpContext.Session.GetString("ChatHistory");
            if (string.IsNullOrEmpty(historyJson))
            {
                return new List<Content>(); // Trả về list rỗng nếu chưa có
            }
            try
            {
                return JsonSerializer.Deserialize<List<Content>>(historyJson) ?? new List<Content>();
            }
            catch (JsonException) // Xử lý nếu JSON trong session bị lỗi
            {
                return new List<Content>();
            }
        }

        private void SaveChatHistory(List<Content> history)
        {
            var historyJson = JsonSerializer.Serialize(history);
            HttpContext.Session.SetString("ChatHistory", historyJson);
        }
        // ----------------------------------------------------


        [HttpPost] // Chỉ chấp nhận phương thức POST
        public async Task<IActionResult> PostMessage([FromBody] ChatInput input)
        {
            if (string.IsNullOrWhiteSpace(input?.Message))
            {
                return BadRequest(new { error = "Tin nhắn không được để trống." });
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // 1. Lấy lịch sử chat từ Session
                var chatHistory = GetChatHistory();

                // 2. Thêm tin nhắn mới của người dùng vào lịch sử
                chatHistory.Add(new Content { Role = "user", Parts = new List<Part> { new Part { Text = input.Message } } });

                // 3. Chuẩn bị request body cho Gemini
                var requestPayload = new GeminiRequest
                {
                    Contents = chatHistory // Gửi toàn bộ lịch sử
                };

                var jsonPayload = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
                {
                    // Bỏ qua các thuộc tính null để JSON gọn hơn (tùy chọn)
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // 4. Gửi yêu cầu POST đến Gemini API
                var response = await httpClient.PostAsync(_geminiApiUrl, httpContent);

                // 5. Xử lý Response
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseBody);

                    // Lấy câu trả lời đầu tiên (thường chỉ có 1 candidate tốt)
                    var botMessageContent = geminiResponse?.Candidates?.FirstOrDefault()?.Content;
                    if (botMessageContent != null && botMessageContent.Parts != null && botMessageContent.Parts.Any())
                    {
                        var botReplyText = botMessageContent.Parts.First().Text;

                        // 6. Thêm câu trả lời của bot vào lịch sử
                        // Quan trọng: Đảm bảo Role là "model"
                        botMessageContent.Role = "model"; // API có thể không trả về role, ta cần gán
                        chatHistory.Add(botMessageContent);

                        // 7. Lưu lại lịch sử vào Session
                        SaveChatHistory(chatHistory);

                        // 8. Trả về câu trả lời cho frontend
                        return Ok(new { reply = botReplyText });
                    }
                    else
                    {
                        // Trường hợp API trả về thành công nhưng không có nội dung mong đợi
                        SaveChatHistory(chatHistory); // Vẫn lưu lịch sử user hỏi
                        return Ok(new { reply = "Xin lỗi, tôi không thể tạo phản hồi lúc này." });
                    }
                }
                else
                {
                    // Xử lý lỗi từ Gemini API
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Lỗi từ Gemini API: {response.StatusCode} - {errorBody}"); // Log lỗi ra console server
                    // Không lưu lịch sử khi có lỗi API
                    return StatusCode((int)response.StatusCode, new { error = "Có lỗi xảy ra khi giao tiếp với AI.", details = errorBody });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi nội bộ server: {ex.Message}"); // Log lỗi
                // Cân nhắc không nên trả về chi tiết lỗi cho client trong production
                return StatusCode(500, new { error = "Lỗi máy chủ nội bộ." });
            }
        }
    }

    // Class đơn giản để nhận input từ frontend
    public class ChatInput
    {
        public string Message { get; set; }
    }
}
