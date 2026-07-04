---
name: new-architecture-di-composition
description: "Pouzij pri: DI registraci, Composition Root, lifetime managementu — Singleton/Scoped/Transient rozhodovani, konfigurace, appsettings.json."
---

# new-architecture-di-composition

Řídit DI registrace a Composition Root napříč host projekty dle `25-DI-and-Composition-Root.md`.

## Kdy použít

- Při nastavování Program.cs v host projektech
- Při volbě lifetime (Singleton/Scoped/Transient) pro služby
- Při konfiguraci appsettings.json
- Při přidávání nové služby do DI

## Životnost služeb

| Vrstva | Lifetime | Služby |
|--------|----------|--------|
| **Core** | **Singleton** | CatalogManager, ForgeBlockRegistry, IConstraintInferencer, ICatalogProvider |
| **BusinessModel** | **Scoped** | BusinessAuthoringDocument, CommandLogStore, PatchEngine, ReplayEngine |
| **Translator** | **Scoped** | BusinessAuthoringHostFacade, ProjectionReadService, DefaultBusinessTranslator, WriteBackService |
| **CLI specific** | **Singleton** | CliOutputFormatter |
| **MCP specific** | **Singleton / Transient** | McpToolRegistry (Singleton), Tools (Transient) |
| **WebApi specific** | **Singleton / Scoped** | ExceptionHandlingMiddleware (Singleton), Controllers (Scoped) |

## Vzorový Composition Root (CLI)

```csharp
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

// Host-specific
builder.Services.AddSingleton<CliOutputFormatter>();

var app = builder.Build();
var facade = app.Services.GetRequiredService<BusinessAuthoringHostFacade>();
```

## Konfigurace (appsettings.json)

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

## Proměnné prostředí (pro citlivé údaje)

| Proměnná | Mapování |
|----------|----------|
| `MetaForge__AI__ApiKey` | API klíč pro AI providera |
| `MetaForge__AI__Endpoint` | Vlastní endpoint |

## Anti-patterny

- ❌ Scoped služba používaná v Singleton službě (captive dependency)
- ❌ BusinessModel služby jako Singleton (nesdílet stav mezi requesty)
- ❌ Host-specific služby registrované v nesprávném host projektu
- ❌ Konfigurační hodnoty hardcodované místo appsettings.json
- ❌ `AddSingleton` v extension metodách pro DI registraci — při vícenásobném volání způsobí duplicitní registrace. **Vždy používat `TryAddSingleton`/`TryAddScoped`/`TryAddTransient` v `Add*` extension metodách.** (Zjištěno 4.7.2026, PROP-028 Issue #1)

## Lessons Learned (z Code Review)

| # | Lekce | Dopad |
|---|-------|-------|
| L1 | **Extension metody pro DI musí používat `TryAdd*`** — `services.AddMetaForgeInfrastructure()` voláno dvakrát → duplicitní registrace. Použít `services.TryAddSingleton<T>()` (z `Microsoft.Extensions.DependencyInjection.Extensions`). | PROP-028 Issue #1 |
| L2 | **AI služby musí být součástí DI registrace** — `AddMetaForgeAi()` neregistruje `PromptRegistry` a `PromptEvaluationService`. Každá nová služba přidaná do projektu musí být součástí `Add*` metody. | PROP-027 Issue #3 |

## Výstupní checklist

- [ ] Lifetime je správně zvolen (Singleton/Scoped/Transient)
- [ ] Composition Root je v Program.cs host projektu
- [ ] appsettings.json existuje a obsahuje správnou strukturu
- [ ] AI konfigurace je volitelná (Provider: "None" = vypnuto)
- [ ] Žádné captive dependency
- [ ] Extension metody používají `TryAdd*` (ne holé `Add*`)
