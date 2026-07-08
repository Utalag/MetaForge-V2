# PROP-025: Generators — Incremental, Partial Class, Scaffolding + Monetization

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-007 (Generators base — hotovo), PROP-020 (BusinessModel upgrade), PROP-024 (Core rozšíření)

---

## Cíl

Rozšířit generátory o produkčně použitelný codegen pipeline:
1. **Incremental generation** — generovat jen změněné soubory
2. **Partial class / user-code regions** — generátor a vývojář sdílí stejnou třídu
3. **Project scaffolding** — generování celého `.csproj` projektu
4. **Monetizační model** — tier-based přístup ke generování kódu

---

## 🏦 Monetizační model generování kódu

> **Klíčové rozhodnutí:** Generování výstupního kódu NENÍ automaticky zdarma.
> Platforma poskytuje odstupňovaný přístup:

### Tier model

```
┌─────────────────────────────────────────────────────┐
│  TIER 0 — FREE (Sandbox)                            │
│  • Testovací generování v sandboxu                  │
│  • Výstup nelze exportovat (pouze náhled)           │
│  • Omezený počet entit (max 3)                      │
│  • Vodoznak v generovaném kódu                      │
├─────────────────────────────────────────────────────┤
│  TIER 1 — DOMAIN (Free / Low-cost)                  │
│  • Domain vrstva: entity, value objects, rozhraní  │
│  • ForgeBlock Source Generatory (kompilace u uživ.) │
│  • Exportovatelné .cs soubory                       │
│  • MIT licence                                      │
├─────────────────────────────────────────────────────┤
│  TIER 2 — INFRASTRUCTURE (Paid)                     │
│  • Repository vrstva (EF Core, Dapper, Mongo)       │
│  • Service vrstva                                   │
│  • API controllers (Minimal API / Controllers)      │
│  • Mapping (AutoMapper / Mapperly)                  │
│  • Validation (FluentValidation)                    │
├─────────────────────────────────────────────────────┤
│  TIER 3 — FULL (Enterprise)                         │
│  • CI/CD pipeline (GitHub Actions)                  │
│  • Docker + docker-compose                          │
│  • Azure/AWS deployment templaty                    │
│  • Monitoring + health checks                       │
│  • Custom ForgeBlock development kit                │
└─────────────────────────────────────────────────────┘
```

### Implementace tierů

```csharp
public enum GeneratorTier
{
    Sandbox = 0,    // náhled, bez exportu
    Domain = 1,     // entity, value objects
    Infrastructure = 2, // repos, services, API
    Full = 3,       // CI/CD, deployment
}

public sealed class GeneratorLicense
{
    public GeneratorTier Tier { get; init; }
    public string? LicenseKey { get; init; }
    public int MaxEntities { get; init; } = int.MaxValue;
    public bool AllowExport { get; init; } = true;
}

public sealed class TieredCodeGenerator : CodeGenerator
{
    private readonly GeneratorLicense _license;

    public override GeneratedCodeArtifact Generate(RootElement element)
    {
        // TIER 0: vždy vodoznak
        // TIER 1: jen domain elementy
        // TIER 2: domain + infrastructure
        // TIER 3: vše

        if (_license.Tier == GeneratorTier.Domain && IsInfrastructureElement(element))
            throw new LicenseException("Infrastructure generování vyžaduje TIER 2+");
        
        return base.Generate(element);
    }
}
```

---

## 1. Incremental generation (B1)

### Rozsah

```csharp
public sealed class IncrementalCodeGenerator : TieredCodeGenerator
{
    private readonly Dictionary<string, string> _outputCache = new();

    /// <summary>
    /// Vygeneruje jen elementy, jejichž hash se změnil.
    /// </summary>
    public IReadOnlyList<GeneratedCodeArtifact> GenerateIncremental(
        IEnumerable<RootElement> elements,
        string outputDirectory)
    {
        var results = new List<GeneratedCodeArtifact>();

        foreach (var element in elements)
        {
            var artifact = Generate(element);
            var filePath = Path.Combine(outputDirectory, artifact.FileName);
            var newHash = ComputeHash(artifact.SourceCode);

            if (_outputCache.TryGetValue(filePath, out var cachedHash) && cachedHash == newHash)
                continue; // Přeskočit — nezměněno

            _outputCache[filePath] = newHash;
            results.Add(artifact);
        }

        return results.AsReadOnly();
    }
}
```

---

## 2. Partial class / user-code regions (B2)

### Rozsah

Generátor generuje `Customer.g.cs` (partial), vývojář píše do `Customer.cs`:

