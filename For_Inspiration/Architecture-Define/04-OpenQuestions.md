# MetaForge — Otevřené architektonické otázky

Datum: 2026-04-17
Status: Živý dokument — aktualizováno 2026-04-28 (OQ-006 doplněno o Node Assist apply)

---

## Formát záznamu

```
### OQ-NNN — Název otázky
Status: Otevřená | Rozhodnuto | Odloženo
Oblast: vrstva nebo komponenta
Kontext: proč otázka vznikla
Možnosti: ...
Rozhodnutí: (vyplnit při uzavření)
```

---

## Otevřené otázky

---

### OQ-001 — Checkpoint ownership v pipeline

**Status:** Rozhodnuto 2026-04-18  
**Oblast:** `MetaForge.BusinessModel` — `ReplayProjectionQueryService`, `SourceStage`

**Rozhodnutí:**  
`ReplayProjectionQueryService` si drží `SaveCheckpointAsync()` jako explicitní veřejnou metodu mimo pipeline (možnost 2).

- `GetProjectionAsync()` spustí pipeline a po úspěšném replay zavolá interně `SaveCheckpointAsync(result)` — checkpoint je side-effect service, ne pipeline
- `SourceStage` zůstane čistá — pouze čte a streamuje příkazy, žádné side-effecty
- `SaveCheckpointAsync()` je veřejná metoda — viditelná, testovatelná, volatelná externě pokud je potřeba
- Konfigurace: `CheckpointEnabled` flag v service konfiguraci pro snadné vypnutí v testech
- `ICheckpointStrategy` se **nevytváří** — zbytečná abstrakce pro jednu implementaci

**Kontext:**  
Dnes žije `SaveCheckpoint()` přímo v `ReplayProjectionQueryService`. Checkpoint validace závisí na výsledku replay — jsou těsně svázané. V pipeline modelu by `SaveCheckpoint()` logicky patřil do `SourceStage`, ale je to side-effecting operace mimo hlavní proud `ExecuteAsync()`.

**Možnosti:**
1. `ICheckpointStrategy` jako injektovaná závislost do `SourceStage` — clean, ale přidává interface
2. `ReplayProjectionQueryService` si drží checkpoint jako separátní metodu mimo pipeline — méně čistá, ale jednodušší
3. Checkpoint zůstane v `ReplayProjectionQueryService` jako explicitní public metoda, pipeline ho neřeší

**Otázka:** Která možnost lépe respektuje single responsibility aniž by přidala zbytečnou komplexitu?

---

### OQ-002 — AI klienti po sloučení MCP tools

**Status:** Rozhodnuto 2026-04-17  
**Oblast:** `MetaForge.Mcp`, systémový prompt

**Kontext:**  
Dnes AI klienti (Claude, GPT) volají `GetBusinessExpertProjection` explicitně protože vědí, že vrátí víc — type resolution, diagnostiku, suggestions. Po sloučení do jedné `GetBusinessProjection(detail?)` musí vědět, že mají poslat `detail=Expert` kdykoli potřebují katalog a diagnostiku.

**Rozhodnutí:**  
`Basic` zůstává výchozí hodnotou — efektivní pro jednoduché přehledy a operace. Guidance pro AI klienty je zabudována přímo v popisu MCP toolu `GetBusinessProjection`: tool description a description parametru `detail` explicitně uvádějí, že `detail=expert` je nutný kdykoli AI potřebuje type resolution, diagnostiku, návrhy presetů nebo analýzu relací. AI klienti tuto informaci čtou automaticky při discovery toolů přes MCP protokol.

---

### OQ-003 — ProjectionOptions — kde patří?

**Status:** Rozhodnuto 2026-04-17  
**Oblast:** Hranice `MetaForge.BusinessModel` vs. `MetaForge.Translator`

**Kontext:**  
`IProjectionQueryService` patří do `MetaForge.BusinessModel`. `ExpertProjectionBuilder` (který dělá Expert obohacení) patří do `MetaForge.Translator`. Enum `ProjectionDetailLevel` byl nahrazen `ProjectionOptions` objektem, aby uživatel mohl volit libovolnou granularitu sekcí.

