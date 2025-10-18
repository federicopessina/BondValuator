# Use .NET 8 runtime as the base image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use .NET 8 SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["BondValuator.Console/BondValuator.Console.csproj", "BondValuator.Console/"]
RUN dotnet restore "./BondValuator.Console/BondValuator.Console.csproj"

# Copy all files
COPY . . 
WORKDIR "/src/BondValuator.Console"

# Build the project
RUN dotnet build "./BondValuator.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the project to a folder
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BondValuator.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Copy input/output folders and appsettings.json explicitly
FROM base AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Copy Data folder and appsettings.json
COPY BondValuator.Console/Data ./Data
COPY BondValuator.Console/appsettings.json ./

# Set entry point
ENTRYPOINT ["dotnet", "BondValuator.Console.dll"]
