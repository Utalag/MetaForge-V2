# PROP-026: Host Surfaces — CLI, MCP, WebApi, REPL upgrade

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-005 (Host Surfaces base — hotovo), PROP-020 (BusinessModel upgrade)

---

## Cíl

Profesionalizovat host surfaces pro produkční nasazení:
1. **CLI** — migrace na `System.CommandLine` + `Spectre.Console`
2. **MCP** — dynamický tool discovery, proper error handling
3. **WebApi** — Minimal API endpointy (PROP-011)
4. **REPL** — interaktivní režim s našeptáváním

---

## 1. CLI upgrade (C1)

### Současný stav

```csharp
// Program.cs — switch-case dispatcher, manuální parsování args
switch (command) { case "add-entity": ... }
```

### Cílový stav — System.CommandLine + Spectre.Console

```csharp
// Program.cs — deklarativní commandy s validací a help textem
var rootCommand = new RootCommand("MetaForge — AI-powered code generation platform");

var addEntityCommand = new Command("add-entity", "Přidá novou business entitu")
{
    new Argument<string>("name", "Název entity (např. 'Customer')"),
    new Option<string>("--summary", "Popis entity"),
    new Option<string>("--preset", "ID preset/ForgeBlock šablony")
};
addEntityCommand.SetHandler(async (name, summary, preset, facade) =>
{
    var entityId = facade.AddEntity(name);
    AnsiConsole.MarkupLine($"[green]✅ Entita '{name}' vytvořena ([bold]{entityId}[/])[/]");
}, /* binding */);

var projectionCommand = new Command("projection", "Zobrazí aktuální projekci modelu");
projectionCommand.SetHandler(async (facade) =>
{
    var view = facade.GetProjection();
    RenderProjectionTable(view); // Spectre.Console Table
}, /* binding */);

rootCommand.AddCommand(addEntityCommand);
rootCommand.AddCommand(projectionCommand);
// ...
```

### Spectre.Console výstup

```
┌──────────────────────────────────────────────────────────┐
│  MetaForge CLI v1.0                                     │
├──────────────────────────────────────────────────────────┤
│  Projekt: PayrollCalculation                            │
│  Entit: 5  │  Relací: 4  │  Commandů: 47               │
└──────────────────────────────────────────────────────────┘

📋 Entity: Customer
┌──────────────┬──────────┬──────────┬──────────────┐
│ Atribut      │ Typ      │ Povinný  │ CoreDetail   │
├──────────────┼──────────┼──────────┼──────────────┤
│ Email        │ email    │ ✅       │ 🟢 Synced    │
│ FirstName    │ text     │ ✅       │ ⚪ New       │
│ GrossSalary  │ money    │ ✅       │ 🟡 Enriched  │
└──────────────┴──────────┴──────────┴──────────────┘
```

### Výstup

| Soubor | Umístění |
|--------|----------|
| `Program.cs` (přepsání) | `Src/MetaForge.Cli/` |
| `Commands/AddEntityCommand.cs` | `Src/MetaForge.Cli/Commands/` |
| `Commands/ProjectionCommand.cs` | `Src/MetaForge.Cli/Commands/` |
| `Formatting/CliOutputFormatter.cs` | `Src/MetaForge.Cli/Formatting/` |

---

## 2. MCP — dynamický tool discovery (C2)

### Současný stav

Tools jsou definované natvrdo v `GetToolList()`.

### Cílový stav

Tools se generují dynamicky podle stavu dokumentu:

```csharp
public static class McpToolDiscovery
{
    public static IReadOnlyList<McpTool> DiscoverTools(BusinessAuthoringDocument document)
    {
        var tools = new List<McpTool>
        {
            new("add_entity", "Přidá novou business entitu"),
            new("list_entities", "Vypíše všechny entity"),
            new("get_projection", "Vrátí aktuální projekci"),
        };

        // Dynamické tools podle kontextu
        foreach (var entity in document.Entities)
        {
            tools.Add(new($"add_attribute_{entity.Id}",
                $"Přidá atribut k entitě '{entity.Name}'"));

            if (entity.Attributes.Any(a => a.CoreDetail is null))
            {
                tools.Add(new($"enrich_{entity.Id}",
                    $"Spustí AI enrichment pro entitu '{entity.Name}'"));
            }
        }

        return tools.AsReadOnly();
    }
}
```

