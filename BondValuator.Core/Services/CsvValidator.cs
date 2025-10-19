using System.Globalization;

namespace BondValuator.Core.Services;

public static class CsvValidator
{
    /// <summary>
    /// Validates a CSV file and returns a list of issues (each as a string message).
    /// Does not throw, does not log — caller handles reporting.
    /// </summary>
    public static List<string> ValidateFile(string path)
    {
        var issues = new List<string>();

        if (!File.Exists(path))
        {
            issues.Add($"File not found: {path}");
            return issues;
        }

        var lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            issues.Add($"File '{Path.GetFileName(path)}' is empty or missing data rows.");
            return issues;
        }

        int expectedColumns = 10;
        int lineNumber = 1; // header line
        foreach (var line in lines.Skip(1))
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                issues.Add($"Line {lineNumber}: empty or whitespace row.");
                continue;
            }

            var cols = line.Split(';');
            if (cols.Length != expectedColumns)
            {
                issues.Add($"Line {lineNumber}: expected {expectedColumns} columns, found {cols.Length}.");
                continue;
            }

            // Validate numeric fields
            if (!decimal.TryParse(cols[3], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                issues.Add($"Line {lineNumber}: invalid FaceValue '{cols[3]}'.");

            if (!double.TryParse(cols[7], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                issues.Add($"Line {lineNumber}: invalid YearsToMaturity '{cols[7]}'.");

            if (!double.TryParse(cols[8], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                issues.Add($"Line {lineNumber}: invalid DiscountFactor '{cols[8]}'.");
        }

        return issues;
    }
}
