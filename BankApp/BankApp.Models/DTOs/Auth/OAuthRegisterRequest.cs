public class OAuthRegisterRequest
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderToken { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}