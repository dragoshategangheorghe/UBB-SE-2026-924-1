using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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

            string truncated = json.Length > 2000 ? json[..2000] + "…" : json;
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

            Debug.WriteLine($"EnsureSuccessAsync FAILED: {endpoint}");
            Debug.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"Body: {(body.Length > 500 ? body[..500] : body)}");

            if (body.Length > 2000)
            {
                body = body[..2000] + "…";
            }

            throw new HttpRequestException(
                $"Request to '{endpoint}' failed: {(int)response.StatusCode} {response.StatusCode}. Content-Type: {contentType}. Body: {body}",
                null,
                response.StatusCode);
        }

        private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
        {
            Debug.WriteLine($"ReadJsonAsync called with T={typeof(T).Name}");
            Debug.WriteLine($"Content-Type: {response.Content.Headers.ContentType?.MediaType}");

            if (typeof(T) == typeof(string))
            {
                var text = await response.Content.ReadAsStringAsync();
                if (text.StartsWith("\"") && text.EndsWith("\""))
                {
                    text = text[1..^1];
                }
                return (T)(object)text;
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            catch (Exception jsonEx)
            {
                string body = await response.Content.ReadAsStringAsync();
                if (body.Length > 2000)
                {
                    body = body[..2000] + "…";
                }

                throw new InvalidOperationException(
                    $"Failed to parse JSON response as {typeof(T).Name}. Raw body: {body}",
                    jsonEx);
            }
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
