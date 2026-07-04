---
name: new-architecture-infrastructure
description: "Pouzij pri: praci s Infrastructure vrstvou — ICommandLogRepository, IDocumentRepository, JsonCommandLogRepository, JsonDocumentRepository, InMemoryCommandLogRepository, FileSystemProvider."
---

# new-architecture-infrastructure

Řídit implementaci Infrastructure vrstvy dle `11-Infrastructure.md`. Hlídat oddělení persistence kontraktů od implementací.

## Kdy použít

- Při práci se soubory v `Src/MetaForge.Infrastructure/`
- Při implementaci ICommandLogRepository, IDocumentRepository
- Při implementaci JSON file-based persistence
- Při implementaci in-memory repository pro testy

## Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **Infrastructure je oddělená vrstva** | BusinessModel nezná soubory ani databázi |
| 2 | **Kontrakty v Infrastructure, implementace zde** | Rozhraní definovaná lokálně v Infrastructure projektu |
| 3 | **JSON file-based jako default** | CommandLog jako JSONL, dokument jako JSON |
| 4 | **In-memory implementace pro testy** | Rychlé, izolované, bez I/O |

## Klíčové typy

### Kontrakty

```csharp
public interface ICommandLogRepository
{
    void Append(CommandEnvelope envelope);
    IReadOnlyList<CommandEnvelope> GetAll();
    int Count { get; }
}

public interface IDocumentRepository
{
    void Save(BusinessAuthoringDocument document);
    BusinessAuthoringDocument? Load();
    bool Exists { get; }
}
```

### Implementace

| Třída | Účel |
|-------|-------|
| `JsonCommandLogRepository` | Append-only JSONL soubor, každý command na vlastním řádku |
| `JsonDocumentRepository` | JSON serializace celého dokumentu |
| `InMemoryCommandLogRepository` | List<CommandEnvelope> v paměti — pro testy |
| `FileSystemProvider` | Abstrakce nad IO — usnadňuje testování |

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
// Produkce
builder.Services.AddSingleton<ICommandLogRepository>(
    sp => new JsonCommandLogRepository("Data/commandlog.jsonl"));
builder.Services.AddSingleton<IDocumentRepository>(
    sp => new JsonDocumentRepository("Data/document.json"));

// Testy
builder.Services.AddSingleton<ICommandLogRepository, InMemoryCommandLogRepository>();
```

## Anti-patterny

- ❌ BusinessModel obsahující logiku ukládání (using System.IO)
- ❌ CommandLogRepository umožňující mazání nebo úpravu záznamů
- ❌ JSON serializace cyklických referencí (BusinessAuthoringDocument je strom)
- ❌ Synchronní I/O v `async` metodách — `File.AppendAllText` uvnitř `lock` vracející `Task.CompletedTask`. Není to skutečně async. Pro produkci použít `await File.AppendAllTextAsync` (pokud dostupné) nebo `Task.Run`.

## Lessons Learned (z Code Review)

| # | Lekce | Dopad |
|---|-------|-------|
| L1 | **InMemory repository musí být thread-safe** — `InMemoryCommandLogRepository` bez `lock` není bezpečný pro paralelní testy. Vždy přidat `lock` na `List<T>` operace. | Opraveno 4.7.2026 |
| L2 | **Async metody musí dělat async I/O** — `AppendAsync` volající synchronní `File.AppendAllText` blokuje vlákno. Pro MVP akceptovatelné, pro produkci nutno předělat. | PROP-028 Issue #2 |
| L3 | **FileSystemProvider jako otevřená třída** — `sealed` + `virtual` = chyba kompilace. Pro mockování musí být třída `public class` (ne `sealed`). | Opraveno 4.7.2026 |

## Výstupní checklist

- [ ] Kontrakty definované v Infrastructure projektu
- [ ] CommandLog je append-only (JSONL formát)
- [ ] In-memory implementace existuje pro testy
- [ ] JSON serializace je otestovaná
