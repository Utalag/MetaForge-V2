# AI Layer

> Kontrakty v Core/Translator, implementace v MetaForge.Ai

---

## Princip

- **Kontrakty** jsou definovány v projektech, které je potřebují (Core, Translator).
- **Implementace** jsou v `MetaForge.Ai`.
- Technická transportní abstrakce `IAiBackendAdapter` je definována i implementována v MetaForge.Ai.

## Implementace (MetaForge.Ai)

→ `MetaForge.Ai/Inference/`
- `AiConstraintInferencer : IConstraintInferencer` — AI implementace kontraktu z Core

→ `MetaForge.Ai/Translation/`
- `AiTranslationService : ITranslationService` — AI implementace kontraktu z Translatoru

→ `MetaForge.Ai/Abstractions/`
- `IAiBackendAdapter` — adapter pro Ollama, OpenAI, Azure (definice i implementace zde)

## DI registrace

```csharp
// Volitelné — jen když je AI nakonfigurováno
builder.Services.AddSingleton<IConstraintInferencer, AiConstraintInferencer>();
builder.Services.AddSingleton<ITranslationService, AiTranslationService>();
builder.Services.AddSingleton<IAiBackendAdapter>(sp => new OllamaAdapter("http://localhost:11434"));
```
