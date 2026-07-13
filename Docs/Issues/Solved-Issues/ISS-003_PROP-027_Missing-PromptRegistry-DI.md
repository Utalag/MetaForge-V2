# ISS-003 AddMetaForgeAi neregistruje PromptRegistry

Datum: 2026-04-07
PROP: PROP-027
Soubor: `Src/MetaForge.Ai/AiServiceRegistration.cs`
Závažnost: ⚠️ Nízká
Stav: Resolved (2026-07-08)
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-027 (AI Layer — MetaForge.Ai projekt, Ollama, PromptRegistry).

## 2. Popis problému

`AddMetaForgeAi()` neregistruje `PromptRegistry` ani `PromptEvaluationService` — nově přidané služby v rámci PROP-027 nejsou součástí DI registrace.

## 3. Dopad

- `PromptRegistry` a `PromptEvaluationService` nelze použít přes DI — musí být vytvářeny ručně.
- Zvyšuje riziko chyb při použití těchto služeb v jiných vrstvách.

## 4. Doporučené řešení

Přidat do `AddMetaForgeAi()`:

```csharp
services.AddSingleton<PromptRegistry>();
services.AddSingleton<PromptEvaluationService>();
```

## 5. Otevřené otázky

- Žádné.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*
- 2026-07-08: `AddMetaForgeAi()` nyní registruje `PromptRegistry` a `PromptEvaluationService` jako Singletony.

---

## Související

- Vazby: `PROP-027`
