using RenewGuard.Domain.Models;

namespace RenewGuard.UnitTests;

public class NextCancelWindowTests
{
    private static readonly DateOnly Today = new(2026, 10, 18);

    private static CancelWindow? W(DateOnly end, DateOnly deadline) => new CancelWindow(end, deadline);

    [Fact]
    public void RollsForwardOneTerm_AfterInitialTermEnded() =>
        Assert.Equal(W(new DateOnly(2027, 2, 28), new DateOnly(2026, 12, 30)),
            RenewalCalculator.NextCancelWindow(new DateOnly(2026, 2, 28), true, 12, 60, Today));

    [Fact]
    public void RollsForwardMultipleTerms() =>
        Assert.Equal(W(new DateOnly(2027, 3, 1), new DateOnly(2027, 1, 15)),
            RenewalCalculator.NextCancelWindow(new DateOnly(2026, 3, 1), true, 6, 45, Today));

    [Fact]
    public void UsesInitialTerm_WhenDeadlineStillAhead() =>
        Assert.Equal(W(new DateOnly(2027, 2, 28), new DateOnly(2026, 10, 31)),
            RenewalCalculator.NextCancelWindow(new DateOnly(2027, 2, 28), true, 12, 120, Today));

    [Fact]
    public void DeadlinePassedForCurrentTerm_MovesToNextTerm() =>
        Assert.Equal(W(new DateOnly(2028, 2, 28), new DateOnly(2027, 10, 31)),
            RenewalCalculator.NextCancelWindow(new DateOnly(2027, 2, 28), true, 12, 120, new DateOnly(2026, 11, 15)));

    [Fact]
    public void DeadlineExactlyToday_StillCounts() =>
        Assert.Equal(W(new DateOnly(2026, 11, 17), new DateOnly(2026, 10, 18)),
            RenewalCalculator.NextCancelWindow(new DateOnly(2026, 11, 17), true, 12, 30, Today));

    [Fact]
    public void NotAutoRenewing_AndDeadlinePassed_ReturnsNull() =>
        Assert.Null(RenewalCalculator.NextCancelWindow(new DateOnly(2026, 2, 28), false, 12, 60, Today));

    [Fact]
    public void AutoRenewsButRenewalLengthUnknown_DoesNotRoll() =>
        Assert.Null(RenewalCalculator.NextCancelWindow(new DateOnly(2026, 2, 28), true, null, 60, Today));

    [Fact]
    public void MonthEndDoesNotDrift() =>
        Assert.Equal(W(new DateOnly(2026, 3, 31), new DateOnly(2026, 3, 31)),
            RenewalCalculator.NextCancelWindow(new DateOnly(2026, 1, 31), true, 1, 0, new DateOnly(2026, 3, 15)));

    [Fact]
    public void MissingNotice_ReturnsNull() =>
        Assert.Null(RenewalCalculator.NextCancelWindow(new DateOnly(2027, 2, 28), true, 12, null, Today));
}
