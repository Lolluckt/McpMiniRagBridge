using System;
using System.Linq;
using System.Threading.Tasks;

namespace McpMiniRagBridge.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text);
    }

    /// <summary>
    /// Test implementation of IEmbeddingService:
    //// - If text contains “EF Core” (case insensitive),
    /// This ensures that “EF Core” will have similarity = 1,
    /// and any other query will have similarity = 0.
    /// </summary>
    public class DummyEmbedding : IEmbeddingService
    {
        private static readonly Dictionary<string, float[]> KeywordVectors = new()
    {
        { "ef core", GenerateVector(1f, 0f) },
        { "authentication", GenerateVector(0f, 1f) },
        { "performance", GenerateVector(0f, 0f, 1f) },
    };

        private static float[] GenerateVector(float a, float b = 0f, float c = 0f)
        {
            return Enumerable.Range(0, 64)
                .Select(i => i % 3 == 0 ? a : i % 3 == 1 ? b : c)
                .ToArray();
        }

        public Task<float[]> GetEmbeddingAsync(string text)
        {
            string normalized = text.ToLowerInvariant();
            foreach (var kv in KeywordVectors)
            {
                if (normalized.Contains(kv.Key))
                    return Task.FromResult(kv.Value);
            }
            foreach (var kv in KeywordVectors)
            {
                if (kv.Key.Split(' ').Any(part => normalized.Contains(part)))
                    return Task.FromResult(kv.Value);
            }
            return Task.FromResult(new float[64]);
        }
    }

}



/*
 * ==============================
 * Example of OpenAI Embedding API
 * ==============================
 *
 * public class OpenAiEmbedding : IEmbeddingService
 * {
 *     private readonly HttpClient _http;
 *     private readonly string _apiKey;
 *
 *     public OpenAiEmbedding(IConfiguration config)
 *     {
 *         _http = new HttpClient();
 *         _apiKey = config["OpenAI:ApiKey"] ?? throw new Exception("OpenAI API key missing");
 *     }
 *
 *     public async Task<float[]> GetEmbeddingAsync(string text)
 *     {
 *         var payload = new { input = text, model = "text-embedding-3-small" };
 *         var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings")
 *         {
 *             Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
 *         };
 *         request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
 *         var response = await _http.SendAsync(request);
 *         var json = await response.Content.ReadAsStringAsync();
 *         using var doc = JsonDocument.Parse(json);
 *         var vector = doc.RootElement
 *             .GetProperty("data")[0]
 *             .GetProperty("embedding")
 *             .EnumerateArray()
 *             .Select(x => x.GetSingle())
 *             .ToArray();
 *         return vector;
 *     }
 * }
 *
 * You need registration in DI:
 * builder.Services.AddSingleton<IEmbeddingService, OpenAiEmbedding>();
 *
 * Configuration, you need to paste your API key in file:
 * appsettings.json: { "OpenAI": { "ApiKey": "sk-..." } }
 * or environment variable: OpenAI__ApiKey
 */

