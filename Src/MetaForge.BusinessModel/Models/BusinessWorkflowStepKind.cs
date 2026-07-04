namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Druh workflow kroku.
/// </summary>
public enum BusinessWorkflowStepKind
{
    /// <summary>Manuální krok — vyžaduje lidskou akci.</summary>
    Manual = 0,

    /// <summary>Automatický krok — provádí systém.</summary>
    Automatic = 1,

    /// <summary>Rozhodovací krok — větvení na základě podmínky.</summary>
    Decision = 2,

    /// <summary>Čekací krok — čeká na externí událost.</summary>
    Wait = 3,
}
