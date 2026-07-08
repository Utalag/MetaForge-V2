# PROP-029: ForgeBlocks — Rozšíření a marketplace

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-008 (ForgeBlocks base — hotovo), PROP-015 (CatalogManager propojení), PROP-017 (Packaging)

---

## Cíl

Rozšířit ForgeBlock ekosystém:
1. **Nové ForgeBlock balíky** — EF Core, AutoMapper, FluentValidation, MediatR
2. **NuGet distribuce** — každý ForgeBlock jako NuGet balík
3. **Marketplace discovery** — registr dostupných ForgeBlocků

---

## 1. Nové ForgeBlock balíky (F1)

### Prioritizace dle `27-ForgeBlock-External-Libraries.md`

| # | Balík | ForgeBlock handle | Vhodnost | Generuje |
|---|-------|-------------------|----------|----------|
| 1 | **EF Core** | `orm-ef-core` | ⭐⭐⭐⭐⭐ | DbContext, entity config, migrace, repository |
| 2 | **AutoMapper** | `mapping-automapper` | ⭐⭐⭐⭐⭐ | Profile třídy, mapping config |
| 3 | **FluentValidation** | `validation-fluent` | ⭐⭐⭐⭐⭐ | Validátory pro každou entitu |
| 4 | **MediatR** | `cqrs-mediatr` | ⭐⭐⭐⭐ | Command/Query + Handlery |
| 5 | **Serilog** | `logging-serilog` | ⭐⭐⭐⭐ | Logger konfigurace, enrichery |
| 6 | **MassTransit** | `messaging-masstransit` | ⭐⭐⭐⭐ | Consumer/Producer, message DTO |

### Struktura ForgeBlock balíku

```
Src/ForgeBlocks/EntityFramework/
├── MetaForge.ForgeBlocks.EntityFrameworkCore.csproj
├── EfCoreForgeBlock.cs              ← implementuje IForgeBlockCapabilityPackage
├── Capabilities/
│   ├── DbContextCapability.cs
│   ├── EntityConfigurationCapability.cs
│   └── RepositoryCapability.cs
├── Templates/
│   ├── DbContext.scriban
│   ├── EntityConfig.scriban
│   └── Repository.scriban
└── Catalog/
    └── EfCoreCatalogEntries.cs
```

### Příklad: EfCoreForgeBlock

```csharp
public sealed class EfCoreForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "orm-ef-core";
    public string Version => "1.0.0";
    public ForgeBlockPackageDescriptor Descriptor { get; } = new()
    {
        DisplayName = "Entity Framework Core",
        Description = "Generuje DbContext, entity konfiguraci, migrace a repository vrstvu",
        Tags = ["orm", "ef-core", "database", "sql", "migration"],
        Tier = GeneratorTier.Infrastructure,  // ← PAID TIER
    };

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = [
        new("generate-dbcontext", "Generuje AppDbContext s DbSet<T> pro každou entitu"),
        new("generate-entity-config", "Generuje IEntityTypeConfiguration<T> pro každou entitu"),
        new("generate-repository", "Generuje repository interface + implementaci"),
        new("generate-migration", "Generuje EF Core migraci"),
    ];

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = [
        new("ef-core-config", "Konfigurace EF Core — connection string, provider"),
        new("ef-core-audit", "Audit — CreatedAt, UpdatedAt, IsDeleted"),
        new("ef-core-soft-delete", "Soft delete — IsDeleted flag + query filter"),
    ];

    public void Register(ForgeBlockRegistry registry)
    {
        foreach (var entry in CatalogEntries)
            registry.RegisterCapability(Handle, entry);
    }
}
```

---

## 2. NuGet distribuce (F2)

### Koncept

Každý ForgeBlock je samostatný NuGet balík:

```
MetaForge.ForgeBlocks.EntityFrameworkCore.1.0.0.nupkg
├── lib/net10.0/
│   └── MetaForge.ForgeBlocks.EntityFrameworkCore.dll
├── content/
│   └── Templates/
│       ├── DbContext.scriban
│       └── Repository.scriban
└── MetaForge.ForgeBlocks.EntityFrameworkCore.props
    └── <MetaForgeForgeBlock>true</MetaForgeForgeBlock>
```

### Auto-discovery

Při startu aplikace se automaticky objeví všechny nahrané ForgeBlocky:

```csharp
public static class ForgeBlockDiscovery
{
    public static void DiscoverFromAssemblies(ForgeBlockRegistry registry, IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var packageTypes = assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IForgeBlockPackage)) && !t.IsAbstract);

            foreach (var type in packageTypes)
            {
                var package = (IForgeBlockPackage)Activator.CreateInstance(type)!;
                registry.Register(package);
            }
        }
    }
}
```

---

## 3. Marketplace discovery

### Koncept

Centrální NuGet feed s ForgeBlock balíky:

```
https://packages.metaforge.io/v3/index.json
└── metaforge-forgeblocks/
    ├── orm-ef-core/
    ├── mapping-automapper/
    ├── validation-fluent/
    └── ...
```

### CLI integrace

```bash
# Vyhledání ForgeBlocků
metaforge forgeblock search "ef core"
  → orm-ef-core (Entity Framework Core) ⭐⭐⭐⭐⭐  v1.0.0
  → orm-ef-core-lite (EF Core Lite)      ⭐⭐⭐⭐    v0.9.0

# Instalace
metaforge forgeblock install orm-ef-core
  → Instaluji MetaForge.ForgeBlocks.EntityFrameworkCore...
  → ✅ Nainstalováno

# Použití v modelu
metaforge add-entity Customer --forgeblock orm-ef-core
```

---

## Monetizační vazba

| ForgeBlock | Tier | Cena |
|------------|------|------|
| Math, String, Validation (basic) | Free | Zdarma |
| AutoMapper | Domain | Zdarma / Low-cost |
| EF Core, Dapper | Infrastructure | Placené |
| MediatR, MassTransit | Infrastructure | Placené |
| Serilog | Infrastructure | Placené |

---

## Odhad

| Fáze | Dny |
|------|-----|
| EF Core ForgeBlock | 2 dny |
| AutoMapper ForgeBlock | 1 den |
| FluentValidation ForgeBlock | 1 den |
| NuGet packaging + .props | 1 den |
| ForgeBlockDiscovery (auto-load) | 0,5 dne |
| Marketplace feed (statický NuGet) | 0,5 dne |
| CLI install/uninstall/search | 1 den |
| Testy | 1 den |
| **Celkem** | **8 dní** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-008 (ForgeBlocks base) | ✅ Hotovo |
| PROP-015 (CatalogManager propojení) | 🟡 Kandidát |
| PROP-017 (Packaging) | 🟢 Nízká |
| PROP-025 (Generators monetization) | 📝 Navrženo (pro tier systém) |
