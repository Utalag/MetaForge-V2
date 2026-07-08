# PROP-038: Core DX, Diagnostics & Pipeline — Fluent Builder, MetadataBag, DiagnosticBag, TransformPipeline

Typ výsledku: Candidate Proposal
Zdroj podnětu: Koumák — Perplexity konverzace e0609fe1 (rozšíření Core)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-08

Priorita: High
Oblast: Core, Generators
Owner:
Datum vytvoření: 2026-07-07
Aktualizováno: 2026-07-08

Navazuje na:
- Perplexity konverzace: https://www.perplexity.ai/search/e0609fe1-9b12-4655-9731-ffc0ab9d73aa
- PROP-035 (C#-First Core Migration) — Fluent Builder API
- PROP-039 — Mixin MVP (TransformPipeline rozhraní používá mixiny)

Blokuje:
- PROP-039 (vyžaduje TransformPipeline)
- Generátorová pipeline (vyžaduje DiagnosticBag)

Související soubory:
- `Src/MetaForge.Core/Elements/` — elementy + MetadataBag + AttributeElement
- `Src/MetaForge.Core/Diagnostics/` — nový namespace
- `Src/MetaForge.Core/Transforms/` — nový namespace

---

## 1. Kontext

Perplexity analyzovalo Core a identifikovalo "game changer" nápady. Následná diskuse vykrystalizovala do kompromisu:

> **AttributeElement** zůstává pro C#-specific atributy (`[Required]`, `[JsonIgnore]`).
> **MetadataBag** přibývá jako univerzální anotační systém (dokumentace, generátorové hinty, multi-target).
> **XmlDocElement** se neručí — je plně nahrazen MetadataBag klíči `Docs.*`.
> **TransformPipeline** se přesouvá z PROP-039 sem (DiagnosticBag + Pipeline patří k sobě).

### Filozofický princip

> **Core je read-only.** Nikdo needituje Core přímo. Všechny změny tečou přes BusinessAuthoringDocument. Core je derivovaný, deterministický, verzovatelný artefakt.

---

## 2. Problém dnes

- **Fluent Builder API:** Existuje jako `With*` metody na elementech, ale chybí top-level `TypeModel.Define()` entry point.
- **Žádný jednotný anotační systém:** Atributy jsou `AttributeElement` s `object?` argumenty. Dokumentace chybí úplně. Generátorové hinty neexistují.
- **Žádná strukturovaná diagnostika:** Chyby a varování se řeší ad-hoc, výjimkami nebo logováním.
- **Žádná transformační pipeline:** Každá úprava modelu před generováním se musí dělat ručně.

---

## 3. Cíl

- **Fluent Builder API:** Jednotný `TypeModel.Define("X").Class("Y", ...).Build()` entry point.
- **MetadataBag:** Univerzální key-value anotace na každém elementu. `Validation.Required=true`, `Docs.Summary="..."`, `Generation.Ignore=false`.
- **AttributeElement:** Ponechán pro C#-specific `[Attribute]` zápis. Komplementární k MetadataBag.
- **DiagnosticBag:** Strukturovaná diagnostika s fázovým sběrem, monadickým `BuildResult<T>`, 4 severities, `ElementPath`.
- **TransformPipeline:** Middleware chain nad immutable modelem. Každý `IModelTransform` je čistá funkce.

---

## 4. Architektonické invarianty

1. **BusinessAuthoringDocument je jediný source of truth.** Vše ostatní je generované a read-only.
2. **Core je read-only derivace.** Nikdo needituje Core přímo. Všechny změny tečou přes BusinessAuthoringDocument.
3. **Builder je thin facade.** `.Build()` vrací immutable element. Žádný mutable state mimo lambda.
4. **DiagnosticBag je additivní.** Existující kód se nemění.
5. **Transformy jsou čisté funkce.** Žádné side effects. `Apply()` nesmí mutovat vstup.
6. **MetadataBag je komplementární k AttributeElement.** Oba storage, jeden dotazovací bod pro generátor.
7. **Deterministický výstup.** Stejný input → stejný output. Vždy.

---

## 5. Scope

### In scope
- **Fluent Builder API** — `TypeModel.Define()` entry point + buildery pro všechny elementy.
- **MetadataBag** — key-value anotace, `MetadataScope` (Domain/Validation/Generation/Ai/Documentation), merge.
- **DiagnosticBag** — 4 severities (Hidden/Info/Warning/Error), `ElementPath` struktura, `BuildResult<T>`, 3 reportéry.
- **TransformPipeline** — `IModelTransform`, `TransformContext`, chainování, fail-fast.
- **Vestavěné transformy** — `AttributeReflectionTransform` (AttributeElement → MetadataBag reflexe).

### Out of scope
- Konkrétní C#-generující transformy (NotifyPropertyChanged atd.) — až PROP-039.
- Generování XML dokumentace z modelu — role Generators, ne Core.
- IDE integrace (LSP) — až později.

---

## 6. Návrh řešení

### 6.1 Fluent Builder API

**Rozhodnutí:** Stávající `With*` metody na elementech zůstávají. Přidává se top-level `TypeModel.Define()` jako entry point pro celý model.

```csharp
// Entry point — definice celého modelu
var model = TypeModel.Define("MyApp.Domain")
    .Class("Order", cls => cls
        .Sealed()
        .Implements("IAggregate")
        .Property("Id", StrongType.Guid, p => p.Init())
        .Property("Status", "OrderStatus", p => p.GetSet())
        .Method("Place", m => m
            .Returns(StrongType.Void)
            .ExpressionBody(...))
        .Metadata(m => m
            .Set("Docs.Summary", "Represents a customer order."))
    )
    .Enum("OrderStatus", e => e
        .Member("Pending")
        .Member("Active"))
    .Build();

// Builder vrací immutable TypeModel
public class TypeModelBuilder
{
    public TypeModelBuilder Class(string name, Action<ClassBuilder> configure);
    public TypeModelBuilder Enum(string name, Action<EnumBuilder> configure);
    public TypeModelBuilder Interface(string name, Action<InterfaceBuilder> configure);
    public TypeModel Build();
}

public class ClassBuilder
{
    public ClassBuilder Sealed();
    public ClassBuilder Partial();
    public ClassBuilder Abstract();
    public ClassBuilder Record();
    public ClassBuilder Implements(string interfaceName);
    public ClassBuilder Property(string name, StrongType type, Action<PropertyBuilder> configure);
    public ClassBuilder Method(string name, Action<MethodBuilder> configure);
    public ClassBuilder Attribute(string attributeName, params object?[] args);
    public ClassBuilder Metadata(Action<MetadataBag> configure);
    public ClassElement Build();
}
```

### 6.2 MetadataBag

```csharp
public enum MetadataScope { Domain, Validation, Generation, Ai, Documentation }

public sealed class MetadataBag
{
    public MetadataBag Set<T>(string key, T value, MetadataScope scope = MetadataScope.Domain);
    public T? Get<T>(string key);
    public bool Has(string key);
    public IEnumerable<MetadataEntry> Where(MetadataScope scope);
    public MetadataBag Merge(MetadataBag other, MergeStrategy strategy = MergeStrategy.Override);
}

public enum MergeStrategy { Override, Skip, Throw }

public sealed record MetadataEntry(string Key, object? Value, MetadataScope Scope);
```

**Standardizované klíče:**

| Klíč | Typ | Scope | Význam |
|------|-----|-------|--------|
| `Validation.Required` | `bool` | Validation | `[Required]` |
| `Validation.MinLength` | `int` | Validation | `[MinLength(n)]` |
| `Validation.MaxLength` | `int` | Validation | `[MaxLength(n)]` |
| `Validation.Range.Min` | `double` | Validation | `[Range(min, max)]` |
| `Validation.Range.Max` | `double` | Validation | `[Range(min, max)]` |
| `Docs.Summary` | `string` | Documentation | `/// <summary>` |
| `Docs.Param.{name}` | `string` | Documentation | `/// <param name="...">` |
| `Docs.Returns` | `string` | Documentation | `/// <returns>` |
| `Docs.Exception.{type}` | `string` | Documentation | `/// <exception cref="...">` |
| `Docs.Remarks` | `string` | Documentation | `/// <remarks>` |
| `Generation.Ignore` | `bool` | Generation | Přeskočit generování |
| `Generation.UsePartial` | `bool` | Generation | Generovat jako partial class |
| `Generation.FileName` | `string` | Generation | Vlastní název souboru |
| `Ai.Context` | `string` | Ai | Kontext pro AI inference |
| `Ai.Example` | `string` | Ai | Příklad pro few-shot |
| `Domain.BusinessName` | `string` | Domain | Business název (čeština) |
| `Domain.Glossary` | `string` | Domain | Business glossary |

### 6.3 AttributeElement

Ponechán beze změny — viz `Src/MetaForge.Core/Abstractions/AttributeElement.cs`.

```csharp
// Existující — beze změny
public sealed class AttributeElement
{
    public string Name { get; set; } = string.Empty;
    public List<object?> Arguments { get; } = new();
}
```

### 6.4 Diagnostický model — DiagnosticBag

```csharp
public enum DiagnosticSeverity { Hidden, Info, Warning, Error }

public sealed record ElementPath(
    string Root,      // "TypeModel"
    string Element,   // "ClassElement/Order"
    string? Segment   // "PropertyElement/Total" nebo null
);

public sealed record Diagnostic(
    string Code,                    // "MF-DB-001"
    string Message,                 // lidsky čitelná
    DiagnosticSeverity Severity,    // Hidden/Info/Warning/Error
    ElementPath Location,           // přesná pozice v modelu
    string? HelpUrl = null,         // odkaz na dokumentaci
    string? SuggestedFix = null     // hint pro tooling
);

public interface IDiagnosticCollector
{
    void Report(Diagnostic diagnostic);
    bool HasErrors { get; }
    IReadOnlyList<Diagnostic> ToReadOnly();
}

public sealed class DiagnosticBag : IDiagnosticCollector
{
    private readonly List<Diagnostic> _items = new();
    public void Report(Diagnostic d) => _items.Add(d);
    public bool HasErrors => _items.Any(d => d.Severity == DiagnosticSeverity.Error);
    public IReadOnlyList<Diagnostic> ToReadOnly() => _items.AsReadOnly();
}

public interface IDiagnosticReporter
{
    void Report(IReadOnlyList<Diagnostic> diagnostics);
}

// Vestavěné reportéry
public sealed class ConsoleDiagnosticReporter : IDiagnosticReporter { ... }
public sealed class JsonDiagnosticReporter : IDiagnosticReporter { ... }
public sealed class InMemoryDiagnosticReporter : IDiagnosticReporter { ... }
```

### 6.5 Fázový model — BuildResult\<T\>

```csharp
// Výsledek každé fáze pipeline — monadický wrapper
public sealed record BuildResult<T>(T Value, DiagnosticBag Bag)
{
    public bool IsSuccess => !Bag.HasErrors;

    // Monadické chainování — zastaví se při Error
    public BuildResult<TOut> Then<TOut>(Func<T, BuildResult<TOut>> next)
        => IsSuccess ? next(Value) : new(default!, Bag);
}
```

**Fázový sběr diagnostiky:**

```
[Model Build Phase]        [Transform Phase]         [Emit Phase]
TypeModel.Build()    →    IModelTransform.Apply() → IEmitTarget.Emit()
       │                         │                        │
       ▼                         ▼                        ▼
  DiagnosticBag          TransformContext.Diag       EmitContext.Diag
(structural errors)     (semantic warnings)         (output warnings)
```

Pravidlo: Pipeline se zastaví při první `Error`. Warnings a Info procházejí všemi fázemi.

### 6.6 TransformPipeline

```csharp
public interface IModelTransform
{
    string Name { get; }                                      // "AttributeReflection"
    TypeModel Apply(TypeModel model, TransformContext context);
}

public sealed class TransformContext
{
    public IDiagnosticCollector Diagnostics { get; }
    public PipelineOptions Options { get; }
    public IReadOnlyDictionary<string, object> State { get; } // cross-transform sdílení
}

public sealed class PipelineOptions
{
    public bool FailFast { get; set; } = true;
    public bool EnableReflection { get; set; } = true;       // AttributeElement → MetadataBag
}

public sealed class TransformPipeline
{
    private readonly List<IModelTransform> _transforms = new();

    public TransformPipeline Add(IModelTransform transform);
    public TransformPipeline ConditionalAdd(
        Func<TransformContext, bool> predicate,
        IModelTransform transform);

    public BuildResult<TypeModel> Run(TypeModel model, PipelineOptions? options = null)
    {
        var bag = new DiagnosticBag();
        var ctx = new TransformContext(bag, options ?? new());
        var current = model;

        foreach (var step in _transforms)
        {
            current = step.Apply(current, ctx);
            if (ctx.Diagnostics.HasErrors && ctx.Options.FailFast)
                break;
        }

        return new BuildResult<TypeModel>(current, bag);
    }
}
```

### 6.7 AtributeReflectionTransform

```csharp
// Transform: AttributeElement → MetadataBag reflexe
// Aby generátor měl jednotný dotazovací bod primárně z MetadataBag
public class AttributeReflectionTransform : IModelTransform
{
    public string Name => "AttributeReflection";

    public TypeModel Apply(TypeModel model, TransformContext ctx)
    {
        foreach (var element in model.GetAllElements())
        {
            foreach (var attr in element.Attributes)
            {
                var key = attr.Name switch
                {
                    "Required" or "RequiredAttribute" => "Validation.Required",
                    "JsonIgnore" or "JsonIgnoreAttribute" => "Generation.JsonIgnore",
                    "StringLength" or "StringLengthAttribute" => "Validation.MaxLength",
                    "Range" or "RangeAttribute" => "Validation.Range.Min",
                    _ => null
                };

                if (key != null && !element.Metadata.Has(key))
                {
                    element.Metadata.Set(key, true);
                    ctx.Diagnostics.Report(new Diagnostic(
                        "MF-REF-001", $"Reflected {attr.Name} → {key}",
                        DiagnosticSeverity.Info,
                        new ElementPath("TypeModel", element.Name)));
                }
            }
        }
        return model;
    }
}
```

### 6.8 Integrace do RootElement

```csharp
public abstract class RootElement
{
    // -- existující --
    public string Name { get; set; } = string.Empty;
    public List<AttributeElement> Attributes { get; init; } = [];

    // -- nové --
    public MetadataBag Metadata { get; init; } = new();

    // Jednotný dotazovací bod pro generátor:
    // 1. Zkontroluje MetadataBag
    // 2. Fallback na AttributeElement reflexi
    public bool HasSemanticAttribute(string metadataKey, string[] attributeNames);
}
```

---

## 7. Implementační dopad

### Změněné projekty nebo soubory

| Soubor | Akce |
|--------|------|
| `Src/MetaForge.Core/Abstractions/MetadataBag.cs` | **Nový** — MetadataBag, MetadataScope, MetadataEntry, MergeStrategy |
| `Src/MetaForge.Core/Abstractions/RootElement.cs` | **Upravený** — +MetadataBag property |
| `Src/MetaForge.Core/Diagnostics/DiagnosticSeverity.cs` | **Nový** — enum |
| `Src/MetaForge.Core/Diagnostics/ElementPath.cs` | **Nový** — record |
| `Src/MetaForge.Core/Diagnostics/Diagnostic.cs` | **Nový** — record |
| `Src/MetaForge.Core/Diagnostics/IDiagnosticCollector.cs` | **Nový** — interface |
| `Src/MetaForge.Core/Diagnostics/DiagnosticBag.cs` | **Nový** — implementace |
| `Src/MetaForge.Core/Diagnostics/BuildResult.cs` | **Nový** — monadický wrapper |
| `Src/MetaForge.Core/Diagnostics/IDiagnosticReporter.cs` | **Nový** — interface |
| `Src/MetaForge.Core/Diagnostics/Reporters/` | **Nové** — Console, JSON, InMemory |
| `Src/MetaForge.Core/Transforms/IModelTransform.cs` | **Nový** — interface |
| `Src/MetaForge.Core/Transforms/TransformContext.cs` | **Nový** |
| `Src/MetaForge.Core/Transforms/TransformPipeline.cs` | **Nový** |
| `Src/MetaForge.Core/Transforms/AttributeReflectionTransform.cs` | **Nový** |
| `Src/MetaForge.Core/Builders/TypeModelBuilder.cs` | **Nový** |
| `Src/MetaForge.Core/Builders/ClassBuilder.cs` | **Nový** |
| `Src/MetaForge.Core/Builders/PropertyBuilder.cs` | **Nový** |
| `Src/MetaForge.Core/Builders/MethodBuilder.cs` | **Nový** |
| `Src/MetaForge.Core/Builders/EnumBuilder.cs` | **Nový** |
| `Src/MetaForge.Core/Builders/InterfaceBuilder.cs` | **Nový** |
| `Tests/MetaForge.Core.Tests/Builders/` | **Nové** — testy builderů |
| `Tests/MetaForge.Core.Tests/Diagnostics/` | **Nové** — testy diagnostiky |
| `Tests/MetaForge.Core.Tests/Transforms/` | **Nové** — testy pipeline |

### API a kontrakty

- `TypeModel.Define()` je additivní — stávající `new ClassElement { ... }` funguje dál.
- `RootElement.Metadata` je nová property — žádný breaking change.
- `AttributeElement` zůstává beze změny.
- `DiagnosticBag` a `TransformPipeline` jsou nové — nic nemění.

### Testy

- **Buildery:** `TypeModel.Define("X").Class("Y", ...).Build()` produkuje správný `TypeModel`.
- **DiagnosticBag:** Sběr, filtrování, 4 severity, reportování do Console/JSON/InMemory.
- **BuildResult\<T\>:** Monadické `.Then()` řetězení, fail-fast při Error.
- **TransformPipeline:** Chain 2+ transformů, `BuildResult<TypeModel>` výstup.
- **AttributeReflection:** `AttributeElement("Required")` → `MetadataBag["Validation.Required"] = true`.

---

## 8. Implementační fáze

### Fáze 1: Fluent Builder API + MetadataBag (4-6h)
- `TypeModelBuilder`, `ClassBuilder`, `PropertyBuilder`, `MethodBuilder`, `EnumBuilder`, `InterfaceBuilder`.
- `MetadataBag`, `MetadataScope`, standardizované klíče.
- Integrace do `RootElement` (+MetadataBag property).
- **DoD:** `TypeModel.Define("X").Class("Y", cls => cls.Metadata(m => m.Set("Docs.Summary", "test"))).Build()` funguje.

### Fáze 2: DiagnosticBag (3-4h)
- `DiagnosticSeverity`, `ElementPath`, `Diagnostic`, `IDiagnosticCollector`, `DiagnosticBag`.
- `BuildResult<T>` s monadickým `.Then()`.
- `ConsoleDiagnosticReporter`, `JsonDiagnosticReporter`, `InMemoryDiagnosticReporter`.
- **DoD:** Pipeline se zastaví při Error, warnings procházejí dál.

### Fáze 3: TransformPipeline + AttributeReflection (4-6h)
- `IModelTransform`, `TransformContext`, `TransformPipeline`.
- `AttributeReflectionTransform` — AttributeElement → MetadataBag reflexe.
- **DoD:** Chain transformů, fail-fast, reflexe atributů do MetadataBag.

**Celkem: 11-16h (1,5-2 dny)**

---

## 9. Otevřené otázky

- OQ-038-01: Má `TypeModel.Define()` přijímat namespace jako string, nebo má být samostatná metoda `.WithNamespace("...")` na builderu?
- OQ-038-02: Má `MetadataBag` podporovat DefaultValue (výchozí hodnota, pokud není klíč nastaven)?
- OQ-038-03: Má `TransformPipeline` podporovat asynchronní transformy (`Task<TypeModel> ApplyAsync(...)`) už teď, nebo až později?

---

## 10. Rizika a trade-offy

| Riziko | Dopad | Mitigace |
|--------|-------|----------|
| MetadataBag klíče nejsou typově bezpečné | Runtime chyby místo compile-time | Standardizované klíče + unit testy pro známé klíče |
| Duplicita: AttributeElement + MetadataBag | Zmatek, co kam psát | Jasné pravidlo: C# `[Attribute]` → AttributeElement, vše ostatní → MetadataBag |
| TransformPipeline výkon | Zpomalení build | Deferred: incremental dirty-tracking (PROP-039) |
| Buildery se rozrostou | Udržovatelnost | Rozdělit do partial tříd |

---

## 11. Validace

- **Build:** Všechny nové soubory se zkompilují.
- **Testy:** Builder testy, DiagnosticBag testy, TransformPipeline testy.
- **Smoke:**
  ```csharp
  var model = TypeModel.Define("Test")
      .Class("Order", cls => cls
          .Property("Id", StrongType.Guid, p => p.Init())
          .Attribute("[Required]")
          .Metadata(m => m.Set("Docs.Summary", "Order entity")))
      .Build();
  // model.Classes[0].Metadata.Get<string>("Docs.Summary") == "Order entity"
  // model.Classes[0].Attributes[0].Name == "Required"
  ```

---

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.
