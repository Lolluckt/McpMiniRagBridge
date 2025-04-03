using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using McpMiniRagBridge.Data;
using McpMiniRagBridge.Services;
using McpMiniRagBridge.Tools;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });

        Console.Error.WriteLine("[Server] MCP STDIO server starting...");

        var tools = typeof(DocumentSearchTool)
            .GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var tool in tools)
        {
            Console.Error.WriteLine($"[DEBUG] MCP tool discovered: {tool.Name}");
        }

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite("Data Source=documents.db"));

                services.AddSingleton<IEmbeddingService, DummyEmbedding>();
                services.AddScoped<VectorSearch>();

                services
                    .AddMcpServer()
                    .WithStdioServerTransport()
                    .WithToolsFromAssembly();

                services.AddHostedService<DatabaseSeeder>();
            });

        await builder.RunConsoleAsync();
    }
}

public class DatabaseSeeder : IHostedService
{
    private readonly IServiceProvider _services;

    public DatabaseSeeder(IServiceProvider services) => _services = services;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Documents.Any())
        {
            db.Documents.AddRange(
                new RagDocument { Title = "Authentication Setup", Content = "services.AddAuthentication(...)" },
                new RagDocument { Title = "EF Core Tips", Content = "EF Core is a powerful ORM. Use AsNoTracking for read-only queries." },
                new RagDocument { Title = "Performance Best Practices", Content = "Performance: Cache responses and avoid N+1 queries." }
            );

            await db.SaveChangesAsync(cancellationToken);
            Console.Error.WriteLine("[Seeder] Sample documents added to database.");
        }
        else
        {
            Console.Error.WriteLine("[Seeder] Documents already exist, skipping seeding.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
