# MetaForge.Translator

Překladová a orchestrační vrstva mezi **BusinessModel** (authoring dokumenty) a výstupními profily (DTO, code export, MCP tool surface, expert projekce).

## Principy

- **Determinismus první** – `DefaultBusinessTranslator` vždy funguje bez AI
- **AI jako volitelné rozšíření** – nikdy povinná závislost
- **Neutralita vůči výstupu** – Translator nesmí předpokládat, že jediný konzument je code export
- **Two-phase AI pipeline** – Conversation AI → `SemanticBrief` → Translation AI → patche
- **Zero-Fault** – nevalidní dokumenty jsou odmítnuty před překladem (`EnsureValidDocument`)
- **Observabilita** – každá operace instrumentována OpenTelemetry metrikami a tracingem

## Vrstvy

| Vrstva | Adresář | Účel |
|--------|---------|------|
| **Host/Facade** | `Host/` | `BusinessAuthoringHostFacade` – veřejné API pro CLI, MCP, desktop |
| **Konverzace** | `Conversation/` | AI konverzační engine, command write path, node assist |
| **Prompting** | `Prompting/` | AI prompt šablony, response parsing, envelope repair |
| **Překlad** | `.` (root) | Deterministic BusinessModel → DTO překlad (bez AI) |
| **Projekce** | `Host/` | Expert, workflow a authoring context projekce |
| **Telemetrie** | `Telemetry/` | OpenTelemetry metrika a tagy |
| **Tracing** | `Trace/` | Structured execution tracing |
| **Konfigurace** | `Configuration/` | Authoring konfigurace (`metaforge.authoring.json`) |
