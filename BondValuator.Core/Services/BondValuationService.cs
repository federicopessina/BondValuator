using BondValuator.Core.Models;

namespace BondValuator.Core.Services;

public class BondValuationService
{
    public decimal CalculatePresentValue(BondRecord bond)
    {
        decimal faceValue = bond.FaceValue;
        double df = bond.DiscountFactor;
        double years = bond.YearsToMaturity;
        double rate = ExtractRate(bond.Rate);

        switch (bond.Type.Trim().ToLower())
        {
            case "bond":
                int paymentsPerYear = GetPaymentsPerYear(bond.PaymentFrequency);
                double cpp = rate / paymentsPerYear;
                double n = years * paymentsPerYear;
                return (decimal)(((1 + cpp) * n * (double)faceValue) * df);

            case "zero-coupon":
                return (decimal)(((1 + rate) * years * (double)faceValue) * df);

            case "inflation-linked":
                paymentsPerYear = GetPaymentsPerYear(bond.PaymentFrequency);
                cpp = rate / paymentsPerYear;
                n = years * paymentsPerYear;
                return (decimal)(((1 + cpp) * n * (double)faceValue) * df);

            default:
                throw new InvalidOperationException($"Unknown bond type: {bond.Type}");
        }
    }

    private static double ExtractRate(string rateText)
    {
        if (string.IsNullOrWhiteSpace(rateText))
            return 0;

        rateText = rateText.Trim().Replace("%", "");

        if (rateText.Contains("Inflation+", StringComparison.OrdinalIgnoreCase))
        {
            var part = rateText.Split('+')[1];
            return double.TryParse(part, out var r) ? r / 100.0 : 0;
        }

        if (double.TryParse(rateText, out var rate))
            return rate / 100.0;

        return 0;
    }

    private static int GetPaymentsPerYear(string frequency)
    {
        return frequency.ToLower() switch
        {
            "annual" => 1,
            "semi-annual" => 2,
            "quarterly" => 4,
            _ => 1
        };
    }
}
