using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výraz přístupu ke členu — reprezentuje odkaz na property, field, nebo vnořenou cestu.
/// Např. "FirstName", "Address.City", "this.Salary".
/// </summary>
public sealed class MemberAccessExpression : Expression
{
    public override string Kind => "MemberAccess";
    public override ExpressionKind ExpressionKind => ExpressionKind.MemberAccess;

    /// <summary>Cesta ke členu oddělená tečkami (např. "Customer.Address.City").</summary>
    public string MemberPath { get; init; } = string.Empty;

    /// <summary>
    /// Vytvoří výraz přístupu ke členu.
    /// </summary>
    /// <param name="memberPath">Tečková cesta ke členu.</param>
    /// <param name="resultType">Očekávaný typ členu.</param>
    public MemberAccessExpression(string memberPath, TypeModel? resultType = null)
    {
        MemberPath = memberPath;
        ResultType = resultType ?? TypeModel.Object;
    }
}
