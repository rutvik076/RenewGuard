using System.Text.Json;
using RenewGuard.Domain.Models;

namespace RenewGuard.UnitTests;

public class RenewalCalculatorTests
{
    [Fact]
    public void ReturnsNull_WhenNoticePeriodIsNull() =>
        Assert.Null(RenewalCalculator.CalculateLastCancelDate(new DateOnly(2027, 3, 1), null));

    [Fact]
    public void ReturnsNull_WhenEndDateIsNull() =>
        Assert.Null(RenewalCalculator.CalculateLastCancelDate(null, 30));

    [Fact]
    public void HandlesLeapYear() =>
        Assert.Equal(new DateOnly(2024, 2, 29),
            RenewalCalculator.CalculateLastCancelDate(new DateOnly(2024, 3, 1), 1));

    [Fact]
    public void CanReturnPastDate() =>
        Assert.Equal(new DateOnly(2024, 12, 2),
            RenewalCalculator.CalculateLastCancelDate(new DateOnly(2025, 1, 1), 30));

    [Fact]
    public void ZeroNotice_ReturnsEndDate() =>
        Assert.Equal(new DateOnly(2027, 3, 1),
            RenewalCalculator.CalculateLastCancelDate(new DateOnly(2027, 3, 1), 0));

    [Fact]
    public void NegativeNotice_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RenewalCalculator.CalculateLastCancelDate(new DateOnly(2027, 3, 1), -5));
}

public class ExtractedFieldTests
{
    [Fact]
    public void NullValue_SurvivesJsonRoundTrip()
    {
        var original = new ExtractedField<int?>(null, null, null, 0.0);

        var json = JsonSerializer.Serialize(original);
        var back = JsonSerializer.Deserialize<ExtractedField<int?>>(json);

        Assert.Contains("\"Value\":null", json);
        Assert.NotNull(back);
        Assert.Null(back!.Value);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Confidence_OutOfRange_Throws(double confidence) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ExtractedField<int?>(30, "quote", 1, confidence));
}