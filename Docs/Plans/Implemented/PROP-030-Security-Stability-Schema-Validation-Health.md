# PROP-030: Bezpečnost a stabilita — Schema Migration, Validation Pipeline, Health Checks

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-020 (BusinessModel upgrade), PROP-026 (Host Surfaces), PROP-028 (Infrastructure)

---

## Cíl

Zavést průřezové mechanismy pro produkční stabilitu:
1. **Schema migration** — migrace starých commandů na nové verze schématu
2. **Validation pipeline** — centrální validace vstupů s user-friendly chybami
3. **Health checks** — monitoring stavu aplikace

---

## 1. Schema Migration (G1)

### Problém

`SchemaVersion = "1.0"` je všude natvrdo. Co když se schéma změní?

```
CommandLog (verze 1.0):
  [1] AddEntity "Customer" payload="Customer"
  [2] AddAttribute "Email" type="email"

Po upgradu na verzi 2.0 (payload je teď JSON):
  [1] AddEntity "Customer" payload="Customer"  ← STARÝ FORMÁT!
  [2] AddAttribute "Email" payload="{\"type\":\"email\"}"  ← NOVÝ FORMÁT

Replay selže — command [1] má starý formát payloadu.
```

### Řešení

```csharp
public interface ICommandMigration
{
    /// <summary>Verze schématu, ze které migrujeme.</summary>
    string FromVersion { get; }

    /// <summary>Verze schématu, do které migrujeme.</summary>
    string ToVersion { get; }

    /// <summary>Migruje command na novou verzi.</summary>
    CommandEnvelope Migrate(CommandEnvelope command);
}

public sealed class CommandMigrationEngine
{
    private readonly List<ICommandMigration> _migrations = new();

    public void RegisterMigration(ICommandMigration migration)
        => _migrations.Add(migration);

    public CommandEnvelope Migrate(CommandEnvelope command)
    {
        var current = command;
        foreach (var migration in _migrations)
        {
            if (current.SchemaVersion == migration.FromVersion)
                current = migration.Migrate(current);
        }
        return current;
    }

    public IReadOnlyList<CommandEnvelope> MigrateAll(IReadOnlyList<CommandEnvelope> commands)
        => commands.Select(Migrate).ToList().AsReadOnly();
}
```

### Příklad: V1→V2 migrace

```csharp
public sealed class V1ToV2Migration : ICommandMigration
{
    public string FromVersion => "1.0";
    public string ToVersion => "2.0";

    public CommandEnvelope Migrate(CommandEnvelope command) => command switch
    {
        // V1: Payload = "Customer" (plain string)
        // V2: Payload = {"name": "Customer"} (JSON)
        { CommandType: "AddEntity" } => command with
        {
            Payload = $$"""{"name": "{{command.Payload}}" }""",
            SchemaVersion = "2.0"
        },
        _ => command with { SchemaVersion = "2.0" }
    };
}
```

### Integrace s ReplayEngine

```csharp
public sealed class ReplayEngine
{
    private readonly CommandMigrationEngine _migration;

    public BusinessAuthoringDocument Replay(IReadOnlyList<CommandEnvelope> commands)
    {
        // Automaticky migruj staré commandy
        var migrated = _migration.MigrateAll(commands);

        var document = new BusinessAuthoringDocument();
        foreach (var command in migrated)
            ApplyCommand(document, command);

        return document;
    }
}
```

---

## 2. Validation Pipeline (G2)

### Problém

Současné CLI spadne na `ArgumentException` s technickou hláškou:

```
Unhandled exception: System.ArgumentException: Název entity nesmí být prázdný.
   at MetaForge.Translator.Host.BusinessAuthoringHostFacade.AddEntity(String name)
```

### Řešení

Centrální validační pipeline s user-friendly výstupem:

```csharp
public sealed class ValidationPipeline
{
    public ValidationResult Validate<T>(T input) where T : class
    {
        var errors = new List<ValidationError>();

        // 1. Data Annotations
        var context = new ValidationContext(input);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        Validator.TryValidateObject(input, context, results, validateAllProperties: true);
        errors.AddRange(results.Select(r => new ValidationError(r.ErrorMessage!, r.MemberNames)));

        // 2. Fluent-style rules
        // ...

        // 3. Business rules
        // ...

        return new ValidationResult(errors.Count == 0, errors.AsReadOnly());
    }
}

public sealed record ValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationError> Errors
);

public sealed record ValidationError(
    string Message,
    IEnumerable<string>? Path = null,
    string? Suggestion = null
);
```

