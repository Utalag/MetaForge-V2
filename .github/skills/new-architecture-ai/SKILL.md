---
name: new-architecture-ai
description: "Pouzij pri: praci s AI vrstvou — IAiBackendAdapter, OllamaAdapter, AiConstraintInferencer, AiTranslationService, graceful fallback strategii, prompt building."
---

# new-architecture-ai

Řídit implementaci AI vrstvy dle `09-AI-Layer.md`. Hlídat oddělení kontraktů (v Core/Translator) od implementací (v MetaForge.Ai) a graceful fallback.

## Kdy použít

- Při práci se soubory v `Src/MetaForge.Ai/`
- Při implementaci AI implementací Core/Translator kontraktů
- Při práci s IAiBackendAdapter a provider adaptéry
- Při návrhu prompt building a fallback strategie

## Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **AI je volitelná** | Systém musí fungovat bez AI. Žádná feature nesmí vyžadovat AI. |
| 2 | **Kontrakty oddělené od implementací** | Rozhraní v Core/Translator, implementace v MetaForge.Ai |
| 3 | **Tier 2 AI vrací strukturovaná data** | Nikdy volný text pro uživatele — vrací strukturované objekty |
| 4 | **Graceful fallback** | AI selhání → vrátit null → host surface rozhodne o hlášce |
| 5 | **AI pracuje nad synchronizovaným stavem** | Ne nad prázdným promptem — vždy dostává aktuální projekci |

## Architektura

```
Core/Translator (interface)
    ├── IConstraintInferencer (Core)
    ├── IBusinessTranslator (Translator)
    │
    └── MetaForge.Ai (implementace)
        ├── AiConstraintInferencer : IConstraintInferencer
        ├── AiTranslationService
        ├── Abstractions/
        │   └── IAiBackendAdapter
        └── Adapters/
            └── OllamaAdapter
```

## Klíčové typy

### IAiBackendAdapter

```csharp
public interface IAiBackendAdapter
{
    string ProviderName { get; }
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
    Task<string?> SendAsync(string prompt, CancellationToken ct = default);
    Task<T?> SendJsonAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}
```

### OllamaAdapter

```csharp
public sealed class OllamaAdapter : IAiBackendAdapter
{
    public OllamaAdapter(string baseUrl = "http://localhost:11434", string model = "gemma3:12b");
    public string ProviderName => "Ollama";
    public Task<bool> IsAvailableAsync(CancellationToken ct = default);
    public Task<string?> SendAsync(string prompt, CancellationToken ct = default);
    public Task<T?> SendJsonAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}
```

### DI registrace

```csharp
// Volitelné — jen když je AI nakonfigurováno
builder.Services.AddMetaForgeAi("http://localhost:11434", "gemma3:12b");

// Deterministická cesta jako fallback (vždy registrována)
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();
```

## Fallback strategie

| Scénář | AI chování | Fallback |
|--------|-----------|----------|
| AI nedostupné (timeout) | Vrátit null | Deterministický inferencer |
| AI vrací invalidní data | Vrátit null | Původní hodnota beze změny |
| AI není nakonfigurováno | Neregistrovat | Deterministická cesta jako default |
| AI vrací strukturovaná data | Aplikovat enrichment | — |

## Anti-patterny

- ❌ AI jako povinná závislost (žádný graceful fallback)
- ❌ AI kontrakty definované v MetaForge.Ai (patří do Core/Translator)
- ❌ Tier 2 AI vracející volný text pro uživatele
- ❌ AI volaná bez kontextu (synchronizovaného stavu)

## Výstupní checklist

- [ ] AI je volitelná — systém funguje bez AI
- [ ] Fallback je graceful (žádné šíření výjimek)
- [ ] Tier 2 AI vrací strukturovaná data
- [ ] Kontrakty jsou v Core/Translator, ne v MetaForge.Ai
- [ ] Provider abstrakce je rozšiřitelná (Ollama, OpenAI, Azure)
