using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KapwaKuha.Services
{
    public static class GoogleAuthService
    {
        // 1. PASTE YOUR NEW WEB APPLICATION CLIENT ID HERE
        private const string ClientId = "1004459749091-j9b1fnk3ha6tbicc1q8j0qhfad1p1rkt.apps.googleusercontent.com";

        private const string RedirectUri = "http://127.0.0.1:8080/";
        private const string Scope = "openid email profile";

        private static string? _accessToken;
        private static string? _refreshToken;
        private static string? _idToken;
        private static bool _isRunning = false;

        public static Task SignOut()
        {
            _accessToken = null;
            _refreshToken = null;
            _idToken = null;
            return Task.CompletedTask;
        }

        public static bool IsSignedIn => !string.IsNullOrEmpty(_accessToken);

        public static async Task<(string Email, string Name)> GoogleLoginAsync(CancellationToken ct = default)
        {
            if (_isRunning)
                throw new InvalidOperationException("Google sign-in is already in progress.");
            _isRunning = true;

            var (verifier, challenge) = GeneratePkce();
            var state = RandomBase64Url(16);
            var authUrl = BuildAuthUrl(challenge, state);

            var listener = new HttpListener();
            listener.Prefixes.Add(RedirectUri);

            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                _isRunning = false;
                throw new InvalidOperationException("Could not start local server: " + ex.Message, ex);
            }

            try
            {
                // Open cleanly using the default system browser tab
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });

                using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

                var contextTask = listener.GetContextAsync();
                var cancelTask = Task.Delay(Timeout.Infinite, linked.Token)
                                     .ContinueWith(_ => (HttpListenerContext)null!, TaskContinuationOptions.OnlyOnCanceled);

                var winner = await Task.WhenAny(contextTask, cancelTask);
                if (winner != contextTask)
                    throw new OperationCanceledException("Google login timed out or was cancelled.");

                var context = await contextTask;

                var html = "<html><body style='font-family:Segoe UI;text-align:center;margin-top:80px'>" +
                            "<h2 style='color:#4CAF50'>&#10003; Login successful!</h2>" +
                            "<p>You can close this window and return to KapwaKuha.</p></body></html>";
                var bytes = Encoding.UTF8.GetBytes(html);
                var resp = context.Response;
                resp.ContentType = "text/html; charset=utf-8";
                resp.ContentLength64 = bytes.Length;
                resp.OutputStream.Write(bytes, 0, bytes.Length);
                resp.OutputStream.Flush();
                resp.Close();

                listener.Stop();
                listener.Close();

                var query = context.Request.QueryString;
                if (query["error"] != null)
                    throw new InvalidOperationException($"Google error: {query["error"]}");
                if (query["state"] != state)
                    throw new InvalidOperationException("State mismatch.");

                var code = query["code"] ?? throw new InvalidOperationException("No auth code returned.");
                var tokens = await ExchangeCodeAsync(code, verifier);

                _accessToken = tokens.AccessToken;
                _refreshToken = tokens.RefreshToken;
                _idToken = tokens.IdToken;

                var payload = DecodeJwtPayload(tokens.IdToken);
                var email = payload.GetValueOrDefault("email") as string ?? "";
                var name = payload.GetValueOrDefault("name") as string ?? "";

                return (email, name);
            }
            finally
            {
                try { listener.Stop(); } catch { }
                try { listener.Close(); } catch { }
                _isRunning = false;
            }
        }

        private static string BuildAuthUrl(string challenge, string state)
        {
            var sb = new StringBuilder("https://accounts.google.com/o/oauth2/v2/auth?");
            sb.Append("client_id=").Append(Uri.EscapeDataString(ClientId));
            sb.Append("&redirect_uri=").Append(Uri.EscapeDataString(RedirectUri));
            sb.Append("&response_type=code");
            sb.Append("&scope=").Append(Uri.EscapeDataString(Scope));
            sb.Append("&code_challenge=").Append(challenge);
            sb.Append("&code_challenge_method=S256");
            sb.Append("&state=").Append(state);
            sb.Append("&nonce=").Append(RandomBase64Url(16)); // Added nonce
            sb.Append("&prompt=select_account");
            sb.Append("&access_type=offline");
            return sb.ToString();
        }

        private static async Task<TokenResponse> ExchangeCodeAsync(string code, string verifier)
        {
            using var http = new HttpClient();
            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["redirect_uri"] = RedirectUri,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["code_verifier"] = verifier
            });
            var resp = await http.PostAsync("https://oauth2.googleapis.com/token", body);
            var json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Token exchange failed: {json}");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new TokenResponse(
                root.GetProperty("access_token").GetString() ?? "",
                root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() ?? "" : "",
                root.GetProperty("id_token").GetString() ?? ""
            );
        }

        private static Dictionary<string, object?> DecodeJwtPayload(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return new();
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            while (payload.Length % 4 != 0) payload += '=';
            var bytes = Convert.FromBase64String(payload);
            using var doc = JsonDocument.Parse(bytes);
            var result = new Dictionary<string, object?>();
            foreach (var prop in doc.RootElement.EnumerateObject())
                result[prop.Name] = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : prop.Value.GetRawText();
            return result;
        }

        private static (string Verifier, string Challenge) GeneratePkce()
        {
            var verifier = RandomBase64Url(32);
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
            var challenge = Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return (verifier, challenge);
        }

        private static string RandomBase64Url(int byteLength)
        {
            var bytes = RandomNumberGenerator.GetBytes(byteLength);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private record TokenResponse(string AccessToken, string RefreshToken, string IdToken);
    }
}