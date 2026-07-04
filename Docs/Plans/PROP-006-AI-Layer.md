# PROP-006: AI Layer (volitelná)

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-04
> **Autor:** Copilot (C# Implementer)

## Cíl

Vytvořit volitelnou AI vrstvu s graceful fallback — systém funguje bez AI.

## Výstup

- `Src/MetaForge.Ai/MetaForge.Ai.csproj` — reference na Core + Translator
- `Abstractions/IAiBackendAdapter.cs` — transportní abstrakce pro AI backend
- `Adapters/OllamaAdapter.cs` — adapter pro lokální Ollama server
- `Inference/AiConstraintInferencer.cs` — AI implementace IConstraintInferencer
- `Translation/AiTranslationService.cs` — AI enrichment atributů
- `AiServiceRegistration.cs` — DI extension metoda `AddMetaForgeAi()`

## Principy

- AI je VOLITELNÁ — systém musí fungovat bez AI
- AI pouze DOPLŇUJE deterministickou cestu
- AI vrací null při jakékoliv chybě = fallback na deterministickou cestu
- Všechna catch bloky vrací null/empty — nikdy nevyhazují výjimku

## Zpětná vazba / Poznámky

Po code review opraven sync-over-async deadlock v AiConstraintInferencer (Task.Run wrapper). Ollama URL/model jsou hardcodované — konfigurace přes DI je plánována.
