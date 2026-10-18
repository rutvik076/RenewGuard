namespace RenewGuard.Domain.Models;

public readonly record struct CancelWindow(DateOnly TermEnd, DateOnly Deadline);

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

    // Finds the first term whose cancellation deadline is today or later.
    // Auto-renewing contracts roll forward in whole renewal terms. Each term
    // end is computed from the initial end date (not from the previous
    // term end) so month-end dates do not drift.
    public static CancelWindow? NextCancelWindow(
        DateOnly? initialTermEnd,
        bool? autoRenews,
        int? renewalTermMonths,
        int? noticePeriodDays,
        DateOnly asOf)
    {
        if (initialTermEnd is null || noticePeriodDays is null)
            return null;

        if (noticePeriodDays < 0 || noticePeriodDays > 3650)
            return null;

        var canRoll = autoRenews == true && renewalTermMonths is > 0 and <= 120;
        var maxTerms = canRoll ? 600 : 0;

        for (var n = 0; n <= maxTerms; n++)
        {
            var termEnd = n == 0
                ? initialTermEnd.Value
                : initialTermEnd.Value.AddMonths(n * renewalTermMonths!.Value);

            var deadline = termEnd.AddDays(-noticePeriodDays.Value);
            if (deadline >= asOf)
                return new CancelWindow(termEnd, deadline);
        }

        return null;
    }
}
