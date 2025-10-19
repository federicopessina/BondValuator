using BondValuator.Core.Models;

namespace BondValuator.Core.Services;

public static class CsvParser
{
    public static List<BondRecord> LoadBonds(string path)
    {
        var lines = File.ReadAllLines(path);
        var bonds = new List<BondRecord>();

        foreach (var line in lines.Skip(1)) // skip header
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cols = line.Split(';');
            if (cols.Length < 10)
                continue;

            bonds.Add(new BondRecord
            {
                BondID = cols[0],
                Issuer = cols[1],
                Rate = cols[2],
                FaceValue = decimal.Parse(cols[3]),
                PaymentFrequency = cols[4],
                Rating = cols[5],
                Type = cols[6],
                YearsToMaturity = double.Parse(cols[7]),
                DiscountFactor = double.Parse(cols[8]),
                DeskNotes = cols[9]
            });
        }

        return bonds;
    }
}
