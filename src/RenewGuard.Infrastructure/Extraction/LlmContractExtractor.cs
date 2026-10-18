using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using RenewGuard.Application.Abstractions;
using RenewGuard.Domain.Models;

namespace RenewGuard.Infrastructure.Extraction;

public sealed class FlexibleStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetDecimal().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.True => "true",
            JsonTokenType.False => "false",
            _ => throw new JsonException("Unexpected JSON token for a field value.")
        };

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}

public class LlmField
{
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Value { get; set; }
    public string? Evidence { get; set; }
    public int? Page { get; set; }
    public double Confidence { get; set; }
}

public class LlmExtraction
{
    public LlmField VendorName { get; set; } = new();
    public LlmField EffectiveDate { get; set; } = new();
    public LlmField CurrentTermEndDate { get; set; } = new();
    public LlmField AutoRenews { get; set; } = new();
    public LlmField RenewalTermMonths { get; set; } = new();
    public LlmField NoticePeriodDays { get; set; } = new();
    public LlmField PriceEscalationPercent { get; set; } = new();
    public LlmField TerminationForConvenience { get; set; } = new();
}

public sealed class LlmContractExtractor(IChatClient chatClient) : IContractExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SystemPrompt = """
        You extract key terms from a vendor/SaaS contract.

        Respond with ONLY a single JSON object. No markdown, no code fences,
        no explanation before or after. Use exactly this shape:

        {
          "VendorName":                { "Value": null, "Evidence": null, "Page": null, "Confidence": 0.0 },
          "EffectiveDate":             { "Value": null, "Evidence": null, "Page": null, "Confidence": 0.0 },
          "CurrentTermEndDate":        { "Value": null, "Evidence": null, "Page": null, "Confidence": 0.0 },
          "AutoRenews":                { "Value": null, "Evidence": null, "Page": null, "Confidence": 0.0 },
          "RenewalTermMonths":         { "Value": null, "Evidence": null, "Page": null, "Confidence": 0.0 },
          "NoticePeriodDays":          { "Value": null, "Evidence": null, "Page": null, "Confidence": 0.0 },
          "PriceEscalationPercent":    { "Value": null, "Evidence": null, "Page": null, "Confidence": 0.0 },
          "TerminationForConvenience": { "Value": null, "Evidence": null, "Page": null, "Confidence": 0.0 }
        }

        For EACH field:
        - Value: a JSON string in quotes, or null if the contract does not state it
        - Evidence: an EXACT, word-for-word quote copied from the contract that
          supports the value (null when Value is null)
        - Page: always null
        - Confidence: a number from 0.0 to 1.0

        Formats for Value:
        - Dates: "YYYY-MM-DD"
        - Booleans: "true" or "false"
        - Integers: digits only, e.g. "60". Convert months to days
          (1 month = 30 days) ONLY for NoticePeriodDays.
        - PriceEscalationPercent: a plain number such as "5" for a FIXED
          percentage increase. If the increase is only index-linked (for
          example CPI) or unspecified, return null. For "greater of X% or
          CPI" return X.

        Rules:
        1. Never guess. If the contract does not clearly state a field,
           Value and Evidence must be null.
        2. Do not infer values from industry norms.
        3. Evidence must be copied exactly, never paraphrased.
        4. CurrentTermEndDate is when the first/current term expires.
        5. TerminationForConvenience is "true" only if a party may end the
           contract early without cause; "false" only if the contract
           explicitly says it may not.
        """;

    public async Task<ContractExtraction> ExtractAsync(
        string contractText, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, $"CONTRACT:\n{contractText}")
        };

        var response = await chatClient.GetResponseAsync(
            messages, cancellationToken: cancellationToken);

        var raw = ParseModelJson(response.Text);
        return Map(raw, contractText);
    }

    private static LlmExtraction ParseModelJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
            throw new InvalidOperationException($"Model did not return JSON. Raw reply: {text}");

        var json = text[start..(end + 1)];
        return JsonSerializer.Deserialize<LlmExtraction>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Could not parse model JSON. Raw reply: {text}");
    }

    private static ContractExtraction Map(LlmExtraction raw, string contract)
    {
        var haystack = Normalize(contract);

        ExtractedField<T?> Build<T>(LlmField f, Func<string, T?> parse)
        {
            if (string.IsNullOrWhiteSpace(f.Value) ||
                string.IsNullOrWhiteSpace(f.Evidence) ||
                !haystack.Contains(Normalize(f.Evidence), StringComparison.OrdinalIgnoreCase))
            {
                return new ExtractedField<T?>(default, null, null, 0.0);
            }

            var parsed = parse(f.Value.Trim());
            if (parsed is null)
                return new ExtractedField<T?>(default, null, null, 0.0);

            var confidence = Math.Clamp(f.Confidence, 0.0, 1.0);
            return new ExtractedField<T?>(parsed, f.Evidence, f.Page, confidence);
        }

        return new ContractExtraction(
            Build<string>(raw.VendorName, s => s),
            Build<DateOnly?>(raw.EffectiveDate, ParseDate),
            Build<DateOnly?>(raw.CurrentTermEndDate, ParseDate),
            Build<bool?>(raw.AutoRenews, ParseBool),
            Build<int?>(raw.RenewalTermMonths, ParseInt),
            Build<int?>(raw.NoticePeriodDays, s => ParseInt(s) is >= 0 and var n ? n : null),
            Build<decimal?>(raw.PriceEscalationPercent, ParseDecimal),
            Build<bool?>(raw.TerminationForConvenience, ParseBool));
    }

    private static string Normalize(string s) =>
        string.Join(' ', s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static DateOnly? ParseDate(string s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var d) ? d : null;

    private static bool? ParseBool(string s) =>
        bool.TryParse(s, out var b) ? b : null;

    private static int? ParseInt(string s) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    private static decimal? ParseDecimal(string s) =>
        decimal.TryParse(s.TrimEnd('%'), NumberStyles.Number,
            CultureInfo.InvariantCulture, out var d) ? d : null;
}
