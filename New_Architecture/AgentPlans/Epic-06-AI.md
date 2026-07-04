# Epic 6 — AI Layer

> **Cíl:** Vytvořit volitelnou AI vrstvu s graceful fallback.
> **Výstup:** Projekt `MetaForge.Ai` s IAiBackendAdapter, OllamaAdapter a AI implementacemi.
> **Závislosti:** Epic 2 (Core), Epic 4 (Translator).

---

## DŮLEŽITÉ: AI je VOLITELNÁ

- Systém MUSÍ fungovat bez AI.
- AI pouze DOPLŇUJE deterministickou cestu — nikdy ji nenahrazuje.
- AI vrací null při jakékoliv chybě = fallback na deterministickou cestu.

---

## TASK-6.1.1 — Založení projektu MetaForge.Ai

**Vstup:** `MetaForge.slnx`, Epic 2 a 4 dokončeny.
**Výstup:** Class library projekt `Src/MetaForge.Ai/MetaForge.Ai.csproj`.
**Soubory:** `Src/MetaForge.Ai/MetaForge.Ai.csproj`, `MetaForge.slnx`

**Kód — `MetaForge.Ai.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Ai</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MetaForge.Core\MetaForge.Core.csproj" />
    <ProjectReference Include="..\MetaForge.Translator\MetaForge.Translator.csproj" />
  </ItemGroup>
</Project>
```

**Ověření:** `dotnet build Src/MetaForge.Ai/` projde.
**Riziko:** Nízké.
**Rollback:** Odeber projekt ze slnx, smaž složku.

---

## TASK-6.2.1 — IAiBackendAdapter rozhraní

**Vstup:** TASK-6.1.1 (projekt existuje).
**Výstup:** Soubor `Src/MetaForge.Ai/Abstractions/IAiBackendAdapter.cs`.
**Soubory:** `Src/MetaForge.Ai/Abstractions/IAiBackendAdapter.cs`

**Kód:**

```csharp
namespace MetaForge.Ai.Abstractions;

/// <summary>
/// Technická transportní abstrakce pro AI backend (Ollama, OpenAI, Azure).
/// Definice I IMPLEMENTACE jsou v MetaForge.Ai.
/// </summary>
public interface IAiBackendAdapter
{
    /// <summary>Název providera (pro logování).</summary>
    string ProviderName { get; }

    /// <summary>Je backend dostupný?</summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Pošle prompt a vrátí odpověď jako text.
    /// Vrací null při jakékoliv chybě (graceful fallback).
    /// </summary>
    Task<string?> SendAsync(string prompt, CancellationToken ct = default);

    /// <summary>
    /// Pošle prompt a vrátí odpověď jako naparsovaný JSON.
    /// Vrací null při jakékoliv chybě.
    /// </summary>
    Task<T?> SendJsonAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}
```

**Ověření:** `dotnet build` projde.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-6.2.2 — OllamaAdapter implementace

**Vstup:** TASK-6.2.1 (IAiBackendAdapter).
**Výstup:** Soubor `Src/MetaForge.Ai/Adapters/OllamaAdapter.cs`.
**Soubory:** `Src/MetaForge.Ai/Adapters/OllamaAdapter.cs`

**Kód:**

```csharp
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MetaForge.Ai.Abstractions;

namespace MetaForge.Ai.Adapters;

/// <summary>
/// Adapter pro Ollama — lokální AI server (např. Gemma 4 12B).
/// Endpoint: http://localhost:11434
/// </summary>
public sealed class OllamaAdapter : IAiBackendAdapter
{
    private readonly HttpClient _http;
    private readonly string _model;

    public string ProviderName => "Ollama";

    /// <param name="baseUrl">URL Ollama serveru (výchozí http://localhost:11434).</param>
    /// <param name="model">Název modelu (výchozí gemma3:12b).</param>
    public OllamaAdapter(string baseUrl = "http://localhost:11434", string model = "gemma3:12b")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _model = model;
    }

    /// <summary>Ověří, zda je Ollama server dostupný.</summary>
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync("/api/tags", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Pošle prompt do Ollama /api/generate a vrátí odpověď.</summary>
    public async Task<string?> SendAsync(string prompt, CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                model = _model,
                prompt = prompt,
                stream = false,
                options = new { temperature = 0.1 }
            };

            var response = await _http.PostAsJsonAsync("/api/generate", request, ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(ct);
            return json?.Response?.Trim();
        }
        catch
        {
            return null; // Graceful fallback — nikdy nevyhazovat výjimku
        }
    }

    /// <summary>Pošle prompt, parsuje odpověď jako JSON.</summary>
    public async Task<T?> SendJsonAsync<T>(string prompt, CancellationToken ct = default) where T : class
    {
        var text = await SendAsync(prompt, ct);
        if (string.IsNullOrWhiteSpace(text)) return null;

        try
        {
            // Extrahuj JSON z odpovědi (může být obalený markdown ```json ... ```)
            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = text[jsonStart..(jsonEnd + 1)];
                return JsonSerializer.Deserialize<T>(json);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // Response model pro Ollama /api/generate
    private class OllamaGenerateResponse
    {
        public string? Response { get; set; }
    }
}
```

