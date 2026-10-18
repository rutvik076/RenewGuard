using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using RenewGuard.Domain.Models;
using RenewGuard.Infrastructure.Extraction;

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new InvalidOperationException("Set the ANTHROPIC_API_KEY environment variable.");
var model = Environment.GetEnvironmentVariable("RENEWGUARD_MODEL") ?? "claude-haiku-5-5";
var extractor = new LlmContractExtractor(ChatClientFactory.Create(model, apiKey));

var inv = CultureInfo.InvariantCulture;

var fields = new (string Name, Func<ContractExtraction, string?> Get)[]
{
    ("vendor_name", e => e.VendorName.Value),
    ("effective_date", e => e.EffectiveDate.Value?.ToString("yyyy-MM-dd", inv)),
    ("current_term_end_date", e => e.ResolvedTermEndDate?.ToString("yyyy-MM-dd", inv)),
    ("auto_renews", e => e.AutoRenews.Value?.ToString().ToLowerInvariant()),
    ("renewal_term_months", e => e.RenewalTermMonths.Value?.ToString(inv)),
    ("notice_period_days", e => e.NoticePeriodDays.Value?.ToString(inv)),
    ("price_escalation_percent", e => e.PriceEscalationPercent.Value?.ToString("0.####", inv)),
    ("termination_for_convenience", e => e.TerminationForConvenience.Value?.ToString().ToLowerInvariant()),
};

var ids = args.Length > 0
    ? args
    : Directory.GetFiles("data/contracts", "*.txt")
        .Select(p => Path.GetFileNameWithoutExtension(p)!)
        .OrderBy(x => x)
        .ToArray();

var stats = fields.ToDictionary(f => f.Name, _ => new Counts());
var byDifficulty = new Dictionary<string, (int Correct, int Total)>();
var failures = new List<string>();
var errors = new List<string>();
var latencies = new List<double>();

foreach (var id in ids)
{
    var contractPath = $"data/contracts/{id}.txt";
    var expectedPath = $"data/expected/{id}.json";
    if (!File.Exists(contractPath) || !File.Exists(expectedPath))
    {
        errors.Add($"{id}: missing contract or answer key file");
        continue;
    }

    using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(expectedPath));
    var root = doc.RootElement;
    var difficulty = root.TryGetProperty("difficulty", out var d) && d.ValueKind == JsonValueKind.String
        ? d.GetString()!
        : "unknown";

    ContractExtraction result;
    var sw = Stopwatch.StartNew();
    try
    {
        result = await extractor.ExtractAsync(await File.ReadAllTextAsync(contractPath));
    }
    catch (Exception ex)
    {
        errors.Add($"{id}: {ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
        Console.WriteLine($"{id} ERROR");
        continue;
    }
    sw.Stop();
    latencies.Add(sw.Elapsed.TotalSeconds);
    Console.WriteLine($"{id} done in {sw.Elapsed.TotalSeconds:0.0}s");

    foreach (var (name, get) in fields)
    {
        var expected = ReadExpected(root, name);
        var actual = get(result);
        var c = stats[name];
        string kind;

        if (expected is null)
        {
            c.NullExpected++;
            if (actual is null) { c.Correct++; kind = "correct"; }
            else { c.Hallucinated++; kind = "HALLUCINATED"; }
        }
        else if (actual is null) { c.Missed++; kind = "MISSED"; }
        else if (Same(name, expected, actual)) { c.Correct++; kind = "correct"; }
        else { c.Wrong++; kind = "WRONG"; }

        byDifficulty.TryGetValue(difficulty, out var dv);
        byDifficulty[difficulty] = (dv.Correct + (kind == "correct" ? 1 : 0), dv.Total + 1);

        if (kind != "correct")
            failures.Add($"| {id} | {name} | {expected ?? "null"} | {actual ?? "null"} | {kind} |");
    }
}

// ---- Report ----
var totalCells = stats.Values.Sum(s => s.Total);
var totalCorrect = stats.Values.Sum(s => s.Correct);
var totalHallucinated = stats.Values.Sum(s => s.Hallucinated);
var totalNullExpected = stats.Values.Sum(s => s.NullExpected);
var totalMissed = stats.Values.Sum(s => s.Missed);
var totalWrong = stats.Values.Sum(s => s.Wrong);

