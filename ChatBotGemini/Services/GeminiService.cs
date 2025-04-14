using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatBotGemini.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        public  string apiKey = "AIzaSyDKnla8sJUXCg9gpDFXW84-7TN7YS3ylnc";
        public  string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=AIzaSyAHE6r_TifCeDRJiKYR_VZkLN3WuwBcbyA";

        public GeminiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SendMessage(string userMessage)
        {
            var requestBody = new
            {
                contents = new[]
                {
                new {
                    parts = new[]
                    {
                        new { text = userMessage }
                    }
                }
            }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseContent);
            var reply = doc.RootElement
                           .GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0]
                           .GetProperty("text")
                           .GetString();

            return reply;
        }
    }
}
