using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// stdout is reserved for the MCP protocol, so send all logs to stderr
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

var baseUrl = Environment.GetEnvironmentVariable("RENEWGUARD_API_URL") ?? "http://localhost:5238";
builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") });

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
