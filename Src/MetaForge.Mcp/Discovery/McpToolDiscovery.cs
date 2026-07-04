using MetaForge.BusinessModel.Models;
using MetaForge.Mcp.Models;

namespace MetaForge.Mcp.Discovery;

/// <summary>
/// Dynamický discovery MCP toolů podle stavu dokumentu.
/// Generuje kontextově relevantní tools pro AI asistenty.
/// </summary>
public static class McpToolDiscovery
{
    /// <summary>
    /// Objeví všechny dostupné tools podle aktuálního stavu dokumentu.
    /// </summary>
    public static IReadOnlyList<McpToolDescriptor> DiscoverTools(BusinessAuthoringDocument document)
    {
        var tools = new List<McpToolDescriptor>
        {
            // Vždy dostupné
            new("add_entity", "Přidá novou business entitu do modelu.", new()
            {
                ["name"] = ("string", "Název entity (např. 'Customer')"),
                ["summary"] = ("string?", "Volitelný popis entity"),
            }),
            new("list_entities", "Vypíše všechny entity v modelu."),
            new("get_projection", "Vrátí aktuální projekci celého modelu."),
            new("add_attribute", "Přidá atribut k entitě.", new()
            {
                ["entity_id"] = ("string", "ID entity"),
                ["name"] = ("string", "Název atributu"),
                ["type"] = ("string", "Typ atributu (string, int, decimal, ...)"),
                ["is_required"] = ("bool", "Je atribut povinný?"),
            }),
        };

        // Dynamické tools podle entit
        foreach (var entity in document.Entities)
        {
            tools.Add(new($"get_entity_{entity.Id[..8]}",
                $"Zobrazí detail entity '{entity.Name}'.",
                new() { ["entity_id"] = ("string", $"ID entity ({entity.Id[..8]})") }));

            tools.Add(new($"add_attribute_to_{entity.Id[..8]}",
                $"Přidá atribut k entitě '{entity.Name}'.",
                new()
                {
                    ["name"] = ("string", "Název atributu"),
                    ["type"] = ("string", "Typ atributu"),
                }));

            // Enrichment tool — jen pokud jsou atributy bez CoreDetail
            if (entity.Attributes.Any(a => a.CoreDetail is null))
            {
                tools.Add(new($"enrich_{entity.Id[..8]}",
                    $"Spustí AI enrichment pro entitu '{entity.Name}'."));
            }
        }

        return tools.AsReadOnly();
    }

    /// <summary>
    /// Vrátí seznam toolů jako JSON-kompatibilní objekty.
    /// </summary>
    public static IReadOnlyList<object> GetToolListForJson(BusinessAuthoringDocument document)
    {
        return DiscoverTools(document).Select(t => new
        {
            name = t.Name,
            description = t.Description,
            inputSchema = t.Parameters.Count > 0 ? new
            {
                type = "object",
                properties = t.Parameters.ToDictionary(
                    p => p.Key,
                    p => new
                    {
                        type = p.Value.Type,
                        description = p.Value.Description,
                    }),
                required = t.Parameters
                    .Where(p => !p.Value.Type.EndsWith('?'))
                    .Select(p => p.Key)
                    .ToList(),
            } : null,
        }).ToList<object>();
    }
}

/// <summary>
/// Deskriptor MCP toolu s parametry.
/// </summary>
public sealed record McpToolDescriptor(
    string Name,
    string Description,
    Dictionary<string, (string Type, string Description)>? Parameters = null
);
