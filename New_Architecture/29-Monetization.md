# Monetizace

> Kreditový systém, tier licence a MCP-ready billing gate pro C#-first MetaForge.
> Monetizace je navržena jako **průřezová vrstva** — gate je ve Facade, nikoli v host surfaces.

---

## Principy

1. **Gate na úrovni Facade** — všechny host surfaces (MCP, CLI, WebApi) procházejí stejným billing checkpointem
2. **Modelování zdarma** — chat, projekce, discovery, náhledy — bez poplatku
3. **Platí se za výstup** — code export, AI inference, ForgeBlock marketplace
4. **MCP-native** — billing funguje i když klient komunikuje pouze přes MCP tools (žádný dashboard)
5. **Graceful degradation** — bez licence nebo bez kreditů = základní funkcionalita zůstane
6. **Tier systém + kredity** — předplatné odemyká vyšší limity, kredity umožňují pay-as-you-go

---

## Architektura

```
┌──────────────────────────────────────────────────────┐
│                    HOST SURFACES                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────────────┐    │
│  │   MCP    │  │   CLI    │  │     WebApi       │    │
│  │ (tools)  │  │(commands)│  │  (REST API)      │    │
│  └────┬─────┘  └────┬─────┘  └────────┬─────────┘    │
│       │             │                  │              │
│       └─────────────┼──────────────────┘              │
│                     │                                 │
│                     ▼                                 │
│  ┌─────────────────────────────────────────────────┐  │
│  │         BusinessAuthoringHostFacade              │  │
│  │  ═══════════════════════════════════════════════  │  │
│  │  │              BILLING GATE                │    │  │
│  │  │  IGenerationCostPolicy.CanGenerateAsync() │    │  │
│  │  └───────────────────────────────────────────┘    │  │
│  └──────────┬──────────────────────────────┬──────────┘  │
│             │                              │             │
│             ▼                              ▼             │
│  ┌──────────────────┐          ┌──────────────────┐      │
│  │   BusinessModel  │          │    Generator     │      │
│  │  (zdarma)        │          │  (zpoplatněno)   │      │
│  └──────────────────┘          └──────────────────┘      │
└──────────────────────────────────────────────────────────┘
```

---

## Tier licence

| Feature | Free | Pro ($29/měsíc) | Enterprise ($199/měsíc) |
|---------|------|-----------------|------------------------|
| **Modelování + projekce** | ✅ | ✅ | ✅ |
| **C# export** | ✅ (≤10 elementů) | ✅ (neomezeně) | ✅ (neomezeně) |
| **TS/Python/Java/Go export** | ❌ | ✅ | ✅ |
| **ForgeBlock Math/Random/Mapper** | ✅ | ✅ | ✅ |
| **ForgeBlock marketplace** | ❌ | ✅ (stažení) | ✅ (stažení + tvorba) |
| **AI constraint inference** | ❌ | ✅ (500/měsíc) | ✅ (neomezeně) |
| **AI translation** | ❌ | ✅ (100/měsíc) | ✅ (neomezeně) |
| **MCP tools** | ✅ (modelování) | ✅ (vše) | ✅ (vše) |
| **On-premise deployment** | ❌ | ❌ | ✅ |
| **Vlastní ForgeBlocky** | ❌ | ❌ | ✅ |
| **SLA + support** | ❌ | ❌ | ✅ |
| **Audit log** | ❌ | ❌ | ✅ |

---

## Coin systém

Každý element nese atribut **Coin** — cenu v kreditech.

- Každý typ elementu má pevnou výchozí Coin hodnotu (definovanou v kódu)
- Coin hodnoty lze změnit pouze v `metaforge.coins.json` (platform operator, ne uživatel)
- Cena generace = `∑ TotalCoin všech elementů v projektu`
- **Čím víc kódu, tím dráž** — každý element + jeho children přidávají Coiny do sumy

### Výchozí Coin hodnoty

| Element | Výchozí Coin | Definice |
|---------|-------------|----------|
| `ClassElement` | 10 | Výchozí v `ClassElement.Coin` |
| `InterfaceElement` | 6 | Výchozí v `InterfaceElement.Coin` |
| `EnumElement` | 3 | Výchozí v `EnumElement.Coin` |
| `EnumMemberElement` | 1 | Výchozí v `EnumMemberElement.Coin` |
| `StructElement` | 10 | Výchozí v `StructElement.Coin` |
| `PropertyElement` | 2 | Výchozí v `PropertyElement.Coin` |
| `MethodElement` | 5 | Výchozí v `MethodElement.Coin` |
| `ParameterElement` | 1 | Výchozí v `ParameterElement.Coin` |

