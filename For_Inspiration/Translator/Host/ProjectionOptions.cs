namespace MetaForge.Translator;

/// <summary>
/// Definuje, ktere sekce projekce se maji stavet.
/// Pouzij tovarni metody <see cref="Basic"/>, <see cref="Expert"/> nebo <see cref="Custom"/> pro vytvoreni instanci.
/// </summary>
public sealed class ProjectionOptions
{
    /// <summary>Zakladni projekce — jen replay, zadne expert sekce.</summary>
    public static ProjectionOptions Basic() => new();

    /// <summary>Plna expert projekce — vsechny sekce zapnuty.</summary>
    public static ProjectionOptions Expert() => new()
    {
        Diagnostics = true,
        TypeResolution = true,
        Suggestions = true,
        RelationAnalysis = true,
    };

    /// <summary>Projekce pro node-level assist — expert sekce + authoring context.</summary>
    public static ProjectionOptions NodeAssist(bool includeDiscovery = false) => new()
    {
        Diagnostics = true,
        TypeResolution = true,
        Suggestions = true,
        RelationAnalysis = true,
        AuthoringContext = true,
        DiscoveryContext = includeDiscovery,
    };

    /// <summary>Uzivatelsky volitelna kombinace expert sekci.</summary>
    public static ProjectionOptions Custom(
        bool diagnostics = false,
        bool typeResolution = false,
        bool suggestions = false,
        bool relationAnalysis = false,
        bool workflow = false,
        bool authoringContext = false,
        bool discoveryContext = false) => new()
    {
        Diagnostics = diagnostics,
        TypeResolution = typeResolution,
        Suggestions = suggestions,
        RelationAnalysis = relationAnalysis,
        Workflow = workflow,
        AuthoringContext = authoringContext,
        DiscoveryContext = discoveryContext,
    };

    /// <summary>Stavet diagnosticky souhrn (pocty poznamek, constraintu, computed, atd.).</summary>
    public bool Diagnostics { get; private init; }

    /// <summary>Stavet type resolution — catalog lookup, preset mapping, underlying types.</summary>
    public bool TypeResolution { get; private init; }

    /// <summary>Stavet navrhy presetu a strong-type kandidatu.</summary>
    public bool Suggestions { get; private init; }

    /// <summary>Stavet analyzu relaci (chybejici navigace, entity lookup).</summary>
    public bool RelationAnalysis { get; private init; }

    /// <summary>Stavet workflow projekci.</summary>
    public bool Workflow { get; private init; }

    /// <summary>Stavet authoring context projekci.</summary>
    public bool AuthoringContext { get; private init; }

    /// <summary>Stavet discovery context projekci.</summary>
    public bool DiscoveryContext { get; private init; }

    /// <summary>True pokud je zapnuta alespon jedna expert sekce.</summary>
    public bool HasAnyExpertSection => Diagnostics || TypeResolution || Suggestions || RelationAnalysis;

    /// <summary>True pokud je zapnuta alespon jedna workflow nebo context sekce.</summary>
    public bool HasAnyWorkflowOrContextSection => Workflow || AuthoringContext || DiscoveryContext;
}
