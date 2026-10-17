using RenewGuard.Domain.Models;

namespace RenewGuard.UnitTests;

public class ContractTests
{
    [Fact]
    public void CalculateLastCancelDate_ReturnsNull_WhenNoticePeriodIsNull()
    {
        var result = Contract.CalculateLastCancelDate(
            new DateOnly(2027, 3, 1), null);

        Assert.Null(result);
    }

    [Fact]
    public void CalculateLastCancelDate_HandlesLeapYear()
    {
        var result = Contract.CalculateLastCancelDate(
            new DateOnly(2024, 3, 1), 1);

        Assert.Equal(new DateOnly(2024, 2, 29), result);
    }

    [Fact]
    public void CalculateLastCancelDate_CanReturnPastDate()
    {
        var result = Contract.CalculateLastCancelDate(
            new DateOnly(2025, 1, 1), 30);

        Assert.Equal(new DateOnly(2024, 12, 2), result);
    }

    [Fact]
    public void CalculateLastCancelDate_ReturnsNull_WhenEndDateIsNull()
    {
        var result = Contract.CalculateLastCancelDate(null, 30);

        Assert.Null(result);
    }
}