# IDEA-018 Execution Trace Recorder

Stav: Idea
Oblast: Infrastructure, Observability, Host
Zdroj: For_Inspiration/Architecture-Define/10-Observability-and-Telemetry.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept observability definoval dvouúrovňový tracing (basic/detailní), `IExecutionTraceRecorder`, generování Mermaid diagramů z trace a ForgeBlock Observability capability. Současná implementace (PROP-022) má pouze základní `Meter` metriky — tracing chybí.

Nápad vychází z `10-Observability-and-Telemetry.md`, kde je popsáno:
- `IExecutionTraceRecorder` — interface pro zaznamenání průchodu requestu vrstvami
- Dva režimy: basic (komponenta, operace, výsledek) a detailní (+ názvy tříd, rozhodovací uzly)
- Mermaid diagram generovaný z trace záznamu
- ForgeBlock Observability capability pro generované projekty
- CLI tool: `metaforge trace export`

## 2. Problém dnes

- PROP-022 implementoval metriky (Meter + MeterListener), ale `IExecutionTraceRecorder` neexistuje.
- Nelze vysledovat, kudy prošel request (Facade → Translator → Core → Generators).
- Ladění AI chování je obtížné — není vidět, jaká rozhodnutí AI udělala a proč.
- PROP-022 je označen jako "Hotovo", ale tracing je pouze "Návrh".

## 3. Předběžný směr řešení

- `IExecutionTraceRecorder` v Translator vrstvě
- `OtelExecutionTraceRecorder` — OTel implementace s `ActivitySource`
- Dva režimy: basic (vždy) a detailní (opt-in)
- Trace kroky: komponenta, operace, výsledek, doba trvání, rozhodovací uzly
- Volitelné generování Mermaid diagramů z trace
- CLI tool: `metaforge trace export --format mermaid|json`

Dotčené vrstvy: Translator (interface), Infrastructure (OTel), Host (export), CLI (tool).

## 4. Signál hodnoty

- Ladění a diagnostika — vidět, co se děje uvnitř platformy.
- AI decision tracing — jaké AI volby byly učiněny, proč a s jakým výsledkem.
- ForgeBlock autoři vidí, jak jejich balíček ovlivňuje výkon.
- Zapadá do produktizace — enterprise vyžaduje auditovatelnost.

## 5. Rizika a nejasnosti

- Detailní tracing může generovat obrovské objemy dat — jak škálovat?
- OQ-xxx: Jaký je poměr cena/výnos pro tracing v MVP fázi?
- OQ-xxx: Patří `IExecutionTraceRecorder` do Translator nebo do samostatného projektu?

## 6. Doporučený další krok

Follow-up k PROP-022. Měl by být plánován až po stabilizaci metrik a pokud je reálná poptávka po detailním tracingu.

Vazby: PROP-022 (Observability baseline)
