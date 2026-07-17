# Infrastructure

> Persistence, konfigurace, caching, file system

**Aktualizace:** PROP-028 (2026-07-04) — JSONL persistence, IOptions&lt;T&gt; konfigurace, checkpoint caching.

---

## Princip

- Infrastructure je oddělená vrstva — BusinessModel nezná soubory ani databázi.
- Kontrakty definované v BusinessModel/Core jsou implementovány zde.
- Projekty: `MetaForge.Infrastructure` (net10.0). NuGet závislosti: `Microsoft.Extensions.Options`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Hosting.Abstractions`.

## Složková struktura

```
Src/MetaForge.Infrastructure/
├── MetaForge.Infrastructure.csproj
├── InfrastructureServiceRegistration.cs   ← DI extension method
├── Persistence/
│   ├── ICommandLogRepository.cs           ← kontrakt pro append-only command log
│   ├── IDocumentRepository.cs             ← kontrakt pro JSON snapshot dokumentu
│   ├── JsonCommandLogRepository.cs        ← JSONL append-only soubor
│   ├── JsonDocumentRepository.cs          ← JSON snapshot celého dokumentu
│   └── InMemoryCommandLogRepository.cs    ← pro testy (in-memory)
├── Caching/
│   ├── IProjectionCache.cs                ← kontrakt pro checkpoint caching
│   ├── CheckpointProjectionCache.cs       ← checkpointy pro rychlý replay
│   └── BusinessProjectionCheckpoint.cs    ← snapshot dokumentu po N commandech
├── Configuration/
│   ├── MetaForgeOptions.cs                ← root konfigurace (Storage, Ai)
│   ├── StorageOptions.cs                  ← cesty, auto-save
│   └── AiOptions.cs                       ← endpoint, model, temperature
└── FileSystem/
    └── FileSystemProvider.cs              ← abstrakce IO pro testovatelnost
```

---

## Persistence

### ICommandLogRepository

```csharp
public interface ICommandLogRepository
{
    Task AppendAsync(CommandEnvelope envelope, CancellationToken ct = default);
    Task<IReadOnlyList<CommandEnvelope>> LoadAllAsync(CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
}
```

### JsonCommandLogRepository — JSONL append-only

```csharp
// Ukládá commandy jako JSONL (jeden JSON objekt na řádku).
// Thread-safe přes lock. Vytvoří adresář automaticky.
public sealed class JsonCommandLogRepository : ICommandLogRepository
{
    public JsonCommandLogRepository(IOptions<StorageOptions> options);
    public Task AppendAsync(CommandEnvelope envelope, CancellationToken ct = default);
    public async Task<IReadOnlyList<CommandEnvelope>> LoadAllAsync(CancellationToken ct = default);
    public async Task<int> GetCountAsync(CancellationToken ct = default);
}
```

### IDocumentRepository + JsonDocumentRepository

```csharp
public interface IDocumentRepository
{
    Task SaveAsync(BusinessAuthoringDocument document, CancellationToken ct = default);
    Task<BusinessAuthoringDocument?> LoadAsync(CancellationToken ct = default);
}

// Ukládá celý dokument jako JSON (pretty-print). Vytvoří adresář automaticky.
public sealed class JsonDocumentRepository : IDocumentRepository
{
    public JsonDocumentRepository(IOptions<StorageOptions> options);
    public async Task SaveAsync(BusinessAuthoringDocument document, CancellationToken ct = default);
    public async Task<BusinessAuthoringDocument?> LoadAsync(CancellationToken ct = default);
}
```

### InMemoryCommandLogRepository

```csharp
// Pro testy — uchovává commandy v List<CommandEnvelope>.
public sealed class InMemoryCommandLogRepository : ICommandLogRepository { ... }
```

---

## Caching — Checkpointy (PROP-028)

Při 10 000+ commandech je replay pomalý. Checkpoint = snapshot dokumentu po N commandech:

```csharp
public sealed record BusinessProjectionCheckpoint
{
    public int CommandIndex { get; init; }
    public string DocumentJson { get; init; } = "{}";
    public DateTimeOffset CreatedAt { get; init; }
    public string SchemaVersion { get; init; } = "1.0";
}

public interface IProjectionCache
{
    Task<BusinessAuthoringDocument?> TryGetFromCheckpointAsync(CancellationToken ct = default);
    Task SaveCheckpointAsync(BusinessAuthoringDocument document, int commandIndex, CancellationToken ct = default);
    BusinessProjectionCheckpoint? GetLatestCheckpoint();
}

// Implementace: ukládá checkpoint do JSON souboru v checkpointPath.
// Loaduje nejnovější checkpoint při startu.
public sealed class CheckpointProjectionCache : IProjectionCache
{
    public CheckpointProjectionCache(IOptions<StorageOptions> options);
    // ...
}
```

---

## Konfigurace — IOptions&lt;T&gt; (PROP-028)

```csharp
public sealed class MetaForgeOptions
{
    public const string SectionName = "MetaForge";
    public StorageOptions Storage { get; init; } = new();
    public AiOptions Ai { get; init; } = new();
}

public sealed class StorageOptions
{
    public string CommandLogPath { get; init; } = "data/commands.jsonl";
    public string DocumentPath { get; init; } = "data/document.json";
    public string CheckpointPath { get; init; } = "data/checkpoints/";
    public bool AutoSave { get; init; } = true;
    public int AutoSaveIntervalMs { get; init; } = 5000;

