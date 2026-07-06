namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Druh kontraktu metody (Design by Contract).
/// </summary>
public enum ConstraintKind
{
    /// <summary>Podmínka ověřená před provedením metody (guard clause).</summary>
    Precondition,

    /// <summary>Invariant — podmínka platná po celou dobu životnosti objektu.</summary>
    Invariant,

    /// <summary>Podmínka ověřená po provedení metody (výstupní garance).</summary>
    Postcondition
}
