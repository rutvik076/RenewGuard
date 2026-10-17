namespace RenewGuard.Domain.Models;

public static class Contract
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
}