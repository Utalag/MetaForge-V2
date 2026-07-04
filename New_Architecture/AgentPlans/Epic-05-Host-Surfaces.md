# Epic 5 — Host Surfaces

> **Cíl:** Vytvořit tenké CLI a MCP host surfaces — žádná business logika, jen volání Facade.
> **Výstup:** `MetaForge.Cli` a `MetaForge.Mcp` projekty.
> **Závislosti:** Epic 4 (Translator/Facade).

---

## DŮLEŽITÉ: Host surfaces jsou TENKÉ

- ŽÁDNÁ business logika v CLI ani MCP.
- Pouze parsování vstupu → volání Facade metody → formátování výstupu.
- Error handling: try/catch → exit code nebo error response.

---

## TASK-5.1.1 — Založení projektu MetaForge.Cli

**Vstup:** `MetaForge.slnx`, Epic 4 dokončen.
**Výstup:** Konzolový projekt `Src/MetaForge.Cli/MetaForge.Cli.csproj`.
**Soubory:** `Src/MetaForge.Cli/MetaForge.Cli.csproj`, `MetaForge.slnx`

**Kód — `MetaForge.Cli.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Cli</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MetaForge.Translator\MetaForge.Translator.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
  </ItemGroup>
</Project>
```

**Aktualizace `MetaForge.slnx`** — přidej:

```xml
    <Project Path="Src/MetaForge.Cli/MetaForge.Cli.csproj" />
```

**Ověření:** `dotnet build Src/MetaForge.Cli/` projde.
**Riziko:** Nízké.
**Rollback:** Odeber projekt ze slnx, smaž složku.

---

## TASK-5.1.2 — CLI Program.cs s DI registration

**Vstup:** TASK-5.1.1 (projekt existuje).
**Výstup:** Soubor `Src/MetaForge.Cli/Program.cs`.
**Soubory:** `Src/MetaForge.Cli/Program.cs`

**Kód:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.Core.Catalog;
using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Core.Inference;
using MetaForge.Translator.Host;
using MetaForge.Translator.Translation;

// === Composition Root pro CLI ===
var builder = Host.CreateApplicationBuilder(args);

// Core — Singleton
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();

// BusinessModel — Scoped
builder.Services.AddScoped<BusinessAuthoringDocument>();
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();

// Translator — Scoped
builder.Services.AddScoped<IBusinessTranslator, DefaultBusinessTranslator>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<WriteBackService>();
builder.Services.AddScoped<BusinessAuthoringHostFacade>();

var app = builder.Build();

// === Jednoduchý CLI dispatcher ===
var argsList = args.ToList();
if (argsList.Count == 0)
{
    PrintHelp();
    return 0;
}

// Získej Facade z DI
var facade = app.Services.GetRequiredService<BusinessAuthoringHostFacade>();

try
{
    var command = argsList[0].ToLowerInvariant();
    switch (command)
    {
        case "add-entity":
            HandleAddEntity(facade, argsList);
            break;
        case "update-entity":
            HandleUpdateEntity(facade, argsList);
            break;
        case "delete-entity":
            HandleDeleteEntity(facade, argsList);
            break;
        case "add-attribute":
            HandleAddAttribute(facade, argsList);
            break;
        case "projection":
            HandleProjection(facade);
            break;
        case "list-entities":
            HandleListEntities(facade);
            break;
        case "help" or "--help" or "-h":
            PrintHelp();
            break;
        default:
            Console.Error.WriteLine($"Neznámý příkaz: {command}");
            PrintHelp();
            return 1;
    }

    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Chyba: {ex.Message}");
    return 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Chyba: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Neošetřená chyba: {ex.Message}");
    return 2;
}

// === Command handlers ===

static void HandleAddEntity(BusinessAuthoringHostFacade facade, List<string> args)
{
    if (args.Count < 2)
    {
        Console.Error.WriteLine("Použití: add-entity <název>");
        return;
    }
    var id = facade.AddEntity(args[1]);
    Console.WriteLine($"Entita '{args[1]}' přidána. Id: {id}");
}

static void HandleUpdateEntity(BusinessAuthoringHostFacade facade, List<string> args)
{
    if (args.Count < 3)
    {
        Console.Error.WriteLine("Použití: update-entity <id> <nový-název>");
        return;
    }
    facade.UpdateEntity(args[1], args[2]);
    Console.WriteLine($"Entita '{args[1]}' přejmenována na '{args[2]}'.");
}

static void HandleDeleteEntity(BusinessAuthoringHostFacade facade, List<string> args)
{
    if (args.Count < 2)
    {
        Console.Error.WriteLine("Použití: delete-entity <id>");
        return;
    }
    facade.DeleteEntity(args[1]);
    Console.WriteLine($"Entita '{args[1]}' smazána.");
}

