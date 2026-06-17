using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KapwaKuha.Services
{
    public static class SmsService
    {
        // Replace with your actual Semaphore API key
        private const string ApiKey = "YOUR_SEMAPHORE_API_KEY_HERE";
        private const string ApiUrl = "https://api.semaphore.co/api/v4/messages";

        public static async Task SendAsync(string phoneNumber, string message)
        {
            using var client = new HttpClient();

            var payload = new Dictionary<string, string>
            {
                { "apikey",  ApiKey      },
                { "number",  phoneNumber },
                { "message", message     }
            };

            var content = new FormUrlEncodedContent(payload);

            try
            {
                var response = await client.PostAsync(ApiUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SmsService] Failed: {ex.Message}");
            }
        }
    }
}