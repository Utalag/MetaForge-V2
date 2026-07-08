# PROP-041 ConstructorElement + FieldElement

Typ výsledku: Candidate Proposal
Zdroj podnětu: AI — Perplexity Deep Research (konverzace 2293d4a6)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-08

Priorita: High
Oblast: Core, Generators
Owner:
Datum vytvoření: 2026-07-08
Aktualizováno: 2026-07-08

Navazuje na:
- PROP-037 (C# Completeness) — chybějící elementy
- PROP-040 (Core Member Consistency) — IMemberElement
- Perplexity revize: https://www.perplexity.ai/search/2293d4a6-aca7-4219-aeac-8d3285213a71

Blokuje:
- PROP-043 (Core Test Expansion)

Související soubory:
- `Src/MetaForge.Core/Elements/Members/`
- `Src/MetaForge.Core/Elements/Types/ClassElement.cs`

## 1. Kontext

Perplexity Deep Research identifikoval dvě chybějící reprezentace v Core:

1. **ConstructorElement** — klasický konstruktor `public Foo(int x) { ... }` nemá svůj element. Primární konstruktory jsou jako `PrimaryConstructorParameters` na ClassElement, ale běžné konstruktory neexistují.
2. **FieldElement** — C# pole (`private int _count;`) nejsou reprezentována vůbec. Pro generování DI kontejnerů, instancí s readonly fields, nebo full tříd to bude chybět.

## 2. Problém dnes

- **Žádný ConstructorElement:** Non-record třídy nemají konstruktory v modelu. Generátor nemůže produkovat konstruktory s DI parametry nebo inicializační logikou.
- **Žádný FieldElement:** Private readonly fields (typický DI pattern: `private readonly ILogger _logger`) nelze modelovat. Generátor produkuje třídy bez polí, jen s properties.
- Generování realistických POCO/service tříd s DI je blokováno.

## 3. Cíl

- Vytvořit `ConstructorElement` pro reprezentaci C# konstruktorů (parametry, access modifier, tělo).
- Vytvořit `FieldElement` pro reprezentaci C# polí (private/public, readonly, static, inicializace).
- Integrovat do `ClassElement` (kolekce konstruktorů a polí).
- Implementovat `IMemberElement` (PROP-040).

## 4. Architektonické invarianty

- Core nesmí nést logiku, která patří do vyšší vrstvy.
- Konstruktory jsou additivní k PrimaryConstructorParameters.

## 5. Scope

### In scope
- `ConstructorElement` — Name, Parameters, AccessModifier, Body, Initializer (this()/base()).
- `FieldElement` — Name, Type, AccessModifier, IsReadOnly, IsStatic, DefaultValue.
- Integrace do `ClassElement` (Constructors, Fields kolekce).
- Factory metody (Basic, Private, Static).

### Out of scope
- Generátorové změny (samostatný follow-up).
- Destruktory/finalizery.
- Field inicializace jako Expression (pouze string pro MVP).

## 6. Návrh řešení

### ConstructorElement

```csharp
public sealed class ConstructorElement : IMemberElement
{
    public string Name { get; set; } = ".ctor";  // v C# konstruktor = jméno třídy
    public List<ParameterElement> Parameters { get; init; } = new();
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public BlockStatement? Body { get; set; }
    public string? Initializer { get; set; }  // "this(x, 0)" nebo "base(name)"
    public List<AttributeElement> Attributes { get; init; } = new();
    public MetadataBag Metadata { get; init; } = new();
    public string? XmlSummary { get; set; }
    public int Coin { get; set; } = 3;
}
```

### FieldElement

```csharp
public sealed class FieldElement : IMemberElement
{
    public string Name { get; set; } = string.Empty;
    public DataTypes.TypeModel Type { get; set; } = DataTypes.TypeModel.Object;
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Private;
    public bool IsReadOnly { get; set; }
    public bool IsStatic { get; set; }
    public string? DefaultValue { get; set; }  // string reprezentace
    public List<AttributeElement> Attributes { get; init; } = new();
    public MetadataBag Metadata { get; init; } = new();
    public string? XmlSummary { get; set; }
    public int Coin { get; set; } = 1;
}
```

### ClassElement rozšíření

```csharp
public List<ConstructorElement> Constructors { get; init; } = new();
public List<FieldElement> Fields { get; init; } = new();
```

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Src/MetaForge.Core/Elements/Members/ConstructorElement.cs` — nový
- `Src/MetaForge.Core/Elements/Members/FieldElement.cs` — nový
- `Src/MetaForge.Core/Elements/Types/ClassElement.cs` — +Constructors, +Fields

### Testy
- ConstructorElement: factory metody, parametry, initializer, TotalCoin
- FieldElement: factory metody, readonly, static, DefaultValue
- ClassElement: Constructors a Fields kolekce
- 20-25 nových testů

### Dokumentace
- Update Docs/Core/02-Type-Kinds.md
- Update New_Architecture/04-Core-Elements.md

## 8. Implementační fáze

### Fáze 1 — ConstructorElement
- Vytvořit třídu
- Factory metody (Basic, Private, Static, WithInitializer)
- Testy

### Fáze 2 — FieldElement
- Vytvořit třídu
- Factory metody (Basic, ReadOnly, Static, Constant)
- Testy

### Fáze 3 — Integrace
- Přidat Constructors a Fields do ClassElement
- Rozšířit TotalCoin o Constructors a Fields
