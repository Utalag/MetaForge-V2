# PROP-019: Translator — IAiTranslator a AI-assisted překlad

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot

## Cíl

Přidat do Translator vrstvy rozhraní `IAiTranslator` a implementaci `AiTranslationService` pro AI-assisted enrichment business atributů. AI je volitelná — pokud není dostupná, systém fallbackuje na deterministický `DefaultBusinessTranslator`.

## Odůvodnění

Aktuální `DefaultBusinessTranslator` překládá atributy deterministicky (podle názvu typu → `TypeModel`). Pro pokročilejší případy je potřeba AI:
- Odvození constraintů z názvu atributu (např. "Email" → `email_format`, `max_length:254`)
- Návrh C# typu (např. "Money" → `decimal`)
- Enrichment s výchozími hodnotami a validačními pravidly
- Generování computed expression z přirozeného jazyka

## Obsah

### 1. IAiTranslator

```csharp
public interface IAiTranslator
{
    /// <summary>AI je dostupná a reaguje.</summary>
    bool IsAvailable { get; }

    /// <summary>Spustí prompt s daným systémovým a uživatelským promptem.</summary>
    Task<string?> CompletePromptAsync(string systemPrompt, string userPrompt);
}
```

Fallback: pokud `IsAvailable == false`, volající použije `DefaultBusinessTranslator`.

### 2. AiTranslationService

Implementace přes Ollama (localhost:11434) nebo jiný AI backend.

```csharp
public sealed class AiTranslationService : IAiTranslator
{
    // Podporuje Ollama, OpenAI-compatible API
    // Konfigurace přes AIInferenceSettings (endpoint, model, temperature)
}
```

### 3. Rozšíření DefaultBusinessTranslator

`DefaultBusinessTranslator.TryEnrich()` bude mít možnost volat `IAiTranslator` pro AI enrichment:

```csharp
public sealed class DefaultBusinessTranslator : IBusinessTranslator
{
    private readonly IAiTranslator? _aiTranslator;  // nullable — fallback

    public EnrichmentResult? TryEnrich(BusinessAttributeNode attribute)
    {
        // 1. Zkusí deterministická pravidla
        // 2. Pokud nic, zkusí AI (je-li dostupná)
        // 3. Vrátí null pokud ani jedno nic nedalo
    }
}
```

### 4. Prompt modely

| Model | Účel |
|-------|-------|
| `AuthoringTranslationModelPrompt` | Prompt pro překlad atributu → TypeModel + constraints |
| `SemanticBriefJson` | Strukturovaný JSON výstup pro AI odpověď |

## Výstup

| Soubor | Umístění |
|--------|----------|
| `IAiTranslator.cs` | `Src/MetaForge.Translator/Translation/` |
| `AiTranslationService.cs` | `Src/MetaForge.Translator/Translation/` |
| `AuthoringTranslationModelPrompt.cs` | `Src/MetaForge.Translator/Prompting/ModelPrompts/` |
| `SemanticBriefJson.cs` | `Src/MetaForge.Translator/Prompting/` |
| Rozšíření `DefaultBusinessTranslator` | AI enrichment branch |
| Testy | `Tests/MetaForge.Translator.Tests/Translation/` |

## Závislosti

| Komponenta | Stav |
|------------|------|
| `IBusinessTranslator`, `DefaultBusinessTranslator` | ✅ Hotovo |
| `IAiRuntimeAdapter`, `HttpAiRuntimeAdapter` | ❌ Potřebuje `MetaForge.Ai` projekt |
| `AIInferenceSettings` (konfigurace) | ❌ Potřebuje definovat |
| HttpClient (pro Ollama API) | ✅ Vestavěný |

## Rozhodnutí — AI runtime adapter

`IAiRuntimeAdapter` a `HttpAiRuntimeAdapter` **nepatří do Translator**. Jsou to čistě AI infrastruktura. Translator bude:

- **Varianta A (doporučeno):** Volat Ollama API přímo přes `HttpClient` v `AiTranslationService` — jednoduché, bez závislosti na AI projektu.
- **Varianta B:** Počkat na `MetaForge.Ai` projekt a použít jeho `IAiRuntimeAdapter`.

Volíme **variantu A** — `AiTranslationService` bude volat Ollama API přímo. Až vznikne `MetaForge.Ai`, můžeme překlopit.

## Odhad

| Fáze | Dny |
|------|-----|
| `IAiTranslator` rozhraní | 0,25 dne |
| `AiTranslationService` (Ollama HTTP) | 0,5 dne |
| Prompt modely | 0,5 dne |
| Rozšíření `DefaultBusinessTranslator` | 0,5 dne |
| Testy s mockovaným HttpClient | 0,5 dne |
| **Celkem** | **2,25 dne** |