    // PROP-061: Feedback Platform
    public string FeedbackCachePath { get; init; } = "data/feedback/";
    public string LearningArchivePath { get; init; } = "data/learning/archive.jsonl";
}

public sealed class AiOptions
{
    public string Endpoint { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = "llama3";
    public double Temperature { get; init; } = 0.3;
    public int MaxTokens { get; init; } = 500;
    public int TimeoutSeconds { get; init; } = 120;
}
```

### appsettings.json

```json
{
  "MetaForge": {
    "Storage": {
      "CommandLogPath": "data/commands.jsonl",
      "AutoSave": true,
      "AutoSaveIntervalMs": 5000
    },
    "Ai": {
      "Endpoint": "http://localhost:11434",
      "Model": "llama3",
      "Temperature": 0.3,
      "MaxTokens": 500
    }
  }
}
```

---

## DI registrace (PROP-028)

```csharp
// Extension method pro jednoduchou registraci
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddMetaForgeInfrastructure(
        this IServiceCollection services,
        bool useJsonPersistence = true)
    {
        // Bindování konfigurace
        services.AddOptions<MetaForgeOptions>()
            .BindConfiguration(MetaForgeOptions.SectionName);

        // Persistence
        if (useJsonPersistence)
        {
            services.AddSingleton<ICommandLogRepository, JsonCommandLogRepository>();
            services.AddSingleton<IDocumentRepository, JsonDocumentRepository>();
        }
        else
        {
            services.AddSingleton<ICommandLogRepository, InMemoryCommandLogRepository>();
        }

        // Caching
        services.AddSingleton<IProjectionCache, CheckpointProjectionCache>();
        return services;
    }
}
```

---

## FileSystem

```csharp
// Abstrakce IO operací pro testovatelnost (virtual metody)
public class FileSystemProvider
{
    public virtual bool DirectoryExists(string path);
    public virtual void CreateDirectory(string path);
    public virtual bool FileExists(string path);
    public virtual Task<string> ReadAllTextAsync(string path, CancellationToken ct = default);
    public virtual Task WriteAllTextAsync(string path, string content, CancellationToken ct = default);
    public virtual void AppendAllText(string path, string content);
    public virtual Task<string[]> ReadAllLinesAsync(string path, CancellationToken ct = default);
    public virtual string[] GetFiles(string path, string searchPattern);
}
```



### Verification (PROP-057 — ✅ implementováno 2026-07-17)

Src/MetaForge.Infrastructure/Verification/:
- **VerificationState** enum — Unknown, Running, Passed, Failed, Stale
- **VerificationRecord** — ElementId, Fingerprint, State, LastVerified, FailureDiagnostics
- **IVerificationStateStore** — GetAsync/SetAsync/InvalidateAsync
- **InMemoryVerificationStateStore** — Dictionary-based implementace

### Sandbox (PROP-058 — ✅ kontrakty 2026-07-17, MVP pending)

Src/MetaForge.Infrastructure/Sandbox/:
- **ISandboxExecutionService** — ExecuteAsync(SandboxExecutionRequest)
- **SandboxExecutionRequest** — Method, Contract?, InputJson, Mode, Timeout
- **SandboxExecutionResult** — Success, OutputJson, ExceptionMessage, CompilationErrors
- **SandboxMode** — Preview (tolerant) / Export (strict)

