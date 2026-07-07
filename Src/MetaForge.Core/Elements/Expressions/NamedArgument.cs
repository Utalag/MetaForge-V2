namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Argument volání metody nebo konstruktoru — volitelně pojmenovaný.
/// Pokud <see cref="Name"/> je null, jde o poziční argument.
/// </summary>
public sealed record NamedArgument
{
    /// <summary>Název parametru (named argument), nebo null pro poziční zápis.</summary>
    public string? Name { get; init; }

    /// <summary>Hodnota argumentu.</summary>
    public Expression Value { get; init; } = default!;

    public NamedArgument() { }

    /// <summary>Vytvoří poziční argument (bez jména).</summary>
    public NamedArgument(Expression value)
    {
        Value = value;
    }

    /// <summary>Vytvoří pojmenovaný argument.</summary>
    public NamedArgument(string name, Expression value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>Implicitní konverze z Expression na poziční argument — pro pohodlné volání.</summary>
    public static implicit operator NamedArgument(Expression value) => new(value);
}