**Rozhodnutí:**  
`ProjectionOptions` patří do `MetaForge.Translator` — stejně jako `ExpertProjectionBuilder`, na jehož přepínačích závisí. `IProjectionQueryService` v `MetaForge.BusinessModel` zůstane **bez jakéhokoliv options parametru** — pouze `async` replay + `streamId`. Orchestraci zajistí nová třída `ProjectionReadService` v Translator vrstvě.

- `Basic()` a `Expert()` jsou statické tovární metody na `ProjectionOptions` — ne enum hodnoty.  
- `Custom(diagnostics, typeResolution, suggestions, ...)` umožňuje libovolnou kombinaci bez breaking change interface.  
- `ExpertProjectionBuilder` zůstane beze změny; dostane `ProjectionOptions` a interně přeskočí nezapnuté sekce.

---

### OQ-004 — Streaming threshold

**Status:** Rozhodnuto 2026-04-17  
**Oblast:** `MetaForge.BusinessModel` — `ReplayProjectionQueryService`

**Kontext:**  
Dnes je `IProjectionQueryService.GetBusinessProjection()` synchronní. Přechod na `async` změní interface — breaking change pro všechny sync volající (CLI, testy). Streaming se vyplatí až od určité velikosti logu.

**Rozhodnutí:**  
Kombinace obou metrik — file size primární (přímo mapuje na memory pressure), command count jako secondary backup.

- **File size threshold: 1 MB** — primární metrika, přímo koreluje s memory alokací při `ReadAll()`
- **Command count threshold: 500** — secondary, pokud file size selže (např.compressed nebo metadata-heavy log)
- **Streaming aktivní když:** `fileSize > 1 MB || commandCount > 500`
- **Konfigurace:** `StreamingThresholdOptions` jako `internal record` v `SourceStage`, default hodnoty hardcoded, možnost přepsat při inicializaci service (pro testování)
- **Interface vždy async** — Fáze 1 vyřešila, `GetProjectionAsync` je vždy `Task<T>`, žádný sync fallback na úrovni interface
- **CLI migrace** — Fáze 1 vyřešila, `await` na všech volajících

**Soubory k implementaci:**
- `Src/MetaForge.BusinessModel/CommandLog/StreamingThresholdOptions.cs` (internal record)
- `Src/MetaForge.BusinessModel/CommandLog/SourceStage.cs` — přidat `IAsyncEnumerable<CommandEnvelope>` variantu a threshold check

---

### OQ-005 — SyncCurrentDocumentFromProjection lifecycle po Read/Write splitu

**Status:** Rozhodnuto 2026-04-17  
**Oblast:** `MetaForge.Translator` — `AuthoringConversationService`

**Kontext:**  
`SyncCurrentDocumentFromProjection()` se dnes volá v konstruktoru `AuthoringConversationService` — synchronizuje write dokument s replay výsledkem při startu. Po read/write splitu bude tato synchronizace odpovědností `CommandWriteService`.

**Rozhodnutí:**  
- **Write dokument je autoritativní za runtime** — změny aplikované přes `CommandWriteService` aktualizují `_currentDocument` přímo, bez synchronizace přes projekci.
- **Projekce je autoritativní při startu** — `SyncCurrentDocumentFromProjection()` se volá **pouze jednou**: při inicializaci `CommandWriteService` (konstruktor nebo lazy-init). Zajišťuje, že write dokument reflektuje command log, pokud existují logy z předchozích sessions.
- **Při `ResetDocument()`** se write dokument nastaví na čistý stav; `SyncCurrentDocumentFromProjection()` se po resetu nevolá — reset je explicitní akce uživatele.
- **Projekce nikdy nepřepisuje write dokument za runtime** — read path je zcela oddělená od write dokumentu.
- **Failure fallback** — pokud projekce při startu selže (korupce logu), `CommandWriteService` zachová poslední persistovaný write dokument jako fallback; chyba je logována jako varování, nikoli výjimka.

---

### OQ-006 — Auto-apply a Conflict resolution UX

**Status:** Rozhodnuto 2026-04-18  
**Oblast:** `MetaForge.Translator`, MCP/CLI surface