### Výpočet

```
classElement.TotalCoin      = Coin + ∑Property.Coin + ∑(Method.Coin + ∑Parameter.Coin)
interfaceElement.TotalCoin  = Coin + ∑Property.Coin + ∑(Method.Coin + ∑Parameter.Coin)
structElement.TotalCoin     = Coin + ∑Property.Coin + ∑(Method.Coin + ∑Parameter.Coin)
enumElement.TotalCoin       = Coin + ∑EnumMember.Coin
rootElement.TotalCoin       = Coin   (pro ostatní)

appRoot.TotalCoin = ∑(rootElement.TotalCoin) přes všechny projekty a jejich root elementy

cenaExportu = appRoot.TotalCoin   // žádný jazykový multiplikátor (C#-first)
```

### metaforge.coins.json

```json
{
  "coffee": {
    "ClassElement": 10,
    "InterfaceElement": 6,
    "EnumElement": 3,
    "EnumMemberElement": 1,
    "StructElement": 10,
    "PropertyElement": 2,
    "MethodElement": 5,
    "ParameterElement": 1
  }
}
```

- Soubor leží v kořenu projektu nebo ve `~/.metaforge/coins.json`
- Pokud soubor neexistuje, použijí se výchozí hodnoty z kódu
- Pokud v souboru chybí některý element, použije se jeho výchozí hodnota
- Načítá se při startu hostitele (`CoinConfigLoader`)
- **Není přístupný uživatelům** — mění ho pouze platform operator

### CoinConfigLoader

```csharp
public sealed class CoinConfigLoader
{
    private static readonly string ConfigPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "metaforge.coins.json");

    private Dictionary<string, int>? _coins;

    public int GetCoin<T>() where T : class
    {
        _coins ??= LoadCoins();
        var key = typeof(T).Name;
        return _coins.TryGetValue(key, out var coin) ? coin : GetDefaultCoin<T>();
    }

    private static Dictionary<string, int> LoadCoins()
    {
        if (!File.Exists(ConfigPath)) return new();
        var json = File.ReadAllText(ConfigPath);
        var doc = JsonDocument.Parse(json);
        var section = doc.RootElement.GetProperty("coffee");
        return section.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetInt32());
    }

    private static int GetDefaultCoin<T>() => typeof(T).Name switch
    {
        nameof(ClassElement) => 10,
        nameof(InterfaceElement) => 6,
        nameof(EnumElement) => 3,
        nameof(EnumMemberElement) => 1,
        nameof(StructElement) => 10,
        nameof(PropertyElement) => 2,
        nameof(MethodElement) => 5,
        nameof(ParameterElement) => 1,
        _ => 0
    };
}
```

### Aplikace Coinů při startu

```csharp
// V Composition Root (Program.cs)
var coinLoader = new CoinConfigLoader();

// Varianta A — CoinApplicator post-processing před exportem
services.AddSingleton(coinLoader);

// Varianta B — builder nastaví Coin při vytvoření elementu
public class MetaForgeBuilder
{
    private readonly CoinConfigLoader _coins;

    public MetaForgeBuilder(CoinConfigLoader coins)
    {
        _coins = coins;
    }

    public ClassElement AddClass(string name)
    {
        var cls = new ClassElement();
        cls.Coin = _coins.GetCoin<ClassElement>();  // přepíše výchozí 10
        return cls;
    }
}
```

### IGenerationCostPolicy — zjednodušeno

```csharp
public interface IGenerationCostPolicy
{
    Task<BillingCheckResult> CanGenerateAsync(string? userId, int requiredCoins, CancellationToken ct);
    Task DeductAsync(string? userId, int coins, CancellationToken ct);
}

public sealed record BillingCheckResult(
    bool Allowed,
    int RequiredCoins,
    int AvailableCoins,
    string? DenialReason = null);
```

### Facade gate

```csharp
public sealed class BusinessAuthoringHostFacade
{
    private readonly IGenerationCostPolicy _costPolicy;

    public async Task<GenerationResult> GenerateCodeAsync(
        AppRoot appRoot, string? userId = null)
    {
        var coins = appRoot.TotalCoin;

        var check = await _costPolicy.CanGenerateAsync(userId, coins);
        if (!check.Allowed)
            return GenerationResult.InsufficientCoins(coins, check.AvailableCoins);

        var code = _generator.Generate(appRoot);
        await _costPolicy.DeductAsync(userId, coins);

        return GenerationResult.Success(code, coins);
    }
}
```

