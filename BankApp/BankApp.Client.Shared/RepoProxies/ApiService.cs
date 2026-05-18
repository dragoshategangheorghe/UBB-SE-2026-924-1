using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BankApp.Client.RepoProxies
{
    public class ApiService
    {
        private static readonly JsonSerializerOptions JsonWriteOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        private readonly HttpClient _httpClient;
        private string? _token;
        private int? _currentUserId;

        private const int JsonTruncateLength = 2000;

        public ApiService(string baseUrl = "http://localhost:5024")
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void SetCurrentUserId(int userId)
        {
            _currentUserId = userId;
        }

        public int? GetCurrentUserId()
        {
            return _currentUserId;
        }

        public void ClearToken()
        {
            _token = null;
            _currentUserId = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public string? GetToken()
        {
            return _token;
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonWriteOptions);
                await EnsureSuccessAsync(response, endpoint);
                return await ReadJsonAsync<TResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP ERROR: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
                throw;
            }
        }

        /// <summary>
        /// For endpoints that return HTTP 400 with a typed JSON body on validation/business failure (e.g. auth login/register).
        /// </summary>
        public async Task<TResponse?> PostAllowBadRequestAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonWriteOptions);
            string json = await response.Content.ReadAsStringAsync();
            TResponse? parsed = DeserializeWebJson<TResponse>(json);

            if (response.IsSuccessStatusCode)
            {
                return parsed;
            }

            if (response.StatusCode == HttpStatusCode.BadRequest && parsed != null)
            {
                return parsed;
            }

            string truncated = json.Length > JsonTruncateLength ? json[..JsonTruncateLength] + "..." : json;
            throw new HttpRequestException(
                $"Request to '{endpoint}' failed: {(int)response.StatusCode} {response.StatusCode}. Body: {truncated}",
                null,
                response.StatusCode);
        }

        private static T? DeserializeWebJson<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(
                    json,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            catch (JsonException)
            {
                return default;
            }
        }

        public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            await EnsureSuccessAsync(response, endpoint);
            return await ReadJsonAsync<TResponse>(response);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(endpoint, data, JsonWriteOptions);
            await EnsureSuccessAsync(response, endpoint);
            return await ReadJsonAsync<TResponse>(response);
        }

        public async Task<DownloadResponse?> PostDownloadAsync<TRequest>(string endpoint, TRequest data)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonWriteOptions);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await CreateDownloadResponseAsync(response);
        }

        public async Task<DownloadResponse?> GetDownloadAsync(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await CreateDownloadResponseAsync(response);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, string endpoint)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string contentType = response.Content.Headers.ContentType?.MediaType ?? "(none)";
            string body = await response.Content.ReadAsStringAsync();
            if (body.Length > JsonTruncateLength)
            {
                body = body[..JsonTruncateLength] + "â€¦";
            }

            throw new HttpRequestException(
                $"Request to '{endpoint}' failed: {(int)response.StatusCode} {response.StatusCode}. Content-Type: {contentType}. Body: {body}",
                null,
                response.StatusCode);
        }

        private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
        {
            string body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return default;
            }

            if (typeof(T) == typeof(string))
            {
                return (T?)(object)ReadStringBody(body);
            }

            try
            {
                return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            catch (Exception jsonEx)
            {
                if (body.Length > JsonTruncateLength)
                {
                    body = body[..JsonTruncateLength] + "â€¦";
                }

                throw new InvalidOperationException(
                    $"Failed to parse JSON response as {typeof(T).Name}. Raw body: {body}",
                    jsonEx);
            }
        }

        private static string ReadStringBody(string body)
        {
            if (body.Length >= 2 && body[0] == '"' && body[^1] == '"')
            {
                try
                {
                    return JsonSerializer.Deserialize<string>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? string.Empty;
                }
                catch (JsonException)
                {
                    // Fall back to the raw response body if the server returned plain text.
                }
            }

            return body;
        }

        private static async Task<DownloadResponse> CreateDownloadResponseAsync(HttpResponseMessage response)
        {
            string? fileName = response.Content.Headers.ContentDisposition?.FileNameStar ??
                               response.Content.Headers.ContentDisposition?.FileName;

            return new DownloadResponse
            {
                Content = await response.Content.ReadAsByteArrayAsync(),
                ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
                FileName = (fileName ?? "download.bin").Trim('"')
            };
        }
    }
}