static void HandleAddAttribute(BusinessAuthoringHostFacade facade, List<string> args)
{
    if (args.Count < 3)
    {
        Console.Error.WriteLine("Použití: add-attribute <entity-id> <název> [typ] [required]");
        return;
    }
    var entityId = args[1];
    var name = args[2];
    var type = args.Count > 3 ? args[3] : "string";
    var required = args.Count > 4 && bool.TryParse(args[4], out var r) && r;

    var attrId = facade.AddAttribute(entityId, name, type, required);
    Console.WriteLine($"Atribut '{name}' (typ: {type}) přidán k entitě. Id: {attrId}");
}

static void HandleProjection(BusinessAuthoringHostFacade facade)
{
    var projection = facade.GetProjection();
    Console.WriteLine($"Projekt: {projection.ProjectName}");
    Console.WriteLine($"Počet entit: {projection.Entities.Count}");
    Console.WriteLine();

    foreach (var entity in projection.Entities)
    {
        Console.WriteLine($"  Entita: {entity.Name} ({entity.Id})");
        foreach (var attr in entity.Attributes)
        {
            var req = attr.IsRequired ? " [required]" : "";
            var maxLen = attr.MaxLength.HasValue ? $" [max:{attr.MaxLength}]" : "";
            Console.WriteLine($"    - {attr.Name}: {attr.CoreType.BaseType}{req}{maxLen}");
        }
    }
}

static void HandleListEntities(BusinessAuthoringHostFacade facade)
{
    var doc = facade.GetDocument();
    Console.WriteLine($"Projekt: {doc.ProjectName}");
    Console.WriteLine($"Verze schématu: {doc.SchemaVersion}");
    Console.WriteLine($"Commandů v logu: {facade.GetCommandCount()}");
    Console.WriteLine();

    foreach (var entity in doc.Entities)
    {
        Console.WriteLine($"  [{entity.Id}] {entity.Name} ({entity.Attributes.Count} atributů)");
    }
}

static void PrintHelp()
{
    Console.WriteLine("MetaForge CLI — C#-first platforma pro modelování a generování");
    Console.WriteLine();
    Console.WriteLine("Příkazy:");
    Console.WriteLine("  add-entity <název>                    Přidá novou entitu");
    Console.WriteLine("  update-entity <id> <nový-název>       Přejmenuje entitu");
    Console.WriteLine("  delete-entity <id>                    Smaže entitu");
    Console.WriteLine("  add-attribute <entity-id> <název> [typ] [required]  Přidá atribut");
    Console.WriteLine("  projection                            Zobrazí aktuální projekci");
    Console.WriteLine("  list-entities                         Vypíše všechny entity");
    Console.WriteLine("  help                                  Tato nápověda");
}
```

**Ověření:** `dotnet build Src/MetaForge.Cli/` projde. `dotnet run --project Src/MetaForge.Cli -- help` vypíše nápovědu.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-5.2.1 — CLI appsettings.json

**Vstup:** TASK-5.1.1 (projekt existuje).
**Výstup:** Soubor `Src/MetaForge.Cli/appsettings.json`.
**Soubory:** `Src/MetaForge.Cli/appsettings.json`

**Kód:**

```json
{
  "MetaForge": {
    "Catalog": {
      "BuiltInPresetsPath": "Data/Presets",
      "EnableFileSystemProvider": false
    },
    "AI": {
      "Provider": "None",
      "Endpoint": "",
      "Model": ""
    },
    "Persistence": {
      "CommandLogPath": "Data/commandlog.json",
      "AutoSave": true,
      "AutoSaveIntervalSeconds": 30
    },
    "Logging": {
      "Level": "Information",
      "Console": true
    }
  }
}
```

**Ověření:** Soubor existuje. Je validní JSON.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-5.3.1 — Založení projektu MetaForge.Mcp

**Vstup:** `MetaForge.slnx`, Epic 4 dokončen.
**Výstup:** Konzolový projekt `Src/MetaForge.Mcp/MetaForge.Mcp.csproj` (MCP běží jako stdio server).
**Soubory:** `Src/MetaForge.Mcp/MetaForge.Mcp.csproj`, `MetaForge.slnx`

**Kód — `MetaForge.Mcp.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Mcp</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MetaForge.Translator\MetaForge.Translator.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
  </ItemGroup>
</Project>
```

**Ověření:** `dotnet build Src/MetaForge.Mcp/` projde.
**Riziko:** Nízké.
**Rollback:** Odeber projekt ze slnx, smaž složku.

---

## TASK-5.3.2 — MCP Program.cs (JSON-RPC stdio server)

**Vstup:** TASK-5.3.1 (projekt existuje).
**Výstup:** Soubor `Src/MetaForge.Mcp/Program.cs`.
**Soubory:** `Src/MetaForge.Mcp/Program.cs`

**Kód:**

```csharp
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.Core.Catalog;
using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Core.Inference;
using MetaForge.Translator.Host;
using MetaForge.Translator.Translation;

// === MCP JSON-RPC stdio server ===
// Čte JSON-RPC requesty ze stdin, zapisuje response na stdout.
// Logování jde na stderr (aby nerušilo JSON-RPC komunikaci).

var builder = Host.CreateApplicationBuilder(args);

// Core — Singleton
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();

// BusinessModel — Scoped
builder.Services.AddScoped<BusinessAuthoringDocument>();
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();

