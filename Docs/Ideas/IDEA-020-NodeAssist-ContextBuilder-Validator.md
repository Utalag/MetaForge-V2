# IDEA-020 NodeAssist ContextBuilder a OperationValidator

Stav: Idea
Oblast: Translator, AI, Host
Zdroj: For_Inspiration/Architecture-Define/01-Layers.md, 02-Projection-Pipeline.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept definoval Node-level Assist jako samostatný subsystem s `NodeAssistContextBuilder`, `NodeAssistService` a `NodeAssistOperationValidator`. Současná implementace (PROP-019, PROP-026) má `AssistNodeAsync` a `ApplyNodeAssistOperations` na Facade úrovni, ale chybí detailní context building a operation validace.

Nápad vychází z `01-Layers.md` a `02-Projection-Pipeline.md`, kde je popsáno:
- `NodeAssistContextBuilder` — skládá node-scoped kontext z `ProjectionView` (entity, atributy, CoreDetail, sync state, vztahy)
- `NodeAssistService` — AI orchestrátor: načte kontext, zavolá AI, zpracuje odpověď
- `NodeAssistOperationValidator` — whitelist + entity scope validace před apply
- Node-level assist jako read consumer pipeline — nejdřív projekce, pak AI, pak validace

## 2. Problém dnes

- `AssistNodeAsync` existuje, ale kontext se skládá ad-hoc — chybí `NodeAssistContextBuilder`, který by konzistentně připravil kontext z projekce.
- Chybí whitelist validace — jaké operace AI smí navrhnout? (AddAttribute ano, DeleteEntity ne).
- Chybí entity scope — AI může navrhnout změnu entity, na kterou nemá scope.
- `NodeAssistOperationValidator` neexistuje — validace je implicitní nebo chybí.

## 3. Předběžný směr řešení

- `NodeAssistContextBuilder` — staví `NodeAssistContext` z `ProjectionView` pro konkrétní node (entity, její atributy, CoreDetail, relace, sync state)
- `NodeAssistService` — orchestrátor: `GetProjectionAsync → BuildContext → AI call → ParseResult → Validate → return`
- `NodeAssistOperationValidator` — whitelist povolených operací (AddAttribute, UpdateAttribute, ApplyEnrichment, ...), kontrola entity scope, kontrola referenční integrity
- Integrace do `BusinessAuthoringHostFacade.AssistNodeAsync()`

Dotčené vrstvy: Translator (context builder, validator), AI (AI call), Host (MCP/CLI volání).

## 4. Signál hodnoty

- Node Assist je bezpečnější — AI nemůže navrhnout destruktivní operace.
- Kontext je konzistentní — AI dostává stejný pohled jako projekce.
- Validace před apply zabraňuje nekonzistentnímu stavu.
- Nezbytný krok pro to, aby Node Assist mohl být použit v produkci.

## 5. Rizika a nejasnosti

- Whitelist musí být rozšiřitelný — ForgeBlocky mohou přidávat vlastní operace.
- OQ-xxx: Jaký je minimální whitelist pro MVP?
- OQ-xxx: Má validator kontrolovat i CoreDetail konzistenci?

## 6. Doporučený další krok

Follow-up k PROP-019 a PROP-026. Měl by být plánován jako součást stabilizace Node Assist pro produkční použití.

Vazby: PROP-019 (IAiTranslator), PROP-026 (Host Surfaces), PROP-020 (CoreDetail, SyncState)
