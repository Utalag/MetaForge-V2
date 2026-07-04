namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Sémantická operace (např. Add, Concat, Compare).
/// </summary>
public sealed record ComputedOperation(
    string OperationId,
    string DisplayName,
    string? Description = null
);
