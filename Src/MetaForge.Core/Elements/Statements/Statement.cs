namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Abstraktní bázová třída pro všechny statementy (příkazy).
/// Statementy reprezentují imperativní logiku v těle metod a konstruktorů —
/// přiřazení, podmínky, cykly, návratové hodnoty.
/// </summary>
public abstract class Statement
{
    /// <summary>
    /// Druh statementu — pro typově bezpečný dispatch v rendereru.
    /// </summary>
    public abstract StatementKind StatementKind { get; }
}
