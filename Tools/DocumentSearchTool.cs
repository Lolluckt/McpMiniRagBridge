using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol.Types;
using McpMiniRagBridge.Services;

namespace McpMiniRagBridge.Tools;

[McpServerToolType]
public static class DocumentSearchTool
{
    [McpServerTool(Name = "search_docs"), Description("Finds relevant documents based on user query.")]
    public static async Task<CallToolResponse> SearchDocs(
        [Description("The user search query.")] string query,
        [FromServices] VectorSearch vector)
    {
        Console.Error.WriteLine($"[MCP Tool] search_docs called with query: \"{query}\"");

        try
        {
            var results = await vector.SearchAsync(query);
            Console.Error.WriteLine($"[MCP Tool] search_docs got {results.Count} results");

            if (results == null || results.Count == 0)
            {
                return new CallToolResponse
                {
                    Content = new List<Content>
                    {
                        new Content
                        {
                            Type = "text",
                            Text = "⚠️ No documents found for your query."
                        }
                    }
                };
            }

            var contentItems = results.Select(result =>
            {
                Console.Error.WriteLine($"[MCP Tool] → {result.Title}");
                return new Content
                {
                    Type = "text",
                    Text = $"📄 {result.Title}: {result.Snippet}"
                };
            }).ToList();

            return new CallToolResponse { Content = contentItems };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[MCP Tool ERROR] {ex.Message}\n{ex.StackTrace}");

            return new CallToolResponse
            {
                Content = new List<Content>
                {
                    new Content
                    {
                        Type = "text",
                        Text = $"❌ Error occurred: {ex.Message}"
                    }
                }
            };
        }
    }
}

public class SearchResultDto
{
    [Description("Title of the matched document")]
    public string Title { get; set; } = string.Empty;

    [Description("Snippet or preview from the document content")]
    public string Snippet { get; set; } = string.Empty;
}
