using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChamadosAPI
{
    public class BaseAPI : IDisposable, IAsyncDisposable
    {
        private const string BASE_URL = "http://localhost:8000/apis/";
        protected static readonly HttpClient _httpClient = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private bool _disposed;

        public BaseAPI(string? token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private static string BuildUrl(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            var uriBuilder = new UriBuilder(new Uri(new Uri(BASE_URL), endpoint));

            if (queryParams != null && queryParams.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                uriBuilder.Query = query;
            }

            return uriBuilder.ToString();
        }

        public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            var url = BuildUrl(endpoint, queryParams);
            var response = await _httpClient.GetAsync(url);
            return await HandleResponse<T>(response);
        }

        public async Task<T> PostAsync<T>(string endpoint, object? data = null)
        {
            var url = BuildUrl(endpoint);
            var content = data != null
                ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                : null;

            var response = await _httpClient.PostAsync(url, content);
            return await HandleResponse<T>(response);
        }

        public async Task<T> DeleteAsync<T>(string endpoint)
        {
            var url = BuildUrl(endpoint);
            var response = await _httpClient.DeleteAsync(url);
            return await HandleResponse<T>(response);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return default!;
                }

                try
                {
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions) ?? default!;
                }
                catch (JsonException jsonEx)
                {
                    throw new InvalidOperationException($"Erro ao desserializar a resposta para {typeof(T).Name}: {jsonEx.Message}", jsonEx);
                }
            }
            else
            {
                Dictionary<string, string>? errorDict = null;
                try
                {
                    errorDict = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, _jsonOptions);
                }
                catch (JsonException)
                {
                    // Se não for um JSON válido, ignora e mantém a resposta como string
                }

                if (errorDict != null && errorDict.Count > 0)
                {
                    var firstError = errorDict.First();
                    throw new HttpRequestException($"Erro {response.StatusCode}: {firstError.Key} - {firstError.Value}");
                }

                throw new HttpRequestException($"Erro HTTP {response.StatusCode}: {response.ReasonPhrase}\n{responseContent}");
            }
        }

        // Implementação do padrão IDisposable
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Não é necessário chamar Dispose no _httpClient Singleton
            }

            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            await Task.CompletedTask;
        }
    }
}
