# Ready-to-Run implementační prompty

> Prompty pro malý model (Gemma 4 12B). Každý prompt je samostatný, lokálně srozumitelný a jednoznačný.

---

## Jak používat tyto prompty

1. Zadej prompt malému modelu jako celý kontext.
2. Model implementuje přesně to, co prompt říká.
3. Ověř výstup podle "Ověření" sekce.
4. Commitni výsledek.
5. Pokračuj dalším promptem v pořadí.

---

## Prompt 1 — Vytvoření solution souboru

```
Vytvoř nový .NET solution soubor `MetaForge.slnx` v kořenu repozitáře.

Požadavky:
- Formát: slnx (nový XML formát)
- Žádné projekty zatím
- Jen prázdná solution struktura

Výstupní soubor: MetaForge.slnx

Ověření: `dotnet build MetaForge.slnx` projde bez chyb (0 projektů).
```

---

## Prompt 2 — Vytvoření projektu MetaForge.Core

```
Vytvoř nový C# class library projekt:

Cesta: Src/MetaForge.Core/MetaForge.Core.csproj

Požadavky:
- TargetFramework: net9.0
- Nullable: enable
- ImplicitUsings: enable
- Žádné NuGet závislosti

Přidej projekt do solution MetaForge.slnx.

Ověření: `dotnet build Src/MetaForge.Core/MetaForge.Core.csproj` projde bez chyb.
```

---

## Prompt 3 — BaseType enum

```
Vytvoř soubor: Src/MetaForge.Core/DataTypes/BaseType.cs

Obsah:

namespace MetaForge.Core.DataTypes;

//context//
// Účel: Výčet základních typů v jazykově agnostickém typovém modelu.
// Vrstva: Core.
// Vstup: Žádný — je to enum.
// Výstup: Používá se v TypeModel jako základ typu.
// Závislosti: Žádné.
// Nezávislosti: Nezávisí na žádné jiné vrstvě.
// Invarianty: Nesmí obsahovat C#-specifické typy (string, int jsou zde jako abstrakce).
// Související typy: TypeModel.
// Testy: Core.Tests/DataTypes/BaseTypeTests.cs.

/// <summary>
/// Základní typy v jazykově agnostickém typovém modelu.
/// </summary>
public enum BaseType
{
    String,
    Int,
    Long,
    Float,
    Double,
    Decimal,
    Bool,
    DateTime,
    DateOnly,
    TimeOnly,
    Guid,
    Byte,
    Object
}

Ověření: Build projde. Enum má 13 hodnot.
```

---

## Prompt 4 — TypeModel record

```
Vytvoř soubor: Src/MetaForge.Core/DataTypes/TypeModel.cs

Obsah:

namespace MetaForge.Core.DataTypes;

//context//
// Účel: Immutable popis typu atributu nebo parametru v jazykově agnostické formě.
// Vrstva: Core.
// Vstup: BaseType + modifikátory.
// Výstup: Používá se v CatalogManager, Translator, Generator.
// Závislosti: BaseType enum.
// Nezávislosti: Nezávisí na Elements ani Generators.
// Invarianty: BaseType nesmí být default. IsCollection a IsNullable jsou nezávislé.
// Související typy: BaseType, PropertyElement, CatalogManager.
// Testy: Core.Tests/DataTypes/TypeModelTests.cs.

/// <summary>
/// Jazykově agnostický popis typu.
/// </summary>
public record TypeModel(
    BaseType BaseType,
    bool IsNullable = false,
    bool IsCollection = false,
    string? CollectionType = null,
    IReadOnlyList<TypeModel>? GenericArgs = null
);

Ověření: Build projde. Record má 5 properties.
```

---

## Prompt 5 — RootElement abstraktní třída

```
Vytvoř soubor: Src/MetaForge.Core/Abstractions/RootElement.cs

Obsah:

namespace MetaForge.Core.Abstractions;

//context//
// Účel: Bázová třída pro top-level elementy (ClassElement, InterfaceElement, EnumElement).
// Vrstva: Core.
// Vstup: Žádný — je to abstrakce.
// Výstup: Společné properties pro všechny top-level typy.
// Závislosti: Žádné.
// Nezávislosti: Nezávisí na Members. Nenese informaci o cílovém jazyce.
// Invarianty: Name nesmí být null ani prázdný. Namespace je volitelný.
// Související typy: ClassElement, InterfaceElement, EnumElement.
// Testy: Core.Tests/Abstractions/RootElementTests.cs.

/// <summary>
/// Bázová třída pro top-level elementy typového modelu.
/// </summary>
public abstract class RootElement
{
    /// <summary>Unikátní identifikátor elementu.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Název elementu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Volitelný namespace.</summary>
    public string? Namespace { get; set; }

    /// <summary>Druh elementu — implementuje potomek.</summary>
    public abstract string Kind { get; }

    /// <summary>Using direktivy potřebné pro tento element.</summary>
    public List<string> Usings { get; } = new();
}

Ověření: Build projde. Třída je abstract, nemá závislost na jazykové abstrakci cílového jazyka.
```

---

## Prompt 6 — CommandEnvelope record

