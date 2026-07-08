using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Builders;

/// <summary>
/// Výsledek fluent builder API — kompletní definice modelu s namespace a elementy.
/// </summary>
public sealed record ModelDefinition
{
    /// <summary>Namespace modelu.</summary>
    public string Namespace { get; init; } = string.Empty;

    /// <summary>Všechny top-level elementy (třídy, interfacy, enumy, structy).</summary>
    public IReadOnlyList<RootElement> RootElements { get; init; } = Array.Empty<RootElement>();
}

/// <summary>
/// Definice celého typového modelu. Entry point pro fluent builder API.
/// Vrací immutable <see cref="ModelDefinition"/> po zavolání <see cref="Build"/>.
/// </summary>
public sealed class TypeModelBuilder
{
    private readonly string _namespace;
    private readonly List<RootElement> _elements = [];

    public TypeModelBuilder(string ns) => _namespace = ns;

    /// <summary>Přidá třídu do modelu.</summary>
    public TypeModelBuilder Class(string name, Action<ClassBuilder> configure)
    {
        var builder = new ClassBuilder(name);
        configure(builder);
        _elements.Add(builder.Build());
        return this;
    }

    /// <summary>Přidá interface do modelu.</summary>
    public TypeModelBuilder Interface(string name, Action<InterfaceBuilder> configure)
    {
        var builder = new InterfaceBuilder(name);
        configure(builder);
        _elements.Add(builder.Build());
        return this;
    }

    /// <summary>Přidá enum do modelu.</summary>
    public TypeModelBuilder Enum(string name, Action<EnumBuilder> configure)
    {
        var builder = new EnumBuilder(name);
        configure(builder);
        _elements.Add(builder.Build());
        return this;
    }

    /// <summary>Přidá struct do modelu.</summary>
    public TypeModelBuilder Struct(string name, Action<StructBuilder> configure)
    {
        var builder = new StructBuilder(name);
        configure(builder);
        _elements.Add(builder.Build());
        return this;
    }

    /// <summary>Sestaví immutable definici modelu.</summary>
    public ModelDefinition Build() => new()
    {
        Namespace = _namespace,
        RootElements = _elements.AsReadOnly(),
    };
}

/// <summary>
/// Entry point pro fluent definici typového modelu.
/// </summary>
public static class TypeModelExtensions
{
    /// <summary>Začne definici modelu v daném namespace.</summary>
    public static TypeModelBuilder Define(string ns) => new(ns);
}
