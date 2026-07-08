# IDEA-016 Output Readiness Model

Stav: Idea
Oblast: Core, Workflow, Generators, Host
Zdroj: For_Inspiration/Architecture-Define/09-Authoring-Kernel-and-Multi-Output-Model.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept definuje readiness model pro výstupy: `Draft → Enriched → ExportReady`. Každý výstup (codegen, workflow export, capability surface) má vlastní readiness stav. Současná implementace nemá žádný koncept "je model ready na export".

Nápad vychází z `09-Authoring-Kernel-and-Multi-Output-Model.md` a `TentativePlan.md`, kde je readiness model klíčovým prvkem authoring kernelu.

## 2. Problém dnes

- Model může být v jakémkoliv stavu — neexistuje kontrola, zda je "hotový" pro export.
- Chybí `OutputReadinessView` jako projekční sekce.
- Nelze říct: "Tento model je ready na codegen, ale ne na workflow export."
- Validační brány neexistují — CoreValidace je základní, ale nerozlišuje per output type.
- Zero-Fault export princip nelze vynutit bez readiness modelu.

## 3. Předběžný směr řešení

- `OutputReadiness` enum: `Draft`, `Enriched`, `ExportReady`
- `OutputReadinessView` — readiness per output type (codegen, workflow, capability)
- Per-output validace: codegen readiness (všechny atributy resolvnuté, CoreDetail přítomno), workflow readiness (bindingy hotové)
- `IOutputReadinessEvaluator` — strategie per output type
- Integrace do `ProjectionView` jako volitelná sekce

Dotčené vrstvy: Core (readiness evaluace), Translator (projekce), Host (zobrazení readiness).

## 4. Signál hodnoty

- Uživatel ví, v jakém stavu je jeho model a co chybí k exportu.
- Zero-Fault export: invalidní model se nikdy neexportuje.
- Otevírá cestu k monetizaci (exportní readyness → credit gate).
- Zapadá do authoring kernel interpretace — model není jen "kód nebo nic".

## 5. Rizika a nejasnosti

- Readiness per output může být komplexní — kolik output typů je reálných?
- Jak oddělit "uživatel záměrně nechal model v Draft" od "model není hotový"?
- OQ-xxx: Kdo definuje readiness kritéria? Core, Translator, nebo per-host?

## 6. Doporučený další krok

Follow-up k `TentativePlan` — Output Readiness je A5 v doporučeném pořadí. Měl by být plánován až po stabilizaci workflow a write-back modelu.

Vazby: PROP-020 (BusinessModel), PROP-035 (C#-first), TentativePlan (A5), IDEA-015 (Workflow)
