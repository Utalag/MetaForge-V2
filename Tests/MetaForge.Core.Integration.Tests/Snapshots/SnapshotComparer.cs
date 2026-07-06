using FluentAssertions;

namespace MetaForge.Core.Integration.Tests;

/// <summary>
/// Helper pro snapshot testování generovaného kódu.
/// Porovnává vygenerovaný C# kód s uloženým .expected.cs souborem.
/// </summary>
public static class SnapshotComparer
{
    /// <summary>
    /// Cesta ke Snapshots/ složce v projektu.
    /// </summary>
    private static string GetSnapshotsDir()
    {
        // V test runneru: bin/Debug/net10.0/Snapshots/ (CopyToOutputDirectory)
        var outputDir = Path.Combine(AppContext.BaseDirectory, "Snapshots");
        if (Directory.Exists(outputDir))
            return outputDir;

        // Fallback: hledání v projektu (pro dotnet test z kořene)
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "Snapshots")))
        {
            dir = Path.GetDirectoryName(dir);
        }
        return dir is not null ? Path.Combine(dir, "Snapshots") : outputDir;
    }

    /// <summary>
    /// Ověří, že generatedCode odpovídá Snapshots/{category}/{testName}.expected.cs.
    /// Pokud soubor neexistuje → vytvoří ho (first-run).
    /// Pokud se liší → fail s diff zprávou.
    /// </summary>
    public static void Verify(string category, string testName, string generatedCode)
    {
        var snapshotDir = Path.Combine(GetSnapshotsDir(), category);
        Directory.CreateDirectory(snapshotDir);

        var filePath = Path.Combine(snapshotDir, $"{testName}.expected.cs");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, generatedCode);
            Assert.Fail($"Snapshot vytvořen: {filePath}. Zkontroluj a spusť testy znovu.");
        }

        var expected = File.ReadAllText(filePath);
        generatedCode.Should().Be(expected, $"snapshot mismatch pro {category}/{testName}");
    }

    /// <summary>
    /// Validuje syntaxi generovaného kódu přes Roslyn.
    /// </summary>
    public static void AssertValidSyntax(string generatedCode)
    {
        var isValid = SyntaxValidator.IsValid(generatedCode, out var diagnostics);
        isValid.Should().BeTrue($"syntax error:{Environment.NewLine}{diagnostics}");
    }
}
