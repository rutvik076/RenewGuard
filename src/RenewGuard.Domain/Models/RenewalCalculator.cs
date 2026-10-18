namespace RenewGuard.Domain.Models;

public static class RenewalCalculator
{
    public static DateOnly? CalculateLastCancelDate(
        DateOnly? currentTermEndDate,
        int? noticePeriodDays)
    {
        if (currentTermEndDate is null || noticePeriodDays is null)
            return null;

        if (noticePeriodDays < 0)
            throw new ArgumentOutOfRangeException(nameof(noticePeriodDays));

        return currentTermEndDate.Value.AddDays(-noticePeriodDays.Value);
    }

    // An explicitly stated end date always wins. Otherwise the end date is
    // the effective date plus the initial term (the anniversary date).
    public static DateOnly? ResolveTermEndDate(
        DateOnly? statedEndDate,
        DateOnly? effectiveDate,
        int? initialTermMonths)
    {
        if (statedEndDate is not null)
            return statedEndDate;

        if (effectiveDate is null || initialTermMonths is null || initialTermMonths <= 0)
            return null;

        return effectiveDate.Value.AddMonths(initialTermMonths.Value);
    }
}
