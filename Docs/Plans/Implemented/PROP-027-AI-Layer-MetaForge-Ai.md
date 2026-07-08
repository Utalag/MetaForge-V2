# PROP-027: AI Layer — MetaForge.Ai projekt, OllamaAdapter, PromptRegistry

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-006 (AI Layer kontrakty — hotovo), PROP-019 (IAiTranslator), PROP-020 (BusinessModel upgrade)

---

## Cíl

Vytvořit `MetaForge.Ai` projekt s plnou AI integrací:
1. **OllamaAdapter** — první AI backend (localhost:11434)
2. **PromptRegistry** — verzované, testované prompty
3. **PromptEvaluator** — automatické vyhodnocení kvality promptů

---

## 1. MetaForge.Ai — struktura projektu (D1)

```
Src/MetaForge.Ai/
├── MetaForge.Ai.csproj
├── Abstractions/
│   └── IAiBackendAdapter.cs
├── Inference/
│   ├── AiConstraintInferencer.cs      ← implementuje IConstraintInferencer (Core)
│   └── Prompting/
│       └── ConstraintPromptBuilder.cs
├── Translation/
│   ├── AiTranslationService.cs        ← implementuje ITranslationService (Translator)
│   └── Prompting/
│       ├── TranslationPromptBuilder.cs
│       └── SemanticBriefJson.cs
├── Backends/
│   ├── OllamaAdapter.cs               ← Ollama HTTP API
│   └── OpenAiAdapter.cs               ← OpenAI-compatible (budoucí)
├── Prompts/
│   ├── PromptRegistry.cs
│   ├── PromptEvaluator.cs
│   └── Templates/
│       ├── constraint-inference.prompt.md
│       ├── translation-enrich.prompt.md
│       └── entity-suggestion.prompt.md
└── Configuration/
    └── AiOptions.cs
```

### IAiBackendAdapter

```csharp
public interface IAiBackendAdapter
{
    bool IsAvailable { get; }
    Task<string?> CompletePromptAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
    Task<JsonDocument?> CompleteStructuredAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}
```

### OllamaAdapter

```csharp
public sealed class OllamaAdapter : IAiBackendAdapter, IDisposable
{
    private readonly HttpClient _http;
    private readonly string _model;

    public OllamaAdapter(string endpoint = "http://localhost:11434", string model = "llama3")
    {
        _http = new HttpClient { BaseAddress = new Uri(endpoint), Timeout = TimeSpan.FromMinutes(2) };
        _model = model;
    }

    public bool IsAvailable => CheckHealth();

    public async Task<string?> CompletePromptAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var request = new { model = _model, prompt = $"{systemPrompt}\n\n{userPrompt}", stream = false };
        var response = await _http.PostAsJsonAsync("/api/generate", request, ct);
        // ...
    }
}
```

### DI registrace

```csharp
// Program.cs — volitelná AI
if (builder.Configuration.GetSection("Ai").Exists())
{
    builder.Services.Configure<AiOptions>(builder.Configuration.GetSection("Ai"));
    builder.Services.AddSingleton<IAiBackendAdapter, OllamaAdapter>();
    builder.Services.AddSingleton<IConstraintInferencer, AiConstraintInferencer>();
    builder.Services.AddSingleton<ITranslationService, AiTranslationService>();
}
```

---

## 2. PromptRegistry (D2)

### Koncept

Prompty jako **verzované soubory** s metadaty:

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
1. Konkrétní C# typ (string, decimal, Guid, ...)
2. Validační pravidla (max length, regex, range)
3. Výchozí hodnotu (pokud dává smysl)

Vstupní formát: JSON s atributem.
Výstupní formát: JSON podle schématu...
```

```csharp
public sealed class PromptRegistry
{
    private readonly Dictionary<string, PromptTemplate> _prompts = new();

    public void LoadFromDirectory(string path)
    {
        foreach (var file in Directory.GetFiles(path, "*.prompt.md"))
        {
            var template = PromptTemplate.Parse(File.ReadAllText(file));
            _prompts[template.Name] = template;
        }
    }

    public PromptTemplate Get(string name) => _prompts[name];
}

public sealed record PromptTemplate(
    string Name,
    int Version,
    string Model,
    double Temperature,
    string SystemPrompt,
    string UserPromptTemplate // s {{placeholders}}
);
```

---

## 3. PromptEvaluator (D3)

### Koncept

Automatické vyhodnocení kvality promptu na testovacích datech:

```csharp
public sealed class PromptEvaluator
{
    public async Task<PromptEvalResult> EvaluateAsync(
        PromptTemplate prompt,
        IReadOnlyList<PromptTestCase> testCases,
        IAiBackendAdapter backend)
    {
        var results = new List<TestCaseResult>();

        foreach (var testCase in testCases)
        {
            var output = await backend.CompletePromptAsync(
                prompt.SystemPrompt,
                prompt.UserPromptTemplate.Replace("{{input}}", testCase.Input));

            var passed = testCase.Validator(output);
            results.Add(new TestCaseResult(testCase.Name, passed, output));
        }

        return new PromptEvalResult(
            PromptName: prompt.Name,
            PassRate: (double)results.Count(r => r.Passed) / results.Count,
            Results: results
        );
    }
}

public sealed record PromptTestCase(
    string Name,
    string Input,
    Func<string?, bool> Validator  // např. output => output?.Contains("decimal") == true
);
```

---

## Odhad

| Fáze | Dny |
|------|-----|
| MetaForge.Ai projekt + csproj | 0,25 dne |
| IAiBackendAdapter | 0,25 dne |
| OllamaAdapter (HTTP) | 1 den |
| AiConstraintInferencer | 0,5 dne |
| AiTranslationService | 1 den |
| PromptRegistry + .prompt.md loader | 0,5 dne |
| PromptEvaluator | 0,5 dne |
| Prompt templates (min. 3) | 0,5 dne |
| DI integrace + konfigurace | 0,5 dne |
| Testy s mockovaným HttpClient | 1 den |
| **Celkem** | **6 dní** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-006 (AI kontrakty — IConstraintInferencer, ITranslationService) | ✅ Hotovo |
| PROP-020 (BusinessModel upgrade — CoreDetail pro enrichment) | 🟢 Schváleno |
| Ollama (localhost:11434) | Externí — musí běžet |
