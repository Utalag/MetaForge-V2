// ---------------------------------------------------------------------------
// MetaForge.Core — DelegateElement
// Represents a C# delegate declaration.
// Vrstva: Core / Elements / Types
// 
// PROPOSAL: PROP-037 — C# Completeness
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Represents a C# delegate declaration.
/// Example: public delegate TResult MyDelegate(T1 arg1, T2 arg2);
/// </summary>
public sealed class DelegateElement : RootElement
{
    /// <inheritdoc />
    public override string Kind => "delegate";

    /// <summary>Return type of the delegate.</summary>
    public DataTypes.TypeModel ReturnType { get; set; } = DataTypes.TypeModel.Void;

    /// <summary>Delegate parameters.</summary>
    public List<ParameterElement> Parameters { get; init; } = new();

    /// <summary>Generic type parameters (e.g., "T" in delegate TResult Func&lt;T&gt;()).</summary>
    public List<string> TypeParameters { get; init; } = new();

    /// <summary>Generic type constraints.</summary>
    public List<GenericConstraint> TypeConstraints { get; init; } = new();

    /// <summary>Access modifier. Default: Public for top-level, Internal for nested.</summary>
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <inheritdoc />
    public override int TotalCoin => Coin + Parameters.Sum(p => p.Coin);

    /// <summary>Creates a basic public delegate.</summary>
    public static DelegateElement Basic(string name, DataTypes.TypeModel returnType) => new()
    {
        Name = name,
        ReturnType = returnType
    };

    /// <summary>Creates a generic delegate with type parameters.</summary>
    public static DelegateElement Generic(string name, DataTypes.TypeModel returnType, params string[] typeParameters) => new()
    {
        Name = name,
        ReturnType = returnType,
        TypeParameters = typeParameters.ToList()
    };

    // Fluent extensions
    public DelegateElement WithAccess(AccessModifier access) { AccessModifier = access; return this; }
    public DelegateElement WithParameter(ParameterElement param) { Parameters.Add(param); return this; }
    public DelegateElement WithParameters(params ParameterElement[] parameters) { Parameters.AddRange(parameters); return this; }
    public DelegateElement WithConstraint(GenericConstraint constraint) { TypeConstraints.Add(constraint); return this; }
}