**Rozhodnutí:**  
Model autority je asymetrický – Business a Core mají každý svou doménu:
- **Business metadata** (Name, Description, intent) – výhradně Business autoritativní. Core je nikdy nepřepisuje.
- **CoreDetail** (ValueObject, IsStrongType, ResolvedPresetId, ValueObjectName) – po enrichmentu výhradně Core autoritativní.

Princip:
1. Při `update_core_detail` se business display pole (např. typ název) automaticky regenerují z CoreDetail **bez uživatelské interakce** – stav přejde na `Synced`.
2. Uživatel pak pokračuje editací business metadata (→ `BusinessEdited`) s následným překladem přes `ApplyEnrichment` (→ `Synced`).
3. Cyklus se opakuje libovolněkrát.

Důsledek: stavy `CoreEdited` a `Conflict` **neexistují**. `AttributeSyncState` má pouze tři hodnoty: `New`, `Synced`, `BusinessEdited`. Žádný interaktivní conflict prompt ani dedikovaný conflict tool nejsou potřeba – Core vždy autoritativní znamená žádná uživatelská rozhodnutí.

**Aplikace na Node Assist (2026-04-28):**
Node Assist `ApplyNodeAssistOperations` následuje stejný princip — AI navrhuje `BusinessPatchOperation[]`, ale uživatel musí explicitně potvrdit apply. Žádný auto-apply. Validátor (`NodeAssistOperationValidator`) zajišťuje, že operace míří pouze do povoleného node scope. Tím se zamezí jak konfliktům autority, tak nechtěným side-effectům mimo cílový node.

---

### OQ-007 — CoreDetail serializace v BusinessAuthoringDocument

**Status:** Rozhodnuto 2026-04-18  
**Oblast:** `MetaForge.BusinessModel`, `BusinessProjectionCheckpointStore`

**Rozhodnutí:**  
`BusinessAttributeCoreDetail` (bez `SyncState`) je součástí serializovaného dokumentu – **slouží jako cache/redundance pro výkon**. Zdrojem pravdy pro CoreDetail je vždy kombinace Core + command log:

1. `SyncState` je výhradně `[JsonIgnore]` computed property – přepočítává se při každém replay z history commandů.
2. CoreDetail se mění výhradně přes patch operace (`apply_preset`, `enrich_attribute`, `update_core_detail`) – nikdy direkt.
3. Při startu: `SyncCurrentDocumentFromProjection()` rekonstruuje `CoreDetail` a `SyncState` z command logu. Serializovaný dokument je fallback, pokud je k dispozici.
4. Checkpoint zahrnuje serializovaný CoreDetail, ale není autoritativní – při replay se vždy přepočítá z commandů.

Garance konzistence: CoreDetail v dokumentu a CoreDetail z replay jsou identické, pokud command log není poškozen. Test: load dokument bez CoreDetail → replay → CoreDetail se sestaví z commandů bez pádu.

---

### OQ-008 — Multi-vrstvá registrace ForgeBlock balíčků — kde žijí interface kontrakty?

**Status:** Rozhodnuto 2026-04-18  
**Oblast:** `MetaForge.Core`, `MetaForge.Mcp`, `MetaForge.Cli`, `MetaForge.Translator`

**Rozhodnutí:**  
Neutrální metadata v Core (možnost 3). `IForgeBlockRegistry` se rozšíří o neutrální pojmy pro capabilitu metadata — `CapabilityMetadata`, `ParameterMetadata`, `ReturnMetadata` — bez MCP/CLI specifik.

- Core definuje pouze **metadata struktura** — jméno, popis, parametry, návratový typ capability. Žádné MCP `ToolDefinition` nebo CLI-specifické atributy.
- Balíček implementuje `IForgeBlockCapabilityPackage.RegisterCapabilities(IForgeBlockRegistry)` — volá `registry.RegisterCapability(CapabilityMetadata)`.
- **Bootstrap per-host** — MCP bootstrap vezme `CapabilityMetadata` ze registru a staví si z toho MCP `ToolDefinition` (mapa na MCP schema). CLI bootstrap dělá obdobně.
- Skaluje se bez limitu — jakýkoliv počet budoucích hostů (WebUI, Desktop, ...) si vezme metadata a adaptuje podle svého schématu.
- Balíček závisí **výhradně na Core**, ne na MCP/CLI.

