# MetaForge — Observability a Telemetry

> Definice observability modelu platformy — metriky, tracing a strukturované logování.

- **Datum:** 2026-05-14
- **Status:** Částečně implementováno (metriky — Plán 14 hotovo; tracing — Plán 22 návrh)
- **Soulad s vrstvami:** Viz 01-Layers.md — Host drží telemetry bootstrap, Facade drží use-case metriky, Core bez exporter coupling

---

## 1. Vrstvy observability

| Vrstva | Co měří | Nástroj | Stav |
|--------|---------|---------|------|
| **Metriky** | Doba trvání, počty operací, AI latency | `Meter` + `MeterListener` | ✅ Hotovo (Plán 14) |
| **Tracing** | Průchod requestu vrstvami, rozhodovací uzly | `ActivitySource` + `IExecutionTraceRecorder` | 📝 Návrh (Plán 22) |
| **Logování** | Strukturované záznamy | `ILogger` (Microsoft.ExtLogging) | ✅ Základ hotov |

---

## 2. Metriky (Plán 14)

### 2.1 Implementované metrické nástroje

| Metrika | Typ | Popis |
|---------|-----|-------|
| `metaforge.facade.operation.duration` | Histogram | Doba trvání facade operací |
| `metaforge.facade.operation.count` | Counter | Počty operací |
| `metaforge.ai.request.duration` | Histogram | Doba trvání AI requestů |
| `metaforge.ai.request.count` | Counter | Počty AI requestů |
| `metaforge.ai.fallback.count` | Counter | Počty fallbacků na deterministickou cestu |

### 2.2 Host export

- `MetaForge.ServiceDefaults` — konfigurace OTLP exportu (opt-in přes proměnné prostředí)
- `MetaForge.AppHost` — Aspire orchestrace s dashboardem
- Exportér: OpenTelemetry Protocol (OTLP)

---

## 3. Tracing (Plán 22 — návrh)

### 3.1 Dvouúrovňový režim

| Režim | Popis | Výchozí |
|-------|-------|---------|
| **Základní** | Komponenta, operace, výsledek, doba trvání — nízká kardinalita | Vždy zapnuto |
| **Detailní** | + názvy tříd, vstupy/výstupy, rozhodovací uzly, chybové cesty | Opt-in |

### 3.2 Architektura

- `IExecutionTraceRecorder` — interface v `MetaForge.Translator`
- `OtelExecutionTraceRecorder` — OTel implementace
- `ActivitySource` v `MetaForgeTelemetry`
- Export přes OTLP do Jaeger/Zipkin/Aspire Dashboard

### 3.3 Plánované komponenty

- Generátor Mermaid diagramů z trace záznamu
- ForgeBlock Observability capability pro generované projekty
- CLI tool: `metaforge trace export`

---

## 4. Tagy a pravidla

### Povolené tagy (všechny režimy)

| Tag | Význam |
|-----|--------|
| `metaforge.component` | Logické jméno komponenty |
| `metaforge.operation` | Název operace |
| `metaforge.result` | `ok`, `error`, `validation_error`, `cancelled` |
| `metaforge.decision` | Název rozhodnutí |
| `metaforge.selected_option` | Vybraná větev |
| `metaforge.fallback` | Název fallbacku |

### Zakázané tagy

- `projectName`, `streamId`, `entityId`, `attributeId`, `behaviorId`
- `correlationId`, `mutationId`
- raw user message, raw AI prompt, raw AI response

---

## 5. Vztah k Architektuře

- Core: **žádný** telemetry/exporter coupling
- Translator: `IExecutionTraceRecorder`, `MetaForgeTelemetry` (Meter + ActivitySource)
- Host (WebApi, MCP, CLI): bootstrap a konfigurace exportu
- ServiceDefaults: OTLP konfigurace
- AppHost: Aspire dashboard integrace

---

## 6. Plánovaný rozvoj

| Priorita | Feature | Plán |
|----------|---------|------|
| P1 | Metrics baseline | ✅ Plán 14 |
| P2 | Basic tracing | 📝 Plán 22 |
| P3 | Mermaid flow diagramy | 📝 Plán 22 |
| P4 | ForgeBlock Observability capability | 📝 Plán 22 |

---

## Související dokumenty

- `01-Layers.md` — vrstvy platformy
- `02-Projection-Pipeline.md` — projekční pipeline
- `09-Authoring-Kernel-and-Multi-Output-Model.md` — authoring kernel
- `Plán 14 — Platform Metrics I` — implementace metrik
- `Plán 22 — Execution Trace` — tracing návrh
