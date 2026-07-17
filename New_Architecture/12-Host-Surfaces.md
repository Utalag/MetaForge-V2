# Host Surfaces

> CLI, MCP — tenké vrstvy bez business logiky

**Aktualizace:** PROP-026 (2026-07-04) — CLI migrováno na System.CommandLine + Spectre.Console, MCP s dynamickým tool discovery, WebApi zatím neimplementováno.
**Aktualizace:** PROP-061 (2026-07-18) — CLI `list-feedback`, `dismiss-feedback` commandy (Commands/FeedbackCommands.cs). MCP `get_feedback`, `dismiss_feedback` tooly (Tools/FeedbackTools.cs).

---

## Princip

- Host surfaces volají pouze `BusinessAuthoringHostFacade`.
- Žádná business logika v host vrstvě.
- CLI používá `System.CommandLine` (deklarativní commandy) + `Spectre.Console` (formátovaný výstup).
- MCP komunikuje přes JSON-RPC (stdin/stdout) s dynamickým discovery toolů podle stavu dokumentu.

---

## CLI (MetaForge.Cli)

```
Src/MetaForge.Cli/
├── MetaForge.Cli.csproj          (System.CommandLine, Spectre.Console)
├── Program.cs                    ← kompletní DI setup + command definitions
├── appsettings.json
└── Formatting/
│   └── CliOutputFormatter.cs     ← Spectre.Console tabulky, panely, markdown
└── Commands/
    └── FeedbackCommands.cs      ← PROP-061: list-feedback, dismiss-feedback
```

### Program.cs — System.CommandLine

```csharp
// Commandy jsou definovány přímo v Program.cs (Commands/ adresář zatím prázdný).
// Každý command je samostatný RootCommand s argumenty, options a SetHandler.

var root = new RootCommand("MetaForge - AI-powered C# code generation platform");

// add-entity <name>
var addEntity = new Command("add-entity", "Přidá novou business entitu");
addEntity.AddArgument(new Argument<string>("name", "Název entity"));
addEntity.SetHandler((ctx) => {
    var id = GetFacade().AddEntity(ctx.ParseResult.GetValueForArgument<string>("name"));
    CliOutputFormatter.Success($"Entita vytvořena (ID: {id})");
});

// list-entities
var listCmd = new Command("list-entities", "Vypíše všechny entity");
listCmd.SetHandler(() => CliOutputFormatter.RenderEntityList(GetFacade().GetProjection()));

// projection [--entity <name>]
var projection = new Command("projection", "Zobrazí projekci modelu");
projection.AddOption(new Option<string>("--entity", "Detail konkrétní entity"));
projection.SetHandler((ctx) => {
    var view = GetFacade().GetProjection();
    var name = ctx.ParseResult.GetValueForOption<string>("--entity");
    if (!string.IsNullOrWhiteSpace(name)) CliOutputFormatter.RenderEntityTable(view, name);
    else { CliOutputFormatter.RenderHeader(view); CliOutputFormatter.RenderEntityList(view); }
});

// add-attribute <entity-id> <name> [--type] [--required]
var addAttr = new Command("add-attribute", "Přidá atribut k entitě");
addAttr.AddArgument(new Argument<string>("entity-id", "ID entity"));
addAttr.AddArgument(new Argument<string>("name", "Název atributu"));
addAttr.AddOption(new Option<string>("--type", () => "string", "Typ atributu"));
// ...

// delete-entity <id>
// info

root.AddCommand(addEntity); root.AddCommand(listCmd);
root.AddCommand(projection); root.AddCommand(addAttr);
// ...
return root.Invoke(args);
```

### CliOutputFormatter

```csharp
// Složka: Src/MetaForge.Cli/Formatting/

public static class CliOutputFormatter
{
    public static void RenderHeader(ProjectionView view);       // Spectre.Console Panel
    public static void RenderEntityTable(ProjectionView view, string entityName);  // Table
    public static void RenderEntityList(ProjectionView view);   // Table
    public static void Success(string message);   // [green]✅
    public static void Error(string message);     // [red]❌
    public static void Warning(string message);   // [yellow]⚠️
    public static void Info(string message);      // [blue]ℹ️
}
```