var sb = new StringBuilder();
sb.AppendLine("# Evaluation results");
sb.AppendLine();
sb.AppendLine($"- Model: `{model}`");
sb.AppendLine($"- Date: {DateTime.UtcNow:yyyy-MM-dd}");
sb.AppendLine($"- Contracts scored: {latencies.Count} (errors: {errors.Count})");
if (latencies.Count > 0)
    sb.AppendLine($"- Latency per contract: avg {latencies.Average():0.0}s, max {latencies.Max():0.0}s");
sb.AppendLine();
sb.AppendLine("## Overall");
sb.AppendLine();
if (totalCells > 0)
{
    sb.AppendLine($"- Field accuracy: **{100.0 * totalCorrect / totalCells:0.0}%** ({totalCorrect}/{totalCells})");
    sb.AppendLine($"- Wrong values: {totalWrong}");
    sb.AppendLine($"- Missed values (null when the key has a value): {totalMissed}");
    sb.AppendLine(totalNullExpected > 0
        ? $"- Hallucination rate (value returned where the key says null): **{100.0 * totalHallucinated / totalNullExpected:0.0}%** ({totalHallucinated}/{totalNullExpected})"
        : "- Hallucination rate: n/a (no null cases in the set)");
}
sb.AppendLine();
sb.AppendLine("## Per field");
sb.AppendLine();
sb.AppendLine("| Field | Accuracy | Wrong | Hallucinated | Missed |");
sb.AppendLine("|---|---|---|---|---|");
foreach (var (name, _) in fields)
{
    var s = stats[name];
    var acc = s.Total > 0 ? 100.0 * s.Correct / s.Total : 0;
    sb.AppendLine($"| {name} | {acc:0.0}% | {s.Wrong} | {s.Hallucinated} | {s.Missed} |");
}
sb.AppendLine();
sb.AppendLine("## By difficulty");
sb.AppendLine();
sb.AppendLine("| Difficulty | Accuracy |");
sb.AppendLine("|---|---|");
foreach (var (diff, v) in byDifficulty.OrderBy(k => k.Key))
    sb.AppendLine($"| {diff} | {100.0 * v.Correct / v.Total:0.0}% ({v.Correct}/{v.Total}) |");

if (failures.Count > 0)
{
    sb.AppendLine();
    sb.AppendLine("## Mistakes");
    sb.AppendLine();
    sb.AppendLine("| Contract | Field | Expected | Got | Type |");
    sb.AppendLine("|---|---|---|---|---|");
    foreach (var f in failures) sb.AppendLine(f);
}

if (errors.Count > 0)
{
    sb.AppendLine();
    sb.AppendLine("## Errors");
    sb.AppendLine();
    foreach (var e in errors) sb.AppendLine($"- {e}");
}

Directory.CreateDirectory("docs");
await File.WriteAllTextAsync("docs/eval-results.md", sb.ToString());
Console.WriteLine();
Console.WriteLine(sb.ToString());
Console.WriteLine("Saved to docs/eval-results.md");

// ---- Helpers ----
static string? ReadExpected(JsonElement root, string key)
{
    if (!root.TryGetProperty(key, out var el) || el.ValueKind == JsonValueKind.Null)
        return null;

    return el.ValueKind switch
    {
        JsonValueKind.String => el.GetString(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Number => el.GetDecimal().ToString("0.####", CultureInfo.InvariantCulture),
        _ => null
    };
}

static bool Same(string field, string expected, string actual) =>
    field == "vendor_name"
        ? NormalizeText(expected) == NormalizeText(actual)
        : string.Equals(expected, actual, StringComparison.Ordinal);

static string NormalizeText(string s) =>
    string.Join(' ', s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();

class Counts
{
    public int Correct, Wrong, Hallucinated, Missed, NullExpected;
    public int Total => Correct + Wrong + Hallucinated + Missed;
}

