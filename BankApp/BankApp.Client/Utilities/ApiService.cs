using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BankApp.Client.Utilities
{
    public class ApiService
    {
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
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, data);
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP ERROR: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(endpoint, data);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<DownloadResponse?> PostDownloadAsync<TRequest>(string endpoint, TRequest data)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, data);
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
