using MetaForge.BusinessModel.Models;
using MetaForge.Generators.Monetization;

namespace MetaForge.Generators;

/// <summary>
/// Generátor projektové struktury — vytváří adresáře, .csproj a solution soubory.
/// Respektuje licenční tier — různé vrstvy pro různé tiery.
/// </summary>
public sealed class ProjectScaffoldGenerator
{
    private readonly GeneratorLicense _license;

    /// <summary>
    /// Vytvoří scaffold generátor s licencí.
    /// </summary>
    public ProjectScaffoldGenerator(GeneratorLicense license)
    {
        _license = license;
    }

    /// <summary>
    /// Vygeneruje kompletní projektovou strukturu z business dokumentu.
    /// </summary>
    /// <param name="document">Business dokument s entitami.</param>
    /// <param name="outputDirectory">Kořenový výstupní adresář.</param>
    /// <returns>Seznam vytvořených cest.</returns>
    public async Task<IReadOnlyList<string>> ScaffoldAsync(
        BusinessAuthoringDocument document,
        string outputDirectory)
    {
        if (!_license.AllowExport)
            throw LicenseException.TierTooLow(GeneratorTier.Domain, _license.Tier);

        var createdPaths = new List<string>();
        var projectName = ToSafeProjectName(document.ProjectName);

        // 1. Základní adresářová struktura
        var srcDir = Path.Combine(outputDirectory, "src", projectName);
        var domainDir = Path.Combine(srcDir, "Domain");
        var testsDir = Path.Combine(outputDirectory, "tests", $"{projectName}.Tests");

        Directory.CreateDirectory(domainDir);
        createdPaths.Add(domainDir);

        Directory.CreateDirectory(testsDir);
        createdPaths.Add(testsDir);

        // 2. Domain .csproj (vždy — Tier 1+)
        var domainCsproj = GenerateDomainCsproj(projectName);
        var domainCsprojPath = Path.Combine(srcDir, $"{projectName}.csproj");
        await File.WriteAllTextAsync(domainCsprojPath, domainCsproj);
        createdPaths.Add(domainCsprojPath);

        // 3. Infrastructure vrstva (Tier 2+)
        if (_license.Tier >= GeneratorTier.Infrastructure)
        {
            var infraDir = Path.Combine(srcDir, "Infrastructure");
            Directory.CreateDirectory(infraDir);
            createdPaths.Add(infraDir);

            var infraCsproj = GenerateInfrastructureCsproj(projectName);
            var infraCsprojPath = Path.Combine(infraDir, $"{projectName}.Infrastructure.csproj");
            await File.WriteAllTextAsync(infraCsprojPath, infraCsproj);
            createdPaths.Add(infraCsprojPath);

            // API vrstva
            var apiDir = Path.Combine(srcDir, "Api");
            Directory.CreateDirectory(apiDir);
            createdPaths.Add(apiDir);
        }

        // 4. Solution soubor (Tier 2+)
        if (_license.Tier >= GeneratorTier.Infrastructure)
        {
            var slnPath = Path.Combine(outputDirectory, $"{projectName}.slnx");
            var slnContent = GenerateSolutionFile(projectName);
            await File.WriteAllTextAsync(slnPath, slnContent);
            createdPaths.Add(slnPath);
        }

        // 5. Test .csproj (Tier 1+)
        var testCsproj = GenerateTestCsproj(projectName);
        var testCsprojPath = Path.Combine(testsDir, $"{projectName}.Tests.csproj");
        await File.WriteAllTextAsync(testCsprojPath, testCsproj);
        createdPaths.Add(testCsprojPath);

        // 6. GlobalUsings.cs (Tier 1+)
        var globalUsings = GenerateGlobalUsings();
        var globalUsingsPath = Path.Combine(srcDir, "GlobalUsings.cs");
        await File.WriteAllTextAsync(globalUsingsPath, globalUsings);
        createdPaths.Add(globalUsingsPath);

        return createdPaths.AsReadOnly();
    }

    /// <summary>
    /// Převede název projektu na bezpečný identifikátor.
    /// </summary>
    private static string ToSafeProjectName(string name)
    {
        var safe = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.').ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "GeneratedProject" : safe;
    }

    private static string GenerateDomainCsproj(string projectName) => $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <RootNamespace>{projectName}</RootNamespace>
          </PropertyGroup>
        </Project>
        """;

    private static string GenerateInfrastructureCsproj(string projectName) => $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <RootNamespace>{projectName}.Infrastructure</RootNamespace>
          </PropertyGroup>
          <ItemGroup>
            <ProjectReference Include="..\{projectName}.csproj" />
          </ItemGroup>
        </Project>
        """;

    private static string GenerateTestCsproj(string projectName) => $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <RootNamespace>{projectName}.Tests</RootNamespace>
          </PropertyGroup>
          <ItemGroup>
            <ProjectReference Include="..\src\{projectName}\{projectName}.csproj" />
          </ItemGroup>
          <ItemGroup>
            <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
            <PackageReference Include="xunit" Version="2.9.3" />
            <PackageReference Include="FluentAssertions" Version="8.2.0" />
          </ItemGroup>
        </Project>
        """;

    private static string GenerateSolutionFile(string projectName) => $"""
        <Solution>
          <Folder Name="/src/">
            <Project Path="src/{projectName}/{projectName}.csproj" />
            <Project Path="src/{projectName}/Infrastructure/{projectName}.Infrastructure.csproj" />
          </Folder>
          <Folder Name="/tests/">
            <Project Path="tests/{projectName}.Tests/{projectName}.Tests.csproj" />
          </Folder>
        </Solution>
        """;

    private static string GenerateGlobalUsings() => """
        // Auto-generated by MetaForge
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;
        global using System.Threading.Tasks;
        """;
}
