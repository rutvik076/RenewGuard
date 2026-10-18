using RenewGuard.Domain.Models;

namespace RenewGuard.UnitTests;

public class ResolveTermEndDateTests
{
    [Fact]
    public void StatedDate_Wins() =>
        Assert.Equal(new DateOnly(2026, 2, 28),
            RenewalCalculator.ResolveTermEndDate(new DateOnly(2026, 2, 28), new DateOnly(2024, 3, 1), 24));

    [Fact]
    public void ComputesFromEffectiveDateAndTerm() =>
        Assert.Equal(new DateOnly(2026, 3, 1),
            RenewalCalculator.ResolveTermEndDate(null, new DateOnly(2024, 9, 1), 18));

    [Fact]
    public void ReturnsNull_WhenEffectiveDateMissing() =>
        Assert.Null(RenewalCalculator.ResolveTermEndDate(null, null, 12));

    [Fact]
    public void ReturnsNull_WhenTermMissing() =>
        Assert.Null(RenewalCalculator.ResolveTermEndDate(null, new DateOnly(2025, 1, 1), null));

    [Fact]
    public void ReturnsNull_WhenTermIsZero() =>
        Assert.Null(RenewalCalculator.ResolveTermEndDate(null, new DateOnly(2025, 1, 1), 0));
}
