namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výraz složený z operace a operandů — stromová struktura.
/// </summary>
public sealed class ComputedExpression : Expression
{
    public override string Kind => "Computed";

    /// <summary>Sémantická operace.</summary>
    public ComputedOperation Operation { get; set; } = new("identity", "Identita");

    /// <summary>Operandy — mohou být Expression nebo listy (konstanty, reference).</summary>
    public List<Expression> Operands { get; } = new();
}
