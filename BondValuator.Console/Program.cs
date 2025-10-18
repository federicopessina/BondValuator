using Microsoft.Extensions.Configuration;
using BondValuator.Core.Models;
using BondValuator.Core.Services;
using System.Globalization;

// ----------------------
// 1. Determine base path
// ----------------------
string basePath = AppContext.BaseDirectory;

// Fallback for local development (dotnet run)
if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
{
    basePath = Directory.GetCurrentDirectory();
}

// ----------------------
// 2. Load configuration
// ----------------------
var config = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string inputDir = config["Paths:InputDirectory"] ?? "Data/Input";
string outputDir = config["Paths:OutputDirectory"] ?? "Data/Output";
int rounding = int.Parse(config["Valuation:DefaultRounding"] ?? "2");

// ----------------------
// 3. Build input/output paths
// ----------------------
string inputFile = args.Length > 0 ? args[0] : "bond_positions_sample.csv";
string inputPath = Path.Combine(basePath, inputDir, inputFile);
string outputFullDir = Path.Combine(basePath, outputDir);
string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
string outputFile = $"valuations_{timestamp}.csv";
string outputPath = Path.Combine(outputFullDir, outputFile);

// ----------------------
// 4. Ensure directories exist
// ----------------------
if (!File.Exists(inputPath))
{
    Console.WriteLine($"❌ Input file not found: {Path.GetFullPath(inputPath)}");
    Console.WriteLine("Make sure the CSV exists in the input directory.");
    return;
}

if (!Directory.Exists(outputFullDir))
{
    Directory.CreateDirectory(outputFullDir);
    Console.WriteLine($"📁 Created output directory: {Path.GetFullPath(outputFullDir)}");
}

// ----------------------
// 5. Load CSV and calculate PV
// ----------------------
var bonds = CsvParser.LoadBonds(inputPath);
var service = new BondValuationService();
var results = bonds
    .Select(b => (bond: b, pv: service.CalculatePresentValue(b)))
    .ToList();

// ----------------------
// 6. Write output CSV
// ----------------------
CsvWriter.WriteResults(outputPath, results, rounding);

Console.WriteLine($"✅ Valuation complete. {results.Count} records processed.");
Console.WriteLine($"📄 Output written to: {outputPath}");