**Kontext:**  
Vize je: každý ForgeBlock balíček je samostatná registrační jednotka, která se sám zaregistruje do všech vrstev které potřebuje — Core, Translator, MCP, CLI, Help. `IForgeBlockPackage.Register(IForgeBlockRegistry)` funguje pro Core facety. Ale `IForgeBlockRegistry` žije v Core a Core nesmí záviset na MCP ani CLI.

Při zavedení `IForgeBlockMcpPackage` a `IForgeBlockCliPackage` musíme rozhodnout kde tyto interface žijí.

**Možnosti:**
1. **Per-host projekt** — `IForgeBlockMcpPackage` žije v `MetaForge.Mcp`, `IForgeBlockCliPackage` v `MetaForge.Cli`. Balíček odkazuje na daný host projekt pokud chce contribuovat. Nevýhoda: balíček musí mít projekt referenci na MCP nebo CLI.
2. **Sdílený kontrakt projekt** — Nový `MetaForge.ForgeBlocks.Contracts` (nebo rozšíření `MetaForge.Core`) drží všechny registration interface. Balíček závisí pouze na kontraktech, ne na implementaci hostu.
3. **Pouze optional interfaces v Core** — `IForgeBlockRegistry` dostane volitelná rozšíření pro MCP/CLI metadata (neutrální pojmy, ne MCP-specifické), a MCP si sestaví tools ze svých dat sám.

**Otázka:** Možnost 2 (sdílený kontrakt projekt) nebo 3 (neutrální interface v Core) — co lépe zachovává dependency pravidla a zároveň umožní balíčku přidat vlastní MCP tools?

---

### OQ-009 — Business katalog — kde žijí business-level presety a kdo je aplikuje?

**Status:** Rozhodnuto 2026-04-18  
**Oblast:** `MetaForge.Core.Catalog`, `MetaForge.BusinessModel`, Marketplace

**Rozhodnutí:**  
Unified katalog s layered providers (možnost 3). `CatalogManager` zůstává jediná catalog infrastructure; přibývá `BusinessTemplateCatalogProvider` (Priority 20) který dodává business šablony vedle stávajících Core presetů.

- **Preset je unified šablona** nesoucí dvě semínka: `BusinessTemplate` (seed pro business metadata – jméno entity, atributy, popis) a `CoreDetailTemplate` (seed pro CoreDetail pole – `ValueObject`, `IsStrongType`, `ResolvedPresetId`, `ValueObjectName`).
- `CatalogItem` se rozšíří o obě semínka; stávající Core-only záznamy mají `BusinessTemplate = null`.
- Aplikace presetu = inject obou semínek → state přejde na `Synced`. Dispatch je výhradně v BusinessModel vrstvě, Core o business pojmech neví.
- `MarketplaceCatalogProvider` (Priority 50) bude fungovat pro obě úrovně – Core presety i Business šablony.
- Separátní `BusinessTemplateCatalog` **nevzniká** – jedna catalog infrastructure, více providerů.

**Kontext:**  
`CatalogManager` a `CatalogItemType` byly navrženy v době, kdy Core byl hlavní zdroj pravdy. `CatalogItemType` obsahuje: `ValueObject`, `ClassPreset`, `InterfacePreset`, `EnumPreset`, `StructPreset`, `ForgeBlock` — všechno jsou Core-level konstrukty. Business vrstva (`BusinessAuthoringDocument`, `BusinessEntityNode`, atributy, chování) vznikla až v druhé fázi — katalog to nereflektuje.

**Problém:**  
Při přidání presetů jako „Uživatel" (entita s atributy name, email, roles) nebo „Warehouse" nelze použít stávající `CatalogItemType`. Aplikace takového presetu není `ResolveType()` v Core — je to série `BusinessPatchOperation` (`AddEntity` + `AddAttribute`×N). Core o tom neví a vědět nemůže.

