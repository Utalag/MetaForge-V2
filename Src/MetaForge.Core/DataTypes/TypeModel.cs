namespace MetaForge.Core.DataTypes;

/// <summary>
/// Immutable popis typu — základní stavební kámen typového modelu.
/// Kombinuje BaseType, nullable, kolekci, custom název a generické argumenty.
/// </summary>
public sealed record TypeModel
{
    /// <summary>Základní datový typ.</summary>
    public DataType BaseType { get; init; }

    /// <summary>Je typ nullable?</summary>
    public bool IsNullable { get; init; }

    /// <summary>Je typ kolekce (List, Array, IEnumerable)?</summary>
    public bool IsCollection { get; init; }

    /// <summary>Vlastní název typu (pro Entity, EnumValue, Struct, Record).</summary>
    public string? CustomTypeName { get; init; }

    /// <summary>Generické argumenty (např. List&lt;T&gt; má jeden GenericArgument T).</summary>
    public List<TypeModel> GenericArguments { get; init; } = [];

    /// <summary>Je to void (bez návratové hodnoty)?</summary>
    public bool IsVoid => BaseType == DataType.Void
                          && !IsNullable && !IsCollection
                          && GenericArguments.Count == 0;

    /// <summary>
    /// Rozlišuje sémantiku "nullable" mezi hodnotovým typem (<see cref="Nullable{T}"/>)
    /// a nullable reference type (NRT) anotací (`string?`). Výchozí je <see cref="NullabilityKind.Oblivious"/>,
    /// což znamená, že se rozdíl nesleduje (zpětně kompatibilní s <see cref="IsNullable"/>).
    /// </summary>
    public NullabilityKind Nullability { get; init; } = NullabilityKind.Oblivious;

    // === Factory metody pro často používané typy ===

    public static TypeModel Void { get; } = new() { BaseType = DataType.Void };
    public static TypeModel String { get; } = new() { BaseType = DataType.String };
    public static TypeModel Int32 { get; } = new() { BaseType = DataType.Int32 };
    public static TypeModel Bool { get; } = new() { BaseType = DataType.Bool };
    public static TypeModel Object { get; } = new() { BaseType = DataType.Object };
    public static TypeModel Decimal { get; } = new() { BaseType = DataType.Decimal };
    public static TypeModel Guid { get; } = new() { BaseType = DataType.Guid };
    public static TypeModel DateTime { get; } = new() { BaseType = DataType.DateTime };

    /// <summary>Vytvoří TypeModel s daným BaseType.</summary>
    public static TypeModel Of(DataType baseType) => new() { BaseType = baseType };

    /// <summary>Vytvoří nullable variantu tohoto typu.</summary>
    public TypeModel MakeNullable() => this with { IsNullable = true };

    /// <summary>Vytvoří variantu typu jako `Nullable&lt;T&gt;` (hodnotový typ, např. `int?`).</summary>
    public TypeModel MakeValueTypeNullable() => this with
    {
        IsNullable = true,
        Nullability = NullabilityKind.ValueTypeNullable,
    };

    /// <summary>Vytvoří variantu typu jako nullable reference type anotaci (např. `string?`).</summary>
    public TypeModel MakeReferenceNullable() => this with
    {
        IsNullable = true,
        Nullability = NullabilityKind.ReferenceAnnotated,
    };

    /// <summary>Vytvoří kolekční variantu tohoto typu.</summary>
    public TypeModel MakeCollection() => this with { IsCollection = true };

    /// <summary>Nastaví vlastní název typu.</summary>
    public TypeModel WithCustomName(string name) => this with { CustomTypeName = name };

    /// <summary>Přidá generický argument.</summary>
    public TypeModel WithGenericArg(TypeModel arg) => this with
    {
        GenericArguments = [..GenericArguments, arg]
    };
}

/// <summary>
/// Rozlišuje druh nullable sémantiky pro <see cref="TypeModel"/>.
/// Analogie k Roslyn <c>NullableAnnotation</c> — odděluje hodnotové <c>Nullable&lt;T&gt;</c>
/// od nullable reference type (NRT) anotace, aby se předešlo sémantické nejednoznačnosti.
/// </summary>
public enum NullabilityKind
{
    /// <summary>Rozdíl se nesleduje (výchozí, zpětně kompatibilní chování).</summary>
    Oblivious,

    /// <summary>Hodnotový typ obalený jako <c>Nullable&lt;T&gt;</c>, např. `int?`.</summary>
    ValueTypeNullable,

    /// <summary>Referenční typ s NRT anotací `?`, např. `string?`.</summary>
    ReferenceAnnotated,

    /// <summary>Referenční typ explicitně anotovaný jako non-nullable, např. `string` v `#nullable enable` kontextu.</summary>
    ReferenceNotAnnotated,
}
