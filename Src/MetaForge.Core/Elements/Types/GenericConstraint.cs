namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Model pro C# generic type constraint (<c>where T : class, new(), IComparable&lt;T&gt;</c>).
/// Používá se na type elementech (Class, Interface, Struct) a na MethodElement.
/// </summary>
public sealed class GenericConstraint
{
    /// <summary>Název typového parametru, kterého se omezení týká.</summary>
    public string TypeParameterName { get; init; } = string.Empty;

    /// <summary>Seznam druhů omezení. Např. ["class", "new()", "IComparable&lt;T&gt;", "BaseType"].</summary>
    public List<ConstraintKind> Constraints { get; init; } = [];

    /// <summary>Název bázové třídy, pokud jde o constraint na bázový typ (volitelné).</summary>
    public string? BaseTypeName { get; init; }

    /// <summary>Názvy interfaců, pokud jsou constrainty na interfacy.</summary>
    public List<string> InterfaceNames { get; init; } = [];

    /// <summary>Bezparametrový konstruktor pro deserializaci.</summary>
    public GenericConstraint() { }

    /// <summary>Vytvoří constraint "where T : class".</summary>
    public static GenericConstraint Class(string typeParam) => new()
    {
        TypeParameterName = typeParam,
        Constraints = { ConstraintKind.Class },
    };

    /// <summary>Vytvoří constraint "where T : struct".</summary>
    public static GenericConstraint Struct(string typeParam) => new()
    {
        TypeParameterName = typeParam,
        Constraints = { ConstraintKind.Struct },
    };

    /// <summary>Vytvoří constraint "where T : new()".</summary>
    public static GenericConstraint ParameterlessCtor(string typeParam) => new()
    {
        TypeParameterName = typeParam,
        Constraints = { ConstraintKind.ParameterlessCtor },
    };

    /// <summary>Vytvoří constraint "where T : notnull".</summary>
    public static GenericConstraint NotNull(string typeParam) => new()
    {
        TypeParameterName = typeParam,
        Constraints = { ConstraintKind.NotNull },
    };

    /// <summary>Vytvoří constraint "where T : unmanaged".</summary>
    public static GenericConstraint Unmanaged(string typeParam) => new()
    {
        TypeParameterName = typeParam,
        Constraints = { ConstraintKind.Unmanaged },
    };

    /// <summary>Vytvoří constraint "where T : BaseType".</summary>
    public static GenericConstraint BaseType(string typeParam, string baseType) => new()
    {
        TypeParameterName = typeParam,
        Constraints = { ConstraintKind.BaseType },
        BaseTypeName = baseType,
    };

    /// <summary>Vytvoří constraint "where T : IInterface".</summary>
    public static GenericConstraint Interface(string typeParam, params string[] interfaces) => new()
    {
        TypeParameterName = typeParam,
        Constraints = { ConstraintKind.Interface },
        InterfaceNames = interfaces.ToList(),
    };

    /// <summary>Vytvoří kombinovaný constraint "where T : class, new()".</summary>
    public static GenericConstraint ClassWithCtor(string typeParam) => new()
    {
        TypeParameterName = typeParam,
        Constraints = { ConstraintKind.Class, ConstraintKind.ParameterlessCtor },
    };
}

/// <summary>
/// Druhy C# generic constraintů podporovaných v Core modelu.
/// </summary>
public enum ConstraintKind
{
    /// <summary><c>where T : class</c> (referenční typ).</summary>
    Class,

    /// <summary><c>where T : struct</c> (hodnotový typ).</summary>
    Struct,

    /// <summary><c>where T : new()</c> (bezparametrový konstruktor).</summary>
    ParameterlessCtor,

    /// <summary><c>where T : notnull</c> (non-nullable typ).</summary>
    NotNull,

    /// <summary><c>where T : unmanaged</c> (unmanaged typ).</summary>
    Unmanaged,

    /// <summary><c>where T : BaseClass</c> (dědičnost z bázové třídy).</summary>
    BaseType,

    /// <summary><c>where T : IInterface</c> (implementace interfacu).</summary>
    Interface,
}
