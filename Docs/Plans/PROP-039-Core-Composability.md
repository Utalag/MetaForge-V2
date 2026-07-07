# PROP-039 Core Composability — TransformPipeline, Mixin/Trait, ConventionRegistry

Typ výsledku: Candidate Proposal
Zdroj podnětu: Koumák — Perplexity konverzace e0609fe1 (rozšíření Core)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-07

Priorita: Medium
Oblast: Core, Generators
Owner:
Datum vytvoření: 2026-07-07
Aktualizováno: 2026-07-07

Navazuje na:
- Perplexity konverzace: https://www.perplexity.ai/search/e0609fe1-9b12-4655-9731-ffc0ab9d73aa
- PROP-038 (Core DX & Diagnostics) — předpokládá Fluent Builder API
- PROP-035 (C#-First Core Migration) — elementy musí být C#-first

Blokuje:
- —

## 1. Kontext

Perplexity identifikovalo tři středně náročné koncepty, které výrazně zvyšují kompozici a znovupoužitelnost Core modelu. Všechny sdílejí společné téma: **transformace a rozšiřování modelu před emisí kódu**.

## 2. Problém dnes

- **Žádná transformační pipeline:** Každá úprava modelu před generováním (např. přidání INotifyPropertyChanged) se musí dělat ručně v kódu generátoru.
- **Žádný mechanismus pro sdílené chování:** `IAuditable` (CreatedAt, UpdatedAt) se musí kopírovat do každé třídy ručně.
- **Žádné globální konvence:** Pojmenovávací pravidla (PascalCase, I-prefix) se aplikují ad-hoc.

## 3. Cíl

- **TransformPipeline:** Middleware chain nad immutable modelem. Každý `IModelTransform` je čistá funkce: `TypeModel → TypeModel`.
- **Mixin/Trait systém:** Deklarativní sada `PropertyElement` + `MethodElement`, která se build-time expanduje do `ClassElement`.
- **ConventionRegistry:** Globální pravidla (pojmenování, struktura) s možností override per-element.

## 4. Architektonické invarianty

- Core nesmí nést logiku, která patří do vyšší vrstvy.
- Transformy jsou čisté funkce — žádné side effects.
- Mixiny jsou build-time expanze — žádný runtime weaving.
- Konvence jsou overrideovatelné per-element.

## 5. Scope

### In scope
- **TransformPipeline** s `IModelTransform`, `TransformContext`, chainováním a podmíněnými transformy.
- **3 vestavěné transformy:** `NotNullableAnnotationsTransform`, `NotifyPropertyChangedTransform`, `InjectLoggerTransform`.
- **Mixin/Trait:** `ElementMixin` jako `IReadOnlyList<PropertyElement>` + `IReadOnlyList<MethodElement>`.
- **2 vestavěné mixiny:** `IAuditableMixin` (CreatedAt, UpdatedAt, CreatedBy), `ISoftDeleteMixin` (IsDeleted, DeletedAt).
- **ConventionRegistry:** globální pojmenovávací pravidla, `IConvention` interface.

### Out of scope
- Runtime weaving / AOP — jen build-time expanze.
- Vizuální designer pro pipeline.
- Automatická inference pojmenování z existujícího kódu.

## 6. Návrh řešení

### 6.1 TransformPipeline

```csharp
public interface IModelTransform
{
    TypeModel Apply(TypeModel model, TransformContext context);
    TransformCapabilities Capabilities { get; }
}

public sealed record TransformCapabilities(
    bool ModifiesTypes,       // Přidává/mění ClassElement
    bool ModifiesMembers,     // Přidává/mění Property/Method
    bool ModifiesExpressions  // Mění AST
);

public sealed class TransformPipeline
{
    private readonly List<IModelTransform> _transforms = [];

    public TransformPipeline Add(IModelTransform transform) { ... }

    public TransformPipeline ConditionalAdd(
        Func<TransformContext, bool> predicate,
        IModelTransform transform) { ... }

    public TypeModel Apply(TypeModel model, TransformContext? context = null)
    {
        var current = model;
        foreach (var transform in _transforms)
            current = transform.Apply(current, context ?? new());
        return current;
    }
}
```

### 6.2 Mixin/Trait

```csharp
public sealed record ElementMixin(
    string Name,
    IReadOnlyList<PropertyElement> Properties,
    IReadOnlyList<MethodElement> Methods,
    IReadOnlyList<AttributeElement>? Attributes = null
);

public static class Mixins
{
    public static readonly ElementMixin Auditable = new("Auditable",
        Properties: [
            new("CreatedAt", TypeModel.DateTime) { IsReadOnly = true },
            new("UpdatedAt", TypeModel.DateTime) { IsReadOnly = true },
            new("CreatedBy", TypeModel.String) { IsReadOnly = true }
        ],
        Methods: [
            new("MarkUpdated") { Body = ... }
        ]);

    public static readonly ElementMixin SoftDelete = new("SoftDelete",
        Properties: [
            new("IsDeleted", TypeModel.Bool) { AccessModifier = AccessModifier.Private },
            new("DeletedAt", TypeModel.DateTime) { AccessModifier = AccessModifier.Private }
        ],
        Methods: [
            new("SoftDelete") { ... },
            new("Restore") { ... }
        ]);
}
```

**Aplikace mixinu:** Build-time expanze — `ClassElement.Mixins.Add(Mixins.Auditable)` → při `.Build()` se properties a methods přidají.

### 6.3 ConventionRegistry

```csharp
public interface IConvention
{
    string Name { get; }
    ConventionScope Scope { get; }
    bool AppliesTo(RootElement element);
    void Apply(RootElement element, ConventionContext context);
}

public enum ConventionScope { Type, Member, Global }

public sealed class ConventionRegistry
{
    public void Register(IConvention convention) { ... }
    public void ApplyTo(TypeModel model) { ... }
}

// Vestavěné konvence:
public sealed class PascalCasePropertiesConvention : IConvention { ... }
public sealed class InterfacePrefixConvention : IConvention { ... }
public sealed class AsyncSuffixConvention : IConvention { ... }
```

## 7. Implementační dopad

- Nové: `Src/MetaForge.Core/Transforms/` — IModelTransform, TransformPipeline, TransformContext, TransformCapabilities
- Nové: `Src/MetaForge.Core/Transforms/BuiltIn/` — NotNullableAnnotationsTransform, NotifyPropertyChangedTransform, InjectLoggerTransform
- Nové: `Src/MetaForge.Core/Mixins/` — ElementMixin, Mixins (Auditable, SoftDelete)
- Nové: `Src/MetaForge.Core/Conventions/` — IConvention, ConventionRegistry, ConventionContext
- Nové: `Src/MetaForge.Core/Conventions/BuiltIn/` — PascalCase, InterfacePrefix, AsyncSuffix
- Upravené: `ClassElement` — `+Mixins` kolekce

### Testy

- TransformPipeline: chain 3 transformů → ověřit výstupní model.
- Mixin: `ClassElement` + `Auditable` → ověřit přidané properties.
- Convention: `PascalCase` → ověřit přejmenování properties.

## 8. Implementační fáze

### Fáze 1: TransformPipeline (4-6h)
- IModelTransform, TransformPipeline, 3 vestavěné transformy
- **DoD:** Chain 3 transformů, výstupní model odpovídá očekávání

### Fáze 2: Mixin/Trait (3-5h)
- ElementMixin, Mixins.Auditable, Mixins.SoftDelete
- Integrace do ClassElement (build-time expanze)
- **DoD:** ClassElement s Mixin.Auditable má CreatedAt, UpdatedAt, CreatedBy

### Fáze 3: ConventionRegistry (2-4h)
- IConvention, ConventionRegistry, 3 vestavěné konvence
- **DoD:** Aplikace PascalCase konvence přejmenuje properties

**Celkem: 9-15h (1-2 dny)**

## 9. Otevřené otázky

- OQ-039-01: Má být TransformPipeline součástí Core, nebo samostatný projekt?
- OQ-039-02: Mají mixiny podporovat i přidávání atributů a XML dokumentace?
- OQ-039-03: Má ConventionRegistry umožňovat per-element override, nebo jen globální?

## 10. Rizika a trade-offy

- Riziko: Mixiny jsou build-time expanze — při debugování není vidět "původní" element. Řešení: source map.
- Riziko: TransformPipeline může zpomalit build. Řešení: incremental (hash-based skip).
- Trade-off: Immutable transformy vs mutable model — immutable je bezpečnější, ale alokuje víc.

## 11. Validace

- Build: Všechny nové soubory se zkompilují.
- Testy: TransformPipeline chain, Mixin expanze, Convention aplikace.
- Smoke: Model s Auditable mixinem → generovaný kód má CreatedAt/UpdatedAt.

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.
