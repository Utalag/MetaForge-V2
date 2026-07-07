# PROP-038 Core DX & Diagnostics — Fluent Builder, DiagnosticBag, AttributeModel, XmlDocModel

Typ výsledku: Candidate Proposal
Zdroj podnětu: Koumák — Perplexity konverzace e0609fe1 (rozšíření Core)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-07

Priorita: High
Oblast: Core, Generators
Owner:
Datum vytvoření: 2026-07-07
Aktualizováno: 2026-07-07

Navazuje na:
- Perplexity konverzace: https://www.perplexity.ai/search/e0609fe1-9b12-4655-9731-ffc0ab9d73aa
- PROP-035 (C#-First Core Migration) — Fluent Builder jako součást Commit 1

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Core/Elements/` — všechny elementy získají Buildery
- `Src/MetaForge.Core/Diagnostics/` — nový namespace

## 1. Kontext

Perplexity analyzovalo Core a identifikovalo 4 "game changer" nápady s nejvyšším poměrem přínos/náročnost. Všechny jsou čistě aditivní — žádné breaking change, okamžitý DX přínos.

## 2. Problém dnes

- **Konstrukce elementů je verbose:** Každý ClassElement se vytváří přes `new ClassElement { ... }` s mnoha řádky.
- **Žádná strukturovaná diagnostika:** Chyby a varování se řeší ad-hoc, výjimkami nebo logováním.
- **Atributy jako string:** `[Required]`, `[JsonProperty]` jsou jen textové anotace bez validace argumentů.
- **XML dokumentace jako string:** `<summary>`, `<param>` tagy nejsou strukturované — generátor nemůže produkovat IntelliSense.

## 3. Cíl

- **Fluent Builder API:** `Class("Order").WithProperty(...).WithMethod(...).Build()` pro všechny elementy.
- **DiagnosticBag:** Lehká kolekce `Diagnostic` s `Severity`, `Code`, `Message`, `ElementPath`.
- **AttributeModel:** `AttributeElement` s `TypeName`, `ConstructorArgs`, `NamedArgs` — first-class, ne string.
- **XmlDocModel:** `XmlDocElement` s `<summary>`, `<param>`, `<returns>`, `<exception>` — strukturovaně.

## 4. Architektonické invarianty

- BusinessAuthoringDocument zůstává source of truth.
- Core nesmí nést logiku, která patří do vyšší vrstvy.
- Builder je thin facade — `.Build()` vrací immutable element.
- DiagnosticBag je additivní — existující kód se nemění.

## 5. Scope

### In scope
- **Fluent Builder API** pro: ClassElement, MethodElement, PropertyElement, EnumElement, StructElement, InterfaceElement, TypeModel.
- **DiagnosticBag** s `DiagnosticSeverity` (Error/Warning/Info), `ElementPath`, reportéry (Console, JSON, InMemory).
- **AttributeElement** — first-class model s `ConstructorArguments` a `NamedArguments`.
- **XmlDocElement** — strukturovaný model s `Summary`, `Params`, `Returns`, `Exceptions`.
- Integrace s `RootElement` — `Attributes` z `List<AttributeElement>`, `XmlDoc` jako `XmlDocElement?`.

### Out of scope
- Generování XML dokumentace z modelu — to je role Generators.
- IDE integrace (LSP) — až později.
- Validace atributů proti reálným .NET typům — ne, jen strukturální validace.

## 6. Návrh řešení

### 6.1 Fluent Builder API

```csharp
// Vstupní bod
var model = TypeModel.Define("MyApp.Domain")
    .Class("Order", cls => cls
        .Sealed()
        .PrimaryConstructor(ctor => ctor
            .Param("id", StrongType.Guid))
        .Property("Status", "OrderStatus", p => p.GetSet())
        .Method("Place", m => m.Returns(StrongType.Void)
            .ExpressionBody(/* ... */))
    )
    .Build();

// Builder pattern — thin facade
public class ClassElementBuilder
{
    private readonly ClassElement _element = new();
    public ClassElementBuilder Sealed() { _element.IsSealed = true; return this; }
    public ClassElementBuilder WithNamespace(string ns) { _element.Namespace = ns; return this; }
    public ClassElement Build() => _element;
}
```

### 6.2 DiagnosticBag

```csharp
public enum DiagnosticSeverity { Error, Warning, Info }

public sealed record Diagnostic(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    ElementPath? Location
);

public sealed class DiagnosticBag
{
    public void Add(Diagnostic diagnostic) { ... }
    public IReadOnlyList<Diagnostic> All { get; }
    public bool HasErrors { get; }
    public void ReportTo(IDiagnosticReporter reporter) { ... }
}

public interface IDiagnosticReporter
{
    void Report(IReadOnlyList<Diagnostic> diagnostics);
}

// Vestavěné reportéry: ConsoleDiagnosticReporter, JsonDiagnosticReporter, InMemoryDiagnosticReporter
```

### 6.3 AttributeElement

```csharp
public sealed class AttributeElement : RootElement
{
    public string TypeName { get; set; } = string.Empty;          // např. "RequiredAttribute"
    public List<AttributeArgument> ConstructorArgs { get; init; } = [];
    public List<AttributeArgument> NamedArgs { get; init; } = [];
}

public sealed record AttributeArgument(
    string? Name,       // null = positional, non-null = named
    Expression Value
);
```

### 6.4 XmlDocElement

```csharp
public sealed record XmlDocElement(
    string? Summary,
    IReadOnlyList<XmlDocParam> Params,
    string? Returns,
    IReadOnlyList<XmlDocException> Exceptions,
    string? Remarks
);

public sealed record XmlDocParam(string Name, string Description);
public sealed record XmlDocException(string ExceptionType, string Description);
```

### 6.5 Integrace do RootElement

```csharp
public abstract class RootElement
{
    // -- existující --
    public string Name { get; set; } = string.Empty;

    // -- nové --
    public List<AttributeElement> Attributes { get; init; } = [];     // nahrazuje staré string Attributes
    public XmlDocElement? XmlDoc { get; set; }
}
```

## 7. Implementační dopad

### Změněné projekty nebo soubory

- Nové: `Src/MetaForge.Core/Builders/` — ClassElementBuilder, MethodElementBuilder, atd.
- Nové: `Src/MetaForge.Core/Diagnostics/` — Diagnostic, DiagnosticBag, IDiagnosticReporter
- Nové: `Src/MetaForge.Core/Diagnostics/Reporters/` — ConsoleReporter, JsonReporter, InMemoryReporter
- Nové: `Src/MetaForge.Core/Elements/AttributeElement.cs`
- Nové: `Src/MetaForge.Core/Elements/XmlDocElement.cs`
- Upravené: `Src/MetaForge.Core/Elements/RootElement.cs` — `Attributes` typ změníme
- Nové: `Tests/MetaForge.Core.Tests/Builders/`
- Nové: `Tests/MetaForge.Core.Tests/Diagnostics/`

### API a kontrakty

- Buildery jsou additivní — staré `new ClassElement { ... }` funguje dál.
- `RootElement.Attributes` změní typ — **breaking change** (nutná migrace existujícího kódu).
- `DiagnosticBag` je nový — nic nemění.

### Testy

- Builder testy: `Class("X").WithProperty(...).Build()` produkuje správný element.
- DiagnosticBag testy: sběr, filtrování, reportování do Console/JSON/InMemory.

### Dokumentace

- `Docs/Core/08-Builders.md` — jak používat Fluent Builder API.
- `Docs/Core/09-Diagnostics.md` — diagnostický model.

## 8. Implementační fáze

### Fáze 1: Fluent Builder API (3-5h)
- Buildery pro Class, Method, Property, Enum, Struct, Interface, TypeModel.
- Integrace s PROP-035 Commit 1 (RootElement rozšíření).
- **DoD:** `TypeModel.Define("X").Class("Y", ...).Build()` funguje.

### Fáze 2: DiagnosticBag (2-3h)
- Diagnostic, DiagnosticBag, DiagnosticSeverity, ElementPath.
- ConsoleDiagnosticReporter, JsonDiagnosticReporter, InMemoryDiagnosticReporter.
- **DoD:** Lze přidat diagnostiku, reportovat do konzole/JSON.

### Fáze 3: AttributeModel + XmlDocModel (3-5h)
- AttributeElement s positional a named argumenty.
- XmlDocElement se Summary, Params, Returns, Exceptions.
- Integrace do RootElement.
- **DoD:** Lze vytvořit `[Required(ErrorMessage = "...")]` jako AttributeElement.

**Celkem: 8-13h (1-2 dny)**

## 9. Otevřené otázky

- OQ-038-01: Má `RootElement.Attributes` změnit typ z `List<string>` na `List<AttributeElement>` jako breaking change, nebo přidat jako `List<AttributeElement> AttributeElements` vedle starého?
- OQ-038-02: Má Fluent Builder API použít extension methods (mimo element) nebo vnořené třídy (uvnitř elementu)?

## 10. Rizika a trade-offy

- Riziko: `RootElement.Attributes` breaking change — starý kód se nenačte z JSON. Řešení: migrační script.
- Riziko: Buildery se rozrostou — každý element má 10+ metod. Řešení: rozdělit do partial tříd.
- Trade-off: Fluent Builder vs record init — builder je čitelnější v testech, record init je jednodušší.

## 11. Validace

- Build: Všechny nové soubory se zkompilují.
- Testy: Builder testy, DiagnosticBag testy, AttributeElement serializace.
- Smoke: `Class("Order").WithProperty("Id", StrongType.Guid).WithAttribute("Required").Build()`.

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.
