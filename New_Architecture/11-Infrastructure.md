# Infrastructure

> Persistence, file system, caching

---

## Princip

- Infrastructure je oddělená vrstva — BusinessModel nezná soubory ani databázi.
- Kontrakty definované v BusinessModel/Core jsou implementovány zde.

## Složková struktura

```
Src/MetaForge.Infrastructure/
├── MetaForge.Infrastructure.csproj
├── Persistence/
│   ├── ICommandLogRepository.cs
│   ├── IDocumentRepository.cs
│   ├── JsonCommandLogRepository.cs
│   ├── JsonDocumentRepository.cs
│   └── InMemoryCommandLogRepository.cs
└── FileSystem/
    └── FileSystemProvider.cs
```

## DI registrace

```csharp
builder.Services.AddSingleton<ICommandLogRepository, JsonCommandLogRepository>();
builder.Services.AddSingleton<IDocumentRepository, JsonDocumentRepository>();
```

