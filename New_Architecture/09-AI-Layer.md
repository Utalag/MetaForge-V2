# AI Layer

> Kontrakty v Core/Translator, implementace v MetaForge.Ai, Ollama, PromptRegistry

**Aktualizace:** PROP-027 (2026-07-04) — MetaForge.Ai projekt, OllamaAdapter, PromptRegistry, PromptEvaluator.

---

## Princip

- **Kontrakty** jsou definovány v projektech, které je potřebují (Core, Translator).
- **Implementace** jsou v `MetaForge.Ai` (net10.0, NuGet: `System.Text.Json`).
- Technická transportní abstrakce `IAiBackendAdapter` je definována i implementována v MetaForge.Ai.
- AI je **volitelná** — všechny implementace mají graceful fallback (vrací `null`).

## Struktura projektu

```
Src/MetaForge.Ai/
├── MetaForge.Ai.csproj                    (net10.0)
├── AiServiceRegistration.cs               ← DI extension method
├── Abstractions/
│   └── IAiBackendAdapter.cs               ← kontrakt pro AI backend
├── Adapters/
│   └── OllamaAdapter.cs                   ← Ollama HTTP API (/api/generate)
├── Inference/
│   ├── AiConstraintInferencer.cs           ← AI implementace IConstraintInferencer
│   └── Prompting/
│       └── ConstraintPromptBuilder.cs
├── Translation/
│   ├── AiTranslationService.cs             ← AI implementace ITranslationService
│   └── Prompting/
│       ├── TranslationPromptBuilder.cs
│       └── SemanticBriefJson.cs
└── Prompts/
    ├── PromptRegistry.cs                  ← verzované prompt šablony
    ├── PromptTemplate.cs                  ← YAML frontmatter + systémový/uživatelský prompt
    ├── PromptEvaluationService.cs         ← evaluace kvality promptů
    ├── PromptEvaluator.cs                 ← porovnání verzí promptů
    └── Templates/
        ├── constraint-inference.prompt.md
        ├── entity-suggestion.prompt.md
        └── translation-enrich.prompt.md
```

---

## IAiBackendAdapter

```csharp
// Složka: Src/MetaForge.Ai/Abstractions/

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
// Složka: Src/MetaForge.Ai/Adapters/

// Komunikuje s Ollama serverem na localhost:11434/api/generate.
// Default model: gemma4 (lze změnit v konstruktoru).
// Všechny metody chytají výjimky a vrací null = graceful fallback.
public sealed class OllamaAdapter : IAiBackendAdapter
{
    public OllamaAdapter(string endpoint = "http://localhost:11434", string model = "gemma4");
    public string ProviderName => "Ollama";
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default);   // CHECK /api/tags
    public async Task<string?> SendAsync(string prompt, CancellationToken ct = default);
    public async Task<T?> SendJsonAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}
```

---

## AiConstraintInferencer (PROP-027)

```csharp
// Složka: Src/MetaForge.Ai/Inference/
// Implementuje IConstraintInferencer z Core.
// Používá IAiBackendAdapter pro AI inferenci validačních pravidel.
// Staví česky psaný prompt. Při selhání vrací prázdné pole.

public sealed class AiConstraintInferencer : IConstraintInferencer
{
    public AiConstraintInferencer(IAiBackendAdapter backend);
    public IReadOnlyList<string> Infer(string attributeName, TypeModel type);
}
```

---

## AiTranslationService (PROP-027)

```csharp
// Složka: Src/MetaForge.Ai/Translation/
// Implementuje ITranslationService z Translatoru.
// EnrichAsync: zkontroluje dostupnost → pošle AI prompt s atributem/entitou → EnrichmentResult nebo null.

public sealed class AiTranslationService : ITranslationService
{
    public AiTranslationService(IAiBackendAdapter backend);
    public async Task<EnrichmentResult?> EnrichAsync(
        BusinessAttributeNode attribute, ProjectionView context, CancellationToken ct = default);
}
```

---

## PromptRegistry (PROP-027)

Verzované prompt šablony s YAML frontmatter:

```csharp
// Složka: Src/MetaForge.Ai/Prompts/

public sealed class PromptRegistry
{
    public void LoadFromDirectory(string path);                    // načte .prompt.md soubory
    public void Register(PromptTemplate template);                 // ruční registrace
    public PromptTemplate Get(string name);                        // throws if not found
    public bool TryGet(string name, [MaybeNullWhen(false)] out PromptTemplate template);
    public IReadOnlyList<PromptTemplate> GetAll();
}

public sealed record PromptTemplate
{
    public string Name { get; init; }
    public int Version { get; init; } = 1;
    public string Model { get; init; } = "llama3";
    public double Temperature { get; init; } = 0.3;
    public int MaxTokens { get; init; } = 500;
    public string SystemPrompt { get; init; }
    public string UserPromptTemplate { get; init; }
    public IReadOnlyList<string> Tags { get; init; }
    public string? Created { get; init; }
    public string? Author { get; init; }

    public string BuildPrompt(IReadOnlyDictionary<string, string> placeholders);
    // Nahrazuje {{key}} placeholders v UserPromptTemplate
}
```

### Prompt template formát (.prompt.md)

```markdown
---
version: 1
model: llama3
temperature: 0.3
maxTokens: 500
created: 2026-07-04
author: copilot
tags: [enrichment, translation]
---
# System Prompt: Business Attribute Enrichment

Jsi expertní C# vývojář. Na základě business atributu navrhni:
1. Konkrétní C# typ
2. Validační pravidla
3. Výchozí hodnotu

Vstupní formát: JSON s atributem.
Výstupní formát: JSON podle schématu...
```

### Prompt templates

| Soubor | Placeholdery | Účel |
|--------|-------------|------|
| `constraint-inference.prompt.md` | `{{attributeName}}`, `{{attributeType}}` | Inferuje validační pravidla |
| `entity-suggestion.prompt.md` | `{{domainDescription}}` | Navrhuje entity z doménového popisu |
| `translation-enrich.prompt.md` | `{{entityAttributes}}`, `{{attributeName}}`, `{{currentType}}` | Enrichment atributu |

---

## PromptEvaluator (PROP-027)

```csharp
// Složka: Src/MetaForge.Ai/Prompts/

public sealed class PromptEvaluator
{
    public async Task<PromptEvalResult> EvaluateAsync(
        PromptTemplate prompt,
        IReadOnlyList<PromptTestCase> testCases,
        IAiBackendAdapter backend);
}

public sealed class PromptEvaluationService
{
    // Porovná dvě verze promptu na stejných testovacích datech
    public async Task<PromptEvalResult> SelectBestAsync(
        PromptTemplate v1, PromptTemplate v2,
        IReadOnlyList<PromptTestCase> testCases,
        IAiBackendAdapter backend);
}

public sealed record PromptTestCase(string Name, string Input, Func<string?, bool> Validator);
public sealed record PromptEvalResult(string PromptName, double PassRate, IReadOnlyList<TestCaseResult> Results);
public sealed record TestCaseResult(string Name, bool Passed, string? Output);
```

---

## DI registrace (PROP-027)

### AiRepairSuggestionService (PROP-061)

```csharp
// Implementace IRepairSuggestionService — AI-assisted repair návrhy
// Graceful fallback: bez AI vrací prázdné pole, nikdy neblokuje
public sealed class AiRepairSuggestionService : IRepairSuggestionService
{
    public AiRepairSuggestionService(IAiBackendAdapter? ai);
    public Task<IReadOnlyList<RepairRecommendation>> SuggestRepairsAsync(
        AuthoringFeedbackRecord feedback, CancellationToken ct);
}
```

---

```csharp
// Extension method pro jednoduchou registraci
public static class AiServiceRegistration
{
    public static IServiceCollection AddMetaForgeAi(
        this IServiceCollection services,
        string ollamaUrl = "http://localhost:11434",
        string model = "gemma3:12b")
    {
        services.AddSingleton<IAiBackendAdapter>(_ => new OllamaAdapter(ollamaUrl, model));
        services.AddSingleton<IConstraintInferencer, AiConstraintInferencer>();
        services.AddSingleton<ITranslationService, AiTranslationService>();
        return services;
    }
}

// Volitelné — registruj jen pokud je AI nakonfigurováno:
if (builder.Configuration.GetSection("Ai").Exists())
{
    builder.Services.AddMetaForgeAi();
}
```
