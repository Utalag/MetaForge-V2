---
name: new-architecture-security-stability
description: "Pouzij pri: schema migraci, CommandMigration, validation pipeline, health checks, sandbox guard — PROP-030 a souvisejici."
---

# new-architecture-security-stability

Zajistit bezpečnost a stabilitu platformy — schema migrace commandů, validační pipeline, health monitoring, sandbox omezení.

## Kdy použít

- Při implementaci `CommandMigrationEngine` a `ICommandMigration`
- Při práci s `ValidationPipeline`
- Při implementaci health checks (`/healthz`, MCP health tool)
- Při práci se sandbox guardem (TIER 0 omezení)
- Při migraci starých command logů na novou verzi schématu

## Invarianty

| # | Invariant | Důsledek |
|---|-----------|----------|
| 1 | **SchemaVersion konzistentní** | Každý command nese verzi. Migrace probíhá při replayi. |
| 2 | **Migrace je idempotentní** | Dvojitá migrace stejného commandu = stejný výsledek |
| 3 | **Validace před mutací** | Validace musí proběhnout PŘED Apply, ne po |
| 4 | **Health checks non-blocking** | `/healthz` nikdy nesmí spadnout — vrací status i při degradaci |
| 5 | **Sandbox limity vynucené** | TIER 0 nesmí nikdy vygenerovat exportovatelný kód |

## Klíčové typy

### CommandMigrationEngine

```csharp
public sealed class CommandMigrationEngine
{
    public void RegisterMigration(ICommandMigration migration);
    public CommandEnvelope Migrate(CommandEnvelope command);
    public IReadOnlyList<CommandEnvelope> MigrateAll(IReadOnlyList<CommandEnvelope> commands);
}

public interface ICommandMigration
{
    string FromVersion { get; }
    string ToVersion { get; }
    CommandEnvelope Migrate(CommandEnvelope command);
}
```

### ValidationPipeline

```csharp
public sealed class ValidationPipeline
{
    public ValidationResult Validate<T>(T input) where T : class;
}

public sealed record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors);
```

### Health Checks

```csharp
public static class MetaForgeHealthChecks
{
    public static Task<HealthReport> CheckAsync(IServiceProvider services);
    // Kontroluje: CommandLog, AI backend, Storage, ForgeBlock registry
}
```

### SandboxGuard

```csharp
public sealed class SandboxGuard
{
    // TIER 0 limity: max 3 entity, max 5 atributů/entitu, max 100 commandů
    public void Validate(BusinessAuthoringDocument document);
}
```

## Workflow migrace

```
1. Detekce staré verze commandu v CommandLog
2. CommandMigrationEngine najde migraci FromVersion → ToVersion
3. Aplikuje migraci (transformace payloadu, přidání/odebrání polí)
4. Vrátí command s novou SchemaVersion
5. ReplayEngine přehraje migrovaný command
```

## Anti-patterny

- ❌ Přímá modifikace commandů v CommandLog (mimo migrační engine)
- ❌ Ignorování SchemaVersion v CommandEnvelope
- ❌ Blokující health check (musí být async, s timeoutem)
- ❌ Sandbox bez vynucených limitů

## Výstupní checklist

- [ ] Každý command má SchemaVersion
- [ ] Migrační engine pokrývá všechny známé verze
- [ ] Validace probíhá před Apply
- [ ] Health check vrací strukturovaný report
- [ ] Sandbox guard aktivní pro TIER 0