### Integrace s CLI

```csharp
// System.CommandLine — validace před spuštěním handleru
var nameArg = new Argument<string>("name", "Název entity")
    .WithValidation(name =>
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Název entity nesmí být prázdný.";
        if (name.Length > 100)
            return "Název entity je příliš dlouhý (max 100 znaků).";
        return null; // OK
    });

// User-friendly chybový výstup
// ❌ Chyba: Název entity nesmí být prázdný.
// 💡 Nápověda: Použijte 'metaforge add-entity "Customer"' pro vytvoření entity.
```

---

## 3. Health Checks (G3)

### Rozsah

```csharp
public static class MetaForgeHealthChecks
{
    public static async Task<HealthReport> CheckAsync(IServiceProvider services)
    {
        var report = new HealthReport();

        // 1. CommandLog — lze číst?
        try
        {
            var logRepo = services.GetRequiredService<ICommandLogRepository>();
            var count = await logRepo.GetCountAsync(CancellationToken.None);
            report.Add("commandlog", HealthStatus.Healthy, $"Count={count}");
        }
        catch (Exception ex)
        {
            report.Add("commandlog", HealthStatus.Unhealthy, ex.Message);
        }

        // 2. AI backend — je dostupný?
        try
        {
            var ai = services.GetService<IAiBackendAdapter>();
            if (ai is not null)
            {
                report.Add("ai-backend", ai.IsAvailable ? HealthStatus.Healthy : HealthStatus.Degraded,
                    ai.IsAvailable ? "Dostupný" : "Nedostupný");
            }
        }
        catch (Exception ex)
        {
            report.Add("ai-backend", HealthStatus.Unhealthy, ex.Message);
        }

        // 3. Storage — lze zapisovat?
        // 4. ForgeBlock registry — konzistentní?

        return report;
    }
}
```

### WebApi endpoint (TIER 3)

```csharp
app.MapGet("/healthz", async (IServiceProvider sp) =>
{
    var report = await MetaForgeHealthChecks.CheckAsync(sp);
    var statusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
    return Results.Json(report, statusCode: statusCode);
});
```

### MCP health tool

```json
{
  "method": "tools/call",
  "params": { "name": "health_check" }
}
// Response:
{
  "status": "healthy",
  "checks": {
    "commandlog": { "status": "healthy", "count": 47 },
    "ai-backend": { "status": "healthy", "model": "llama3" },
    "storage": { "status": "healthy", "path": "data/" }
  }
}
```

---

## 4. Sandbox bezpečnost

Pro sandbox mód (TIER 0) — izolace a omezení:

```csharp
public sealed class SandboxGuard
{
    private const int MaxEntities = 3;
    private const int MaxAttributesPerEntity = 5;
    private const int MaxCommandLogSize = 100;

    public void Validate(BusinessAuthoringDocument document)
    {
        if (document.Entities.Count > MaxEntities)
            throw new SandboxLimitException($"Sandbox limit: max {MaxEntities} entit.");

        foreach (var entity in document.Entities)
        {
            if (entity.Attributes.Count > MaxAttributesPerEntity)
                throw new SandboxLimitException(
                    $"Sandbox limit: max {MaxAttributesPerEntity} atributů na entitu.");
        }
    }
}
```

---

## Odhad

| Fáze | Dny |
|------|-----|
| CommandMigrationEngine + ICommandMigration | 1 den |
| V1→V2 migrace (payload format) | 0,5 dne |
| Integrace s ReplayEngine | 0,25 dne |
| ValidationPipeline + DataAnnotations | 0,5 dne |
| User-friendly CLI chyby | 0,5 dne |
| Health checks (WebApi + MCP) | 0,5 dne |
| Sandbox guard | 0,25 dne |
| Testy | 0,5 dne |
| **Celkem** | **4 dny** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-020 (BusinessModel upgrade) | 🟢 Schváleno |
| PROP-026 (Host Surfaces — CLI/MCP/WebApi) | 📝 Navrženo |
| PROP-028 (Infrastructure) | 📝 Navrženo |
