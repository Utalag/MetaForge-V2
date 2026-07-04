namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Abstraktní bázová třída pro výrazy.
/// </summary>
public abstract class Expression
{
    /// <summary>Druh výrazu — implementuje potomek.</summary>
    public abstract string Kind { get; }
}
