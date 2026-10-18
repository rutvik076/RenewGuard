namespace RenewGuard.Domain.Models;

public record ContractExtraction(
    ExtractedField<string?> VendorName,
    ExtractedField<DateOnly?> EffectiveDate,
    ExtractedField<DateOnly?> CurrentTermEndDate,
    ExtractedField<bool?> AutoRenews,
    ExtractedField<int?> RenewalTermMonths,
    ExtractedField<int?> NoticePeriodDays,
    ExtractedField<decimal?> PriceEscalationPercent,
    ExtractedField<bool?> TerminationForConvenience,
    ExtractedField<int?> InitialTermMonths)
{
    public DateOnly? ResolvedTermEndDate =>
        RenewalCalculator.ResolveTermEndDate(
            CurrentTermEndDate.Value, EffectiveDate.Value, InitialTermMonths.Value);

    public DateOnly? LastCancelDate =>
        RenewalCalculator.CalculateLastCancelDate(
            ResolvedTermEndDate, NoticePeriodDays.Value);
}