Navíc marketplace vision předpokládá přístup ke katalogu z chatu (MCP) — AI asistent navrhne preset, uživatel potvrdí, systém aplikuje — ale write-back musí jít přes `BusinessAuthoringDocument`, ne přes Core.

**Nutné přepracovat:**  
- `CatalogManager` a `FileSystemCatalogProvider` jsou orientovány na Core presety → Business vrstva potřebuje vlastní katalogovou vrstvu nebo rozšíření
- `CatalogItemType` je neúplný — chybí typy pro business-level šablony
- Aplikace presetu dnes = `ResolveType()` → Core reset; Business preset aplikace = `PatchOperation` série → BusinessModel write

**Možnosti:**
1. **Rozšíření `CatalogItemType` v Core** — přidat `EntityTemplate`, `DomainTemplate`, `ArchitectureTemplate`; aplikační logika v BusinessModel čte typ a dispatch na správnou cestu. Nevýhoda: Core ví o business pojmech.
2. **Separátní `BusinessTemplateCatalog` v BusinessModel** — vlastní catalog infrastructure nezávislá na Core; Core catalog zůstane pro Core presety. Nevýhoda: dvě paralelní catalog infrastructury.
3. **Unified katalog s layered providers** — `CatalogManager` zůstane, ale přidá se `BusinessTemplateCatalogProvider` (Priority 20) který přináší business šablony; typ `CatalogItemType` rozšíří se o business hodnoty. Aplikační dispatch na základě `CatalogItemType` ven z Core do BusinessModel.

**Marketplace dopad:**  
`MarketplaceCatalogProvider` jako 4. provider (Priority 50) musí fungovat pro obě úrovně — Core presety i Business šablony. `CreditCost` pole na `CatalogItem` již existuje — základ je tam.

**Otázka:** Možnost 2 (separátní Business katalog) nebo 3 (unified s layered providers a rozšířeným typem) — co lépe odpovídá nové realitě kde BusinessModel je hlavní zdroj pravdy?

---

### OQ-010 — Kde leží hranice Tier 1 / Tier 2 pro katalog suggestion v chatu?

**Status:** Rozhodnuto 2026-04-18  
**Oblast:** `MetaForge.Ai`, `MetaForge.Translator`, `MetaForge.Chat`

**Rozhodnutí:**  
Deterministická suggestion (možnost 3). `CatalogManager.SuggestPreset(name, type?)` je čistá pattern-matching funkce – žádné AI volání, žádný Tier 2 segment.

- Vstup: název atributu/entity (+ volitelný typ). Výstup: `IReadOnlyList<CatalogItem>` – strukturovaná data, ne volný text.
- Matching pravidla jsou kompilované (regex nebo dictionary lookup) – deterministická, testovatelná, zero-latency.
- Chat suggestion je přirozený výstup tohoto volání: `ExpertProjectionBuilder` (nebo MCP handler) zavolá `SuggestPreset()` a výsledek vloží jako strukturovaný kontext pro LLM. LLM ho jen přeloží do přátelské formy pro uživatele.
- Tier 2 segment pro catalog suggestion **nevzniká** – orchestrace není potřeba.

**Kontext:**  
Dvouúrovňová AI architektura definuje: Tier 1 = user-facing MainChat, Tier 2 = interní specializované modely. Při typování v chatu může platforma v reálném čase navrhovat presety z katalogu (např. "chceš přidat EmailAddress ValueObject?"). Tato suggestion je interně prací Tier 2 (`ConstraintInference` nebo nový `CatalogSuggestion` segment), ale výsledek se zobrazí uživateli skrze Tier 1 chat odpověď.

**Problém:**  
Je katalog suggestion volání do Tier 2 z Tier 1 orchestrátoru (MainChat handler zavolá `CatalogManager.SuggestPreset()` a výsledek vloží do chat odpovědi)? Nebo je to Tier 2 segment který běží paralelně a pushuje suggestions do Tier 1 kontextu?

