namespace MetaForge.Generators;

/// <summary>
/// Výstup generátoru — vygenerovaný soubor s kódem.
/// </summary>
public sealed record GeneratedCodeArtifact(
    string FileName,
    string SourceCode,
    IReadOnlyList<DiagnosticInfo>? Diagnostics = null
);
