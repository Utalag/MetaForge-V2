---
name: new-architecture-forgeblocks
description: "Pouzij pri: praci s ForgeBlocks — IForgeBlockPackage, ForgeBlockRegistry, ForgeBlockCapability, marketplace, NuGet distribuce, CatalogManager propojeni."
---

# new-architecture-forgeblocks

Zajistit konzistentní implementaci ForgeBlock ekosystému dle `27-ForgeBlock-External-Libraries.md`, PROP-008, PROP-029 a souvisejících.

## Kdy použít

- Při práci se soubory v `Src/ForgeBlocks/`
- Při implementaci `IForgeBlockPackage`, `IForgeBlockCapabilityPackage`
- Při práci s `ForgeBlockRegistry`, `ForgeBlockCapability`
- Při vytváření nových ForgeBlock balíků (EF Core, AutoMapper, FluentValidation, ...)
- Při práci s NuGet distribucí ForgeBlocků
- Při propojování ForgeBlocků s `CatalogManager`

## Invarianty

| # | Invariant | Důsledek |
|---|-----------|----------|
| 1 | **ForgeBlock = NuGet balík** | Každý ForgeBlock je samostatně verzovatelný a distribuovatelný |
| 2 | **Registrace přes ForgeBlockRegistry** | Všechny balíky registrované centrálně, thread-safe |
| 3 | **CatalogManager integrace** | ForgeBlock presety se registrují do CatalogManager pro ResolveType |
| 4 | **Tier-aware** | ForgeBlocky respektují monetizační tier model (PROP-025) |
| 5 | **Capability metadata** | Každý ForgeBlock nese capability metadata pro discovery |

## Klíčové typy

### IForgeBlockPackage

```csharp
public interface IForgeBlockPackage
{
    string Handle { get; }                          // "orm-ef-core", "mapping-automapper"
    string Version { get; }                          // "1.0.0"
    IReadOnlyList<ForgeBlockCapability> Capabilities { get; }
    DiscoveryMetadata Discovery { get; }
    void Register(ForgeBlockRegistry registry);
}
```

### IForgeBlockCapabilityPackage

```csharp
public interface IForgeBlockCapabilityPackage : IForgeBlockPackage
{
    ForgeBlockPackageDescriptor Descriptor { get; }
    IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; }
}
```

### ForgeBlockRegistry

```csharp
public sealed class ForgeBlockRegistry
{
    public void Register(IForgeBlockPackage package);
    public IForgeBlockPackage? GetPackage(string handle);
    public IReadOnlyList<IForgeBlockPackage> SearchByTag(string tag);
    public IReadOnlyList<ForgeBlockCapability> GetAllCapabilities();
}
```

## Prioritizace ForgeBlocků

Dle `27-ForgeBlock-External-Libraries.md`:

| Priorita | Balík | Vhodnost | Tier |
|----------|-------|----------|------|
| 1 | EF Core (`orm-ef-core`) | ⭐⭐⭐⭐⭐ | Infrastructure |
| 2 | AutoMapper (`mapping-automapper`) | ⭐⭐⭐⭐⭐ | Domain |
| 3 | FluentValidation (`validation-fluent`) | ⭐⭐⭐⭐⭐ | Infrastructure |
| 4 | MediatR (`cqrs-mediatr`) | ⭐⭐⭐⭐ | Infrastructure |
| 5 | Serilog (`logging-serilog`) | ⭐⭐⭐⭐ | Infrastructure |
| 6 | MassTransit (`messaging-masstransit`) | ⭐⭐⭐⭐ | Infrastructure |

## Struktura ForgeBlock balíku

```
Src/ForgeBlocks/{Název}/
├── MetaForge.ForgeBlocks.{Název}.csproj
├── {Název}ForgeBlock.cs              ← implementuje IForgeBlockCapabilityPackage
├── Capabilities/                     ← jednotlivé capability třídy
├── Templates/                        ← Scriban šablony pro generování
└── Catalog/                          ← CatalogEntries pro CatalogManager
```

## Monetizační tier

| Tier | ForgeBlocky |
|------|-------------|
| **Free** | Math, String, Validation (basic) |
| **Domain** | AutoMapper, Mapperly |
| **Infrastructure** | EF Core, Dapper, FluentValidation, MediatR, Serilog, MassTransit |
| **Full** | Custom ForgeBlock SDK, deployment templaty |

## ForgeBlock Discovery

```csharp
public static class ForgeBlockDiscovery
{
    public static void DiscoverFromAssemblies(ForgeBlockRegistry registry, IEnumerable<Assembly> assemblies);
}
```

## Anti-patterny

- ❌ ForgeBlock bez capability metadat
- ❌ Hardcodované ForgeBlocky — vždy jít přes registry
- ❌ Ignorování tier modelu při generování
- ❌ ForgeBlock závislý na konkrétní host surface

## Výstupní checklist

- [ ] ForgeBlock implementuje `IForgeBlockPackage` nebo `IForgeBlockCapabilityPackage`
- [ ] Registrace přes `ForgeBlockRegistry`
- [ ] CatalogEntries propojené s `CatalogManager`
- [ ] Tier správně nastaven (`GeneratorTier.Domain` / `Infrastructure`)
- [ ] Obsahuje Scriban šablony pro generování
- [ ] Testy: registrace, capability listing, catalog propojení
