using System.Text.RegularExpressions;

namespace MetaForge.Ai.Prompts;

/// <summary>
/// Registr promptů — načítá .prompt.md soubory a umožňuje jejich správu.
/// </summary>
public sealed class PromptRegistry
{
    private readonly Dictionary<string, PromptTemplate> _prompts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Počet registrovaných promptů.</summary>
    public int Count => _prompts.Count;

    /// <summary>
    /// Načte všechny .prompt.md soubory z daného adresáře.
    /// </summary>
    /// <param name="directoryPath">Cesta k adresáři s .prompt.md soubory.</param>
    public void LoadFromDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return;

        foreach (var file in Directory.GetFiles(directoryPath, "*.prompt.md"))
        {
            try
            {
                var template = ParsePromptFile(file);
                if (template is not null)
                {
                    _prompts[template.Name] = template;
                }
            }
            catch
            {
                // Přeskočit poškozené soubory — nechceme padat
            }
        }
    }

    /// <summary>
    /// Zaregistruje prompt programově.
    /// </summary>
    public void Register(PromptTemplate template)
    {
        _prompts[template.Name] = template;
    }

    /// <summary>
    /// Vrátí prompt podle názvu.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Pokud prompt neexistuje.</exception>
    public PromptTemplate Get(string name)
    {
        if (_prompts.TryGetValue(name, out var template))
            return template;

        throw new KeyNotFoundException($"Prompt '{name}' nebyl nalezen. Dostupné prompty: {string.Join(", ", _prompts.Keys)}");
    }

    /// <summary>
    /// Pokusí se získat prompt podle názvu. Vrací null pokud neexistuje.
    /// </summary>
    public PromptTemplate? TryGet(string name)
    {
        return _prompts.TryGetValue(name, out var template) ? template : null;
    }

    /// <summary>
    /// Vrátí všechny registrované prompty.
    /// </summary>
    public IReadOnlyList<PromptTemplate> GetAll()
    {
        return _prompts.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Naparsuje .prompt.md soubor s YAML frontmatter.
    /// Očekává formát:
    /// ---
    /// version: 1
    /// model: llama3
    /// temperature: 0.3
    /// maxTokens: 500
    /// tags: [enrichment, translation]
    /// ---
    /// # System Prompt
    /// ...
    /// </summary>
    private static PromptTemplate? ParsePromptFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var name = Path.GetFileNameWithoutExtension(filePath)
            .Replace(".prompt", "", StringComparison.OrdinalIgnoreCase);

        // Rozdělit na frontmatter a tělo
        var parts = content.Split("---", 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return null;

        var frontmatter = parts[0].Trim();
        var body = parts.Length >= 2 ? parts[1].Trim() : string.Empty;

        // Parsovat frontmatter jako jednoduché key: value
        var metadata = ParseYamlFrontmatter(frontmatter);

        // Rozdělit tělo na system prompt (za # System Prompt) a user prompt template
        var (systemPrompt, userPromptTemplate) = SplitSystemUserPrompt(body);

        return new PromptTemplate
        {
            Name = name,
            Version = TryParseInt(metadata, "version", 1),
            Model = TryGetString(metadata, "model", "llama3"),
            Temperature = TryParseDouble(metadata, "temperature", 0.3),
            MaxTokens = TryParseInt(metadata, "maxTokens", 500),
            Tags = ParseTags(TryGetString(metadata, "tags", "")),
            Created = TryGetString(metadata, "created", ""),
            Author = TryGetString(metadata, "author", ""),
            SystemPrompt = systemPrompt,
            UserPromptTemplate = userPromptTemplate,
        };
    }

    private static string TryGetString(Dictionary<string, string> metadata, string key, string defaultValue)
    {
        return metadata.TryGetValue(key, out var value) ? value : defaultValue;
    }

    private static int TryParseInt(Dictionary<string, string> metadata, string key, int defaultValue)
    {
        return metadata.TryGetValue(key, out var value) && int.TryParse(value, out var parsed)
            ? parsed : defaultValue;
    }

    private static double TryParseDouble(Dictionary<string, string> metadata, string key, double defaultValue)
    {
        return metadata.TryGetValue(key, out var value) && double.TryParse(value, out var parsed)
            ? parsed : defaultValue;
    }

    /// <summary>
    /// Jednoduchý YAML frontmatter parser pro klíč-hodnota dvojice.
    /// </summary>
    private static Dictionary<string, string> ParseYamlFrontmatter(string frontmatter)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in frontmatter.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || !trimmed.Contains(':')) continue;

            var colonIndex = trimmed.IndexOf(':');
            var key = trimmed[..colonIndex].Trim();
            var value = trimmed[(colonIndex + 1)..].Trim();

            // Odstranit uvozovky
            value = value.Trim('"', '\'');
            result[key] = value;
        }
        return result;
    }

    /// <summary>
    /// Rozdělí tělo promptu na system prompt a user prompt template.
    /// Hledá nadpis "# System Prompt" a "# User Prompt Template".
    /// </summary>
    private static (string systemPrompt, string userPromptTemplate) SplitSystemUserPrompt(string body)
    {
        var systemPrompt = string.Empty;
        var userPromptTemplate = string.Empty;

        // Hledat "# System Prompt" nebo "## System Prompt"
        var systemMatch = Regex.Match(body, @"#+\s*System\s*Prompt\s*\n(.*?)(?=#+\s*|$)", RegexOptions.Singleline);
        if (systemMatch.Success)
        {
            systemPrompt = systemMatch.Groups[1].Value.Trim();
        }

        // Hledat "# User Prompt Template" nebo "## User Prompt Template"
        var userMatch = Regex.Match(body, @"#+\s*User\s*Prompt\s*Template\s*\n(.*?)(?=#+\s*|$)", RegexOptions.Singleline);
        if (userMatch.Success)
        {
            userPromptTemplate = userMatch.Groups[1].Value.Trim();
        }

        // Pokud nejsou nalezeny nadpisy, použít celé tělo jako system prompt
        if (string.IsNullOrEmpty(systemPrompt) && string.IsNullOrEmpty(userPromptTemplate))
        {
            systemPrompt = body.Trim();
        }

        return (systemPrompt, userPromptTemplate);
    }

    /// <summary>
    /// Naparsuje tagy z hodnoty jako "[enrichment, translation]" nebo "enrichment, translation".
    /// </summary>
    private static List<string> ParseTags(string tagsValue)
    {
        if (string.IsNullOrWhiteSpace(tagsValue)) return [];

        return tagsValue
            .Trim('[', ']')
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.Trim('"', '\''))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }
}
