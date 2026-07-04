---
name: new-architecture-test-scaffold
description: "Pouzij pri: navrhu a implementaci testu — TestDocumentBuilder, TestCommandLogBuilder, testovaci konvence, xUnit, FluentAssertions, test helpery."
---

# new-architecture-test-scaffold

Zajistit konzistentní testování napříč všemi vrstvami dle `15-Test-Scaffold.md`. Testy jsou first-class citizen — vznikají s každou vrstvou.

## Kdy použít

- Při psaní nových testů
- Při refaktoringu existujících testů
- Při návrhu test helperů
- Při kontrole test coverage

## Testovací principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **Testy jsou first-class citizen** | Vznikají s každou vrstvou, ne až na konci |
| 2 | **Unit testy preferovány** | Rychlé, izolované, deterministické |
| 3 | **Integration testy pro facade** | Ověření orchestrace přes vrstvy |
| 4 | **Žádné testy závislé na AI** | AI path má graceful fallback, testuje se deterministická cesta |
| 5 | **Replay testy** | Ověření, že replay N commandů = expected state |
| 6 | **Generator output testy** | Ověření, že generovaný C# je kompilabilní |

## Test projekty

| Projekt | Co testuje | Typ testů |
|---------|-----------|-----------|
| `MetaForge.Core.Tests` | Typový model, katalog, ForgeBlock registrace, inference | Unit |
| `MetaForge.BusinessModel.Tests` | Dokument, CommandLog, replay, patches | Unit |
| `MetaForge.Translator.Tests` | Facade, projekce, překlad, write-back | Unit + Integration |
| `MetaForge.Generators.Tests` | C# output, kompilabilnost | Unit + Snapshot |

## Testovací konvence

- **Framework:** xUnit
- **Assertions:** FluentAssertions
- **Naming:** `{Třída}_{Metoda}_{Scénář}` nebo `{Třída}_{Scénář}_{Výsledek}`
- **Pattern:** Arrange-Act-Assert vždy
- **Jeden assert per test** (preferenčně, ne dogmaticky)
- **Žádné mocking frameworky** — preferuj fakes a in-memory implementace

## Vzor testu — AAA pattern

```csharp
[Fact]
public void Replay_EmptyLog_ReturnsEmptyDocument()
{
    // Arrange
    var engine = new ReplayEngine();
    var commands = new List<CommandEnvelope>();

    // Act
    var doc = engine.Replay(commands);

    // Assert
    doc.Entities.Should().BeEmpty();
}
```

## Klíčové testovací scénáře

### Core.Tests
- `TypeModel_DefaultValues` — výchozí hodnoty recordů
- `CatalogManager_RegisterAndResolve` — registrace a vyhledání
- `CatalogManager_ResolveUnknown_ReturnsNull` — neznámý typ = null
- `ForgeBlockRegistry_DuplicateHandle_Throws` — duplicitní registrace
- `RuleBasedConstraintInferencer_KnownPattern_InfersConstraints` — inference

### BusinessModel.Tests
- `PatchEngine_AddEntity_AddsToDocument` — entita přidána
- `PatchEngine_AddEntity_CreatesLogEntry` — log záznam
- `ReplayEngine_EmptyLog_EmptyDocument` — prázdný log
- `ReplayEngine_NCommands_CorrectState` — N commandů
- `ReplayEngine_Deterministic` — determinismus
- `CommandLogStore_AppendOnly` — count nikdy neklesá

### Translator.Tests
- `Facade_AddEntity_IntegratesWithPatchAndLog` — write path
- `Translator_KnownType_ReturnsCorrectTypeModel` — překlad
- `Translator_UnknownType_ReturnsObject` — fallback

### Generators.Tests
- `CSharpGenerator_ClassElement_ValidOutput` — obsahuje "public class"
- `CSharpGenerator_WithProperties_GeneratesAll` — všechny properties
- `CSharpGenerator_EmptyName_Throws` — error handling
- `CSharpGenerator_Enum_ContainsEnumDeclaration` — enum generování

## Anti-patterny

- ❌ Testy závislé na AI
- ❌ Sdílený stav mezi testy (každý test má fresh instanci)
- ❌ Testy bez AAA patternu
- ❌ Mockování všeho (preferuj in-memory implementace)

## Výstupní checklist

- [ ] Testy jsou součástí stejného PR jako implementace
- [ ] Unit testy preferovány
- [ ] xUnit + FluentAssertions
- [ ] Žádné testy závislé na AI
- [ ] AAA pattern dodržen
- [ ] Každý Src projekt má odpovídající Tests projekt
