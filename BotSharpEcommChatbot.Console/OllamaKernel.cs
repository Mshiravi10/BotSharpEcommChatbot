using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotSharpEcommChatbot.Console
{
    public class OllamaKernel
    {
        private readonly HttpClient _http;

        public OllamaKernel()
        {
            _http = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
        }

        public async Task<string> AskAsync(string prompt)
        {
            var request = new
            {
                model = "deepseek-r1:8b",
                prompt = prompt,
                stream = false
            };

            var response = await _http.PostAsJsonAsync("/api/generate", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ollama Error: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("response").GetString();
        }
    }
}