**Ověření:** `dotnet build` projde. Adapter se dá vytvořit: `new OllamaAdapter()`.
**Riziko:** Nízké — čistý HTTP klient.
**Rollback:** Smaž soubor.

---

## TASK-6.3.1 — AiConstraintInferencer (AI implementace IConstraintInferencer)

**Vstup:** TASK-6.2.2 (OllamaAdapter), IConstraintInferencer (z Core).
**Výstup:** Soubor `Src/MetaForge.Ai/Inference/AiConstraintInferencer.cs`.
**Soubory:** `Src/MetaForge.Ai/Inference/AiConstraintInferencer.cs`

**Kód:**

```csharp
using MetaForge.Ai.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Inference;

namespace MetaForge.Ai.Inference;

/// <summary>
/// AI implementace IConstraintInferencer — používá AI backend pro odvození constraintů.
/// Při selhání vrací prázdný seznam (graceful fallback).
/// </summary>
public sealed class AiConstraintInferencer : IConstraintInferencer
{
    private readonly IAiBackendAdapter _backend;

    public AiConstraintInferencer(IAiBackendAdapter backend)
    {
        _backend = backend;
    }

    public IReadOnlyList<string> Infer(string attributeName, TypeModel type)
    {
        // Postav prompt
        var prompt = $"""
            Jsi expert na datové modelování. Pro atribut '{attributeName}' typu '{type.BaseType}' odvoď validační pravidla.
            
            Vrať POUZE JSON pole stringů, např.: ["not_empty", "max_length:200"].
            
            Pravidla:
            - not_empty: atribut nesmí být prázdný
            - email_format: musí být validní email
            - phone_format: musí být validní telefon
            - url_format: musí být validní URL
            - min_length:N: minimální délka N
            - max_length:N: maximální délka N
            - range:MIN-MAX: číselný rozsah
            - not_negative: nesmí být záporné
            """;

        // Synchronní volání (pro jednoduchost)
        try
        {
            var result = _backend.SendJsonAsync<List<string>>(prompt).GetAwaiter().GetResult();
            return result?.AsReadOnly() ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>(); // Graceful fallback
        }
    }
}
```

**Ověření:** `dotnet build` projde.
**Riziko:** Střední — AI inference je nedeterministická, fallback je prázdný seznam.
**Rollback:** Smaž soubor.

---

## TASK-6.4.1 — AiTranslationService (AI implementace ITranslationService)

**Vstup:** TASK-6.2.2 (OllamaAdapter), IBusinessTranslator (z Translatoru).
**Výstup:** Soubor `Src/MetaForge.Ai/Translation/AiTranslationService.cs`.
**Soubory:** `Src/MetaForge.Ai/Translation/AiTranslationService.cs`

**Kód:**