### Výstup

| Soubor | Umístění |
|--------|----------|
| Rozšíření `Program.cs` | `Src/MetaForge.Mcp/` |
| `McpToolDiscovery.cs` | `Src/MetaForge.Mcp/Tools/` |

---

## 3. WebApi — Minimal API (C3)

### Rozsah

Nový projekt `MetaForge.WebApi` — tenká vrstva nad Facade:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMetaForge(); // extension method pro DI

var app = builder.Build();

// Domain endpoints (FREE TIER)
var domainApi = app.MapGroup("/api/domain");
domainApi.MapPost("/entities", (AddEntityRequest req, BusinessAuthoringHostFacade f) =>
    Results.Ok(new { id = f.AddEntity(req.Name) }));
domainApi.MapGet("/projection", (BusinessAuthoringHostFacade f) =>
    Results.Ok(f.GetProjection()));

// Infrastructure endpoints (PAID TIER — vyžaduje API key)
var infraApi = app.MapGroup("/api/infrastructure")
    .RequireAuthorization();
infraApi.MapPost("/generate/ef-core", async (GenerateRequest req, TieredCodeGenerator gen) =>
{
    var code = await gen.GenerateInfrastructureAsync(req.Document);
    return Results.Ok(new { files = code });
});

// Sandbox endpoint (FREE — bez exportu)
app.MapPost("/api/sandbox/export", (BusinessAuthoringDocument doc) =>
    Results.Ok(new { preview = GeneratePreview(doc), exportEnabled = false }));
```

### Monetizační integrace

```csharp
// Middleware pro kontrolu licence
public sealed class LicenseValidationMiddleware
{
    public async Task InvokeAsync(HttpContext context, GeneratorLicense license)
    {
        var path = context.Request.Path.Value ?? "";
        
        if (path.StartsWith("/api/infrastructure") && license.Tier < GeneratorTier.Infrastructure)
        {
            context.Response.StatusCode = 402; // Payment Required
            await context.Response.WriteAsJsonAsync(new {
                error = "Infrastructure generování vyžaduje TIER 2+ licenci",
                upgradeUrl = "https://metaforge.io/pricing"
            });
            return;
        }

        await _next(context);
    }
}
```

### Výstup

| Soubor | Umístění |
|--------|----------|
| `MetaForge.WebApi.csproj` | `Src/MetaForge.WebApi/` |
| `Program.cs` | `Src/MetaForge.WebApi/` |
| `MetaForgeServiceExtensions.cs` | `Src/MetaForge.WebApi/` |
| `LicenseValidationMiddleware.cs` | `Src/MetaForge.WebApi/Middleware/` |

---

## 4. Interaktivní REPL (C4)

### Rozsah

```csharp
public static class MetaForgeRepl
{
    public static async Task RunAsync(BusinessAuthoringHostFacade facade)
    {
        AnsiConsole.Write(new FigletText("MetaForge").Color(Color.Blue));

        var prompt = new TextPrompt<string>("[blue]metaforge>[/] ")
            .AllowEmpty();

        while (true)
        {
            var input = AnsiConsole.Prompt(prompt);
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input is "exit" or "quit") break;

            var result = await ExecuteReplCommand(input, facade);
            AnsiConsole.MarkupLine(result);
        }
    }

    // Podpora: add-entity, add-attribute, projection, list, export, help
    // Tab-completion: našeptávání názvů entit a atributů
}
```

---

## Odhad

| Fáze | Dny |
|------|-----|
| CLI — System.CommandLine migrace | 1 den |
| CLI — Spectre.Console výstup | 1 den |
| MCP — dynamický tool discovery | 1 den |
| WebApi — Minimal API projekt | 2 dny |
| WebApi — license middleware | 0,5 dne |
| REPL — interaktivní režim | 1 den |
| Testy | 1 den |
| **Celkem** | **7,5 dne** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-005 (Host Surfaces base) | ✅ Hotovo |
| PROP-020 (BusinessModel upgrade) | 🟢 Schváleno |
| PROP-025 (Generators monetization) | 📝 Navrženo (pro license middleware) |
