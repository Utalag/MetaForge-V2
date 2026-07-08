# PROP-039: Core Composability — Mixin/Trait, ConventionRegistry, Incremental dirty-tracking

Typ výsledku: Candidate Proposal
Zdroj podnětu: Koumák — Perplexity konverzace e0609fe1 (rozšíření Core)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-08

Priorita: Medium
Oblast: Core, Generators
Owner:
Datum vytvoření: 2026-07-07
Aktualizováno: 2026-07-08

Navazuje na:
- Perplexity konverzace: https://www.perplexity.ai/search/e0609fe1-9b12-4655-9731-ffc0ab9d73aa
- **PROP-038** (Core DX, Diagnostics & Pipeline) — TransformPipeline se přesunul tam, tento PROP staví na jeho rozhraních
- PROP-035 (C#-First Core Migration)

Blokuje:
- —

---

## 1. Kontext

Perplexity identifikovalo koncepty pro kompozici a znovupoužitelnost Core modelu. Po filozofické diskusi a upřesnění rozsahu bylo rozhodnuto:

> **TransformPipeline** se přesouvá do PROP-038 (DiagnosticBag + Pipeline patří k sobě).  
> **PROP-039** se zaměřuje na: Mixin/Trait MVP, ConventionRegistry a Incremental dirty-tracking.

---

## 2. Problém dnes

- **Žádný mechanismus pro sdílené chování:** `IAuditable` (CreatedAt, UpdatedAt) se musí kopírovat do každé třídy ručně.
- **Žádné globální konvence:** Pojmenovávací pravidla (PascalCase, I-prefix) se aplikují ad-hoc.
- **Žádný dirty-tracking:** Každý build zpracovává celý model, i když se změnil jediný element.

---

## 3. Cíl

- **Mixin/Trait systém:** Deklarativní sada `PropertyElement` + `MethodElement`, která se build-time expanduje do `ClassElement`. S podporou `ConflictStrategy` pro kolize.
- **ConventionRegistry:** Globální pravidla (pojmenování, struktura) s možností override per-element. Implementováno jako `IModelTransform` (používá pipeline z PROP-038).
- **Incremental dirty-tracking:** `StructuralHash` na každém elementu pro přeskočení nezměněných prvků při opakovaném buildu.

---

## 4. Architektonické invarianty

- Core nesmí nést logiku, která patří do vyšší vrstvy.
- Mixiny jsou build-time expanze — žádný runtime weaving.
- Konvence jsou overrideovatelné per-element.
- Dirty-tracking je additivní — existující kód se nemění.

---

## 5. Scope

### In scope
- **Mixin/Trait:** `ElementMixin` s `ConflictStrategy`, vestavěné mixiny `Auditable`, `SoftDelete`.
- **ConventionRegistry:** `IConvention`, 3 vestavěné konvence, aplikace přes `IModelTransform`.
- **Incremental dirty-tracking:** `ElementFingerprint` (StructuralHash + PipelineVersion), `IEquatable<T>` pro Roslyn.

### Out of scope
- Runtime weaving / AOP — jen build-time expanze.
- Vizuální designer pro pipeline.
- Plný dependency graph pro dirty-tracking — až po adopci Source Generator.

---

## 6. Návrh řešení

### 6.1 Mixin/Trait

```csharp
public enum ConflictStrategy { Skip, Throw, Replace }

public sealed record ElementMixin(
    string Name,
    IReadOnlyList<PropertyElement> Properties,
    IReadOnlyList<MethodElement> Methods,
    IReadOnlyList<AttributeElement>? Attributes = null,
    ConflictStrategy OnConflict = ConflictStrategy.Throw
);

public static class Mixins
{
    public static readonly ElementMixin Auditable = new("Auditable",
        Properties: [
            new("CreatedAt", StrongType.DateTimeOffset) { Accessor = PropertyAccessor.Init },
            new("UpdatedAt", StrongType.DateTimeOffset) { Accessor = PropertyAccessor.GetSet },
            new("CreatedBy", StrongType.String) { Accessor = PropertyAccessor.Init }
        ]);

    public static readonly ElementMixin SoftDelete = new("SoftDelete",
        Properties: [
            new("IsDeleted", StrongType.Bool) { Access = AccessModifier.Private },
            new("DeletedAt", StrongType.DateTimeOffset.AsNullable()) { Access = AccessModifier.Private }
        ],
        Methods: [
            // SoftDelete() a Restore() metody
        ]);
}

// Aplikační transform (IModelTransform z PROP-038)
public class ApplyMixinTransform : IModelTransform
{
    public string Name => "ApplyMixin";

    public TypeModel Apply(TypeModel model, TransformContext ctx)
    {
        return model with
        {
            Classes = model.Classes.Select(cls =>
            {
                foreach (var mixin in cls.Mixins)
                {
                    foreach (var prop in mixin.Properties)
                    {
                        if (cls.Properties.Any(p => p.Name == prop.Name))
                        {
                            switch (mixin.OnConflict)
                            {
                                case ConflictStrategy.Skip: continue;
                                case ConflictStrategy.Throw:
                                    ctx.Diagnostics.Report(new Diagnostic(
                                        "MF-MIX-001", $"Property '{prop.Name}' already exists",
                                        DiagnosticSeverity.Error, ...));
                                    return cls;
                                case ConflictStrategy.Replace: break;
                            }
                        }
                        cls = cls with { Properties = [..cls.Properties, prop] };
                    }
                    cls = cls with { Methods = [..cls.Methods, ..mixin.Methods] };
                }
                return cls;
            }).ToList()
        };
    }
}
```

### 6.2 ConventionRegistry

```csharp
public interface IConvention
{
    string Name { get; }
    ConventionScope Scope { get; }      // Type, Member, Global
    bool AppliesTo(RootElement element);
    RootElement Apply(RootElement element, ConventionContext context);
}

public enum ConventionScope { Type, Member, Global }

public sealed class ConventionContext
{
    public MetadataBag Options { get; init; } = new();  // per-element override
    public IDiagnosticCollector Diagnostics { get; init; } = null!;
}

public sealed class ConventionRegistry
{
    private readonly List<IConvention> _conventions = new();

    public void Register(IConvention convention) { ... }
    public void RegisterRange(IEnumerable<IConvention> conventions) { ... }

    public TypeModel ApplyTo(TypeModel model) { ... }
}

// Vestavěné konvence (jako IModelTransform):
public sealed class PascalCasePropertiesConvention : IModelTransform
{
    public string Name => "PascalCaseProperties";
    public TypeModel Apply(TypeModel model, TransformContext ctx) { ... }
}

public sealed class InterfacePrefixConvention : IModelTransform { ... }
public sealed class AsyncSuffixConvention : IModelTransform { ... }
```

**Aplikace:** ConventionRegistry je použitelný jako `IModelTransform` do TransformPipeline z PROP-038.

### 6.3 Incremental dirty-tracking

```csharp
public abstract class Element
{
    // Nové
    public ElementFingerprint Fingerprint { get; protected set; }

    protected abstract void ComputeHash(HashAlgorithm hasher);
}

public sealed class ElementFingerprint : IEquatable<ElementFingerprint>
{
    public string StructuralHash { get; }       // SHA256 z element tree
    public int PipelineVersion { get; }         // verze TransformPipeline
    public DateTimeOffset LastEmitted { get; }

    public bool Equals(ElementFingerprint? other) { ... }
    public override int GetHashCode() => HashCode.Combine(StructuralHash, PipelineVersion);
}

// Pro Roslyn Incremental Generator:
// IEquatable<T> je vyžadováno pro memoizaci v Source Generator pipeline.
```

---

## 7. Implementační dopad

### Změněné projekty nebo soubory

| Soubor | Akce |
|--------|------|
| `Src/MetaForge.Core/Mixins/ElementMixin.cs` | **Nový** — record + ConflictStrategy |
| `Src/MetaForge.Core/Mixins/BuiltIn/Auditable.cs` | **Nový** — Mixins.Auditable |
| `Src/MetaForge.Core/Mixins/BuiltIn/SoftDelete.cs` | **Nový** — Mixins.SoftDelete |
| `Src/MetaForge.Core/Transforms/ApplyMixinTransform.cs` | **Nový** — IModelTransform |
| `Src/MetaForge.Core/Transforms/BuiltIn/PascalCaseTransform.cs` | **Nový** |
| `Src/MetaForge.Core/Transforms/BuiltIn/InterfacePrefixTransform.cs` | **Nový** |
| `Src/MetaForge.Core/Transforms/BuiltIn/AsyncSuffixTransform.cs` | **Nový** |
| `Src/MetaForge.Core/Conventions/IConvention.cs` | **Nový** |
| `Src/MetaForge.Core/Conventions/ConventionRegistry.cs` | **Nový** |
| `Src/MetaForge.Core/Abstractions/ElementFingerprint.cs` | **Nový** |
| `Src/MetaForge.Core/Abstractions/Element.cs` | **Upravený** — +Fingerprint, +ComputeHash |
| `Tests/MetaForge.Core.Tests/Mixins/` | **Nové** — testy mixinů |
| `Tests/MetaForge.Core.Tests/Conventions/` | **Nové** — testy konvencí |

### API a kontrakty

- Mixiny jsou additivní — nemění existující elementy.
- `Element.Fingerprint` je nová property — žádný breaking change.
- ConventionRegistry je nový — nic nemění.

### Testy

- **Mixin:** `ClassElement` + `Auditable` má CreatedAt, UpdatedAt, CreatedBy.
- **ConflictStrategy:** Kolize → Skip nechá původní, Throw vyhodí Error v DiagnosticBag.
- **ConventionRegistry:** Aplikace PascalCase konvence přejmenuje properties.
- **Fingerprint:** Stejný element → stejný hash. Změna property → jiný hash.

---

## 8. Implementační fáze

### Fáze 1: Mixin MVP (4-6h)
- `ElementMixin`, `ConflictStrategy`, `Mixins.Auditable`, `Mixins.SoftDelete`.
- `ApplyMixinTransform` — IModelTransform použitelný v TransformPipeline z PROP-038.
- **DoD:** `Class("Order").Apply(Mixins.Auditable)` → Order má CreatedAt, UpdatedAt, CreatedBy.

### Fáze 2: ConventionRegistry (3-4h)
- `IConvention`, `ConventionRegistry`, `ConventionContext`.
- `PascalCaseTransform`, `InterfacePrefixTransform`, `AsyncSuffixTransform`.
- **DoD:** Aplikace PascalCase konvence přejmenuje properties na celém modelu.

### Fáze 3: Incremental dirty-tracking (3-4h)
- `ElementFingerprint` s `StructuralHash` + `PipelineVersion`.
- `ComputeHash()` na každém elementu.
- `IEquatable<T>` pro Roslyn Source Generator.
- **DoD:** Dva identické elementy mají stejný fingerprint. Změna property změní hash.

**Celkem: 10-14h (1,5-2 dny)**

---

## 9. Otevřené otázky

- OQ-039-01: Mají mixiny podporovat i přidávání atributů a MetadataBag anotací?
- OQ-039-02: Má ConventionRegistry umožňovat per-element override přes MetadataBag, nebo jen globální?
- OQ-039-03: Má `ElementFingerprint` počítat hash z celého sub-tree (včetně dětí), nebo jen z vlastních properties?

---

## 10. Rizika a trade-offy

| Riziko | Dopad | Mitigace |
|--------|-------|----------|
| Mixiny jsou build-time expanze — při debugování není vidět "původní" element | Horší DX | Source map (odloženo) |
| ConflictStrategy není intuitivní | Chyby při použití | Výchozí `Throw` — explicitní volba |
| Fingerprint výpočet při každém buildu | Režie | Pouze pro elementy, které prošly pipeline (deferred optimalizace) |

---

## 11. Validace

- **Build:** Všechny nové soubory se zkompilují.
- **Testy:** Mixin aplikace, konflikt resolution, konvence, fingerprint rovnost.
- **Smoke:**
  ```csharp
  var model = TypeModel.Define("Test")
      .Class("Order", cls => cls
          .Apply(Mixins.Auditable)
          .Property("Total", StrongType.Decimal))
      .Build();
  // model.Classes[0].Properties.Count == 4
  //   (CreatedAt, UpdatedAt, CreatedBy, Total)
  ```

---

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.

## 11. Validace

- Build: Všechny nové soubory se zkompilují.
- Testy: TransformPipeline chain, Mixin expanze, Convention aplikace.
- Smoke: Model s Auditable mixinem → generovaný kód má CreatedAt/UpdatedAt.

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.
