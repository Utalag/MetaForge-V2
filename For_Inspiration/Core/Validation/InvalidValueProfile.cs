namespace MetaForge.Core.Validation;

/// <summary>
/// Profil neplatných hodnot pro datový typ.
/// Deklarativní constraint kolekce — ResolveTestCases() z nich odvozuje konkrétní test cases.
/// Uživatel vyplní jen to co je relevantní, zbytek zůstane prázdný.
/// Přepsatelné přes with { ForbiddenLiterals = [..base, "extra"] }.
/// </summary>
public sealed record InvalidValueProfile
{
    /// <summary>
    /// Prázdný profil bez omezení.
    /// </summary>
    public static readonly InvalidValueProfile Empty = new();

    /// <summary>
    /// Přímé zakázané C# literály (např. "\"\"", "null", "\"notanemail\"").
    /// Každý se stane jedním test case 1:1.
    /// </summary>
    public IReadOnlyList<string> ForbiddenLiterals { get; init; } = [];

    /// <summary>
    /// Zakázané znaky. Pro každý znak se vygeneruje test case
    /// s řetězcem obsahujícím daný znak (např. ' ' → "\"hello world\"").
    /// </summary>
    public IReadOnlyList<char> ForbiddenCharacters { get; init; } = [];

    /// <summary>
    /// Zakázané vzory (regex nebo substring).
    /// Pro každý se vygeneruje test case s řetězcem odpovídajícím vzoru.
    /// </summary>
    public IReadOnlyList<string> ForbiddenPatterns { get; init; } = [];

    /// <summary>
    /// Omezení numerického rozsahu.
    /// ResolveTestCases() vygeneruje hodnoty těsně pod Min a těsně nad Max.
    /// </summary>
    public RangeConstraint? Range { get; init; }

    /// <summary>
    /// Omezení délky řetězce.
    /// ResolveTestCases() vygeneruje příliš krátký a příliš dlouhý řetězec.
    /// </summary>
    public LengthConstraint? Length { get; init; }

    /// <summary>
    /// Null/empty/whitespace omezení.
    /// Flags — ForbidNull | ForbidEmpty | ForbidWhitespace.
    /// </summary>
    public NullConstraints Nullability { get; init; } = NullConstraints.None;

    /// <summary>
    /// Odvozuje konkrétní test cases ze všech vyplněných constraints.
    /// Čistá funkce bez side effects — Scriban ji může volat opakovaně.
    /// </summary>
    public IReadOnlyList<InvalidTestCase> ResolveTestCases()
    {
        var cases = new List<InvalidTestCase>();

        ResolveNullability(cases);
        ResolveForbiddenLiterals(cases);
        ResolveForbiddenCharacters(cases);
        ResolveForbiddenPatterns(cases);
        ResolveRange(cases);
        ResolveLength(cases);

        return cases;
    }

    private void ResolveNullability(List<InvalidTestCase> cases)
    {
        if (Nullability.HasFlag(NullConstraints.ForbidNull))
            cases.Add(new("Null", "null", "Null value is forbidden."));

        if (Nullability.HasFlag(NullConstraints.ForbidEmpty))
            cases.Add(new("EmptyString", "\"\"", "Empty string is forbidden."));

        if (Nullability.HasFlag(NullConstraints.ForbidWhitespace))
            cases.Add(new("WhitespaceOnly", "\"   \"", "Whitespace-only string is forbidden."));
    }

    private void ResolveForbiddenLiterals(List<InvalidTestCase> cases)
    {
        for (var i = 0; i < ForbiddenLiterals.Count; i++)
        {
            var literal = ForbiddenLiterals[i];
            cases.Add(new($"ForbiddenLiteral_{i}", literal, $"Literal {literal} is forbidden."));
        }
    }

    private void ResolveForbiddenCharacters(List<InvalidTestCase> cases)
    {
        foreach (var ch in ForbiddenCharacters)
        {
            var escaped = ch switch
            {
                '\t' => "\\t",
                '\n' => "\\n",
                '\r' => "\\r",
                ' ' => "space",
                _ => ch.ToString()
            };

            cases.Add(new(
                $"ContainsChar_{escaped}",
                $"\"test{ch}value\"",
                $"Contains forbidden character '{escaped}'."));
        }
    }

    private void ResolveForbiddenPatterns(List<InvalidTestCase> cases)
    {
        for (var i = 0; i < ForbiddenPatterns.Count; i++)
        {
            var pattern = ForbiddenPatterns[i];
            cases.Add(new(
                $"ForbiddenPattern_{i}",
                $"\"{pattern}\"",
                $"Contains forbidden pattern '{pattern}'."));
        }
    }

    private void ResolveRange(List<InvalidTestCase> cases)
    {
        if (Range is null) return;

        var belowMin = Range.MinExclusive ? Range.Min : Range.Min - 1;
        var aboveMax = Range.MaxExclusive ? Range.Max : Range.Max + 1;

        // Formátování: int vs double
        var belowStr = belowMin == Math.Floor(belowMin) ? $"{(long)belowMin}" : $"{belowMin}";
        var aboveStr = aboveMax == Math.Floor(aboveMax) ? $"{(long)aboveMax}" : $"{aboveMax}";

        cases.Add(new(
            "BelowMin",
            belowStr,
            $"Value {belowStr} is below minimum {Range.Min}."));

        cases.Add(new(
            "AboveMax",
            aboveStr,
            $"Value {aboveStr} exceeds maximum {Range.Max}."));
    }

    private void ResolveLength(List<InvalidTestCase> cases)
    {
        if (Length is null) return;

        if (Length.MinLength.HasValue && Length.MinLength.Value > 0)
        {
            var tooShort = new string('x', Math.Max(0, Length.MinLength.Value - 1));
            cases.Add(new(
                "TooShort",
                $"\"{tooShort}\"",
                $"String length {tooShort.Length} is below minimum {Length.MinLength.Value}."));
        }

        if (Length.MaxLength.HasValue)
        {
            var tooLong = new string('x', Length.MaxLength.Value + 1);
            cases.Add(new(
                "TooLong",
                $"\"{tooLong}\"",
                $"String length {tooLong.Length} exceeds maximum {Length.MaxLength.Value}."));
        }
    }
}
