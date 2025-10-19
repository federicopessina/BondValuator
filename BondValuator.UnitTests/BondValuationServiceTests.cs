using FluentAssertions;
using BondValuator.Core.Services;
using BondValuator.Core.Models;

namespace BondValuator.UnitTests;

public class BondValuationServiceTests
{
    private readonly BondValuationService _service = new BondValuationService();

    [Fact]
    public void CalculatePresentValue_ZeroCouponBond_ReturnsCorrectPV()
    {
        var bond = new BondRecord
        {
            Type = "Zero-Coupon",
            FaceValue = 1000,
            YearsToMaturity = 5,
            Rate = "5%",
            DiscountFactor = 0.8
        };

        var pv = _service.CalculatePresentValue(bond);

        // PV = (1+rate)^years * FaceValue * DF
        double expected = ((1 + 0.05) * 5 * 1000) * 0.8;
        pv.Should().BeApproximately((decimal)expected, 0.01m);
    }

    [Fact]
    public void CalculatePresentValue_BondAnnual_ReturnsCorrectPV()
    {
        var bond = new BondRecord
        {
            Type = "Bond",
            FaceValue = 500,
            YearsToMaturity = 2,
            Rate = "10%",
            PaymentFrequency = "Annual",
            DiscountFactor = 0.9
        };

        var pv = _service.CalculatePresentValue(bond);

        // PV = ((1 + cpp)^n * FaceValue) * DF
        double cpp = 0.10 / 1;
        double n = 2 * 1;
        double expected = ((1 + cpp) * n * 500) * 0.9;
        pv.Should().BeApproximately((decimal)expected, 0.01m);
    }

    [Fact]
    public void CalculatePresentValue_InflationLinked_ReturnsCorrectPV()
    {
        var bond = new BondRecord
        {
            Type = "Inflation-Linked",
            FaceValue = 1000,
            YearsToMaturity = 3,
            Rate = "Inflation+1.5%",
            PaymentFrequency = "Semi-Annual",
            DiscountFactor = 0.85
        };

        var pv = _service.CalculatePresentValue(bond);

        double cpp = 0.015 / 2;
        double n = 3 * 2;
        double expected = ((1 + cpp) * n * 1000) * 0.85;
        pv.Should().BeApproximately((decimal)expected, 0.01m);
    }

    [Fact]
    public void CalculatePresentValue_UnknownType_Throws()
    {
        var bond = new BondRecord
        {
            Type = "Unknown",
            FaceValue = 1000,
            YearsToMaturity = 1,
            Rate = "5%",
            DiscountFactor = 1
        };

        Assert.Throws<InvalidOperationException>(() => _service.CalculatePresentValue(bond));
    }
}
