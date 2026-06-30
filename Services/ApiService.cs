/**
 * HTTP client wrapper for the AtRiskRegister PHP REST API.
 *
 * Attaches Bearer JWT to every request. On HTTP 401 attempts one silent
 * refresh using the stored refresh token; re-throws if that also fails.
 * Tokens are held in memory only (no persistence between app launches).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AtRiskTracker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AtRiskTracker.Services
{
    public class ApiService
    {
        // Singleton shared across the application
        public static readonly ApiService Instance = new ApiService();

        private readonly HttpClient _http;
        private string _accessToken;
        private string _refreshToken;
        public UserDto CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser != null;

        private ApiService()
        {
            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private string BaseUrl => AppConfig.Settings.ApiBaseUrl.TrimEnd('/');

        // ----------------------------------------------------------------
        // Auth
        // ----------------------------------------------------------------

        /// <summary>Login with email and password. Stores tokens in memory.</summary>
        public async Task LoginAsync(string email, string password)
        {
            var payload = new { email, password };
            var response = await PostRawAsync("/auth/login.php", payload);
            var result = JsonConvert.DeserializeObject<LoginResponse>(response);
            _accessToken  = result.Data.AccessToken;
            _refreshToken = result.Data.RefreshToken;
            CurrentUser   = result.Data.User;
        }

        /// <summary>Clear stored tokens and current user.</summary>
        public void Logout()
        {
            _accessToken  = null;
            _refreshToken = null;
            CurrentUser   = null;
        }

        /// <summary>Change the current user's password.</summary>
        public async Task ChangePasswordAsync(string currentPassword, string newPassword)
        {
            await PostAsync<object>("/auth/change-password.php", new
            {
                current_password = currentPassword,
                new_password     = newPassword,
            });
        }

        // ----------------------------------------------------------------
        // Public typed request helpers
        // ----------------------------------------------------------------

        /// <summary>HTTP GET, deserialises response as T.</summary>
        public async Task<T> GetAsync<T>(string path, string query = null)
        {
            string url = BaseUrl + path + (query != null ? "?" + query : "");
            var req = BuildRequest(HttpMethod.Get, url);
            string body = await SendWithRetryAsync(req, url, HttpMethod.Get);
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>HTTP POST, deserialises response as T.</summary>
        public async Task<T> PostAsync<T>(string path, object data)
        {
            string body = await PostRawAsync(path, data);
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>HTTP PUT, deserialises response as T.</summary>
        public async Task<T> PutAsync<T>(string path, object data)
        {
            string url  = BaseUrl + path;
            string json = JsonConvert.SerializeObject(data);
            var req = BuildRequest(HttpMethod.Put, url, json);
            string body = await SendWithRetryAsync(req, url, HttpMethod.Put, json);
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>HTTP DELETE with no body.</summary>
        public async Task DeleteAsync(string path)
        {
            string url = BaseUrl + path;
            var req = BuildRequest(HttpMethod.Delete, url);
            await SendWithRetryAsync(req, url, HttpMethod.Delete);
        }

        /// <summary>HTTP DELETE with a JSON body (required by endpoints that read id from php://input).</summary>
        public async Task DeleteAsync(string path, object data)
        {
            string url  = BaseUrl + path;
            string json = JsonConvert.SerializeObject(data);
            var req = BuildRequest(HttpMethod.Delete, url, json);
            await SendWithRetryAsync(req, url, HttpMethod.Delete, json);
        }

        // ----------------------------------------------------------------
        // Internals
        // ----------------------------------------------------------------

        private async Task<string> PostRawAsync(string path, object data)
        {
            string url  = BaseUrl + path;
            string json = JsonConvert.SerializeObject(data);
            var req = BuildRequest(HttpMethod.Post, url, json);
            return await SendWithRetryAsync(req, url, HttpMethod.Post, json);
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string url, string jsonBody = null)
        {
            var req = new HttpRequestMessage(method, url);
            if (!string.IsNullOrEmpty(_accessToken))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            if (jsonBody != null)
                req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            return req;
        }

        /// <summary>
        /// Sends the request. On 401, attempts a token refresh once and retries.
        /// </summary>
        private async Task<string> SendWithRetryAsync(
            HttpRequestMessage req, string url, HttpMethod method, string jsonBody = null)
        {
            var response = await _http.SendAsync(req);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                && _refreshToken != null)
            {
                // Try to refresh
                try
                {
                    await RefreshTokenAsync();
                    // Rebuild request with new token
                    var retry = BuildRequest(method, url, jsonBody);
                    var retryResponse = await _http.SendAsync(retry);
                    await EnsureSuccessAsync(retryResponse);
                    return await retryResponse.Content.ReadAsStringAsync();
                }
                catch
                {
                    Logout();
                    throw new UnauthorizedAccessException("Session expired. Please log in again.");
                }
            }

            await EnsureSuccessAsync(response);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task RefreshTokenAsync()
        {
            string url  = BaseUrl + "/auth/refresh.php";
            string json = JsonConvert.SerializeObject(new { refreshToken = _refreshToken });
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _http.SendAsync(req);
            await EnsureSuccessAsync(response);
            string body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RefreshResponse>(body);
            _accessToken = result.Data.AccessToken;
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string body = string.Empty;
            try { body = await response.Content.ReadAsStringAsync(); } catch { }

            string message = response.ReasonPhrase;
            try
            {
                var err = JObject.Parse(body);
                string errMsg = err["error"]?.ToString() ?? err["message"]?.ToString();
                if (!string.IsNullOrEmpty(errMsg)) message = errMsg;
            }
            catch { }

            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {message}");
        }
    }
}
