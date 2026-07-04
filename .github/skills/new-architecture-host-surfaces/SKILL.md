---
name: new-architecture-host-surfaces
description: "Pouzij pri: praci s host surfaces — CLI, MCP, WebApi. Tenké vrstvy bez business logiky, volaji pouze BusinessAuthoringHostFacade."
---

# new-architecture-host-surfaces

Zajistit, že host surfaces (CLI, MCP, WebApi) zůstanou tenké — žádná business logika, volají pouze `BusinessAuthoringHostFacade`.

## Kdy použít

- Při práci se soubory v `Src/MetaForge.Cli/`, `Src/MetaForge.Mcp/`, `Src/MetaForge.WebApi/`
- Při přidávání nových CLI commandů, MCP tools, WebApi endpointů
- Při návrhu DTO a formátování výstupu

## Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **Host surfaces volají pouze Facade** | Žádná přímá práce s PatchEngine, CommandLogStore atd. |
| 2 | **Žádná business logika v host vrstvě** | Validace, překlad, transformace patří do Translatoru |
| 3 | **Každý host má vlastní Composition Root** | Program.cs s DI registracemi |
| 4 | **Error handling na hranici host vrstvy** | CLI → exit code, MCP → JSON-RPC error, WebApi → ErrorResponse |

## Struktura per host

### CLI (`Src/MetaForge.Cli/`)

```
├── Program.cs                    # Composition Root + dispatcher
├── Commands/
│   ├── AddEntityCommand.cs
│   ├── ProjectionCommand.cs
│   ├── TranslateCommand.cs
│   └── ExportCommand.cs
├── Formatting/
│   └── CliOutputFormatter.cs
└── appsettings.json
```

### MCP (`Src/MetaForge.Mcp/`)

```
├── Program.cs                    # Composition Root + JSON-RPC stdio server
└── Tools/
    ├── AddEntityTool.cs
    ├── GetProjectionTool.cs
    ├── TranslateTool.cs
    └── ExportTool.cs
```

### WebApi (`Src/MetaForge.WebApi/`)

```
├── Program.cs                    # Composition Root
├── Controllers/
│   ├── AuthoringController.cs
│   ├── ProjectionController.cs
│   └── ExportController.cs
├── Dtos/
│   ├── AddEntityRequest.cs
│   ├── ProjectionResponse.cs
│   └── ErrorResponse.cs
└── Middleware/
    ├── ExceptionHandlingMiddleware.cs
    └── RequestLoggingMiddleware.cs
```

## CLI Command dispatcher pattern

```csharp
var command = args[0].ToLowerInvariant();
switch (command)
{
    case "add-entity":    HandleAddEntity(facade, args); break;
    case "projection":    HandleProjection(facade); break;
    case "list-entities": HandleListEntities(facade); break;
    default:              /* error + help */ break;
}
```

## MCP JSON-RPC pattern

```csharp
// Čte requesty ze stdin, zapisuje response na stdout
while ((line = stdin.ReadLine()) is not null)
{
    var request = JsonSerializer.Deserialize<JsonRpcRequest>(line);
    var response = request.Method switch
    {
        "tools/list" => HandleListTools(),
        "tools/call" => HandleToolCall(request, facade),
        _ => ErrorResponse(-32601, "Method not found"),
    };
    stdout.WriteLine(JsonSerializer.Serialize(response));
}
```

## Anti-patterny

- ❌ Business logika v CLI command handleru
- ❌ MCP tool volající PatchEngine přímo (mimo Facade)
- ❌ WebApi controller obsahující doménovou validaci
- ❌ Sdílený kód mezi host surfaces (patří do Translatoru)

## Výstupní checklist

- [ ] Host surface volá pouze BusinessAuthoringHostFacade
- [ ] Žádná business logika v host vrstvě
- [ ] Error handling je na hranici host vrstvy
- [ ] DTO jsou oddělená od doménových modelů
