namespace MetaForge.Core.Common;

/// <summary>
/// Typ vrstvy v Package systému (Branching).
/// </summary>
public enum LayerType
{
    /// <summary>
    /// Doménový model (kořen).
    /// </summary>
    Domain,

    /// <summary>
    /// Databázová vrstva (schémata, indexy, mapování).
    /// </summary>
    Database,

    /// <summary>
    /// Kontraktová vrstva (DTOs, requesty, responses).
    /// </summary>
    Contract,

    /// <summary>
    /// Servisní vrstva (repozitáře, business logika).
    /// </summary>
    Service,

    /// <summary>
    /// API vrstva (kontrolery, CQRS).
    /// </summary>
    Api
}