**Možnosti:**
1. **Tier 1 orchestruje** — MainChat handler (nebo `AuthoringConversationService`) zavolá `CatalogManager.SuggestPreset()` synchronně a zahrne výsledky do systémového kontextu pro LLM. Jednoduché, ale Tier 1 musí znát Tier 2 API.
2. **Tier 2 paralelní segment** — nový `CatalogSuggestion` segment běží paralelně při zpracování každé zprávy, jeho výsledky se přidají do promptu jako enrichment. Čistší separace, ale složitější orchestrace.
3. **Rule-based, ne AI** — `CatalogManager.SuggestPreset()` je deterministická funkce (pattern matching na název atributu), ne AI. Suggestion do chatu se přidá jako strukturovaný kontext bez extra AI volání.

**Otázka:** Možnost 3 (deterministická suggestion) nebo 1 (Tier 1 orchestruje Tier 2 synchronně) — co lépe odpovídá principu "Tier 2 vrací strukturovaná data, nikdy volný text"?

---

---

### OQ-011 — Tag-based search v Discovery

**Status:** Rozhodnuto 2026-04-18  
**Oblast:** `MetaForge.Core` — `DefaultDiscoverySession`, `IDiscoverySession`

**Rozhodnutí:**  
Separátní `SearchByTag(string tag)` metoda na `IDiscoverySession` (možnost 2). Tagy jsou integrální součástí discovery funkcí, nikoliv jen metadata.

- `SearchByTag(string tag)` vrací `IReadOnlyList<DiscoveryItemSummary>` — všechny položky obsahující tag
- Tag matching je case-insensitive, přesná shoda (ne substring)
- Volá se jako `session.SearchByTag("math")` — jasné API, vidět v discovery výpisu
- `TryResolveShortcut` se **neměnit** — zůstane co je (ID, PackageId, DisplayName, SemanticHandles), aby zůstalo jednoduché a předvídatelné
- Skaluje se bez problému — později je trivial přidat `SearchByTags(IReadOnlyList<string>)` pro multi-tag vyhledávání
- Tagy jsou u všech discovery kategorií — capabilities, catalog entries, tools (po Plánu 3), apod.

**Kontext:**  
Tagy existují na všech úrovních ForgeBlock registrace (`ForgeBlockPackageDescriptor`, `ForgeBlockCapabilityDescriptor`, `ForgeBlockCatalogEntryDescriptor`, `ForgeBlockDiscoveryItem`) a jsou surfované v `DiscoveryItemSummary.Tags`. Ale `TryResolveShortcut` je při vyhledávání ignoruje — matchuje pouze ID, PackageId, DisplayName a SemanticHandles.  
Externí balíčky (NuGet → dynamic load) by měly mít dobré tagy aby byly snadno vyhledatelné. Pokud search tagy neprochází, taxonomie tagů nemá efekt na discovery UX.

**Možnosti:**
1. **Rozšířit `TryResolveShortcut`** — přidat tag matching jako poslední fallback (po SemanticHandles). Jednoduchá změna, tag `math` najde vše s tagem `math`.
2. **Separátní `SearchByTag(string tag)`** — explicitní metoda na `IDiscoverySession`. Čistší API, ale přidává nový vstupní bod.
3. **Tags jsou jen metadata pro výpis, ne vyhledávání** — AI klient dostane tagy v odpovědi a filtruje sám na své straně.

**Otázka:** Je tag-based search případ pro platformu (možnost 1/2) nebo pro AI klienta (možnost 3)?

---

## Uzavřené otázky

---

### OQ-012 — Workflow jako sekce BusinessAuthoringDocument vs. separátní dokument

**Status:** Rozhodnuto 2026-04-25  
**Oblast:** `MetaForge.BusinessModel`

**Rozhodnutí:**
Workflow je přímo sekce `BusinessAuthoringDocument` (možnost 1).

- `Workflows` je root-level sibling k `Entities` a `Relations`, ne vnořená sekce entity.
- Workflow používá stejný `CommandLog`, stejný replay a stejný patch surface jako zbytek authoring modelu.
- Samostatný dokument nebo samostatný stream by v této fázi zbytečně fragmentoval source of truth, validaci a AI context.
- Execution historie workflow zůstává mimo authoring dokument a řeší ji samostatná open question OQ-014.

