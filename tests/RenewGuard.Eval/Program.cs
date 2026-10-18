using System.Text.Json;
using RenewGuard.Infrastructure.Extraction;

var id = args.Length > 0 ? args[0] : "c001";

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new InvalidOperationException("Set the ANTHROPIC_API_KEY environment variable.");
var model = Environment.GetEnvironmentVariable("RENEWGUARD_MODEL") ?? "claude-haiku-5-5";

var extractor = new LlmContractExtractor(ChatClientFactory.Create(model, apiKey));

var text = await File.ReadAllTextAsync($"data/contracts/{id}.txt");
var result = await extractor.ExtractAsync(text);

var json = new JsonSerializerOptions { WriteIndented = true };
Console.WriteLine("=== EXTRACTED ===");
Console.WriteLine(JsonSerializer.Serialize(result, json));
Console.WriteLine($"LastCancelDate: {result.LastCancelDate}");
Console.WriteLine();
Console.WriteLine("=== EXPECTED ===");
Console.WriteLine(await File.ReadAllTextAsync($"data/expected/{id}.json"));

