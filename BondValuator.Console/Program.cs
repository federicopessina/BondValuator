using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BondValuator.Core.Services;

// ----------------------
// 1. Setup Logging
// ----------------------
string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logDir);
string logFilePath = Path.Combine(logDir, "bondvaluator.log");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger("BondValuator");

// Helper for writing to both console and file
void LogToFile(string message)
{
    var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
    File.AppendAllText(logFilePath, line + Environment.NewLine);
    Console.WriteLine(line);
}

// ----------------------
// 2. Determine base path
// ----------------------
string basePath = AppContext.BaseDirectory;
if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
{
    basePath = Directory.GetCurrentDirectory();
    logger.LogInformation("Switched base path to development directory: {BasePath}", basePath);
    LogToFile($"Switched base path to development directory: {basePath}");
}

// ----------------------
// 3. Load configuration
// ----------------------
try
{
    var config = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    string inputDir = config["Paths:InputDirectory"] ?? "Data/Input";
    string outputDir = config["Paths:OutputDirectory"] ?? "Data/Output";
    int rounding = int.Parse(config["Valuation:DefaultRounding"] ?? "2");

    logger.LogInformation("Configuration loaded successfully.");
    LogToFile("Configuration loaded successfully.");

    // ----------------------
    // 4. Build input/output paths
    // ----------------------
    string inputFile = args.Length > 0 ? args[0] : "bond_positions_sample.csv";
    string inputPath = Path.Combine(basePath, inputDir, inputFile);
    string outputFullDir = Path.Combine(basePath, outputDir);
    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    string outputFile = $"valuations_{timestamp}.csv";
    string outputPath = Path.Combine(outputFullDir, outputFile);

    logger.LogInformation("Input file path: {Path}", inputPath);
    LogToFile($"Input file path: {inputPath}");
    logger.LogInformation("Output file path: {Path}", outputPath);
    LogToFile($"Output file path: {outputPath}");

    // ----------------------
    // 5. Ensure directories exist
    // ----------------------
    if (!File.Exists(inputPath))
    {
        logger.LogError("❌ Input file not found: {Path}", Path.GetFullPath(inputPath));
        LogToFile($"❌ Input file not found: {Path.GetFullPath(inputPath)}");
        return;
    }

    if (!Directory.Exists(outputFullDir))
    {
        Directory.CreateDirectory(outputFullDir);
        logger.LogInformation("📁 Created output directory: {Path}", Path.GetFullPath(outputFullDir));
        LogToFile($"📁 Created output directory: {Path.GetFullPath(outputFullDir)}");
    }

    // ----------------------
    // 6. Validate CSV
    // ----------------------
    logger.LogInformation("Validating input CSV...");
    LogToFile("Validating input CSV...");

    var validationIssues = CsvValidator.ValidateFile(inputPath);

    if (validationIssues.Count > 0)
    {
        logger.LogWarning("⚠️ CSV validation found {Count} issue(s).", validationIssues.Count);
        LogToFile($"⚠️ CSV validation found {validationIssues.Count} issue(s).");

        foreach (var issue in validationIssues)
        {
            logger.LogWarning(issue);
            LogToFile(issue);
        }
    }
    else
    {
        logger.LogInformation("✅ CSV validation passed with no issues.");
        LogToFile("✅ CSV validation passed with no issues.");
    }

    // ----------------------
    // 7. Load CSV and calculate PV
    // ----------------------
    logger.LogInformation("Loading input CSV...");
    LogToFile("Loading input CSV...");
    var bonds = CsvParser.LoadBonds(inputPath);

    logger.LogInformation("Processing {Count} bonds...", bonds.Count);
    LogToFile($"Processing {bonds.Count} bonds...");
    var service = new BondValuationService();

    var results = bonds
        .Select(b => (bond: b, pv: service.CalculatePresentValue(b)))
        .ToList();

    // ----------------------
    // 8. Write output CSV
    // ----------------------
    CsvWriter.WriteResults(outputPath, results, rounding);

    logger.LogInformation("✅ Valuation complete. {Count} records processed.", results.Count);
    LogToFile($"✅ Valuation complete. {results.Count} records processed.");
    logger.LogInformation("📄 Output written to: {OutputPath}", outputPath);
    LogToFile($"📄 Output written to: {outputPath}");
}
catch (Exception ex)
{
    logger.LogError(ex, "An unexpected error occurred during bond valuation.");
    LogToFile($"❌ ERROR: {ex.Message}\n{ex.StackTrace}");
}
