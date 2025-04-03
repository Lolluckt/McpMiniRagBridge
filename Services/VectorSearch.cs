using McpMiniRagBridge.Data;
using McpMiniRagBridge.Tools;
using Microsoft.EntityFrameworkCore;

namespace McpMiniRagBridge.Services
{
    public class VectorSearch
    {
        private readonly AppDbContext _db;
        private readonly IEmbeddingService _embedding;
        private const float SimilarityThreshold = 0.1f;

        public VectorSearch(AppDbContext db, IEmbeddingService embedding)
        {
            _db = db;
            _embedding = embedding;
        }

        public async Task<List<SearchResultDto>> SearchAsync(string query)
        {
            var queryVec = await _embedding.GetEmbeddingAsync(query);
            var docs = await _db.Documents.AsNoTracking().ToListAsync();

            SearchResultDto? bestResult = null;
            float bestScore = 0f;

            foreach (var doc in docs)
            {
                var contentVec = await _embedding.GetEmbeddingAsync(doc.Content);
                var score = CosineSimilarity(queryVec, contentVec);

                Console.Error.WriteLine($"[DEBUG] Comparing with '{doc.Title}' | Score: {score}");

                if (score <= SimilarityThreshold)
                    continue;

                if (score > bestScore)
                {
                    var snippet = doc.Content.Length > 150
                        ? doc.Content[..150] + "..."
                        : doc.Content;

                    bestResult = new SearchResultDto
                    {
                        Title = doc.Title,
                        Snippet = snippet
                    };

                    bestScore = score;
                }
            }

            return bestResult != null ? new List<SearchResultDto> { bestResult } : new();
        }


        private float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            if (magA == 0 || magB == 0)
                return 0;

            return dot / (float)(Math.Sqrt(magA) * Math.Sqrt(magB));
        }
    }
}
