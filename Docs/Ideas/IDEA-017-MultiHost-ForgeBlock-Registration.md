# IDEA-017 Multi-Host ForgeBlock Registration

Stav: Idea
Oblast: Core, ForgeBlocks, Host
Zdroj: For_Inspiration/Architecture-Define/05-ForgeBlock-Package-Model.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept definoval ForgeBlock jako "multi-vrstvou registrační jednotku" — balíček se sám registruje do Core, MCP, CLI, Translator a AI. Současná implementace (PROP-029) má pouze `IForgeBlockPackage` se 4 Core facety. Chybí per-host interface pro MCP, CLI, Translator a AI adaptery.

Nápad vychází z `05-ForgeBlock-Package-Model.md`, kde je popsáno:
- `IForgeBlockMcpPackage.RegisterMcp()` — MCP tools
- `IForgeBlockCliPackage.RegisterCli()` — CLI příkazy
- `IForgeBlockTranslatorPackage.RegisterTranslator()` — Translator pravidla
- `IForgeBlockAiAdapterPackage.RegisterAiAdapters()` — AI adaptery
- Bootstrap per-host s pattern matchingem (`if (package is IForgeBlockMcpPackage mcp) mcp.RegisterMcp(...)`)

## 2. Problém dnes

- Každý ForgeBlock se registruje pouze do Core facety — nemůže přidat vlastní MCP tools, CLI příkazy ani Translator pravidla.
- Pokud chce balíček přidat MCP tool, musí buď upravit MCP host, nebo záviset na MCP projektu — obojí špatně.
- Chybí neutrální `CapabilityMetadata` v Core — host si metadata mapuje na své schema sám.
- `RequiredTier` není v interface (ISS-010) — nelze vynutit kompilátorem.

## 3. Předběžný směr řešení

- `IForgeBlockMcpPackage` — volitelný interface pro MCP tools (žije v Core, neutrální metadata)
- `IForgeBlockCliPackage` — volitelný interface pro CLI příkazy (neutrální)
- `IForgeBlockTranslatorPackage` — volitelný interface pro Translator pravidla
- `IForgeBlockAiAdapterPackage` — volitelný interface pro AI adaptery
- `CapabilityMetadata` v Core — neutrální popis capability (parameters, return type)
- Bootstrap pattern: host projde balíčky a pattern matchuje na volitelné interface
- Přidat `RequiredTier` do `IForgeBlockCapabilityPackage`

Dotčené vrstvy: Core (interface, metadata), ForgeBlocks (implementace), Host (bootstrap), MCP (mapování), CLI (mapování).

## 4. Signál hodnoty

- ForgeBlock se stává skutečně pluginovou jednotkou — jedna instalace, registrace do všech hostů.
- Hostitelská aplikace nemusí znát konkrétní balíčky — stačí jí interface.
- Neutrální metadata umožňují budoucí hosty (WebApi, Desktop, ...) bez změny balíčků.

## 5. Rizika a nejasnosti

- Kde mají volitelné interface žít? V Core (neutrální) nebo v per-host projektu (MCP-specific)?
- Každý nový host bude přidávat nový interface — jak zabránit explozí?
- OQ-008 z původních OpenQuestions řešila přesně toto — rozhodnutí bylo "neutrální metadata v Core".

## 6. Doporučený další krok

Follow-up k PROP-029. Neutrální `CapabilityMetadata` v Core je první krok; per-host interface až po ověření modelu.

Vazby: PROP-029 (ForgeBlocks), OQ-008 (původní rozhodnutí), ISS-010 (RequiredTier)
