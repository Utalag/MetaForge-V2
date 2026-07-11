# PROP-037 C# Completeness — Chybějící konstrukty, Projektová metadata, Roslyn Importer

Typ výsledku: Candidate Proposal
Zdroj podnětu: GitHub task "Implement the plan" (8 kroků) — kroky 4, 5, 6
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-07

Priorita: Medium
Oblast: Core, Infrastructure, Generators
Owner:
Datum vytvoření: 2026-07-07
Aktualizováno: 2026-07-07

Navazuje na:
- PROP-035 (C#-First Core Migration) — předpokládá C#-first elementy
- PROP-002 (Core base) — ProjectElement již existuje, rozšiřuje se
- PROP-031 (Core Statement System) — statementy jsou cílem importu

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Core/Elements/` — nové member typy
- `Src/MetaForge.Core/Elements/ProjectElement.cs` — rozšíření
- `Src/MetaForge.Core/Elements/Abstractions/` — nový projekt pro framework metadata

## 1. Kontext

GitHub task "Implement the plan" definoval 8 kroků. Kroky 1-3 a 8 jsou pokryty PROP-031, PROP-035 a PROP-034. Zbývající kroky 4 (delegate, event, operator), 5 (projektová metadata) a 6 (Roslyn importer) sdílejí společné téma: **napojení MetaForge na reálný C# ekosystém** — od jazykových konstruktů přes projektovou infrastrukturu až po import existujícího kódu.

## 2. Problém dnes

- **Chybějící C# member typy:** `delegate`, `event`, `operator` overloading nejsou modelovány — generátor nemůže produkovat knihovny s delegáty a událostmi.
- **Chudý ProjectElement:** Dnes jen `Name` a `DefaultNamespace`. Chybí `TargetFramework`, `PackageReference`, analyzer reference, project-to-project vztahy — nelze reprezentovat reálný .NET projekt.
- **Žádný importer:** Neexistuje cesta, jak existující C# kód převést do Core metadat. Platforma tak nemůže analyzovat existující projekty — jen generovat nové.
- **Framework metadata nejsou oddělená:** DI registrace, ASP.NET endpointy, EF konfigurace, `BackgroundService` — tyto koncepty patří nad Core, ale zatím nemají vlastní modelovou vrstvu.

## 3. Cíl

- Doplnit chybějící C# member typy: `DelegateElement`, `EventElement`, `OperatorElement`.
- Rozšířit `ProjectElement` o plnohodnotná projektová metadata.
- Vytvořit samostatnou vrstvu `MetaForge.Core.Framework` pro framework-specifické konstrukce.
- Implementovat Roslyn-based importer: C# zdrojový kód → Core metadata (lossless pro podporované, diagnostika pro nepodporované).

## 4. Architektonické invarianty

- BusinessAuthoringDocument zůstává source of truth.
- Core nesmí nést logiku, která patří do vyšší vrstvy.
- Roslyn závislost NEPATŘÍ do Core — samostatný projekt `MetaForge.Importer`.
- Framework metadata (DI, ASP.NET, EF) jsou **nad** Core, ne v Core.
- Importer produkuje Core metadata + diagnostiku — nikdy nezahazuje nepodporované konstrukce tiše.

## 5. Scope

### In scope
- **Nové member typy:** `DelegateElement`, `EventElement`, `OperatorElement` (unární, binární, konverzní).
- **ProjectElement rozšíření:** `TargetFramework`, `PackageReference` (jméno + verze), `AnalyzerReference`, `ProjectReference` (jméno + cesta).
- **Framework metadata vrstva:** `MetaForge.Core.Framework` namespace s modely pro DI registrace, ASP.NET endpointy, EF konfigurace, `BackgroundService`, middleware, `CancellationToken` konvence.
- **Roslyn-based importer:** Nový projekt `MetaForge.Importer` se dvěma režimy:
  - Lossless: podporované konstrukce → Core metadata.
  - Diagnostic: nepodporované konstrukce → `PendingQuestion` / diagnostické varování.
- **Diagnostický model:** `ImportDiagnostic` (Severity, Code, Message, Location) pro sledování nepodporovaných konstrukcí.

### Out of scope
- Plnohodnotný C# parser — používáme Roslyn (Microsoft.CodeAnalysis).
- Zpětný export z Core do C# přes Importer — to je role Generators.
- Automatická oprava nepodporovaných konstrukcí.

## 6. Návrh řešení

### 6.1 DelegateElement

```csharp
/// <summary>delegate TResult MyDelegate(T1 arg1, T2 arg2);</summary>
public sealed class DelegateElement : RootElement
{
    public TypeModel ReturnType { get; set; } = default!;
    public List<ParameterElement> Parameters { get; init; } = [];
    public List<TypeParameterElement> TypeParameters { get; init; } = [];
    public List<GenericConstraint> TypeConstraints { get; init; } = [];
}
```

### 6.2 EventElement

```csharp
/// <summary>event EventHandler<T> MyEvent;</summary>
public sealed class EventElement : RootElement
{
    public TypeModel EventType { get; set; } = default!;  // typ delegáta
    public bool IsStatic { get; set; }
    public AccessModifier? AddAccessor { get; set; }      // volitelný custom add
    public AccessModifier? RemoveAccessor { get; set; }   // volitelný custom remove
}
```

### 6.3 OperatorElement

```csharp
public enum OperatorKind
{
    UnaryPlus, UnaryMinus, LogicalNot, BitwiseNot,
    Increment, Decrement, True, False,
    Addition, Subtraction, Multiply, Divide, Modulo,
    BitwiseAnd, BitwiseOr, BitwiseXor,
    LeftShift, RightShift, UnsignedRightShift,
    Equality, Inequality, LessThan, GreaterThan,
    LessThanOrEqual, GreaterThanOrEqual,
    Implicit, Explicit  // konverzní operátory
}

/// <summary>public static MyType operator +(MyType a, MyType b) { ... }</summary>
public sealed class OperatorElement : RootElement
{
    public OperatorKind OperatorKind { get; set; }
    public TypeModel ReturnType { get; set; } = default!;
    public List<ParameterElement> Parameters { get; init; } = [];
    public BlockStatement? Body { get; set; }
    public Expression? ExpressionBody { get; set; }
}
```

### 6.4 ProjectElement rozšíření

```csharp
// Rozšíření existujícího ProjectElement:
public sealed class ProjectElement : RootElement
{
    // -- existující --
    public string? DefaultNamespace { get; set; }

    // -- nové --
    public string? TargetFramework { get; set; }              // např. "net10.0"
    public List<PackageReference> PackageReferences { get; init; } = [];
    public List<AnalyzerReference> AnalyzerReferences { get; init; } = [];
    public List<ProjectReference> ProjectReferences { get; init; } = [];
    public List<string> ImplicitUsings { get; init; } = [];   // např. "System", "System.Linq"
    public string? RootNamespace { get; set; }
    public bool NullableEnabled { get; set; } = true;
}

public sealed record PackageReference(string Name, string Version);
public sealed record AnalyzerReference(string Name, string Path);
public sealed record ProjectReference(string Name, string RelativePath);
```

### 6.5 Framework metadata vrstva

Samostatný namespace `MetaForge.Core.Framework` nebo nový projekt `MetaForge.Core.Framework` — NE v základním Core. Modely:

| Koncept | Model | Popis |
|---------|-------|-------|
| DI registrace | `ServiceRegistration` | ServiceType, ImplementationType, Lifetime (Singleton/Scoped/Transient) |
| ASP.NET endpoint | `EndpointDefinition` | Route, HttpMethod, Handler, Parameters, Auth |
| EF konfigurace | `EntityConfiguration` | EntityType, TableName, Properties, Keys, Relationships |
| BackgroundService | `BackgroundServiceDefinition` | ServiceType, CronExpression, ExecuteAsync |
| Middleware | `MiddlewareDefinition` | Type, Order, Pipeline position |
| CancellationToken | `CancellationTokenConvention` | Parametr pojmenovaný `cancellationToken` jako poslední parametr |

```csharp
namespace MetaForge.Core.Framework;

public sealed record ServiceRegistration(
    TypeModel ServiceType,
    TypeModel ImplementationType,
    ServiceLifetime Lifetime
);

public enum ServiceLifetime { Singleton, Scoped, Transient }
```

### 6.6 Roslyn-based Importer

Nový projekt `MetaForge.Importer` (nebo `Src/MetaForge.Importer/`):

```
MetaForge.Importer/
├── Importer.cs              ← entry point: ImportAsync(string csharpCode)
├── SyntaxVisitors/
│   ├── ClassVisitor.cs
│   ├── MethodVisitor.cs
│   ├── PropertyVisitor.cs
│   ├── DelegateVisitor.cs
│   ├── EventVisitor.cs
│   ├── OperatorVisitor.cs
│   └── StatementVisitor.cs
├── Diagnostics/
│   ├── ImportDiagnostic.cs
│   └── UnsupportedConstructHandler.cs
└── MetaForge.Importer.csproj  ← závislost na Microsoft.CodeAnalysis.CSharp
```

**Dva režimy importu:**

```csharp
public enum ImportMode
{
    Strict,     // Pouze podporované konstrukce, nepodporované = chyba
    Lenient     // Podporované → metadata, nepodporované → ImportDiagnostic
}

public sealed record ImportResult(
    AppRoot? Root,                              // null pokud se nepodařilo nic naimportovat
    IReadOnlyList<ImportDiagnostic> Diagnostics // prázdné = vše OK
);

public sealed record ImportDiagnostic(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    FileLinePositionSpan? Location,
    string? UnsupportedConstruct        // co nebylo podporováno
);
```

## 7. Implementační dopad

### Změněné projekty nebo soubory

- Nové: `Src/MetaForge.Core/Elements/Types/DelegateElement.cs`
- Nové: `Src/MetaForge.Core/Elements/Members/EventElement.cs`
- Nové: `Src/MetaForge.Core/Elements/Members/OperatorElement.cs`
- Nové: `Src/MetaForge.Core/Elements/Members/OperatorKind.cs`
- Upravené: `Src/MetaForge.Core/Elements/ProjectElement.cs` — rozšíření
- Nové: `Src/MetaForge.Core/Framework/` — ServiceRegistration, EndpointDefinition, atd.
- Nový projekt: `Src/MetaForge.Importer/` — Roslyn importer
- Nový projekt: `Tests/MetaForge.Importer.Tests/` — testy importu
- Upravené: `MetaForge.slnx` — přidat Importer projekty

### API a kontrakty

- Nové public typy: DelegateElement, EventElement, OperatorElement, OperatorKind.
- ProjectElement získává nové property (všechny nullable/volitelné — ne-breaking).
- Nové public API: ImportResult, ImportDiagnostic, ImportMode.

### Testy

- Unit testy pro každý nový element (Delegate, Event, Operator).
- Import testy: C# snippet → ImportAsync → ověřit metadata + diagnostiku.
- Round-trip testy: C# snippet → import → generování → C# (sémantická ekvivalence).
- Negativní testy: nepodporované konstrukce generují diagnostiku, ne výjimku.

### Dokumentace

- Aktualizace podporované matice (PROP-034) o delegate, event, operator.
- `Docs/Importer/` — jak používat importer, režimy, diagnostika.

## 8. Implementační fáze

### Fáze 1: Chybějící member typy (Krok 4)
- `DelegateElement` + testy (2-3h)
- `EventElement` + testy (2-3h)
- `OperatorElement` + `OperatorKind` + testy (3-4h)
- Renderer rozšíření v Generators (2-3h)
- **Celkem:** 9-13h

### Fáze 2: Projektová metadata (Krok 5a)
- Rozšířit `ProjectElement` (3-4h)
- `PackageReference`, `AnalyzerReference`, `ProjectReference` (1-2h)
- Serializace/deserializace projektových metadat (1-2h)
- **Celkem:** 5-8h

### Fáze 3: Framework metadata vrstva (Krok 5b)
- `ServiceRegistration`, `ServiceLifetime` (2-3h)
- `EndpointDefinition` (2-3h)
- `EntityConfiguration` (2-3h)
- `BackgroundServiceDefinition`, `MiddlewareDefinition` (2-3h)
- **Celkem:** 8-12h

### Fáze 4: Roslyn Importer (Krok 6)
- Projekt `MetaForge.Importer` + závislost na `Microsoft.CodeAnalysis.CSharp` (1h)
- `Importer` entry point + `ImportMode` + `ImportResult` (2-3h)
- Syntax Visitors: Class, Method, Property (4-6h)
- Syntax Visitors: Delegate, Event, Operator, Statement (4-6h)
- `UnsupportedConstructHandler` + diagnostika (3-4h)
- Testovací sada: 20+ C# snippetů → import → ověření (4-6h)
- **Celkem:** 18-26h

### Fáze 5: Integrace a docs
- Propojení s PROP-034 (aktualizace support matrix) (2-3h)
- `Docs/Importer/` dokumentace (2-3h)
- Round-trip integrační testy (3-4h)
- **Celkem:** 7-10h

**Celkem PROP-037: 47-69h (6-9 dní)**

## 9. Otevřené otázky

- OQ-037-01: Má být `MetaForge.Core.Framework` samostatný projekt, nebo namespace v Core?
- OQ-037-02: Jak granularita framework metadata — modelovat celé ASP.NET, nebo jen "nejčastějších 20 konceptů"?
- OQ-037-03: Má Importer používat Roslyn přímo, nebo přes abstrakci (pro budoucí výměnu parseru)?

## 10. Rizika a trade-offy

- Riziko Roslyn závislosti: Importer musí být samostatný projekt — Core nesmí záviset na Roslynu.
- Riziko scope creep: Framework metadata může narůst do nekonečna — MVP = top 10 konceptů.
- Riziko nekompletního importu: Ne všechny C# konstrukce půjdou importovat — diagnostika musí být informativní.
- Trade-off: Vlastní importer vs. existující nástroje (např. Roslyn → JSON Schema) — vlastní dává plnou kontrolu nad mapováním do Core modelu.

## 11. Validace

- Build: MetaForge.Importer se zkompiluje s Roslyn závislostí.
- Testy: 20+ C# snippetů → import → ověření správnosti metadat.
- Smoke scénáře: Import reálné třídy s delegátem, eventem, operátorem, generikou.
- Negativní testy: Import `unsafe` bloku → diagnostika, ne pád.

## 12. Výsledek po dokončení

Vyplnit až při uzavření návrhu.
