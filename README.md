# BondValuator

## Run Locally on Windows

### Run the App

1. Open **Command Prompt** or **PowerShell**.

2. Navigate to the console project folder:
```powershell
cd C:\Path\To\BondValuator\BondValuator.Console
```
3. Ensure the input file exists:
```
Data\Input\bond_positions_sample.csv
```
4. Run the console app:
```powershell
dotnet run
```
Or specify a CSV file:
```powershell
dotnet run -- bond_positions_sample.csv
```
5. Output file is generated in:
```
Data\Output\valuations_YYYYMMDD_HHMMSS.csv
```

### Run Unit Tests
1. Open **Command Prompt** or **PowerShell**.
2. Navigate to the console project folder:
```powershell
cd C:\Path\To\BondValuator\BondValuator.UnitTests
```
3. Run the tests
```
dotnet test
```
