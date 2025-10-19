using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BondValuator.Core.Services;

string logFilePath;

ILoggerFactory loggerFactory;

ILogger logger;

SetupLogging(out logFilePath, out loggerFactory, out logger);

string basePath = DetermineBasePath(logFilePath, logger);

try
{
    string inputDir, outputDir;
    int rounding;
    LoadConfigurations(logFilePath, logger, basePath, out inputDir, out outputDir, out rounding);

    string inputPath, outputFullDir, outputPath;
    BuildInputOutputPaths(args, logFilePath, logger, basePath, inputDir, outputDir, out inputPath, out outputFullDir, out outputPath);

    // ----------------------
    // Ensure directories exist
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

    ValidateCsv(logFilePath, logger, inputPath);

    List<BondValuator.Core.Models.BondRecord> bonds = LoadCsv(logFilePath, logger, inputPath);

    BondValuationService service = CalculatePresentValue(logFilePath, logger, bonds);

    var results = bonds
        .Select(b => (bond: b, pv: service.CalculatePresentValue(b)))
        .ToList();
    WriteOutputCsv(logFilePath, logger, rounding, outputPath, results);
}
catch (Exception ex)
{
    logger.LogError(ex, "An unexpected error occurred during bond valuation.");
    LogToFile($"❌ ERROR: {ex.Message}\n{ex.StackTrace}");
}

/// <summary>
/// Sets up logging to console and file.
/// </summary>
static void SetupLogging(out string logFilePath, out ILoggerFactory loggerFactory, out ILogger logger)
{
    string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
    Directory.CreateDirectory(logDir);
    logFilePath = Path.Combine(logDir, "bondvaluator.log");
    loggerFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Information);
    });
    logger = loggerFactory.CreateLogger("BondValuator");
}

/// <summary>
/// Helper for writing to both console and file
/// </summary>
void LogToFile(string message)
{
    var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
    File.AppendAllText(logFilePath, line + Environment.NewLine);
    Console.WriteLine(line);
}

/// <summary>
/// Function to determine the base path for configuration files.
/// </summary>
string DetermineBasePath(string logFilePath, ILogger logger)
{
    string basePath = AppContext.BaseDirectory;
    if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
    {
        basePath = Directory.GetCurrentDirectory();
        logger.LogInformation("Switched base path to development directory: {BasePath}", basePath);
        LogToFile($"Switched base path to development directory: {basePath}");
    }

    return basePath;
}

/// <summary>
/// Function to load configurations from appsettings.json.
/// </summary>
void LoadConfigurations(string logFilePath, ILogger logger, string basePath, out string inputDir, out string outputDir, out int rounding)
{
    var config = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    inputDir = config["Paths:InputDirectory"] ?? "Data/Input";
    outputDir = config["Paths:OutputDirectory"] ?? "Data/Output";
    rounding = int.Parse(config["Valuation:DefaultRounding"] ?? "2");
    logger.LogInformation("Configuration loaded successfully.");
    LogToFile("Configuration loaded successfully.");
}

/// <summary>
/// Function to build input and output file paths.
/// </summary>
void BuildInputOutputPaths(string[] args, string logFilePath, ILogger logger, string basePath, string inputDir, string outputDir, out string inputPath, out string outputFullDir, out string outputPath)
{
    string inputFile = args.Length > 0 ? args[0] : "bond_positions_sample.csv";
    inputPath = Path.Combine(basePath, inputDir, inputFile);
    outputFullDir = Path.Combine(basePath, outputDir);
    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    string outputFile = $"valuations_{timestamp}.csv";
    outputPath = Path.Combine(outputFullDir, outputFile);
    logger.LogInformation("Input file path: {Path}", inputPath);
    LogToFile($"Input file path: {inputPath}");
    logger.LogInformation("Output file path: {Path}", outputPath);
    LogToFile($"Output file path: {outputPath}");
}

/// <summary>
/// Function to validate the input CSV file.
/// </summary> 
void ValidateCsv(string logFilePath, ILogger logger, string inputPath)
{
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
}

void WriteOutputCsv(string logFilePath, ILogger logger, int rounding, string outputPath, List<(BondValuator.Core.Models.BondRecord bond, decimal pv)> results)
{
    CsvWriter.WriteResults(outputPath, results, rounding);

    logger.LogInformation("✅ Valuation complete. {Count} records processed.", results.Count);
    LogToFile($"✅ Valuation complete. {results.Count} records processed.");
    logger.LogInformation("📄 Output written to: {OutputPath}", outputPath);
    LogToFile($"📄 Output written to: {outputPath}");
}

List<BondValuator.Core.Models.BondRecord> LoadCsv(string logFilePath, ILogger logger, string inputPath)
{
    logger.LogInformation("Loading input CSV...");
    LogToFile("Loading input CSV...");
    var bonds = CsvParser.LoadBonds(inputPath);
    return bonds;
}

BondValuationService CalculatePresentValue(string logFilePath, ILogger logger, List<BondValuator.Core.Models.BondRecord> bonds)
{
    logger.LogInformation("Processing {Count} bonds...", bonds.Count);
    LogToFile($"Processing {bonds.Count} bonds...");
    var service = new BondValuationService();
    return service;
}