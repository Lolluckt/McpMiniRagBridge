using Microsoft.EntityFrameworkCore;

namespace McpMiniRagBridge.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<RagDocument> Documents => Set<RagDocument>();
    }

    public class RagDocument
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