### MCP billing response

```json
{
  "isError": true,
  "content": [{ "type": "text", "text": "Nedostatek Coinů pro export." }],
  "meta": {
    "billing": {
      "requiredCoins": 347,
      "availableCoins": 100,
      "tier": "free",
      "upgradeUrl": "https://metaforge.dev/pricing"
    }
  }
}
```

---

## MCP-native billing

Billing gate je ve Facade, všechny host surfaces (MCP, CLI, WebApi) procházejí stejným kódem.

### Mapování MCP tools na billing

| MCP Tool | Billing kategorie | Podmínka |
|----------|------------------|----------|
| `add_entity` | Free | — |
| `add_attribute` | Free | — |
| `add_behavior` | Free | — |
| `get_projection` | Free | — |
| `query_discovery` | Free | — |
| `export_code` | **Metered** | Pro tier nebo dostatek kreditů |
| `translate_model` | **Metered** (AI) | Pro tier nebo AI tokeny |
| `apply_preset` | **Metered** (marketplace) | Zakoupený preset |
| `purchase_forgeblock` | Free (nákup) | Volá billing API |
| `check_credits` | Free | Vrací stav konta |

### Billing response v MCP

Když metered tool selže kvůli nedostatku kreditů:

```json
{
  "isError": true,
  "content": [{ "type": "text", "text": "Nedostatek kreditů pro export." }],
  "meta": {
    "billing": {
      "requiredCredits": 347,
      "availableCredits": 100,
      "tier": "free",
      "upgradeUrl": "https://metaforge.dev/pricing"
    }
  }
}
```

### Tool handler pattern

```csharp
[McpTool("export_code")]
public async Task<CallToolResponse> ExportCode(
    [McpParam("entityId")] string entityId,
    [McpParam("language")] string language)
{
    var result = await _facade.GenerateCodeAsync(entityId, language);

    if (result.IsInsufficientCredits)
    {
        return new CallToolResponse
        {
            IsError = true,
            Content = [new TextContent { Text = "Nedostatek kreditů." }],
            Meta = new Dictionary<string, object>
            {
                ["billing"] = result.BillingInfo
            }
        };
    }

    return new CallToolResponse
    {
        Content = [new TextContent { Text = result.Code }]
    };
}
```

---

## IGenerationCostPolicy — rozhraní

Rozhraní žije v `MetaForge.Abstractions`. Implementace je injektovaná — platforma o billing modelu neví.

```csharp
public interface IGenerationCostPolicy
{
    int CalculateCost(int creditScore, int languageMultiplier);
    Task<BillingCheckResult> CanGenerateAsync(string? userId, int cost, CancellationToken ct);
    Task DeductAsync(string? userId, int cost, CancellationToken ct);
}

public sealed record BillingCheckResult(
    bool Allowed,
    int RequiredCredits,
    int AvailableCredits,
    string? DenialReason = null);
```

### Implementace

| Třída | Umístění | Chování |
|-------|----------|---------|
| `AlwaysAllowGenerationPolicy` | MetaForge.Abstractions | Vždy povolí (OSS/trial režim) |
| `LocalLicenseGenerationPolicy` | MetaForge.Infrastructure | Odečítá z `~/.metaforge/license.json` |
| `CloudBillingGenerationPolicy` | MetaForge.Infrastructure | Volá REST billing API (SaaS) |
| `EnterpriseLicenseGenerationPolicy` | MetaForge.Infrastructure | Ověřuje enterprise licenci |

---

## ForgeBlock Marketplace

### MCP tools

| Tool | Popis |
|------|-------|
| `marketplace_search` | Vyhledá ForgeBlocky podle capability/handle |
| `marketplace_purchase` | Zakoupí ForgeBlock (odečte z kreditu nebo provede platbu) |
| `marketplace_install` | Nainstaluje zakoupený ForgeBlock do projektu |
| `marketplace_publish` | Publikuje nový ForgeBlock (pouze Enterprise) |

### Model

- **Autoři** vytvářejí ForgeBlock balíčky a publikují je do store
- **Platforma** si bere 30 % z ceny (70 % jde autorovi)
- **Konzumenti** kupují balíčky jednorázově nebo přes předplatné

```yaml
capability: validation-fluent
version: 1.0
price: 5.00           # jednorázová cena v USD
creditCost: 50        # nebo alternativa v kreditech
author: "username"
rating: 4.5
downloads: 1234
```

