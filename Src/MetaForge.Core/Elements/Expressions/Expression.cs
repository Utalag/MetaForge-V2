using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Abstraktní bázová třída pro všechny výrazy v MetaForge Expression systému.
/// Inspirováno System.Linq.Expressions, ale přizpůsobeno pro doménové modelování.
/// </summary>
public abstract class Expression
{
    /// <summary>Druh výrazu — odvozeno z ExpressionKind (computed, není storage).</summary>
    public string Kind => ExpressionKind.ToString();

    /// <summary>Druh výrazu jako enum (pro rychlý dispatch bez string compare).</summary>
    public abstract ExpressionKind ExpressionKind { get; }

    /// <summary>Výsledný typ výrazu (pro typovou kontrolu).</summary>
    public TypeModel ResultType { get; init; } = TypeModel.Object;
}
