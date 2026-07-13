# IDEA-029 ProductionHub — End-to-End Pipeline Integration Test

Stav: Idea → Rozhodnuto (po diskusi s Perplexity)
Oblast: Core, Generators, Tests, Integration
Zdroj: Koumák + Perplexity — konverzace db7f49c1-8f18-4f2e-8da2-b9b6e1f5c122 + Gemini muster
Datum vytvoření: 2026-07-11
Poslední revize: 2026-07-11 (finalizováno po diskusi s Perplexity)

## 1. Kontext

Uživatel nechal vygenerovat testovací projekt "ProductionHub" — fiktivní systém automatizace skladu. Projekt slouží jako "muster" pro otestování průřezu celým MetaForge systémem:
- Definice na vysoké úrovni (JSON deklarativní zápis)
- Průchod Core modelem
- Generování C# kódu přes Generators
- Výstup musí být validní C# odpovídající mustru

Klíčový twist: vstupní JSON nepoužívá Vogen, ale výstupní kód by měl Vogen obsahovat (pro správně označené elementy). To znamená, že pipeline musí zahrnovat enrichment/transform krok.

## 2. Problém dnes

### 2.1 Chybějící AST prvky (odhaleno analýzou)

| Gap | Popis | Dopad |
|-----|-------|-------|
| **VariableDeclarationStatement** | Lokální proměnná `string categoryDescription;`, `bool isWaterproof = ...`, `int coolingCycles = 3;` | Muster nelze plně reprezentovat bez deklarace proměnných |
| **ForStatement omezený** | Jen `Variable`, `Start`, `End`, `Body` — ne obecný C# `for (int i=0; i<n; i++)` | Inicializace, podmínka a inkrement jsou zjednodušené |
| **AssignmentStatement omezený** | Levá strana je prostý `string Variable` — nepodporuje `variable += expr` ani `dict[key] = value` | `_totalProcessedCycles += 1`, `_activeInventory[key] = value` nelze |
| **String interpolace** | `$"{batchId}_UNIT_{i:D3}"` — není v Expression AST | Není pokryto |
| **Indexer přístup** | `dict[key]` — chybí `IndexerExpression` nebo `ElementAccessExpression` | Není pokryto |
| **NewExpression renderer** | NewExpression v Core existuje, ale není jasně doložené renderování | Nutno ověřit testem |

### 2.2 Známé bugy

| Bug | Popis | Zdroj |
|-----|-------|-------|
| **MakeCollection()** | Nekopíruje typ do GenericArguments → `Dictionary<string, ItemMetadata>` se renderuje jako `Dictionary<object, object>` | PROP-032 code review |

### 2.3 Vogen enrichment otázka

- `ItemMetadata` je compound type (3 properties), ne ideální Vogen kandidát. Lepší: `MaterialCode`, `BatchId`, `SystemIdentifier`.
- Enrichment musí být explicitní (metadata flag), ne "tichá heuristika" — jinak golden test není deterministický.
- JSON vstup zatím nerozlišuje pole vs property vs constructor semantics dost bohatě.

## 3. Předběžný směr řešení

### 3.1 Dva testovací scénáře (dle Perplexity)

**Baseline test: `ProductionHub_Baseline_NoEnrichment`**
- JSON vstup → Core model → C# kód **bez Vogen magie**
- Ověřuje: parser/translator correctness, generator correctness
- Přiznává mezery: lokální proměnné, interpolace, indexer, plný `for`
- Success: syntakticky validní C#, sémanticky odpovídající mustru (v rámci omezení)

**Enriched test: `ProductionHub_WithValueObjectVogen`**
- Stejný JSON vstup + metadata flag `"domain.representation": "value-object"` a `"generation.provider": "vogen"`
- Aplikuje se na `MaterialCode` (nebo `BatchId`), ne na celé `ItemMetadata`
- Ověřuje: domain enhancement correctness přes ForgeBlock/transform
- Success: výstup obsahuje Vogen `[ValueObject]` annotated typy

### 3.2 Potřebné Core rozšíření (před baseline testem)

| Priorita | Rozšíření | Kategorie |
|----------|-----------|-----------|
| 🔴 P1 | `VariableDeclarationStatement` — lokální deklarace proměnných s volitelnou inicializací | Statement AST |
| 🔴 P1 | Rozšíření `AssignmentStatement` — levá strana jako `Expression`, ne jen `string` | Statement AST |
| 🟡 P2 | `InterpolatedStringExpression` — string interpolace s embedded výrazy | Expression AST |
| 🟡 P2 | `IndexerExpression` / `ElementAccessExpression` — `dict[key]` | Expression AST |
| 🟡 P2 | Rozšíření `ForStatement` — obecnější inicializace/podmínka/inkrement | Statement AST |
| 🟢 P3 | Oprava `MakeCollection()` — kopírování typu do GenericArguments | TypeModel |

