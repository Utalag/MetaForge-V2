namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Druhy statementů (příkazů) pro dispatch v rendereru.
/// </summary>
public enum StatementKind
{
    /// <summary>Složený blok — { ... }</summary>
    Block,

    /// <summary>Návratová hodnota — return X;</summary>
    Return,

    /// <summary>Podmínka — if (cond) { } else { }</summary>
    If,

    /// <summary>For cyklus — for (init; cond; inc) { }</summary>
    For,

    /// <summary>While cyklus — while (cond) { }</summary>
    While,

    /// <summary>Přiřazení — varName = value;</summary>
    Assignment,

    /// <summary>Výraz jako statement — např. volání metody</summary>
    Expression,
}
