namespace MetaForge.BusinessModel.Validation;

/// <summary>
/// Závažnost validačního problému.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>Varování — neblokující, doporučení k vylepšení.</summary>
    Warning = 0,

    /// <summary>Chyba — blokující, musí být opraveno.</summary>
    Error = 1,
}

/// <summary>
/// Jeden validační problém nalezený v BusinessAuthoringDocument.
/// </summary>
public sealed record BusinessValidationIssue
{
    /// <summary>Závažnost problému.</summary>
    public ValidationSeverity Severity { get; init; }

    /// <summary>Kód problému (např. "MISSING_NAME", "DUPLICATE_ID").</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Popis problému.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Cesta k problematickému elementu (např. "Entities[0].Attributes[2]").</summary>
    public string? Path { get; init; }

    /// <summary>ID problematického elementu.</summary>
    public string? ElementId { get; init; }

    /// <summary>Návrh na opravu.</summary>
    public string? Suggestion { get; init; }
}
