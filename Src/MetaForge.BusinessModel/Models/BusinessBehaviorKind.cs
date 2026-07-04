namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Druh chování (metody) entity.
/// Určuje sémantiku chování pro generátory a validaci.
/// </summary>
public enum BusinessBehaviorKind
{
    /// <summary>Dotaz — vrací data, nemění stav (read-only).</summary>
    Query = 0,

    /// <summary>Příkaz — mění stav, nevrací data (write-only).</summary>
    Command = 1,

    /// <summary>Pravidlo — validace / business pravidlo (vrací bool).</summary>
    Rule = 2,
}