**Kontext:**
Nový architektonický směr počítá s workflow jako first-class součástí authoring modelu. Je potřeba rozhodnout, zda workflow žije přímo v `BusinessAuthoringDocument`, nebo v samostatném dokumentu navázaném na stejný `CommandLog`.

**Možnosti:**
1. Workflow je přímo sekce `BusinessAuthoringDocument`.
2. Workflow je samostatný dokument se společným streamem.
3. Workflow je samostatný stream, ale projekčně se skládá do jednoho authoring view.

**Výsledek:**
Workflow Model I může začít nad jedním dokumentem a jedním replay modelem bez potřeby druhého authoring dokumentu nebo projekčního skládání více streamů.

---

### OQ-013 — Output readiness model a validační brány

**Status:** Rozhodnuto 2026-04-25  
**Oblast:** `MetaForge.Translator`, `MetaForge.BusinessModel`, exportní vrstvy

**Rozhodnutí:**
Hybridní readiness model (možnost 3).

- `Draft`, `Enriched`, `ExportReady` jsou **computed readiness view**, ne persistovaná authoring data v `BusinessAuthoringDocument`.
- Autoritativní validační brána je **per output** — minimálně pro `code`, `workflow`, `mcp` a `docs`.
- Globální high-level readiness summary může existovat pro UI, AI a rychlý přehled, ale nesmí sama o sobě povolovat export.
- Výpočet readiness patří do read nebo projection vrstvy jako `OutputReadinessView`, ne do `MetaForge.Core.MetadataState` a ne do ručně authorovaného business dokumentu.
- Jednotlivé výstupy se nesmí blokovat navzájem zbytečně přísnou validací: dokumentace nebo AI context mohou být použitelné dřív než codegen nebo workflow export.

**Kontext:**
Pokud jeden authoring model vydává více typů výstupů, ne všechny potřebují stejnou míru detailu. Je potřeba rozhodnout, jak bude vypadat společný model stavů typu `Draft`, `Enriched`, `ExportReady` a kdo jej počítá.

**Možnosti:**
1. Jeden globální readiness stav pro celý dokument.
2. Readiness per výstup (`code`, `workflow`, `mcp`, `docs`).
3. Hybrid — globální high-level stav + detail per output.

**Otázka:**
Jak zavést output-aware validační brány tak, aby codegen, workflow export i capability export sdílely jeden authoring kernel, ale neblokovaly se navzájem zbytečně přísnou validací?

**Výsledek:**
MetaForge bude držet jeden authoring kernel, ale readiness bude vyhodnocovat output-aware způsobem: společný přehled ano, exportní autorita vždy per výstup.

---

### OQ-014 — Oddělení authoring modelu a execution historie

**Status:** Rozhodnuto 2026-04-25  
**Oblast:** `MetaForge.BusinessModel`, budoucí runtime vrstva

**Rozhodnutí:**
Execution log odděleně mimo `BusinessAuthoringDocument` (možnost 1), s tím, že runtime pohled se do authoring světa vrací jen jako odvozená projekce.

- `BusinessAuthoringDocument` drží workflow definici, business detail, write-back enrichment a pending questions, ne historii konkrétních běhů.
- Runtime execution historie se persistuje v odděleném execution logu nebo run store mimo authoring source of truth.
- Host nebo read vrstva může nad runtime historií stavět odvozenou projekci, například poslední běh, stav běhu, failure summary nebo audit trail.
- `Execution metadata jako projekce` je správný **read model**, ne správné místo persistence authoring dat.
- Částečné summary v authoring dokumentu se nezavádí, protože by míchalo návrhová a provozní data a časem driftovalo.

**Kontext:**
Workflow jako first-class citizen svádí k ukládání běhové historie procesu do stejného dokumentu. To by ale mohlo rozbít přehlednost source of truth a zamíchat authoring data s provozem.

**Možnosti:**
1. Execution log odděleně mimo `BusinessAuthoringDocument`.
2. Execution metadata jako projekce bez persistence v authoring dokumentu.
3. Částečné summary v dokumentu, detailní runtime historie odděleně.

**Otázka:**
Jak zachovat audit workflow běhů bez toho, aby se authoring dokument změnil v runtime ledger?

