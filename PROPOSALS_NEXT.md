# PROPOSALS_NEXT — Zásobník kandidátních návrhů

> Návrhy, které jsou identifikované, ale zatím neschválené k implementaci.
> Nikdy neimplementovat přímo z tohoto souboru — vždy přesunout do PROPOSALS.md.
> plány jsou uloženy v Docs/Plans/PROP-XXX-*.md a jsou součástí návrhu.

## Kandidátní návrhy

| ID | Název | Vrstva | Priorita | Odhad | Poznámka |
|----|-------|--------|----------|-------|----------|
| PROP-010 | Infrastructure — persistence CommandLogu | Infrastr. | 🟡 Vysoká | — | ⚠️ Sloučeno do PROP-028 |
| PROP-011 | WebApi host surface | Host | 🟢 Nízká | — | ⚠️ Sloučeno do PROP-026 |
| PROP-012 | Payload escaping — JSON místo pipe-delimited | BusinessModel | 🟡 Vysoká | — | ⚠️ Řešeno v rámci PROP-020 |
| PROP-013 | Integrační testy celé pipeline | Tests | 🟢 Nízká | 2-3 dny | CLI → Facade → Patch → Log → Replay → Projection |
| PROP-014 | AI test project | AI | 🟢 Nízká | 2 dny | MetaForge.Ai.Tests s mockovaným HttpClient |
| PROP-015 | ForgeBlock → CatalogManager propojení | Core | 🟡 Vysoká | — | ⚠️ Sloučeno do PROP-029 |
| PROP-016 | Ollama konfigurace přes DI/IOptions | AI | 🟢 Nízká | — | ⚠️ Sloučeno do PROP-027 |
| PROP-017 | Generators — ForgeBlock packaging a katalog | Generators | 🟢 Nízká | — | ⚠️ Sloučeno do PROP-029 |
| PROP-019 | Translator — IAiTranslator a AI-assisted překlad | Translator | 🟡 Vysoká | 2,25 dne | AI enrichment atributů. ⚠️ Závisí na PROP-020 |
| PROP-021 | Testování — Property-based (FsCheck) a Snapshot (Verify) | Tests | 🟢 Nízká | 1,75 dne | ⚠️ Závisí na PROP-020 |
| PROP-022 | Observabilita — OpenTelemetry tracing a BusinessModel diff | Infrastr. | 🟢 Nízká | 2,5 dne | ⚠️ Závisí na PROP-020 |
| PROP-023 | DX a architektonická vylepšení (na zvážení) | Průřezové | ⚪ Na zvážení | 5-9 dní | Typový SyncState, Layer stack, YAML DSL, Undo/redo |
| PROP-024 | Core — StrongType, Expression, Record elementy | Core | 🟡 Vysoká | 6 dní | ValueObject, Expression hierarchy, Record support, Source Gen |
| PROP-025 | Generators — Incremental, partial class, scaffolding + Monetization | Generators | 🔴 Kritická | 8 dní | Tier model, sandbox, project scaffolding, license middleware |
| PROP-026 | Host Surfaces — CLI/MCP/WebApi/REPL upgrade | Host | 🟡 Vysoká | 7,5 dne | System.CommandLine, Spectre.Console, Minimal API, REPL |
| PROP-027 | AI Layer — MetaForge.Ai projekt, Ollama, PromptRegistry | AI | 🟡 Vysoká | 6 dní | OllamaAdapter, PromptRegistry, PromptEvaluator |
| PROP-028 | Infrastructure — Persistence, konfigurace, caching | Infrastr. | 🟡 Vysoká | 5,25 dne | JSONL persistence, IOptions<T>, checkpoint cache |
| PROP-029 | ForgeBlocks — Rozšíření a marketplace | ForgeBlocks | 🟡 Vysoká | 8 dní | EF Core, AutoMapper, FluentValidation, NuGet distribuce |
| PROP-030 | Bezpečnost a stabilita — Schema migration, validace, health | Průřezové | 🟡 Vysoká | 4 dny | CommandMigration, ValidationPipeline, HealthChecks |

## Odložené návrhy