### 3.3 JSON schema rozšíření

Aktuální JSON z Gemini je příliš zjednodušený. Pro baseline test je potřeba rozšířit:

```json
{
  "components": [{
    "type": "class",
    "name": "AutomationController",
    "fields": [
      {"name": "_totalProcessedCycles", "data_type": "int32", "access": "private"},
      {"name": "SystemIdentifier", "data_type": "string", "access": "public", "kind": "property"},
      {"name": "CurrentCoreTemperature", "data_type": "float64", "access": "protected", "kind": "field"},
      {"name": "_systemLogs", "data_type": "list<string>", "access": "private", "kind": "field"},
      {"name": "_activeInventory", "data_type": "map<string, ItemMetadata>", "access": "private", "kind": "field"}
    ],
    "constructors": [{...}],
    "methods": [{
      "name": "RegisterAndRouteBatch",
      "return_type": "boolean",
      "parameters": [...],
      "body": {
        "statements": [
          {"kind": "assignment", "target": "_totalProcessedCycles", "op": "+=", "value": 1},
          {"kind": "variable_declaration", "type": "string", "name": "categoryDescription"},
          {"kind": "switch", "selector": "typeCode.ToUpper()", "cases": [...]},
          {"kind": "for", "init": "int i = 0", "condition": "i < itemQuantity", "increment": "i++", "body": [...]},
          {"kind": "while", "condition": "coolingCycles > 0", "body": [...]}
        ]
      }
    }]
  }]
}
```

Pro enriched variant:
```json
{
  "fields": [
    {"name": "MaterialCode", "data_type": "string", "access": "public", "kind": "property",
     "metadata": {
       "domain.representation": "value-object",
       "generation.provider": "vogen",
       "validation.not_empty": true,
       "validation.regex": "^[A-Z]{2,4}$"
     }
    }
  ]
}
```

## 4. Signál hodnoty

- **První skutečný end-to-end test**: JSON → Core → C#, žádný mock.
- **Odhalení nezdokumentovaných mezer**: lokální proměnné, indexer, interpolace, ForStatement limity.
- **Validace architektury**: ověří, že `BusinessAuthoringDocument` → Core → Generator pipeline funguje pro reálný scénář.
- **Vogen enrichment**: ověří, že ForgeBlock/transform vrstva dokáže obohatit výstup o value objects.
- **Základ pro další integration testy**: úspěšný baseline se stane golden testem pro regrese.
- Zapadá do Epic 2 (Core), Epic 7 (Generators), Epic 9 (Testy).

## 5. Rizika a nejasnosti

- **Rozsah Core rozšíření**: VariableDeclarationStatement + AssignmentStatement rozšíření + interpolace + indexer = odhad 3-5 dní práce. Je to priorita?
- **ForStatement rozšíření**: současný zjednodušený ForStatement funguje pro testy — rozšiřovat ho na obecný C# `for` může být overkill.
- **JSON schema**: kdo definuje "oficiální" JSON schema pro MetaForge vstup? Je to Translator vrstva? BusinessModel?
- **Vogen enrichment**: vyžaduje existenci ForgeBlock pro Vogen — ten zatím není implementovaný.
- **Golden test determinismus**: s enrichment krokem je těžší definovat očekávaný výstup — enrichment se může měnit nezávisle na generátoru.

## 6. Doporučený další krok

**Candidate Proposal** — odhadovaná priorita: P1 (po dokončení IDEA-025 a IDEA-026)

Postup:
1. **Fáze 1**: Rozšířit Core o VariableDeclarationStatement a AssignmentStatement jako Expression (2 dny)
2. **Fáze 2**: Vytvořit baseline integration test `ProductionHub_Baseline` s přiznanými gapy (1 den)
3. **Fáze 3**: Opravit MakeCollection bug (0.5 dne)
4. **Fáze 4**: Přidat InterpolatedStringExpression + IndexerExpression (2 dny)
5. **Fáze 5**: Vytvořit enriched test `ProductionHub_Vogen` (1 den)
6. **Fáze 6**: Implementovat ForgeBlock pro Vogen enrichment (2-3 dny)

Celkem: ~9 dní

Navazuje na: `IDEA-025` (Generator Render Core Tests), `IDEA-026` (Architecture Decision)
Závisí na: Oprava MakeCollection bug, ForgeBlock Vogen
Otevřené otázky:
- OQ: Kdo definuje JSON schema pro MetaForge vstup?
- OQ: Má se ForStatement rozšiřovat na obecný C# `for`, nebo stačí současný zjednodušený model?
