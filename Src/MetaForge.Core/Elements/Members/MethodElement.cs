using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Reprezentuje C# metodu na třídě, interfacu nebo structu.
/// </summary>
public sealed class MethodElement
{
    /// <summary>Název metody.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Návratový typ (výchozí void).</summary>
    public TypeModel ReturnType { get; set; } = TypeModel.Void;

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsStatic { get; set; }
    public bool IsAsync { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }

    /// <summary>Parametry metody.</summary>
    public List<ParameterElement> Parameters { get; } = new();

    /// <summary>Atributy na metodě.</summary>
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Tělo metody jako string (volitelné — pro codegen).</summary>
    public string? Body { get; set; }

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 5;

    /// <summary>Celková cena včetně parametrů.</summary>
    public int TotalCoin => Coin + Parameters.Sum(p => p.Coin);
}
