using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KapwaKuha.Services
{
    public static class SmsService
    {
        // Replace with your actual Semaphore API key
        private const string ApiKey = "2f0ad730c95c8df0bab61712144da2ea";
        private const string ApiUrl = "https://api.semaphore.co/api/v4/messages";

        public static async Task SendAsync(string phoneNumber, string message)
        {

            if (phoneNumber.StartsWith("0"))
                phoneNumber = "63" + phoneNumber.Substring(1);

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

                // If Semaphore returns an error (like 400 Bad Request or 401 Unauthorized)
                if (!response.IsSuccessStatusCode)
                {
                    string apiErrorResponse = await response.Content.ReadAsStringAsync();
                    System.Windows.MessageBox.Show($"[Semaphore API Error]\nStatus: {response.StatusCode}\nDetails: {apiErrorResponse}", "SMS Failure");
                }

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                System.Windows.MessageBox.Show($"[SmsService Network Exception]\n{ex.Message}", "SMS Critical Error");
            }
        }
    }
}