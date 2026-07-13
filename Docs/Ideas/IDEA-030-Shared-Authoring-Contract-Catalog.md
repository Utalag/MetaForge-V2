# IDEA-030 Shared Authoring Contract — Preset & Capability Catalog

Stav: Idea
Oblast: Core, MCP, Catalog, AI
Zdroj: Koumák + Perplexity konverzace db7f49c1-8f18-4f2e-8da2-b9b6e1f5c122
Datum vytvoření: 2026-07-11

## 1. Kontext

Z diskuze o ProductionHub mustru a importním JSON formátu vyplynulo, že MetaForge potřebuje **jeden společný authoring kontrakt** pro všechny vstupní kanály — ne "jazyk hostu", ale "shared authoring contract nad Facade".

Perplexity: *"Host je jen transport, shared authoring language žije nad Facade. Když ten kontrakt připneš k hostu, za chvíli ti začne MCP chtít jednu variantu, CLI druhou a import třetí — a host se ti promění z tenké vrstvy na překladač."*

Zároveň se z diskuze o presets a discovery ukázalo, že katalog presetů a capabilities by měl být **strojařsky čitelný JSON kontrakt pro AI agenty** — ne lidská dokumentace, ale rozhodovací vstup.

## 2. Architektonický princip

```
CLI / MCP / WebApi  ←  tenké host surfaces (transport)
        ↓
BusinessAuthoringHostFacade  ←  jediný vstupní bod
        ↓
BusinessAuthoringDocument  ←  source of truth
        ↓
Preset/Capability Catalog  ←  shared contract (JSON DTO)
```

Dva režimy JSON:
- **"MetaForge authoring JSON"** — primární cesta přes Facade pro authoring (entity, atributy, relace)
- **"Core import JSON"** — vedlejší kanál pro bulk operace, migrace, integrační testy

## 3. Catalog DTO — strojově čitelný formát

### 3.1 Base shape: `CatalogItemDto`

```json
{
  "kind": "preset | capability",
  "id": "builtin.email",
  "name": "email",
  "category": "built-in | forgeblock | domain",
  "summary": "Email field with semantic validation.",
  "semanticTags": ["contact", "identity", "validation"],
  "whenToUse": ["User asks for email field"],
  "avoids": ["Manual string + duplicated validation rules"],
  "version": "1.0.0",
  "editableAfterApply": true
}
```

### 3.2 Specializace: `PresetDto`

Rozšíření o granularitu a composability:

```json
{
  "...base fields...": "...",
  "granularity": "attribute | entity | aggregate | bundle",
  "appliesTo": "attribute | entity | multiple-entities",
  "composability": "safe | requires-review | incompatible-with[list]",
  "creates": {
    "businessShape": "attribute | entity | relation",
    "typeHint": "string | int32 | ...",
    "metadata": { "Validation.Email": true }
  },
  "parameters": [],
  "postApplyExpectedEdits": ["Add validation rules"]
}
```

### 3.3 Specializace: `CapabilityDto`

```json
{
  "...base fields...": "...",
  "kind": "capability",
  "operations": ["round", "abs", "min", "max"],
  "requiresForgeBlocks": ["mf.math"],
  "generatorHints": {
    "nugetPackages": ["MathNet.Numerics"],
    "additionalUsings": ["System.Math"]
  }
}
```

## 4. Granularita presetů

| Úroveň | Příklad | Popis |
|--------|---------|-------|
| `attribute` | `email`, `phone`, `money`, `url` | Jeden atribut s metadaty a validací |
| `entity` | `Vehicle`, `Customer`, `Invoice` | Celá entita s atributy a vztahy |
| `cross-cutting` | `Auditable`, `SoftDelete` | Mixin aplikovatelný na více entit |
| `bundle` | `CarDealership Basic Domain` | Sada entit pro celou doménu |
| `capability` | `mf.math`, `mf.validation` | ForgeBlock balík s operacemi |

## 5. MCP Discovery endpointy

| Endpoint | Popis |
|----------|-------|
| `catalog.listPresets` | Seznam všech presetů (stručný) |
| `catalog.getPreset` | Detail jednoho presetu |
| `catalog.listCapabilities` | Seznam ForgeBlock/capability balíčků |
| `catalog.getCapability` | Detail capability |
| `catalog.search` | Hledání podle tagů, názvu, intentu |
| `catalog.recommendForIntent` | Deterministic matching — vrátí kandidáty podle tagů + typu |

Všechny endpointy jsou **read-only** — host surface zůstává tenká, jen zpřístupňuje katalog.

## 6. Kde katalog žije

Dvě varianty (Perplexity otázka):
- **Runtime registry** — `CatalogManager` v paměti, MCP endpointy jako proxy
- **Verzovatelný obsah repa** — `Docs/Catalog/presets.json`, `Docs/Catalog/capabilities.json`

Doporučení: **obojí** — runtime pro MCP, verzovatelný JSON v repu pro review a governance.

## 7. Vztah k BusinessAuthoringDocument

- Preset se po aplikaci stává součástí `BusinessAuthoringDocument`
- Není to black-box — je to normální součást modelu, editovatelná přes Facade a CommandLog
- Není to bypass architektury — je to "předschválený artefakt"
- Změny po aplikaci jdou standardní cestou (Facade → PatchEngine → CommandLog)

## 8. Signál hodnoty

- **Jeden kontrakt pro všechny kanály** — CLI, MCP, AI agent, import — všichni mluví stejným jazykem
- **Agent může rozhodovat** — strojově čitelný JSON s `semanticTags`, `whenToUse`, `avoids`
- **Prevence host driftu** — jazyk žije nad Facade, ne v hostech
- **Discovery bez AI magie** — deterministic matching, ne "AI business logika v hostu"
- **Základ pro marketplace** — presety a capabilities jako distribuovatelné artefakty

## 9. Doporučený další krok

**Candidate Proposal** — odhad: 3-5 dní pro:
1. Definovat `CatalogItemDto` base shape (JSON schema)
2. Implementovat MCP discovery endpointy
3. Připravit prvních 20 presetů (built-in + doménové)
4. Vytvořit `Docs/Catalog/` s verzovatelným obsahem

Navazuje na: `IDEA-029` (ProductionHub integration), `IDEA-031` (Agent Playbook)
Závisí na: `CatalogManager`, `ForgeBlockRegistry`, MCP infrastruktura
