namespace MetaForge.Core.Validation;

/// <summary>
/// Omezení numerického rozsahu pro validaci hodnot.
/// Používá se v InvalidValueProfile k automatickému generování
/// test cases pod/nad hranicí (Min-1, Max+1).
/// </summary>
/// <param name="Min">Minimální povolená hodnota (inclusive pokud MinExclusive == false).</param>
/// <param name="Max">Maximální povolená hodnota (inclusive pokud MaxExclusive == false).</param>
/// <param name="MinExclusive">True → Min samotné je neplatné (strict greater than).</param>
/// <param name="MaxExclusive">True → Max samotné je neplatné (strict less than).</param>
public sealed record RangeConstraint(
    double Min,
    double Max,
    bool MinExclusive = false,
    bool MaxExclusive = false);

/// <summary>
/// Omezení délky řetězce.
/// Používá se v InvalidValueProfile k automatickému generování
/// řetězců příliš krátkých nebo příliš dlouhých.
/// </summary>
/// <param name="MinLength">Minimální povolená délka (null = bez omezení).</param>
/// <param name="MaxLength">Maximální povolená délka (null = bez omezení).</param>
public sealed record LengthConstraint(
    int? MinLength = null,
    int? MaxLength = null);
