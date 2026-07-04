namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Požadavky na standardní knihovnu pro danou operaci.
/// </summary>
public sealed record StandardLibraryRequirements(
    string OperationId,
    IReadOnlyList<string> RequiredNamespaces,
    IReadOnlyList<string>? RequiredPackages = null,
    string? CSharpExpressionTemplate = null
);
