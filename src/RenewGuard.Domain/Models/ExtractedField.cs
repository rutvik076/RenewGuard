namespace RenewGuard.Domain.Models;

public record ExtractedField<T>(
    T Value,
    string? Evidence,
    int? Page,
    double Confidence)
{
    // Rejects values outside 0.0–1.0
    public double Confidence { get; init; } =
        Confidence is >= 0.0 and <= 1.0
            ? Confidence
            : throw new ArgumentOutOfRangeException(nameof(Confidence));
}