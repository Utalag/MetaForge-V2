# Telemetrie

> OpenTelemetry, metriky, tracing a observability baseline pro MetaForge.

---

## Principy

1. **Telemetrie je oddělena od business logiky** — Core a BusinessModel neexportují telemetrii přímo.
2. **Tracing a metriky se exportují na hranici Facade** — host surfaces a Facade jsou telemetry boundary.
3. **Volitelný OTLP export** — žádná povinná závislost na Aspire nebo OpenTelemetry collectoru.
4. **Žádné PII v telemetrii** — názvy entit a atributů se neexportují jako tagy.
5. **Metriky jsou strukturované** — počítají se operace, ne data.

---

## Kde se telemetrie zapojuje

| Bod | Co se měří |
|-----|-----------|
| `BusinessAuthoringHostFacade` | latency operací (AddEntity, AddAttribute, GetProjection) |
| `DefaultBusinessTranslator` | počet překladů, hit/miss katalogu |
| `AiTranslationService` | latency AI volání, success/error rate |
| `CSharpGenerator` | počet generovaných souborů, velikost výstupu |
| CLI/MCP/WebApi vstup | počet requestů, HTTP status kódy |

---

## OpenTelemetry setup (WebApi)

```csharp
// Program.cs
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddSource("MetaForge.Translator")
               .AddSource("MetaForge.Generators")
               .AddOtlpExporter(options =>
               {
                   options.Endpoint = new Uri("http://localhost:4317");
               });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddMeter("MetaForge.Translator")
               .AddMeter("MetaForge.Generators")
               .AddOtlpExporter();
    });
```

---

## Vlastní metriky

### Translator metriky

```csharp
public class BusinessAuthoringHostFacade
{
    private readonly Counter<long> _entityAddedCounter;
    private readonly Histogram<double> _operationDuration;

    public BusinessAuthoringHostFacade(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MetaForge.Translator");
        _entityAddedCounter = meter.CreateCounter<long>("entities.added");
        _operationDuration = meter.CreateHistogram<double>("operation.duration");
    }

    public void AddEntity(string name)
    {
        var sw = Stopwatch.StartNew();
        // ... operace ...
        sw.Stop();

        _entityAddedCounter.Add(1);
        _operationDuration.Record(sw.ElapsedMilliseconds,
            new KeyValuePair<string, object?>("operation", "AddEntity"));
    }
}
```

### Přehled metrik

| Název metriky | Typ | Tagy |
|--------------|-----|------|
| `entities.added` | Counter | — |
| `entities.deleted` | Counter | — |
| `attributes.added` | Counter | — |
| `translations.performed` | Counter | `source` (catalog/ai/fallback) |
| `operation.duration` | Histogram | `operation` (AddEntity/Translate/Generate) |
| `ai.calls.total` | Counter | `provider`, `success` |
| `ai.calls.duration` | Histogram | `provider` |
| `code.generated` | Counter | `language` (csharp) |
| `code.generation.duration` | Histogram | `elementKind` (class/interface/enum) |

---

## Tracing — activity source

```csharp
public static class TranslatorActivities
{
    public static readonly ActivitySource Source = new("MetaForge.Translator");
}

public class BusinessAuthoringHostFacade
{
    public void AddEntity(string name)
    {
        using var activity = TranslatorActivities.Source.StartActivity("AddEntity");
        activity?.SetTag("entity.name", name);

        try
        {
            // ... operace ...
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

---

## Aspire host model (volitelný)

Pro hosted scénář (MetaForze běžící jako Aspire orchestrovaná aplikace):

```
Aspire AppHost
├── MetaForge.WebApi (container)
├── MetaForge.Mcp (executable)
├── OpenTelemetry Collector (sidecar)
└── Aspire Dashboard (volitelný)
```

```csharp
// AppHost/Program.cs (samostatný projekt, není součástí New_Architecture)
var builder = DistributedApplication.CreateBuilder(args);

var webapi = builder.AddProject<Projects.MetaForge_WebApi>("webapi")
    .WithOtlpExporter();

var mcp = builder.AddProject<Projects.MetaForge_Mcp>("mcp")
    .WithOtlpExporter();

builder.Build().Run();
```

---

## Telemetrie checklist

- [ ] Facade má ActivitySource pro tracing
- [ ] Facade používá IMeterFactory pro metriky
- [ ] Core a BusinessModel nemají telemetrický kód
- [ ] AI volání mají vlastní metriky (success/error, latency)
- [ ] Generátory měří počet a velikost výstupu
- [ ] OTLP export je volitelný (podmíněný konfigurací)
- [ ] Žádné PII v trace tazích ani metrikách