### appsettings.json

```json
{
  "MetaForge": {
    "Catalog": { "BuiltInPresetsPath": "Data/Presets", "EnableFileSystemProvider": false },
    "AI": { "Provider": "None", "Endpoint": "", "Model": "" },
    "Persistence": { "CommandLogPath": "Data/commandlog.json", "AutoSave": true, "AutoSaveIntervalSeconds": 30 },
    "Logging": { "Level": "Information", "Console": true }
  }
}
```

---

## MCP (MetaForge.Mcp) — JSON-RPC

```
Src/MetaForge.Mcp/
├── MetaForge.Mcp.csproj
├── Program.cs                    ← DI + stdin/stdout JSON-RPC loop
├── Discovery/
│   └── McpToolDiscovery.cs       ← dynamický tool discovery podle dokumentu
└── Models/
    └── JsonRpcModels.cs          ← JsonRpcRequest, JsonRpcResponse, JsonRpcError
```

### Program.cs — JSON-RPC loop

```csharp
// Hlavní smyčka: čte JSON-RPC requesty ze stdin, zpracovává, odesílá odpovědi na stdout.
// Podporované metody:
//   tools/list         → seznam dostupných toolů (včetně dynamických dle dokumentu)
//   tools/call         → volání toolu (add_entity, add_attribute, get_projection, list_entities)

static JsonRpcResponse HandleRequest(JsonRpcRequest request, BusinessAuthoringHostFacade facade)
{
    return request.Method switch
    {
        "tools/list" => new JsonRpcResponse { Result = new { tools = GetToolList() } },
        "tools/call" => HandleToolCall(request, facade),
        _ => new JsonRpcResponse { Error = new JsonRpcError { Code = -32601, Message = "Neznámá metoda" } }
    };
}
```

### McpToolDiscovery — dynamický discovery

```csharp
// Složka: Src/MetaForge.Mcp/Discovery/

public static class McpToolDiscovery
{
    public static IReadOnlyList<McpToolDescriptor> DiscoverTools(BusinessAuthoringDocument document)
    {
        var tools = new List<McpToolDescriptor>
        {
            new("add_entity", "Přidá novou business entitu"),
            new("list_entities", "Vypíše všechny entity"),
            new("get_projection", "Vrátí aktuální projekci"),
            new("add_attribute", "Přidá atribut k entitě"),
        };
        // Dynamické tools podle kontextu dokumentu
        foreach (var entity in document.Entities)
        {
            tools.Add(new($"get_entity_{entity.Id[..8]}", $"Detail entity '{entity.Name}'"));
            tools.Add(new($"add_attribute_to_{entity.Id[..8]}", $"Přidá atribut k '{entity.Name}'"));
            if (entity.Attributes.Any(a => a.CoreDetail is null))
                tools.Add(new($"enrich_{entity.Id[..8]}", $"AI enrichment pro '{entity.Name}'"));
        }
        return tools.AsReadOnly();
    }
}

public sealed record McpToolDescriptor(
    string Name, string Description,
    Dictionary<string, (string Type, string Description)>? Parameters = null
);
```

### JsonRpcModels

```csharp
// Složka: Src/MetaForge.Mcp/Models/

public sealed class JsonRpcRequest { public string? Id; public string Method = ""; public JsonElement? Params; }
public sealed class JsonRpcResponse { public string? Id; public object? Result; public JsonRpcError? Error; }
public sealed class JsonRpcError { public int Code; public string Message = ""; }
```

---

## WebApi (MetaForge.WebApi)

> **Zatím neimplementováno.** Plánováno v další fázi — Minimal API s monetizačními middlewary (PROP-026, Fáze 3a).

