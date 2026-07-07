namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Omezení (constraint) generického typového parametru — `where T : ...`.
/// </summary>
public sealed class GenericConstraint
{
    /// <summary>Druh omezení.</summary>
    public GenericConstraintKind Kind { get; init; }

    /// <summary>
    /// Název bázového typu nebo interfacu pro <see cref="GenericConstraintKind.BaseType"/>
    /// a <see cref="GenericConstraintKind.Interface"/> (např. "IComparable&lt;T&gt;").
    /// </summary>
    public string? TypeName { get; init; }

    /// <summary>`where T : class` — T musí být referenční typ.</summary>
    public static GenericConstraint Class() => new() { Kind = GenericConstraintKind.Class };

    /// <summary>`where T : struct` — T musí být hodnotový typ.</summary>
    public static GenericConstraint Struct() => new() { Kind = GenericConstraintKind.Struct };

    /// <summary>`where T : notnull` — T nesmí být nullable.</summary>
    public static GenericConstraint NotNull() => new() { Kind = GenericConstraintKind.NotNull };

    /// <summary>`where T : unmanaged` — T musí být unmanaged typ.</summary>
    public static GenericConstraint Unmanaged() => new() { Kind = GenericConstraintKind.Unmanaged };

    /// <summary>`where T : new()` — T musí mít bezparametrový konstruktor.</summary>
    public static GenericConstraint NewConstructor() => new() { Kind = GenericConstraintKind.NewConstructor };

    /// <summary>`where T : default` — explicitní default constraint (override v derivované metodě).</summary>
    public static GenericConstraint Default() => new() { Kind = GenericConstraintKind.Default };

    /// <summary>`where T : BaseClassName` — T musí dědit z daného typu.</summary>
    public static GenericConstraint BaseType(string typeName) =>
        new() { Kind = GenericConstraintKind.BaseType, TypeName = typeName };

    /// <summary>`where T : IInterfaceName` — T musí implementovat daný interface.</summary>
    public static GenericConstraint Interface(string typeName) =>
        new() { Kind = GenericConstraintKind.Interface, TypeName = typeName };
}

/// <summary>Druh omezení generického typového parametru.</summary>
public enum GenericConstraintKind
{
    /// <summary>`class` — referenční typ.</summary>
    Class,

    /// <summary>`struct` — hodnotový typ.</summary>
    Struct,

    /// <summary>`notnull` — nesmí být nullable.</summary>
    NotNull,

    /// <summary>`unmanaged` — unmanaged typ.</summary>
    Unmanaged,

    /// <summary>`new()` — musí mít bezparametrový konstruktor.</summary>
    NewConstructor,

    /// <summary>`default` — override default constraint.</summary>
    Default,

    /// <summary>Konkrétní bázová třída.</summary>
    BaseType,

    /// <summary>Konkrétní implementovaný interface.</summary>
    Interface,
}
