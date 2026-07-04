# DI a Composition Root

> Jak se propojují vrstvy a kde je Composition Root každého host projektu.

---

## Principy

1. **Každý host projekt má vlastní Composition Root** — CLI, MCP a WebApi mají každý svůj `Program.cs` s DI registrací.
2. **Vrstvy se registrují přes Microsoft.Extensions.DependencyInjection** — žádný custom DI container.
3. **Host surfaces registrují pouze to, co potřebují** — CLI neregistruje MCP tooling a naopak.
4. **Životnost služeb:** Facade a Translator jsou `Scoped`, Core služby jsou `Singleton`.
5. **Konfigurace přichází z appsettings.json** — každý host projekt má vlastní `appsettings.json`.

---

## DI registrace per projekt

### MetaForge.Cli — Program.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Core — Singleton
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<MethodBoundaryAnalyzer>();
builder.Services.AddSingleton<ExpressionRendererRegistry>();
builder.Services.AddSingleton<StandardLibraryTranslatorRegistry>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<ICatalogProvider, FileSystemCatalogProvider>();

// BusinessModel — Scoped
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();

// Translator — Scoped
builder.Services.AddScoped<BusinessAuthoringHostFacade>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<DefaultBusinessTranslator>();
builder.Services.AddScoped<ICommandHandler, CliCommandHandler>();

// CLI specific
builder.Services.AddSingleton<CliOutputFormatter>();

var app = builder.Build();
app.Run();
```

### MetaForge.Mcp — Program.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Core — Singleton
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<MethodBoundaryAnalyzer>();
builder.Services.AddSingleton<ExpressionRendererRegistry>();
builder.Services.AddSingleton<StandardLibraryTranslatorRegistry>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<ICatalogProvider, FileSystemCatalogProvider>();

// BusinessModel — Scoped
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();

// Translator — Scoped
builder.Services.AddScoped<BusinessAuthoringHostFacade>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<DefaultBusinessTranslator>();

// MCP specific — tool registrace
builder.Services.AddSingleton<McpToolRegistry>();
builder.Services.AddTransient<AddEntityTool>();
builder.Services.AddTransient<GetProjectionTool>();
builder.Services.AddTransient<TranslateTool>();

var app = builder.Build();
await app.RunAsync();
```

### MetaForge.WebApi — Program.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Core — Singleton
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<MethodBoundaryAnalyzer>();
builder.Services.AddSingleton<ExpressionRendererRegistry>();
builder.Services.AddSingleton<StandardLibraryTranslatorRegistry>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<ICatalogProvider, FileSystemCatalogProvider>();

// BusinessModel — Scoped
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();

// Translator — Scoped
builder.Services.AddScoped<BusinessAuthoringHostFacade>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<DefaultBusinessTranslator>();

// WebApi specific
builder.Services.AddControllers();
builder.Services.AddSingleton<ExceptionHandlingMiddleware>();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();
```

---

## Konfigurační model

### appsettings.json (sdílená struktura)

```json
{
  "MetaForge": {
    "Catalog": {
      "BuiltInPresetsPath": "Data/Presets",
      "EnableFileSystemProvider": true,
      "FileSystemCatalogPath": "Data/Catalog"
    },
    "AI": {
      "Provider": "None",
      "Endpoint": "",
      "Model": "",
      "ApiKey": ""
    },
    "Persistence": {
      "CommandLogPath": "Data/commandlog.json",
      "AutoSave": true,
      "AutoSaveIntervalSeconds": 30
    },
    "Logging": {
      "Level": "Information",
      "Console": true,
      "File": {
        "Enabled": false,
        "Path": "Logs/metaforge.log"
      }
    }
  }
}
```

### AI konfigurace — proměnné prostředí

Pro citlivé hodnoty (API key) použít proměnné prostředí:

| Proměnná | Mapování |
|----------|----------|
| `MetaForge__AI__ApiKey` | API klíč pro AI providera |
| `MetaForge__AI__Endpoint` | Vlastní endpoint (např. Azure OpenAI) |

---

## Životnost služeb — schéma

```
Singleton (po celou dobu běhu)
├── CatalogManager
├── ForgeBlockRegistry
├── CliOutputFormatter / McpToolRegistry

Scoped (pro každý request/zpracovaný command)
├── CommandLogStore
├── PatchEngine
├── ReplayEngine
├── BusinessAuthoringHostFacade
├── ProjectionReadService
├── DefaultBusinessTranslator
├── Controllers (WebApi) / Commands (CLI) / Tools (MCP)

Transient (pro každé použití)
├── AddEntityTool, GetProjectionTool — MCP
├── IPatchOperation implementace
```

---

## Závislosti NuGet balíků

| Projekt | NuGet balíky |
|---------|-------------|
| `MetaForge.Core` | Žádné |
| `MetaForge.BusinessModel` | Žádné |
| `MetaForge.Translator` | `MetaForge.Core`, `MetaForge.BusinessModel` |
| `MetaForge.Generators` | `MetaForge.Core`, `Scriban` |
| `MetaForge.Cli` | `MetaForge.Translator`, `Microsoft.Extensions.Hosting` |
| `MetaForge.Mcp` | `MetaForge.Translator`, `ModelContextProtocol` |
| `MetaForge.WebApi` | `MetaForge.Translator`, `Microsoft.AspNetCore.App` |
| `MetaForge.Ai` | `MetaForge.Translator`, `Azure.AI.OpenAI` (volitelný) |
| `ForgeBlocks.*` | `MetaForge.Core` |
