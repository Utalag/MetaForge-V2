namespace MetaForge.FrameworkMetadata.AspNetCore;

/// <summary>
/// Metadata pro middleware zaregistrovaný v ASP.NET Core request pipeline
/// (`app.UseMiddleware&lt;T&gt;()` nebo `app.Use(...)`).
/// </summary>
public sealed record MiddlewareMetadata
{
    /// <summary>Název middleware typu nebo inline delegáta.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Pořadí v pipeline (0 = první).</summary>
    public int Order { get; init; }

    /// <summary>Volitelné parametry konfigurace middleware.</summary>
    public List<string> ConfigurationArguments { get; init; } = new();
}