**Výsledek:**
Authoring a runtime zůstávají dvě oddělené pravdy propojené referencemi a projekcemi, ne jedním smíchaným dokumentem.

---

### OQ-015 — Veřejné MVP vs. interní architektonická připravenost

**Status:** Rozhodnuto 2026-04-25  
**Oblast:** Produkt, komunikace platformy

**Rozhodnutí:**
Veřejné MVP = `Builder + Analyst` (možnost 2).

- `Builder` zůstává hlavní veřejný vstup pro modelování, změny modelu a navazující exportní tok.
- `Analyst` je legitimní druhý veřejný režim, protože stojí na projekcích, diagnostice, discovery a vysvětlování stavu modelu.
- `Workflow`, `Orchestrator` a `Agent Host` zatím zůstávají architektonickou připraveností a follow-up směrem, ne veřejným produktovým slibem první vlny.
- Veřejná komunikace nemá vytvářet dojem, že workflow runtime nebo agentní orchestrace jsou už produktově stabilní surface.
- Produktový jazyk má držet MetaForge jako business-first authoring platformu s důrazem na modelování a analýzu, ne zatím na plnou orchestraci.

**Kontext:**
Architektura může unést Builder, Analyst, Orchestrator i Agent Host, ale produktově nelze komunikovat vše najednou.

**Možnosti:**
1. Veřejně držet jen Builder.
2. Veřejně držet Builder + Analyst.
3. Veřejně komunikovat i workflow hned v první vlně.

**Otázka:**
Které režimy mají být veřejné MVP a které mají zatím zůstat pouze jako architektonická připravenost dokumentovaná v `Architecture-Define/`?

**Výsledek:**
Veřejná první vlna má komunikovat dvě srozumitelné hodnoty: tvorbu modelu (`Builder`) a porozumění modelu (`Analyst`). Workflow a agentní orchestrace zůstávají zatím interně připraveným směrem pro další etapy.

---

### OQ-016 — Startovní rozhodnutí pro Workflow Write-Back I

**Status:** Rozhodnuto 2026-04-26  
**Oblast:** `MetaForge.BusinessModel`, `MetaForge.Translator`, replay a write-back boundary

**Rozhodnutí:**
Workflow Write-Back I startuje s úzkou a asymetrickou write-back surface.

- `BindingKind` zůstává v prvním řezu `string`, ne enum.
- `bind_workflow_step` je jediná operace, která smí binding vytvořit nebo plně nahradit.
- `update_workflow_binding` upravuje jen existující binding detail; pokud binding neexistuje, jde o chybu.
- Replay přepíná workflow binding do `BusinessEdited` jen při změně binding-relevant polí (`Kind`, `RelatedEntityId`, `RelatedBehaviorId`, `Actor`, `Inputs`, `Outputs`), ne při změně `Name`, `Summary` nebo `Notes`.
- Translator nebo jiná enrichment orchestrace může běžet přes samostatnou service boundary, ale autoritativní zápis do command logu končí vždy jako `bind_workflow_step`.

**Kontext:**
Po implementaci `Workflow Model I` vznikl další navazující řez pro capability binding a workflow sync metadata. Před startem implementace bylo potřeba uzavřít několik rozhodnutí, která přímo ovlivňují patch surface, replay a hranici mezi translator orchestrace a autoritativním write-back zápisem.

**Možnosti:**
1. Zavést enum pro `BindingKind`, dovolit `update_workflow_binding` vytvářet binding a počítat business edit i pro kosmetická pole.
2. Držet úzký první řez: `BindingKind` jako string, create jen přes `bind_workflow_step`, replay business edit jen pro binding-relevant pole a jednotný finální write-back command.

**Otázka:**
Jak nastavit první write-back řez tak, aby zůstal auditovatelný, neuzamkl předčasně capability taxonomii a nezahltil replay falešnými konflikty?

**Výsledek:**
Workflow Write-Back I může začít nad malou, stabilní mutation surface a zachovat stejný synchronizační pattern jako atributový `CoreDetail` write-back: persistovaný enrichment detail ano, sync stav jen jako computed replay view.
