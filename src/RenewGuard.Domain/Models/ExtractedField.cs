namespace RenewGuard.Domain.Models;

public record ExtractedField<T>(
    T? Value,
    string? Evidence,
    int? Page,
    double Confidence
);