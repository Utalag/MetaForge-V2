namespace MetaForge.Generators.Packaging;

/// <summary>
/// Sloučí dílčí artifacty do jednoho exportního výstupu a doplní doprovodné soubory.
/// </summary>
public static class GeneratedArtifactComposer
{
    /// <summary>
    /// Sloučí artifacty a vygeneruje package manifest.
    /// </summary>
    public static GeneratedCodeArtifact Compose(IEnumerable<GeneratedCodeArtifact> artifacts)
    {
        ArgumentNullException.ThrowIfNull(artifacts);

        var materialized = artifacts.ToList();

        // Sloučí zdrojové kódy
        var combinedCode = string.Join(
            Environment.NewLine + Environment.NewLine,
            materialized
                .Select(a => a.SourceCode)
                .Where(code => !string.IsNullOrWhiteSpace(code)));

        // Sloučí diagnostiky
        var allDiagnostics = materialized
            .SelectMany(a => a.Diagnostics ?? Array.Empty<DiagnosticInfo>())
            .ToList();

        return new GeneratedCodeArtifact(
            FileName: "combined.cs",
            SourceCode: combinedCode,
            Diagnostics: allDiagnostics.Count > 0 ? allDiagnostics.AsReadOnly() : null
        );
    }
}
