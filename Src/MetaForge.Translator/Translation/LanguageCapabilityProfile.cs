namespace MetaForge.Translator.Translation;

/// <summary>
/// Definuje, které C# sémantické koncepty jsou v aktuálním kontextu podporovány.
/// Slouží jako capability gate pro generátory — před generací se ověří,
/// zda model neobsahuje nepodporované C# konstrukty.
///
/// V C#-first architektuře je výchozí profil "vše podporováno" (Supported = ["*"]).
/// Budoucí rozšíření: omezení dle tier licence (např. Professional smí generovat
/// jen <c>class</c>, <c>interface</c>, <c>enum</c>; Enterprise smí vše).
/// </summary>
public sealed class LanguageCapabilityProfile
{
    /// <summary>Název profilu (např. "CSharp-Basic", "CSharp-Professional").</summary>
    public string ProfileName { get; init; } = string.Empty;

    /// <summary>
    /// Které C# koncepty jsou podporovány.
    /// Wildcard "*" znamená "všechny koncepty".
    /// </summary>
    public HashSet<string> Supported { get; init; } = [];

    /// <summary>
    /// Které C# koncepty nejsou podporovány (např. kvůli tier licenci).
    /// Generátor je přeskočí nebo použije fallback.
    /// </summary>
    public HashSet<string> Unsupported { get; init; } = [];

    /// <summary>
    /// Je koncept podporován v aktuálním profilu?
    /// Wildcard "*" v Supported znamená "všechny koncepty podporovány".
    /// </summary>
    public bool IsSupported(string concept) =>
        Supported.Contains("*") || Supported.Contains(concept);

    // === Předdefinované profily ===

    /// <summary>
    /// Výchozí C#-first profil — všechny koncepty podporovány.
    /// Používá se, pokud není aktivní tier-based licensing gate.
    /// </summary>
    public static LanguageCapabilityProfile Default() => new()
    {
        ProfileName = "CSharp-Default",
        Supported = ["*"],
    };

    /// <summary>
    /// Základní profil — jen elementární C# konstrukty.
    /// </summary>
    public static LanguageCapabilityProfile Basic() => new()
    {
        ProfileName = "CSharp-Basic",
        Supported =
        [
            "class", "interface", "enum", "struct",
            "property", "method", "parameter",
            "all_expressions",
        ],
        Unsupported =
        [
            "record", "record_struct",
            "abstract", "sealed", "static_class", "partial",
            "async", "generic", "nullable",
            "expression_body", "primary_constructor",
            "init_only", "required",
            "ref", "in", "out",
        ],
    };

    /// <summary>
    /// Professional profil — běžné C# konstrukty, bez unsafe prvků.
    /// </summary>
    public static LanguageCapabilityProfile Professional() => new()
    {
        ProfileName = "CSharp-Professional",
        Supported = ["*"],
        Unsupported =
        [
            "unsafe", "pointer", "ref_struct",
        ],
    };
}

/// <summary>
/// Úroveň podpory C# konceptu v aktuálním profilu.
/// </summary>
public enum CapabilityLevel
{
    /// <summary>Koncept je plně podporován.</summary>
    Supported,

    /// <summary>Koncept není podporován — generátor použije fallback.</summary>
    Unsupported,
}
