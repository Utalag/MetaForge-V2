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

    // === PROP-031 rozšíření ===

    /// <summary>Switch — switch (expr) { case X: ... default: ... }</summary>
    Switch,

    /// <summary>Foreach cyklus — foreach (var item in collection) { }</summary>
    ForEach,

    /// <summary>Try-catch-finally — try { } catch (Ex) { } finally { }</summary>
    TryCatch,

    /// <summary>Using blok — using (resource) { }</summary>
    Using,

    /// <summary>Using deklarace — using var x = ...; (C# 8+)</summary>
    UsingDeclaration,

    /// <summary>Lokální funkce — void Helper() { } uvnitř metody</summary>
    LocalFunction,
}