// Translator — Scoped
builder.Services.AddScoped<IBusinessTranslator, DefaultBusinessTranslator>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<WriteBackService>();
builder.Services.AddScoped<BusinessAuthoringHostFacade>();

var app = builder.Build();
var facade = app.Services.GetRequiredService<BusinessAuthoringHostFacade>();

// === Hlavní smyčka — čtení requestů ze stdin ===
var stdin = Console.In;
var stdout = Console.Out;

// Pošli inicializační zprávu
var initResponse = new JsonRpcResponse
{
    Id = null,
    Result = new { name = "metaforge-mcp", version = "1.0.0", tools = GetToolList() }
};
stdout.WriteLine(JsonSerializer.Serialize(initResponse));

// Zpracovávej requesty
string? line;
while ((line = stdin.ReadLine()) is not null)
{
    if (string.IsNullOrWhiteSpace(line)) continue;

    try
    {
        var request = JsonSerializer.Deserialize<JsonRpcRequest>(line);
        if (request is null) continue;

        var response = HandleRequest(request, facade);
        stdout.WriteLine(JsonSerializer.Serialize(response));
    }
    catch (Exception ex)
    {
        var errorResponse = new JsonRpcResponse
        {
            Id = null,
            Error = new JsonRpcError { Code = -1, Message = ex.Message }
        };
        stdout.WriteLine(JsonSerializer.Serialize(errorResponse));
    }
}

// === Tool handler ===

static JsonRpcResponse HandleRequest(JsonRpcRequest request, BusinessAuthoringHostFacade facade)
{
    return request.Method switch
    {
        "tools/list" => new JsonRpcResponse { Id = request.Id, Result = new { tools = GetToolList() } },
        "tools/call" => HandleToolCall(request, facade),
        _ => new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Code = -32601, Message = $"Neznámá metoda: {request.Method}" } }
    };
}

static JsonRpcResponse HandleToolCall(JsonRpcRequest request, BusinessAuthoringHostFacade facade)
{
    var toolName = request.Params?.GetProperty("name").GetString() ?? "";
    var args = request.Params?.GetProperty("arguments");

    try
    {
        object result = toolName switch
        {
            "add_entity" => facade.AddEntity(args?.GetProperty("name").GetString() ?? ""),
            "add_attribute" => facade.AddAttribute(
                args?.GetProperty("entity_id").GetString() ?? "",
                args?.GetProperty("name").GetString() ?? "",
                args?.GetProperty("type").GetString() ?? "string",
                args?.GetProperty("required").GetBoolean() ?? false
            ),
            "get_projection" => facade.GetProjection(),
            "list_entities" => facade.GetDocument().Entities.Select(e => new { e.Id, e.Name, AttributeCount = e.Attributes.Count }).ToList(),
            _ => throw new InvalidOperationException($"Neznámý tool: {toolName}")
        };

        return new JsonRpcResponse { Id = request.Id, Result = result };
    }
    catch (Exception ex)
    {
        return new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Code = -32000, Message = ex.Message } };
    }
}

static List<object> GetToolList() => new()
{
    new { name = "add_entity", description = "Přidá novou business entitu", inputSchema = new { type = "object", properties = new { name = new { type = "string", description = "Název entity" } }, required = new[] { "name" } } },
    new { name = "add_attribute", description = "Přidá atribut k entitě", inputSchema = new { type = "object", properties = new { entity_id = new { type = "string" }, name = new { type = "string" }, type = new { type = "string" }, required = new { type = "boolean" } }, required = new[] { "entity_id", "name" } } },
    new { name = "get_projection", description = "Vrátí aktuální projekci business modelu", inputSchema = new { type = "object", properties = new { } } },
    new { name = "list_entities", description = "Vypíše všechny entity", inputSchema = new { type = "object", properties = new { } } },
};

// === JSON-RPC modely ===

public class JsonRpcRequest
{
    public string? Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public JsonElement? Params { get; set; }
}

public class JsonRpcResponse
{
    public string? Id { get; set; }
    public object? Result { get; set; }
    public JsonRpcError? Error { get; set; }
}

public class JsonRpcError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

**Ověření:** `dotnet build Src/MetaForge.Mcp/` projde. Lze spustit — čeká na stdio vstup.
**Riziko:** Střední — MCP protokol je JSON-RPC, formát musí být přesný.
**Rollback:** Smaž soubor.

---

## Souhrn Epic 5 — Co musí existovat po dokončení

```
Src/MetaForge.Cli/
├── MetaForge.Cli.csproj
├── Program.cs
└── appsettings.json

Src/MetaForge.Mcp/
├── MetaForge.Mcp.csproj
└── Program.cs
```

**Celkem souborů:** ~5
**Build:** Oba projekty buildí.
**CLI:** `dotnet run --project Src/MetaForge.Cli -- add-entity "Test"` funguje.
**MCP:** `dotnet run --project Src/MetaForge.Mcp` spustí stdio server.

**Checkpoint:** `git tag checkpoint/epic-5-done`
