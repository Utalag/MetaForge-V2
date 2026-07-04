# PROP-028: Infrastructure — Persistence, Konfigurace, Caching

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-010 (Infrastructure persistence — kandidát), PROP-020 (BusinessModel upgrade)

---

## Cíl

Vytvořit `MetaForge.Infrastructure` projekt s:
1. **JSONL persistencí CommandLogu** — ukládání a načítání commandů
2. **Konfiguračním modelem** — `IOptions<T>` pro všechny vrstvy
3. **Caching vrstvou** — checkpointy pro rychlé replaye

---

## 1. MetaForge.Infrastructure — struktura (E1)

```
Src/MetaForge.Infrastructure/
├── MetaForge.Infrastructure.csproj
├── Persistence/
│   ├── ICommandLogRepository.cs
│   ├── IDocumentRepository.cs
│   ├── JsonCommandLogRepository.cs      ← JSONL append-only soubor
│   ├── JsonDocumentRepository.cs        ← JSON snapshot dokumentu
│   └── InMemoryCommandLogRepository.cs  ← pro testy
├── Caching/
│   ├── IProjectionCache.cs
│   ├── CheckpointProjectionCache.cs
│   └── BusinessProjectionCheckpoint.cs
├── Configuration/
│   ├── MetaForgeOptions.cs
│   ├── StorageOptions.cs
│   └── AiOptions.cs
└── FileSystem/
    └── FileSystemProvider.cs
```

### JsonCommandLogRepository

```csharp
public sealed class JsonCommandLogRepository : ICommandLogRepository
{
    private readonly string _filePath;
    private readonly object _writeLock = new();

    public JsonCommandLogRepository(IOptions<StorageOptions> options)
    {
        _filePath = options.Value.CommandLogPath;
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    public async Task AppendAsync(CommandEnvelope envelope, CancellationToken ct)
    {
        var line = JsonSerializer.Serialize(envelope, JsonOptions);
        lock (_writeLock)
        {
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }

    public async Task<IReadOnlyList<CommandEnvelope>> LoadAllAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath)) return [];
        
        var commands = new List<CommandEnvelope>();
        foreach (var line in await File.ReadAllLinesAsync(_filePath, ct))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var envelope = JsonSerializer.Deserialize<CommandEnvelope>(line, JsonOptions);
            if (envelope is not null) commands.Add(envelope);
        }
        return commands.AsReadOnly();
    }

    public async Task<int> GetCountAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath)) return 0;
        return (await File.ReadAllLinesAsync(_filePath, ct)).Count(l => !string.IsNullOrWhiteSpace(l));
    }
}
```

---

## 2. Konfigurační model (E2)

### MetaForgeOptions

```csharp
public sealed class MetaForgeOptions
{
    public const string SectionName = "MetaForge";
    
    public StorageOptions Storage { get; init; } = new();
    public AiOptions Ai { get; init; } = new();
    public GeneratorOptions Generators { get; init; } = new();
}

public sealed class StorageOptions
{
    public string CommandLogPath { get; init; } = "data/commands.jsonl";
    public string DocumentPath { get; init; } = "data/document.json";
    public string CheckpointPath { get; init; } = "data/checkpoints/";
    public bool AutoSave { get; init; } = true;
    public int AutoSaveIntervalMs { get; init; } = 5000;
}

public sealed class GeneratorOptions
{
    public string OutputDirectory { get; init; } = "output/";
    public GeneratorTier LicenseTier { get; init; } = GeneratorTier.Sandbox;
    public string? LicenseKey { get; init; }
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
    },
    "Generators": {
      "LicenseTier": "Domain",
      "LicenseKey": "mf_abc123"
    }
  }
}
```

---

## 3. Caching — Checkpointy (E3)

### Koncept

Při 10 000+ commandech je replay pomalý. Checkpoint = snapshot dokumentu po N commandech:

```csharp
public sealed record BusinessProjectionCheckpoint
{
    public int CommandIndex { get; init; }       // po kolika commandech
    public string DocumentJson { get; init; }     // serializovaný dokument
    public DateTimeOffset CreatedAt { get; init; }
    public string SchemaVersion { get; init; }
}

public sealed class CheckpointProjectionCache : IProjectionCache
{
    private readonly IOptions<StorageOptions> _options;
    private BusinessProjectionCheckpoint? _latest;

    public async Task<BusinessAuthoringDocument?> TryGetFromCheckpointAsync(
        CommandLogStore log, ReplayEngine replay)
    {
        if (_latest is null)
        {
            _latest = await LoadLatestCheckpointAsync();
            if (_latest is null) return null;
        }

        // Replayuj jen commandy od checkpointu dál
        var commands = log.GetFrom(_latest.CommandIndex + 1);
        var document = JsonSerializer.Deserialize<BusinessAuthoringDocument>(_latest.DocumentJson);
        replay.ReplayFrom(document!, commands, 0);
        return document;
    }

    public async Task SaveCheckpointAsync(BusinessAuthoringDocument document, int commandIndex)
    {
        var checkpoint = new BusinessProjectionCheckpoint
        {
            CommandIndex = commandIndex,
            DocumentJson = JsonSerializer.Serialize(document),
            CreatedAt = DateTimeOffset.UtcNow,
            SchemaVersion = document.SchemaVersion,
        };

        var filePath = Path.Combine(_options.Value.CheckpointPath, $"checkpoint-{commandIndex:D8}.json");
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(checkpoint));
        _latest = checkpoint;
    }
}
```

---

## Odhad

| Fáze | Dny |
|------|-----|
| MetaForge.Infrastructure projekt + csproj | 0,25 dne |
| ICommandLogRepository + IDocumentRepository | 0,25 dne |
| JsonCommandLogRepository (JSONL) | 1 den |
| JsonDocumentRepository (snapshot) | 0,5 dne |
| MetaForgeOptions + StorageOptions + AiOptions | 0,5 dne |
| DI registrace | 0,25 dne |
| CheckpointProjectionCache | 1 den |
| Auto-save background service | 0,5 dne |
| Testy | 1 den |
| **Celkem** | **5,25 dne** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-020 (BusinessModel upgrade — immutable CommandEnvelope) | 🟢 Schváleno |
| PROP-010 (Infrastructure persistence — sloučeno sem) | 🟡 Sloučeno |
