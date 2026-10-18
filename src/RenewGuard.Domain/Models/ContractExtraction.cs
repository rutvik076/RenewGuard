namespace RenewGuard.Domain.Models;

public record ContractExtraction(
    ExtractedField<string?> VendorName,
    ExtractedField<DateOnly?> EffectiveDate,
    ExtractedField<DateOnly?> CurrentTermEndDate,
    ExtractedField<bool?> AutoRenews,
    ExtractedField<int?> RenewalTermMonths,
    ExtractedField<int?> NoticePeriodDays,
    ExtractedField<decimal?> PriceEscalationPercent,
    ExtractedField<bool?> TerminationForConvenience)
{
    public DateOnly? LastCancelDate =>
        RenewalCalculator.CalculateLastCancelDate(
            CurrentTermEndDate.Value, NoticePeriodDays.Value);
}
