/**
 * Authentication DTOs returned by /auth/login.php and /auth/refresh.php.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using Newtonsoft.Json;

namespace AtRiskTracker.Models
{
    public class UserDto
    {
        [JsonProperty("id")]   public int    Id       { get; set; }
        [JsonProperty("email")] public string Email   { get; set; }
        [JsonProperty("fullname")] public string Fullname { get; set; }
        [JsonProperty("role")]  public string Role    { get; set; }
    }

    public class LoginResponseData
    {
        [JsonProperty("accessToken")]  public string AccessToken  { get; set; }
        [JsonProperty("refreshToken")] public string RefreshToken { get; set; }
        [JsonProperty("user")]         public UserDto User        { get; set; }
    }

    public class LoginResponse
    {
        [JsonProperty("data")] public LoginResponseData Data { get; set; }
    }

    public class RefreshResponseData
    {
        [JsonProperty("accessToken")] public string AccessToken { get; set; }
    }

    public class RefreshResponse
    {
        [JsonProperty("data")] public RefreshResponseData Data { get; set; }
    }
}