| ID | Název | Důvod odložení | Datum |
|----|-------|----------------|-------|
| PROP-018 | Translator — ExpertProjection a ProjectionOptions | nejsem si jist správnou funkcionalitou pro host surfaces | 4.7.2026 |
| PROP-020-F5 | BusinessModel — Fáze 5: BusinessBehaviorInputNode, PendingQuestion rozšíření | nízká priorita, neblokuje core flow; PROP-020 Fáze 1–4 dokončeny | 4.7.2026 |

## Issues — Známé problémy k opravě

> Problémy zjištěné při Code Review po implementaci. Každý issue má prioritu a odkaz na dotčený PROP/soubor.
> Při opravě přesunout do `PROPOSALS.md` jako nový PROP nebo task.

| # | Datum | PROP | Soubor | Závažnost | Popis | Doporučené řešení |
|---|-------|------|--------|-----------|-------|-------------------|
| 1 | 4.7.2026 | PROP-028 | `InfrastructureServiceRegistration.cs` | ⚠️ Střední | `AddSingleton` místo `TryAddSingleton` — při vícenásobném volání `AddMetaForgeInfrastructure()` vzniknou duplicitní DI registrace. | Nahradit `AddSingleton` → `TryAddSingleton`. Vyžaduje referenci na `Microsoft.Extensions.DependencyInjection.Extensions`. |
| 2 | 4.7.2026 | PROP-028 | `JsonCommandLogRepository.cs` | ⚠️ Nízká | `AppendAsync` používá synchronní `File.AppendAllText` uvnitř `lock` a vrací `Task.CompletedTask`. Není to pravá async operace — při velkém objemu dat může blokovat vlákno. | Pro produkci: použít `await File.AppendAllTextAsync` (pokud existuje) nebo obalit do `Task.Run`. Pro MVP akceptovatelné. |
| 3 | 4.7.2026 | PROP-027 | `AiServiceRegistration.cs` | ⚠️ Nízká | `AddMetaForgeAi()` neregistruje `PromptRegistry` ani `PromptEvaluationService` — nově přidané služby v PROP-027 nejsou součástí DI. | Přidat `services.AddSingleton<PromptRegistry>()` a `services.AddSingleton<PromptEvaluationService>()` do `AddMetaForgeAi()`. |
| 4 | 4.7.2026 | PROP-024 | `Expression.cs` | 💡 Návrh | Abstraktní `Expression` má `Kind` jako `string` i `ExpressionKind` jako `enum` — redundantní. Časem sjednotit pouze na `ExpressionKind` enum a `Kind` string odstranit (breaking change, nutná migrace). | Ponechat obojí pro zpětnou kompatibilitu. Při další major verzi odstranit `string Kind`. |
| 5 | 4.7.2026 | PROP-025 | `IncrementalCodeGenerator.cs` | ⚠️ Střední | `GetMaxEntities()` vrací hardcodované 3 — nečte z `GeneratorLicense.MaxEntities`. Sandbox limit se nekontroluje správně pro vyšší tiery. | Předat `GeneratorLicense` do metody nebo číst `_license.MaxEntities`. |
| 6 | 4.7.2026 | PROP-025 | `CodeGenerator.cs` | ⚠️ Nízká | `CodeGenerator` změněn z `sealed` na `class` kvůli dědičnosti `TieredCodeGenerator`. To umožňuje nechtěné přepsání metod. | Zvážit kompozici místo dědičnosti (např. `TieredCodeGenerator` wrapuje `CodeGenerator`). |
| 7 | 4.7.2026 | PROP-019 | `OllamaAiTranslator.cs` | ⚠️ Nízká | Duplikuje logiku z `MetaForge.Ai/Adapters/OllamaAdapter` — oba volají Ollama HTTP API. | PROP-019 explicitně volí Variantu A (přímé volání). Až bude MetaForge.Ai stabilní, sjednotit. |
| 8 | 4.7.2026 | PROP-019 | `DefaultBusinessTranslator.cs` | ⚠️ Nízká | `TryEnrichAsync` je nová async metoda, ale `IBusinessTranslator` má jen synchronní `TryEnrich`. Volající musí explicitně používat async verzi. | Přidat `TryEnrichAsync` do `IBusinessTranslator` nebo vytvořit `IAsyncBusinessTranslator`. |

