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

    /// <summary>Switch statement — switch (expr) { case ... }</summary>
    Switch,

    /// <summary>ForEach cyklus — foreach (var x in collection) { }</summary>
    ForEach,

    /// <summary>Try/catch/finally — zachytávání výjimek.</summary>
    TryCatch,

    /// <summary>Using statement/deklarace — deterministické uvolnění IDisposable.</summary>
    Using,

    /// <summary>Lokální funkce definovaná uvnitř těla metody.</summary>
    LocalFunction,
}