---

## AI token billing

| AI služba | Free | Pro | Cena nad limit |
|-----------|------|-----|---------------|
| Constraint inference | ❌ | 500/měsíc | $0.01/volání |
| AI translation | ❌ | 100/měsíc | $0.05/volání |
| Self-healing | ❌ | ❌ (jen Enterprise) | — |
| AI code suggestions | ❌ | 50/měsíc | $0.02/volání |

```csharp
public interface IAiUsagePolicy
{
    Task<AiUsageResult> CanUseAiAsync(string? userId, AiService service);
    Task RecordUsageAsync(string? userId, AiService service);
}

public enum AiService
{
    ConstraintInference,
    Translation,
    SelfHealing,
    CodeSuggestion,
}
```

---

## Lokální licence (standalone)

```json
{
  "version": 1,
  "tier": "pro",
  "expiresAt": "2027-01-01T00:00:00Z",
  "credits": 5000,
  "features": ["csharp", "typescript", "python", "java", "go"],
  "aiQuota": {
    "constraintInference": 500,
    "translation": 100,
    "codeSuggestion": 50
  }
}
```

---

## Mapování na New_Architecture projekty

| Projekt | Monetizační role |
|---------|-----------------|
| `MetaForge.Abstractions` | `IGenerationCostPolicy`, `BillingCheckResult`, `IAiUsagePolicy` |
| `MetaForge.BusinessModel` | `AppRoot.TotalCoin`, `Coin` property na každém elementu |
| `MetaForge.Translator` | Facade — billing gate před generací |
| `MetaForge.Mcp` | MCP tool handlery volají Facade, billing `meta` v odpovědi |
| `MetaForge.Cli` | CLI commandy volají Facade, billing info v outputu |
| `MetaForge.WebApi` | REST endpointy volají Facade, billing info v HTTP hlavičkách |
| `MetaForge.Infrastructure` | `LocalLicenseGenerationPolicy`, `CloudBillingGenerationPolicy`, `CoinConfigLoader` |

---

## Vazba na New_Architecture dokumenty

| Dokument | Vazba |
|----------|-------|
| `03-Core-Abstractions.md` | `AppRoot.TotalCoin`, `RootElement.Coin`, `RootElement.TotalCoin` |
| `04-Core-Elements.md` | Každý element (Class, Property, Method, ...) nese `Coin` + `TotalCoin` |
| `06-Core-Services.md` | `IGenerationCostPolicy` rozhraní |
| `07-BusinessModel.md` | Facade billing gate, `GenerationResult` |
| `09-AI-Layer.md` | `IAiUsagePolicy`, AI token quota |
| `11-Infrastructure.md` | `LocalLicenseGenerationPolicy`, `CoinConfigLoader` |
| `12-Host-Surfaces.md` | MCP/CLI/WebApi — billing meta v odpovědi |
| `16-Risks-and-Rollback.md` | Billing failure = graceful fallback |
| `25-DI-and-Composition-Root.md` | DI registrace `IGenerationCostPolicy` + `CoinConfigLoader` |
| `27-ForgeBlock-External-Libraries.md` | Marketplace pricing pro ForgeBlocky |

---

## Implementační roadmapa

| Fáze | Co | Dependencies |
|------|-----|-------------|
| **1. Základ** | `IGenerationCostPolicy` do `MetaForge.Abstractions` | — |
| | `AlwaysAllowGenerationPolicy` (vždy povolí) | — |
| | `Coin` na elementech (`AppRoot.TotalCoin`) + `metaforge.coins.json` | 03, 04 |
| | Facade billing gate | 07 |
| | MCP billing meta v odpovědi | 12 |
| **2. Licence** | Tier systém (Free/Pro/Enterprise) | Fáze 1 |
| | `LocalLicenseGenerationPolicy` | Fáze 1 |
| | Limit checker dle tieru | Fáze 1 |
| **3. AI billing** | `IAiUsagePolicy` | Fáze 1, 09 |
| | AI token quota | Fáze 1 |
| **4. Marketplace** | ForgeBlock store MCP tools | Fáze 2 |
| | Autor split (70/30) | Fáze 2 |
| **5. Cloud** | `CloudBillingGenerationPolicy` | Fáze 2 |
| | SaaS billing API | Fáze 3 |
| **6. Enterprise** | `EnterpriseLicenseGenerationPolicy` | Fáze 3 |
| | Audit log + SSO | Fáze 3 |
