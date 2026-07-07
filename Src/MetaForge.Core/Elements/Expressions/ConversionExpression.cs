using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výraz konverze typu — reprezentuje explicitní přetypování `(decimal)price`
/// nebo bezpečnou konverzi `price as decimal?` / `price is decimal d`.
/// </summary>
public sealed class ConversionExpression : Expression
{
    public override string Kind => "Conversion";
    public override ExpressionKind ExpressionKind => ExpressionKind.Conversion;

    /// <summary>Výraz, který se konvertuje.</summary>
    public Expression Operand { get; init; }

    /// <summary>Cílový typ konverze.</summary>
    public TypeModel TargetType => ResultType;

    /// <summary>Druh konverze — cast, as, nebo checked cast.</summary>
    public ConversionKind ConversionKind { get; init; } = ConversionKind.Cast;

    /// <summary>
    /// Vytvoří konverzní výraz.
    /// </summary>
    /// <param name="operand">Výraz, který se konvertuje.</param>
    /// <param name="targetType">Cílový typ.</param>
    /// <param name="conversionKind">Druh konverze (cast/as).</param>
    public ConversionExpression(Expression operand, TypeModel targetType, ConversionKind conversionKind = ConversionKind.Cast)
    {
        Operand = operand;
        ConversionKind = conversionKind;
        ResultType = targetType;
    }
}

/// <summary>Druh typové konverze.</summary>
public enum ConversionKind
{
    /// <summary>Explicitní přetypování — `(T)expr`. Vyhodí výjimku při selhání.</summary>
    Cast,

    /// <summary>Bezpečná konverze — `expr as T`. Vrátí null při selhání (jen pro reference/nullable typy).</summary>
    As,

    /// <summary>Explicitní checked přetypování — `checked((T)expr)`.</summary>
    Checked,
}
