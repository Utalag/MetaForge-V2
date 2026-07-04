using System.Collections.Concurrent;
using System.Reflection;
using Scriban;

namespace MetaForge.Generators;

/// <summary>
/// Manager pro Scriban šablony generování kódu.
/// Načítá šablony ze souborů relativně k assembly generátoru a cachuje je.
/// Thread-safe díky ConcurrentDictionary.
/// </summary>
public sealed class TemplateManager
{
    private readonly ConcurrentDictionary<string, Template> _cachedTemplates = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _templatesBasePath;

    /// <summary>
    /// Vytvoří TemplateManager s výchozí cestou k šablonám (relativně k Generators assembly).
    /// </summary>
    public TemplateManager()
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        _templatesBasePath = Path.Combine(assemblyLocation ?? "", "Templates");
    }

    /// <summary>
    /// Vytvoří TemplateManager s vlastní cestou k šablonám.
    /// </summary>
    /// <param name="templatesBasePath">Absolutní cesta k adresáři se šablonami.</param>
    public TemplateManager(string templatesBasePath)
    {
        _templatesBasePath = templatesBasePath;
    }

    /// <summary>
    /// Načte šablonu ze souboru a cachuje ji.
    /// </summary>
    /// <param name="templateName">Název šablony (bez přípony, např. "Class", "Method").</param>
    public Template LoadTemplate(string templateName)
    {
        return _cachedTemplates.GetOrAdd(templateName, _ =>
        {
            var templatePath = Path.Combine(_templatesBasePath, $"{templateName}.scriban");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Šablona nenalezena: {templatePath}");

            var templateContent = File.ReadAllText(templatePath);
            var template = Template.Parse(templateContent);

            if (template.HasErrors)
                throw new InvalidOperationException(
                    $"Chyby při parsování šablony '{templateName}': {string.Join(", ", template.Messages)}");

            return template;
        });
    }

    /// <summary>
    /// Renderuje šablonu s daným modelem.
    /// </summary>
    /// <param name="templateName">Název šablony (bez přípony).</param>
    /// <param name="model">Model pro šablonu.</param>
    /// <returns>Vygenerovaný kód.</returns>
    public string Render(string templateName, object model)
    {
        var template = LoadTemplate(templateName);
        return template.Render(model);
    }

    /// <summary>
    /// Renderuje šablonu s dictionary modelem (podporuje nullable values).
    /// </summary>
    /// <param name="templateName">Název šablony (bez přípony).</param>
    /// <param name="model">Dictionary model pro šablonu.</param>
    /// <returns>Vygenerovaný kód.</returns>
    public string Render(string templateName, IDictionary<string, object?> model)
    {
        var template = LoadTemplate(templateName);
        return template.Render(model);
    }

    /// <summary>
    /// Vyčistí cache šablon.
    /// </summary>
    public void ClearCache()
    {
        _cachedTemplates.Clear();
    }

    /// <summary>
    /// Vrátí počet cachovaných šablon.
    /// </summary>
    public int CachedTemplateCount => _cachedTemplates.Count;

    /// <summary>
    /// Singleton instance pro sdílené použití.
    /// </summary>
    public static TemplateManager Instance { get; } = new();
}
