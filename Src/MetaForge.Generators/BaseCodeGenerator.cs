using MetaForge.Core.Abstractions;

namespace MetaForge.Generators;

/// <summary>
/// Abstraktní bázová třída pro generátory kódu.
/// Poskytuje TemplateManager a pomocnou metodu RenderTemplate pro Scriban šablony.
/// </summary>
public abstract class BaseCodeGenerator
{
    /// <summary>
    /// TemplateManager pro načítání a renderování Scriban šablon.
    /// </summary>
    protected TemplateManager Templates { get; } = TemplateManager.Instance;

    /// <summary>
    /// Vygeneruje kód pro daný RootElement.
    /// </summary>
    public abstract GeneratedCodeArtifact Generate(RootElement element);

    /// <summary>
    /// Vygeneruje kód pro více elementů najednou (např. celý namespace).
    /// </summary>
    public virtual IReadOnlyList<GeneratedCodeArtifact> GenerateAll(IEnumerable<RootElement> elements)
    {
        var results = new List<GeneratedCodeArtifact>();
        foreach (var element in elements)
        {
            var artifact = Generate(element);
            results.Add(artifact);
        }
        return results.AsReadOnly();
    }

    /// <summary>
    /// Renderuje Scriban šablonu s dictionary modelem.
    /// </summary>
    /// <param name="templateName">Název šablony (bez přípony, např. "Class").</param>
    /// <param name="model">Dictionary model pro šablonu.</param>
    /// <returns>Vygenerovaný kód.</returns>
    protected string RenderTemplate(string templateName, Dictionary<string, object?> model)
    {
        return Templates.Render(templateName, model);
    }
}
