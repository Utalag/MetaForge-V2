namespace MetaForge.Core.Validation;

/// <summary>
/// Omezení pro null/empty/whitespace hodnoty.
/// Flags — lze kombinovat: ForbidNull | ForbidEmpty.
/// </summary>
[Flags]
public enum NullConstraints
{
    /// <summary>
    /// Žádná omezení.
    /// </summary>
    None = 0,

    /// <summary>
    /// Zakázána hodnota null.
    /// </summary>
    ForbidNull = 1,

    /// <summary>
    /// Zakázán prázdný řetězec ("").
    /// </summary>
    ForbidEmpty = 2,

    /// <summary>
    /// Zakázán whitespace-only řetězec ("  ", "\t").
    /// </summary>
    ForbidWhitespace = 4,

    /// <summary>
    /// Všechna omezení najednou.
    /// </summary>
    All = ForbidNull | ForbidEmpty | ForbidWhitespace
}