```csharp
using MetaForge.Ai.Abstractions;
using MetaForge.BusinessModel.Models;
using MetaForge.Translator.Host;
using MetaForge.Translator.Translation;

namespace MetaForge.Ai.Translation;

/// <summary>
/// AI implementace enrichmentu — používá AI pro hlubší analýzu atributů.
/// Při selhání vrací null (graceful fallback na deterministický DefaultBusinessTranslator).
/// </summary>
public sealed class AiTranslationService : ITranslationService
{
    private readonly IAiBackendAdapter _backend;

    public AiTranslationService(IAiBackendAdapter backend)
    {
        _backend = backend;
    }

    /// <summary>
    /// Pokusí se o AI enrichment atributu s kontextem projekce.
    /// Vrací null pokud AI selže nebo není k dispozici.
    /// </summary>
    public async Task<EnrichmentResult?> EnrichAsync(
        BusinessAttributeNode attribute,
        ProjectionView context,
        CancellationToken ct = default)
    {
        try
        {
            var available = await _backend.IsAvailableAsync(ct);
            if (!available) return null;

            // Postav prompt s kontextem
            var entityContext = context.Entities
                .FirstOrDefault(e => e.Attributes.Any(a => a.Id == attribute.Id));

            var prompt = $"""
                Analyzuj atribut '{attribute.Name}' typu '{attribute.Type}' v kontextu entity.
                
                Kontext:
                - Entita obsahuje atributy: {(entityContext is not null ? string.Join(", ", entityContext.Attributes.Select(a => a.Name)) : "neznámý kontext")}
                
                Vrať POUZE JSON:
                {{
                    "suggested_csharp_type": "string",
                    "validation_rules": ["not_empty"],
                    "max_length": 200,
                    "default_value": null
                }}
                """;

            var result = await _backend.SendJsonAsync<AiEnrichmentResponse>(prompt, ct);
            if (result is null) return null;

            return new EnrichmentResult(
                AttributeId: attribute.Id,
                SuggestedCSharpType: result.SuggestedCSharpType,
                ValidationRules: result.ValidationRules,
                MaxLength: result.MaxLength,
                DefaultValue: result.DefaultValue
            );
        }
        catch
        {
            return null; // Graceful fallback
        }
    }

    private class AiEnrichmentResponse
    {
        public string? SuggestedCSharpType { get; set; }
        public List<string>? ValidationRules { get; set; }
        public int? MaxLength { get; set; }
        public string? DefaultValue { get; set; }
    }
}
```

**Ověření:** `dotnet build` projde.
**Riziko:** Střední — AI enrichment je best-effort, fallback je null.
**Rollback:** Smaž soubor.

---

## TASK-6.5.1 — DI rozšiřující metoda pro AI registraci

**Vstup:** Všechny předchozí tasky.
**Výstup:** Soubor `Src/MetaForge.Ai/AiServiceRegistration.cs`.
**Soubory:** `Src/MetaForge.Ai/AiServiceRegistration.cs`

**Kód:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using MetaForge.Ai.Abstractions;
using MetaForge.Ai.Adapters;
using MetaForge.Ai.Inference;
using MetaForge.Ai.Translation;
using MetaForge.Core.Inference;
using MetaForge.Translator.Translation;

namespace MetaForge.Ai;

/// <summary>
/// Extension metody pro DI registraci AI služeb.
/// </summary>
public static class AiServiceRegistration
{
    /// <summary>
    /// Zaregistruje AI služby do DI containeru.
    /// Volitelné — pokud se nezavolá, použijí se deterministické fallbacky.
    /// </summary>
    public static IServiceCollection AddMetaForgeAi(
        this IServiceCollection services,
        string ollamaUrl = "http://localhost:11434",
        string model = "gemma3:12b")
    {
        // Transportní adapter
        services.AddSingleton<IAiBackendAdapter>(_ =>
            new OllamaAdapter(ollamaUrl, model));

        // AI implementace — nahradí deterministické fallbacky
        services.AddSingleton<IConstraintInferencer, AiConstraintInferencer>();
        services.AddSingleton<ITranslationService, AiTranslationService>();

        return services;
    }
}
```

**Ověření:** `dotnet build` projde. `services.AddMetaForgeAi()` lze zavolat v CLI/MCP Program.cs.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## Souhrn Epic 6 — Co musí existovat po dokončení

```
Src/MetaForge.Ai/
├── MetaForge.Ai.csproj
├── Abstractions/
│   └── IAiBackendAdapter.cs
├── Adapters/
│   └── OllamaAdapter.cs
├── Inference/
│   └── AiConstraintInferencer.cs
├── Translation/
│   └── AiTranslationService.cs
└── AiServiceRegistration.cs
```

**Celkem souborů:** ~6
**Build:** `dotnet build Src/MetaForge.Ai/` projde bez chyb.

**Checkpoint:** `git tag checkpoint/epic-6-done`
