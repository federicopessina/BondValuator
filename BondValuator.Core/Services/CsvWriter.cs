using BondValuator.Core.Models;
using System.Globalization;

namespace BondValuator.Core.Services;

public static class CsvWriter
{
    public static void WriteResults(string path, IEnumerable<(BondRecord bond, decimal pv)> results, int rounding)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine("BondID;Type;PresentValue;Issuer;Rating;YearsToMaturity;DeskNotes");

        foreach (var (bond, pv) in results)
        {
            writer.WriteLine($"{bond.BondID};{bond.Type};{Math.Round(pv, rounding).ToString(CultureInfo.InvariantCulture)};{bond.Issuer};{bond.Rating};{bond.YearsToMaturity};{bond.DeskNotes}");
        }
    }
}
