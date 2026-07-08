# Plugin vs. Package — ForgeBlock Architecture Decision

Datum: 2026-04-20
Stav: Informativní — kandidát na rozšíření do PROPOSALS.md

---

## Shrnutí

ForgeBlocks dnes používají **pluginový design pattern** (registry, self-registration, discovery) ale **package deployment model** (NuGet, compile-time integrace). Tento dokument analyzuje rozdíly a rozhoduje, zda je přechod na true pluginový model vhodný.

---

## Současný stav — Package model

ForgeBlocks jsou **NuGet balíčky** s pluginovou architekturou:

```
Src/ForgeBlocks/
├── Math/        → MetaForge.ForgeBlocks.Math.nupkg
├── Mapper/      → MetaForge.ForgeBlocks.Mapper.nupkg  
└── Random/      → MetaForge.ForgeBlocks.Random.nupkg
```

Balíčky implementují `IForgeBlockPackage` a registrují se do hosta při build time. Třída `BuiltInForgeBlockPackageBootstrap.CreatePackages()` vytváří seznam built-in balíčků.

### Registry model

```
IForgeBlockRegistry (Core)
    ├── AddCapabilityProvider      → "standard-math", "random-values", "model-mapping"
    ├── AddGeneratorContributor   → Scriban šablony, codegen
    ├── AddCatalogContributor      → Presets, katalogové záznamy
    └── AddDiscoveryContributor    → QueryDiscovery metadata

Optional host-specific registries:
    ├── IForgeBlockMcpRegistry     → MCP tools (plánováno)
    ├── IForgeBlockCliRegistry     → CLI příkazy (plánováno)
    └── IForgeBlockTranslatorRegistry → Domain translation rules (plánováno)
```

---

## Plugin vs. Package — porovnání

| Aspekt | **Package** (dnes) | **Plugin** (true runtime) |
|--------|---------------------|---------------------------|
| Přidání nového ForgeBlock | Nutná rekompilace hosta | Stačí přidat do složky / registry |
| Typová bezpečnost | Plná (compile-time) | Omezená (runtime discovery) |
| Nasazení third-party | Nutný rebuild platformy | Možné bez úpravy platformy |
| Hot reload | ❌ Ne | ✅ Ano |
| Debugování | Standardní .NET | Složitější (loaded assemblies) |
| Version compatibility | NuGet resolution | Ruční management |
| Dependency management | NuGet transitive | Isolated resolution |
| Komplexita | Jednodušší | Náročnější |

---

## Výhody dnešního approachu

| Výhoda | Popis |
|--------|-------|
| **Typová bezpečnost** | `IForgeBlockPackage.Register()` je compile-time ověřen |
| **Jednoduchost** | Žádné runtime loading, isolation, versioning |
| **Testovatelnost** | Standardní .NET test patterns |
| **NuGet ecosystem** | Existující infrastructure pro signing, publishing, versioning |
| **Determinismus** | Známý set capabilities při buildu |

---

## Kdy by pluginy dávaly smysl

**Scénáře vhodné pro true plugin model:**

| Scénář | Proč plugin | Příklad |
|--------|-------------|---------|
| **Third-party ecosystem** | Externí vývojáři přidávají capabilities bez fork platformy | Komunitní ForgeBlocks |
| **Hot reload** | Uživatel mění chování bez restartu | Live coding, rapid prototyping |
| **Dynamic discovery** | Nové capability bez restartu MCP serveru | A/B testing, feature flags |
| **Isolation** | Third-party kód nesmí shodit platformu | Sandboxed execution |

**Scénáře kde package model stačí:**

| Scénář | Proč package | Příklad |
|--------|-------------|---------|
| Interní tým | Build-time integrace je přijatelná | MetaForge built-ins |
| Typová bezpečnost | Priorita na correctness | Validace, generování |
| Verified releases | Testy před distribucí | Signed packages |

---

## Plánované rozšíření

V [05-ForgeBlock-Package-Model.md](Architecture-Define/05-ForgeBlock-Package-Model.md) je v sekci "Fáze rozšíření":

| Fáze | Obsah | Status |
|------|-------|--------|
| Hotovo | 4 registry facety, built-in balíčky | ✅ |
| Hotovo | Discovery contributor v Math, Random, Mapper | ✅ |
| Hotovo | `QueryDiscovery("capabilities")` | ✅ |
| Plánováno | `IForgeBlockMcpPackage` + kategorie `tools` | 🔲 |
| Plánováno | `IForgeBlockCliPackage` — CLI příkazy | 🔲 |
| Plánováno | `IForgeBlockTranslatorPackage` — domain translation | 🔲 |
| Plánováno | `IForgeBlockAiAdapterPackage` — LoRA adapters | 🔲 |
| **Vzdálené** | **Instalace externích balíčků za runtime (NuGet → dynamic load)** | 🔲 |

---

## Přechod na plugin model — co by bylo potřeba

Pokud by se v budoucnu rozhodlo pro true pluginový model:

### 1. Plugin Discovery

```csharp
// Kde hledat pluginy
public interface IForgeBlockPluginDiscoverer
{
    IEnumerable<ForgeBlockPluginInfo> DiscoverPlugins(string searchPath);
}

// Manifest vs. convention
// Option A: Konvence (složka/*.dll)
// Option B: Manifest (plugins.json, manifest.yaml)
```

### 2. Version Compatibility

```csharp
public interface IForgeBlockVersionChecker
{
    bool IsCompatible(ForgeBlockPluginInfo plugin, PlatformVersion platform);
}
```

### 3. Isolation

```csharp
// Sandboxed loading
public interface IForgeBlockPluginLoader
{
    T LoadPlugin<T>(ForgeBlockPluginInfo plugin) where T : class;
    void UnloadPlugin(ForgeBlockPluginInfo plugin);
}
```

### 4. Plugin Manifest

```json
{
  "id": "community.orm",
  "version": "1.0.0",
  "platformVersion": ">=1.0.0",
  "capabilities": ["orm", "query"],
  "entryPoint": "Community.Orm.OrmForgeBlockPackage, Community.Orm"
}
```

---

## Rozhodnutí

**Dnešní package model je správný start** pro MetaForge:

1. ✅ Pluginový design pattern — registry, self-registration, discovery
2. ✅ Jednoduchost — žádná runtime complexity
3. ✅ Typová bezpečnost — compile-time ověření
4. ✅ NuGet infrastructure — signing, publishing, versioning

**Přechod na true plugin model** je vhodný až když:
- Existuje demand pro third-party ForgeBlocks
- Hot reload je business requirement
- Platforma je dostatečně stabilní pro externalizaci

---

## Doporučené kroky

1. **Zdokumentovat** jako architektonický princip v Architecture-Define
2. **Přidat do PROPOSALS.md** jako dlouhodobou variantu pro externí ecosystem
3. **Sledovat** demand — implementovat pouze pokud je odůvodněné

---

## Související dokumenty

- [05-ForgeBlock-Package-Model.md](Architecture-Define/05-ForgeBlock-Package-Model.md) — aktuální architektura
- [01-Layers.md](Architecture-Define/01-Layers.md) — vrstva Core a ForgeBlocks
- [PROPOSALS.md](PROPOSALS.md) — kandidát pro formalizaci rozhodnutí

---

## Kontakt / Autor

Analýza provedena 2026-04-20 při diskusi o výhodnosti pluginového modelu.