```csharp
// Customer.g.cs — GENERATED (přepsán při každé regeneraci)
public partial class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string Email { get; set; }
}

// Customer.cs — USER CODE (nikdy nepřepsán)
public partial class Customer
{
    // Vlastní metody, business logika
    public bool IsValid() => !string.IsNullOrWhiteSpace(Email);
}
```

### User-code regions

```csharp
// Customer.g.cs — GENERATED
public partial class Customer
{
    // <generated>
    public Guid Id { get; set; }
    public string Email { get; set; }
    // </generated>

    // <user-code>
    // Uživatelský kód se zachovává mezi regeneracemi
    // </user-code>
}
```

---

## 3. Project scaffolding (B3)

### Rozsah

Generování kompletního `.csproj` projektu z `BusinessAuthoringDocument`:

```csharp
public sealed class ProjectScaffoldGenerator
{
    public async Task ScaffoldAsync(
        BusinessAuthoringDocument document,
        string outputDirectory,
        GeneratorLicense license)
    {
        // 1. Vytvoř adresářovou strukturu
        Directory.CreateDirectory(Path.Combine(outputDirectory, "src", document.Project.Name, "Domain"));
        Directory.CreateDirectory(Path.Combine(outputDirectory, "src", document.Project.Name, "Infrastructure"));
        Directory.CreateDirectory(Path.Combine(outputDirectory, "tests"));

        // 2. Vygeneruj .csproj
        var csproj = GenerateCsproj(document, license.Tier);
        await File.WriteAllTextAsync(/*...*/);

        // 3. Vygeneruj solution
        var sln = GenerateSolution(document);

        // 4. Domain vrstva (všechny tiery)
        foreach (var entity in document.Entities)
        {
            var classElement = TranslateToClassElement(entity);
            var code = Generate(classElement);
            await File.WriteAllTextAsync(/*...*/);
        }

        // 5. Infrastructure vrstva (jen TIER 2+)
        if (license.Tier >= GeneratorTier.Infrastructure)
        {
            GenerateEfCoreDbContext(document);
            GenerateRepositories(document);
            GenerateApiControllers(document);
        }

        // 6. CI/CD (jen TIER 3)
        if (license.Tier >= GeneratorTier.Full)
        {
            GenerateGitHubActions(document);
            GenerateDockerfile(document);
        }
    }
}
```

### Výstupní struktura

```
output/MyProject/
├── MyProject.slnx
├── src/
│   └── MyProject.Domain/
│       ├── MyProject.Domain.csproj
│       ├── Entities/
│       │   ├── Customer.g.cs
│       │   └── Order.g.cs
│       └── ValueObjects/
│           └── EmailAddress.g.cs
├── infrastructure/                        ← jen TIER 2+
│   ├── MyProject.Infrastructure.csproj
│   ├── Data/
│   │   └── AppDbContext.g.cs
│   └── Repositories/
│       ├── CustomerRepository.g.cs
│       └── OrderRepository.g.cs
├── api/                                   ← jen TIER 2+
│   ├── MyProject.Api.csproj
│   └── Controllers/
│       ├── CustomerController.g.cs
│       └── OrderController.g.cs
└── .github/                               ← jen TIER 3
    └── workflows/
        └── ci.yml
```

---

## 4. ExpressionRenderer rozšíření (B4)

Navazuje na PROP-024 (Expression System). Renderer musí podporovat všechny `ExpressionKind` a generovat kód pro různé jazyky/frameworky:

| Expression | C# | SQL (EF) | TypeScript |
|------------|-----|----------|------------|
| `Binary(a + b)` | `a + b` | `a + b` | `a + b` |
| `MemberAccess(.Name)` | `.Name` | `[Name]` | `.name` |
| `MethodCall(Contains)` | `.Contains(x)` | `LIKE '%x%'` | `.includes(x)` |

---

## Odhad

| Fáze | Dny |
|------|-----|
| Tier model + GeneratorLicense | 1 den |
| TieredCodeGenerator | 0,5 dne |
| Incremental generation | 1 den |
| Partial class / user-code regions | 0,5 dne |
| Project scaffolding | 2 dny |
| ExpressionRenderer rozšíření | 1 den |
| Sandbox mód | 1 den |
| Testy | 1 den |
| **Celkem** | **8 dní** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-007 (Generators base) | ✅ Hotovo |
| PROP-020 (BusinessModel upgrade) | 🟢 Schváleno |
| PROP-024 (Core — Expression, StrongType) | 📝 Navrženo |
