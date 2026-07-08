# PROP-022: Observabilita — OpenTelemetry tracing a BusinessModel diff

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-020 (BusinessModel upgrade), PROP-010 (Infrastructure persistence)

---

## Cíl

Zavést pozorovatelnost (observabilitu) do BusinessModel a Translator vrstvy:
1. **OpenTelemetry tracing** — každý command, replay a patch operace vytváří span
2. **BusinessModel diff** — schopnost zobrazit rozdíl mezi dvěma stavy dokumentu

## Odůvodnění

S rostoucí komplexitou event-sourcovaného systému je kritické vidět:
- **Které commandy způsobují chyby** — tracing s attributes (CommandType, EntityId, Source, Model)
- **Latence AI enrichmentu** — jak dlouho trvá AI-2 fáze
- **Co se změnilo mezi dvěma replayi** — diff pro debugging a UI

---

## 1. OpenTelemetry tracing

### Rozsah

Každá klíčová operace vytváří span:

```csharp
// Aktivita: BusinessModel.CommandLog.Append
using var activity = ActivitySource.StartActivity("CommandLog.Append", ActivityKind.Internal);
activity?.SetTag("command.type", envelope.CommandType);
activity?.SetTag("command.source", envelope.Source.ToString());
activity?.SetTag("command.entity_id", envelope.TargetEntityId);
activity?.SetTag("command.provenance.model", envelope.Provenance.Model);

// Aktivita: BusinessModel.ReplayEngine.Replay
using var activity = ActivitySource.StartActivity("ReplayEngine.Replay");
activity?.SetTag("replay.command_count", commands.Count);
activity?.SetTag("replay.duration_ms", stopwatch.ElapsedMilliseconds);
```

### Spanové schéma

```
PatchEngine.Apply
├── CommandLog.TryAppend
│   ├── command.type = "AddEntity"
│   ├── command.source = "Chat"
│   └── command.mutation_id = "abc123"
├── Validation.Validate
│   ├── validation.errors = 0
│   └── validation.warnings = 2
└── ReplayEngine.Replay (ověření)
    ├── replay.command_count = 47
    └── replay.duration_ms = 12

Translator.EnrichAsync
├── ai.model = "llama3"
├── ai.duration_ms = 340
└── ai.confidence = 0.95
```

### Nástroje

| Nástroj | Účel |
|---------|------|
| `System.Diagnostics.ActivitySource` | Vestavěný .NET tracing (bez externí závislosti) |
| `OpenTelemetry.Exporter.Console` | Výpis do konzole pro development |
| `OpenTelemetry.Exporter.OTLP` | Export do Jaeger/Zipkin pro production |

### Výstup

| Soubor | Umístění |
|--------|----------|
| `BusinessModelActivitySource.cs` | `Src/MetaForge.BusinessModel/Telemetry/` |
| `TranslatorActivitySource.cs` | `Src/MetaForge.Translator/Telemetry/` |
| `TelemetryExtensions.cs` (DI registrace) | `Src/MetaForge.Core/Telemetry/` |

---

## 2. BusinessModel diff

### Rozsah

```csharp
public sealed record BusinessDocumentDiff
{
    public DateTimeOffset LeftTimestamp { get; init; }
    public DateTimeOffset RightTimestamp { get; init; }
    public IReadOnlyList<DiffEntry> Changes { get; init; } = [];
}

public sealed record DiffEntry
{
    public string Path { get; init; }        // "entities/Customer/attributes/Email"
    public DiffKind Kind { get; init; }       // Added, Removed, Modified
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}

public enum DiffKind { Added, Removed, Modified }

public static class BusinessDocumentDiffer
{
    /// <summary>
    /// Porovná dva stavy dokumentu a vrátí seznam změn.
    /// </summary>
    public static BusinessDocumentDiff Diff(
        BusinessAuthoringDocument left,
        BusinessAuthoringDocument right);

    /// <summary>
    /// Vrátí diff mezi dvěma indexy v CommandLog.
    /// </summary>
    public static BusinessDocumentDiff Diff(
        CommandLogStore log,
        int leftIndex,
        int rightIndex);
}
```

### Použití

```csharp
// Debugging: co se změnilo po enrichmentu?
var before = document; // před AI-2
var after = writeBackService.ApplyEnrichment(document, enrichment);
var diff = BusinessDocumentDiffer.Diff(before, after);
// diff.Changes:
//   [Modified] entities/Customer/attributes/Email.CoreDetail.Source: null → Generated
//   [Modified] entities/Customer/attributes/Email.CoreDetail.ValueObjectName: null → "EmailAddress"

// UI: zobrazit diff mezi dvěma verzemi
var diff = BusinessDocumentDiffer.Diff(logStore, version1, version2);
```

### Výstup

| Soubor | Umístění |
|--------|----------|
| `BusinessDocumentDiff.cs` | `Src/MetaForge.BusinessModel/Diff/` |
| `BusinessDocumentDiffer.cs` | `Src/MetaForge.BusinessModel/Diff/` |

---

## Odhad

| Fáze | Dny |
|------|-----|
| OpenTelemetry — ActivitySource + span definice | 0,5 dne |
| OpenTelemetry — DI registrace a export | 0,5 dne |
| BusinessModel diff — modely a differ | 1 den |
| Testy | 0,5 dne |
| **Celkem** | **2,5 dne** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-020 (BusinessModel upgrade) | 🟢 Schváleno |
| PROP-010 (Infrastructure persistence) | 🟡 Kandidát |
| `OpenTelemetry` NuGet (volitelné) | Jen pro exportéry |