---

## Detaily návrhů

### PROP-017: Generators — ForgeBlock packaging a katalog

Více: [`Docs/Plans/PROP-017-Generators-ForgeBlock-Packaging.md`](Docs/Plans/PROP-017-Generators-ForgeBlock-Packaging.md)

### PROP-018: Translator — ExpertProjection a ProjectionOptions

Více: [`Docs/Plans/PROP-018-Translator-ExpertProjection.md`](Docs/Plans/PROP-018-Translator-ExpertProjection.md)

### PROP-019: Translator — IAiTranslator a AI-assisted překlad

Více: [`Docs/Plans/PROP-019-Translator-AiTranslator.md`](Docs/Plans/PROP-019-Translator-AiTranslator.md)

### PROP-020: BusinessModel — Architektonický upgrade dle původního konceptu

Více: [`Docs/Plans/PROP-020-BusinessModel-Architecture-Upgrade.md`](Docs/Plans/PROP-020-BusinessModel-Architecture-Upgrade.md)

> **Stav:** 🟢 Schváleno — přesunuto do PROPOSALS.md

### PROP-021: Testování — Property-based (FsCheck) a Snapshot (Verify)

Více: [`Docs/Plans/PROP-021-Tests-PropertyBased-Snapshot.md`](Docs/Plans/PROP-021-Tests-PropertyBased-Snapshot.md)

### PROP-022: Observabilita — OpenTelemetry tracing a BusinessModel diff

Více: [`Docs/Plans/PROP-022-Observability-Tracing-Diff.md`](Docs/Plans/PROP-022-Observability-Tracing-Diff.md)

### PROP-023: DX a architektonická vylepšení na zvážení

Více: [`Docs/Plans/PROP-023-DX-Architecture-Improvements-Future.md`](Docs/Plans/PROP-023-DX-Architecture-Improvements-Future.md)

### PROP-024: Core — StrongType/ValueObject, Expression System, Record elementy

Více: [`Docs/Plans/PROP-024-Core-StrongType-Expression-Record.md`](Docs/Plans/PROP-024-Core-StrongType-Expression-Record.md)

### PROP-025: Generators — Incremental, Partial Class, Scaffolding + Monetization

Více: [`Docs/Plans/PROP-025-Generators-Incremental-Monetization.md`](Docs/Plans/PROP-025-Generators-Incremental-Monetization.md)

### PROP-026: Host Surfaces — CLI, MCP, WebApi, REPL upgrade

Více: [`Docs/Plans/PROP-026-Host-Surfaces-CLI-MCP-WebApi-REPL.md`](Docs/Plans/PROP-026-Host-Surfaces-CLI-MCP-WebApi-REPL.md)

### PROP-027: AI Layer — MetaForge.Ai projekt, OllamaAdapter, PromptRegistry

Více: [`Docs/Plans/PROP-027-AI-Layer-MetaForge-Ai.md`](Docs/Plans/PROP-027-AI-Layer-MetaForge-Ai.md)

### PROP-028: Infrastructure — Persistence, Konfigurace, Caching

Více: [`Docs/Plans/PROP-028-Infrastructure-Persistence-Config-Cache.md`](Docs/Plans/PROP-028-Infrastructure-Persistence-Config-Cache.md)

### PROP-029: ForgeBlocks — Rozšíření a marketplace

Více: [`Docs/Plans/PROP-029-ForgeBlocks-Expansion-Marketplace.md`](Docs/Plans/PROP-029-ForgeBlocks-Expansion-Marketplace.md)

### PROP-030: Bezpečnost a stabilita — Schema Migration, Validation, Health

Více: [`Docs/Plans/PROP-030-Security-Stability-Schema-Validation-Health.md`](Docs/Plans/PROP-030-Security-Stability-Schema-Validation-Health.md)

---

## Legenda priorit

- 🔴 Kritická — musí se implementovat co nejdříve
- 🟡 Vysoká — důležité pro další vývoj
- 🟢 Nízká — nice to have
- ⚪ Odloženo — zatím se neimplementuje
