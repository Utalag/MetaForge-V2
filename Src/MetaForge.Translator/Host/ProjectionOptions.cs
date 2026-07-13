namespace MetaForge.Translator.Host;

/// <summary>
/// Volitelné sekce projekce — řídí, co ExpertProjection obsahuje.
/// </summary>
public sealed record ProjectionOptions
{
    public static ProjectionOptions Basic() => new();
    public static ProjectionOptions Full() => new()
    {
        Expert = true,
        Workflow = true,
        AuthoringContext = true,
        DiscoveryContext = true,
    };

    /// <summary>ExpertProjection s diagnostikou.</summary>
    public bool Expert { get; init; }

    /// <summary>Workflow binding stavy.</summary>
    public bool Workflow { get; init; }

    /// <summary>Authoring context pro AI.</summary>
    public bool AuthoringContext { get; init; }

    /// <summary>Discovery výsledky (ForgeBlock capability).</summary>
    public bool DiscoveryContext { get; init; }
}
