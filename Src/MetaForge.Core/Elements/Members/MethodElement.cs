using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Statements;

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

    /// <summary>Generické typové parametry metody (např. `T` v `T Get&lt;T&gt;(...)`).</summary>
    public List<TypeParameterElement> TypeParameters { get; } = new();

    /// <summary>Atributy na metodě.</summary>
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Tělo metody jako AST (BlockStatement). Null pro abstraktní metody.</summary>
    public BlockStatement? Body { get; set; }

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 5;

    /// <summary>Celková cena včetně parametrů.</summary>
    public int TotalCoin => Coin + Parameters.Sum(p => p.Coin);

    // === Statické factory metody (M1-M7 matice) ===

    /// <summary>M1: public void Execute() { }</summary>
    public static MethodElement Basic(string name) => new()
    {
        Name = name,
        ReturnType = TypeModel.Void,
    };

    /// <summary>M2: public static double Calc() { }</summary>
    public static MethodElement Static(string name, TypeModel returnType) => new()
    {
        Name = name,
        ReturnType = returnType,
        IsStatic = true,
    };

    /// <summary>M3/M4/M8: public async Task Fetch() { } — async metoda.</summary>
    /// <param name="name">Název metody.</param>
    /// <param name="returnType">Návratový typ (např. TypeModel.Of(DataType.Task), Task&lt;List&lt;string&gt;&gt;).</param>
    public static MethodElement Async(string name, TypeModel returnType) => new()
    {
        Name = name,
        ReturnType = returnType,
        IsAsync = true,
    };

    /// <summary>M5: public abstract string Get(); (bez těla)</summary>
    public static MethodElement Abstract(string name, TypeModel returnType) => new()
    {
        Name = name,
        ReturnType = returnType,
        IsAbstract = true,
        Body = null,
    };

    /// <summary>M6: public virtual void OnEvent() { }</summary>
    public static MethodElement Virtual(string name, TypeModel returnType) => new()
    {
        Name = name,
        ReturnType = returnType,
        IsVirtual = true,
    };

    /// <summary>M7: public override string ToString() { }</summary>
    public static MethodElement Override(string name, TypeModel returnType) => new()
    {
        Name = name,
        ReturnType = returnType,
        IsOverride = true,
    };

    // === Fluent rozšiřovací metody ===

    /// <summary>Nastaví access modifier.</summary>
    public MethodElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }

    /// <summary>Přidá parametr.</summary>
    public MethodElement WithParameter(ParameterElement parameter)
    {
        Parameters.Add(parameter);
        return this;
    }

    /// <summary>Přidá parametry.</summary>
    public MethodElement WithParameters(params ParameterElement[] parameters)
    {
        Parameters.AddRange(parameters);
        return this;
    }

    /// <summary>Přidá generický typový parametr.</summary>
    public MethodElement WithTypeParameter(TypeParameterElement typeParameter)
    {
        TypeParameters.Add(typeParameter);
        return this;
    }

    /// <summary>Nastaví tělo metody (AST).</summary>
    public MethodElement WithBody(BlockStatement? body)
    {
        Body = body;
        return this;
    }

    /// <summary>Nastaví cenu v kreditech.</summary>
    public MethodElement WithCoin(int coin)
    {
        Coin = coin;
        return this;
    }
}