```
Vytvoř soubor: Src/MetaForge.BusinessModel/CommandLog/CommandEnvelope.cs

Nejprve vytvoř projekt Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj:
- TargetFramework: net9.0
- Nullable: enable
- ImplicitUsings: enable
- Žádné NuGet závislosti

Přidej do solution.

Pak vytvoř CommandEnvelope:

namespace MetaForge.BusinessModel.CommandLog;

//context//
// Účel: Immutable záznam jednoho commandu v CommandLog. Append-only — nikdy se nemění.
// Vrstva: BusinessModel.
// Vstup: Vzniká v PatchEngine při mutaci dokumentu.
// Výstup: Ukládá se do CommandLogStore, čte se při replay.
// Závislosti: Žádné externí.
// Nezávislosti: Nezávisí na Core ani Translator.
// Invarianty: Id musí být unikátní. Timestamp musí být monotónně rostoucí. CommandType nesmí být prázdný.
// Související typy: CommandLogStore, ReplayEngine, PatchEngine.
// Testy: BusinessModel.Tests/CommandLog/CommandEnvelopeTests.cs.

/// <summary>
/// Immutable záznam jednoho commandu v CommandLog.
/// </summary>
public record CommandEnvelope(
    string Id,
    DateTimeOffset Timestamp,
    string CommandType,
    string PayloadJson
);

Ověření: Build projde. Record má 4 properties.
```

---

## Prompt 7 — CommandLogStore

```
Vytvoř soubor: Src/MetaForge.BusinessModel/CommandLog/CommandLogStore.cs

namespace MetaForge.BusinessModel.CommandLog;

//context//
// Účel: Append-only úložiště commandů. Žádný delete, žádný update.
// Vrstva: BusinessModel.
// Vstup: CommandEnvelope z PatchEngine.
// Výstup: Sekvence commandů pro replay.
// Závislosti: CommandEnvelope.
// Nezávislosti: Nezávisí na BusinessAuthoringDocument — pouze ukládá commandy.
// Invarianty: APPEND-ONLY. Count nikdy neklesá.
// Související typy: CommandEnvelope, ReplayEngine.
// Testy: BusinessModel.Tests/CommandLog/CommandLogStoreTests.cs.

/// <summary>
/// Append-only úložiště commandů.
/// </summary>
public class CommandLogStore
{
    private readonly List<CommandEnvelope> _commands = new();

    /// <summary>Počet commandů v logu.</summary>
    public int Count => _commands.Count;

    /// <summary>Přidá command do logu. Append-only — nelze mazat.</summary>
    public void Append(CommandEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        _commands.Add(envelope);
    }

    /// <summary>Vrací všechny commandy v pořadí vložení.</summary>
    public IReadOnlyList<CommandEnvelope> GetAll() => _commands.AsReadOnly();
}

Ověření: Build projde. Append zvyšuje Count. GetAll vrací všechny commandy.
```

---

## Prompt 8 — PROPOSALS.md governance soubor

```
Vytvoř soubor: PROPOSALS.md v kořenu repozitáře.

Obsah:

# MetaForge — PROPOSALS

> **Master checklist aktivních návrhů.** Detail každého plánu žije v `Docs/Plans/`.
> Bez schválení návrhu neimplementovat.

---

## Aktuální implementační pořadí

### 1. Základ

| Stav | Plán | Poznámka |
|------|------|----------|
| 🚧 Aktivní | [Plán 1 — Core typový model](Docs/Plans/plan-01-core-typovy-model.md) | Základní abstrakce a datové typy |
| 📝 Návrh | [Plán 2 — BusinessModel](Docs/Plans/plan-02-business-model.md) | Source of truth, CommandLog, replay |
| 📝 Návrh | [Plán 3 — Translator a Facade](Docs/Plans/plan-03-translator-facade.md) | Facade orchestrace, projekce |

### Praktické pravidlo řazení

- Core před BusinessModel.
- BusinessModel před Translator.
- Translator před Host Surfaces.
- Testy paralelně s každou vrstvou.

Ověření: Soubor existuje, má tabulku s alespoň 3 plány.
```

---

## Prompt 9 — Memories.md governance soubor

```
Vytvoř soubor: Memories.md v kořenu repozitáře.

Obsah:

# Memories

Aktivní provozní poznatky pro agent workflow.

## Šablona

### YYYY-MM-DDTHH:MM:SSZ — Krátký nadpis
- Programový blok: Core / BusinessModel / Translator / Host / ForgeBlock
- Priorita dodržování: Very High / High / Medium / Low
- Problém: Co se stalo.
- Příčina: Proč to nastalo.
- Řešení: Co funguje teď.
- Staré řešení: Předchozí postup, pokud existuje.

## Záznamy

(Zatím prázdné — první záznamy vzniknou s implementací.)

Ověření: Soubor existuje, má šablonu záznamu.
```

---

## Doporučené pořadí promptů

1. Prompt 1 — Solution
2. Prompt 2 — MetaForge.Core projekt
3. Prompt 3 — BaseType enum
4. Prompt 4 — TypeModel record
5. Prompt 5 — RootElement
6. Prompt 6 — CommandEnvelope + BusinessModel projekt
7. Prompt 7 — CommandLogStore
8. Prompt 8 — PROPOSALS.md
9. Prompt 9 — Memories.md

Po těchto 9 promptech má nový projekt:
- Solution se 2 projekty
- Core základ (typy, abstrakce)
- BusinessModel základ (CommandLog)
- Governance soubory (markdown-first)
