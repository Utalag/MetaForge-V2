# PROP-054: ForgeBlock DI Extension Methods — Plugin registrace služeb

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-13
> **Autor:** Copilot

## Cíl

Rozšířit ForgeBlock plugin systém o DI extension metody — každý ForgeBlock poskytuje vlastní `Add{BlockName}()` metodu pro registraci svých služeb do DI kontejneru.

## Odůvodnění

Aktuálně máme dva manuální patterny DI registrace:
- `InfrastructureServiceRegistration.AddMetaForgeInfrastructure()` — registruje persistence, caching
- `AiServiceRegistration.AddMetaForgeAi()` — registruje AI služby

ForgeBlocky (EF Core, AutoMapper, FluentValidation) deklarují capability `"generate-di"`, ale nemají implementaci. Ruční `Program.cs` v CLI/MCP musí explicitně znát všechny služby, které se mají zaregistrovat.

Cílem je, aby každý ForgeBlock poskytoval vlastní `Add{BlockName}()` extension metodu, která zaregistruje všechny jeho potřebné služby. `Program.cs` pak jen volá:

```csharp
services.AddMetaForgeInfrastructure();
services.AddMetaForgeAi();
services.AddEfCore();            // ← z ForgeBlock plugin
services.AddAutoMapper();        // ← z ForgeBlock plugin
services.AddFluentValidation();  // ← z ForgeBlock plugin
```

## Architektonický pattern

```
IForgeBlockPackage
  ├── IForgeBlockCapabilityPackage (metadata)
  ├── IForgeBlockTemplateProvider (Scriban šablony — ISS-011)
  └── IForgeBlockDiProvider (DI registrace — NOVÉ)
```

```csharp
public interface IForgeBlockDiProvider
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
```

ForgeBlocky, které potřebují DI registraci, implementují toto rozhraní. Při registraci balíku do `ForgeBlockRegistry` se DI registrace automaticky provede.

## Implementace

### 1. Nové rozhraní v Core (`IForgeBlockDiProvider`)

```csharp
namespace MetaForge.Core.ForgeBlockPackages;

public interface IForgeBlockDiProvider
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
```

### 2. ForgeBlock registry integrace

`ForgeBlockRegistry.Register()` automaticky volá `RegisterServices()` pokud balík implementuje `IForgeBlockDiProvider`.

### 3. EfCoreForgeBlock implementace

```csharp
public sealed class EfCoreForgeBlock : IForgeBlockCapabilityPackage, IForgeBlockTemplateProvider, IForgeBlockDiProvider
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? "Data Source=app.db";
            options.UseSqlite(connectionString);
        });

        // Repository pattern
        services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    }
}
```

### 4. CLI bootstrap

```csharp
// Program.cs
var builder = Host.CreateApplicationBuilder(args);

// Core + Infrastructure
builder.Services.AddMetaForgeInfrastructure(useJsonPersistence: true);

// ForgeBlock DI registrace — automaticky z registry
var forgeBlockRegistry = new ForgeBlockRegistry();
forgeBlockRegistry.Register(new EfCoreForgeBlock());
forgeBlockRegistry.Register(new AutoMapperForgeBlock());
forgeBlockRegistry.Register(new FluentValidationForgeBlock());
forgeBlockRegistry.ApplyToDi(builder.Services, builder.Configuration);
```

## Scope

### In scope
- Nové rozhraní `IForgeBlockDiProvider` v Core
- Implementace pro EF Core, AutoMapper, FluentValidation
- Extenze `ForgeBlockRegistry.ApplyToDi()` pro hromadnou registraci
- ForgeBlock discovery integration — automatická detekce balíků v assembly

### Out of scope
- Automatické generování `Program.cs` — Composition Root zůstává ruční
- Generování DI z metadat (reflection-based) — explicitní implementace
- NuGet package reference automatizace (již v `ForgeBlockPackageIntegrator`)

## Implementační dopad

| Soubor | Typ | Popis |
|--------|-----|-------|
| `Src/MetaForge.Core/ForgeBlockPackages/IForgeBlockDiProvider.cs` | Nový | Rozhraní pro DI registraci |
| `Src/MetaForge.Core/ForgeBlockPackages/ForgeBlockRegistry.cs` | Změna | `ApplyToDi()` metoda |
| `Src/ForgeBlocks/EntityFramework/EfCoreForgeBlock.cs` | Změna | Implementuje `IForgeBlockDiProvider` |
| `Src/ForgeBlocks/AutoMapper/AutoMapperForgeBlock.cs` | Změna | Implementuje `IForgeBlockDiProvider` |
| `Src/ForgeBlocks/FluentValidation/FluentValidationForgeBlock.cs` | Změna | Implementuje `IForgeBlockDiProvider` |
| `Src/MetaForge.Cli/Program.cs` | Změna | Volání `ApplyToDi()` |

## Implementační fáze

### Fáze 1: Rozhraní + registr (~0.5 dne)
- `IForgeBlockDiProvider` v Core
- `ForgeBlockRegistry.ApplyToDi()` metoda

### Fáze 2: EfCoreForgeBlock DI (~0.5 dne)
- `RegisterServices()` — DbContext registration
- NuGet package: `Microsoft.EntityFrameworkCore.Sqlite`

### Fáze 3: AutoMapper + FluentValidation DI (~0.5 dne)
- `RegisterServices()` pro AutoMapper, FluentValidation

### Fáze 4: CLI integrace (~0.5 dne)
- Zapojit `ApplyToDi()` do CLI `Program.cs`
- Test E2E: registrace → build → ověření

## Validace

- Build: `dotnet build` bez chyb
- CLI: `metaforge generate --output ./out` projde pipeline bez chyb
- DI ověření: `ServiceCollection` obsahuje registrované služby (unit test)

## Odhad

| Fáze | Odhad |
|------|-------|
| Rozhraní + registr | 0.5 dne |
| EfCore DI | 0.5 dne |
| AutoMapper + FluentValidation DI | 0.5 dne |
| CLI integrace + testy | 0.5 dne |
| **Celkem** | **~2 dny** |
