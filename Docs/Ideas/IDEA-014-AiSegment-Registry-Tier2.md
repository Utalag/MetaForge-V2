# IDEA-014 AiSegment Registry a Tier 2 Rozšíření

Stav: Idea
Oblast: AI, Core
Zdroj: For_Inspiration/Architecture-Define/06-AI-Tiers-and-Providers.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept AI architektury definoval dvouúrovňový model (Tier 1 = user-facing chat, Tier 2 = interní specializované modely) s `AiSegment` enumem a per-segment konfigurací. Současná implementace (PROP-027) má pouze `IAiBackendAdapter` a `OllamaAdapter` — chybí segment registr a specializované segmenty.

Nápad vychází z `06-AI-Tiers-and-Providers.md`, kde je popsáno:
- `AiSegment` enum s 8+ segmenty: `MainChat`, `ConstraintInference`, `BodyGeneration`, `Healing`, `Prehealing`, `StyleImport`, `Conversation`, `NodeAssist`
- Každý segment má vlastní `AIInferenceSettings` (provider, model, teplotu, max tokeny)
- Konfigurace per-segment v `appsettings.json`
- Provider abstrakce přes `IAiRuntimeAdapterFactory`

## 2. Problém dnes

- PROP-027 implementoval `IAiBackendAdapter` a `OllamaAdapter`, ale chybí:
  - `AiSegment` enum — každý use-case má stejný backend, nelze konfigurovat per-segment
  - `Healing` segment — AI pro opravu kódu (viz IDEA-012)
  - `Prehealing` segment — validace před exportem
  - `StyleImport` segment — extrakce kódovacího stylu z cizího kódu
  - `Conversation` segment — strukturovaná odpověď do chatu (oddělená od MainChat)
- `IAiRuntimeAdapterFactory` neexistuje — nelze per-segment rozhodnout, zda použít Ollama, OpenAI nebo Azure
- PromptRegistry (PROP-027) je dobrý základ, ale segmenty nemají vlastní prompt templaty

## 3. Předběžný směr řešení

Rozšíření MetaForge.Ai:

- `AiSegment` enum: MainChat, AuthoringTranslation, ConstraintInference, BodyGeneration, Healing, Prehealing, StyleImport, Conversation, NodeAssist
- `IAiRuntimeAdapterFactory` — factory rozhraní pro vytvoření adapteru per segment
- Per-segment `AIInferenceSettings` v `AiPlatformConfiguration`
- `AiSegmentSettings` record: `Enabled`, `AIInferenceSettings`, `PromptTemplateName`
- Nové segmenty jako samostatné třídy implementující Core/Translator interface
- Prompt templaty per segment v `PromptRegistry`

Dotčené vrstvy: AI (implementace), Core (interface pro nové segmenty), Translator (napojení na projekci).

## 4. Signál hodnoty

- Jemnozrnná konfigurace AI — levný model pro jednoduché úkoly, drahý pro komplexní.
- Enterprise on-premise: všechny Tier 2 segmenty mohou běžet lokálně (Ollama), Tier 1 v cloudu.
- Healing a StyleImport zvyšují kvalitu výstupu bez zásahu uživatele.
- Per-segment prompt templaty = lepší kontrola nad AI výstupem.

## 5. Rizika a nejasnosti

- Počet segmentů může růst → jak zajistit, aby `AiSegment` nebyl dumping ground pro každý nový use-case?
- Per-segment konfigurace přidává komplexitu do deploymentu.
- OQ-xxx: Jaký je vztah mezi `AiSegment` a `PromptTemplate`? Může jeden segment mít více promptů?

## 6. Doporučený další krok

Candidate Proposal — rozšíření PROP-027. Měl by být plánován jako druhá fáze AI vrstvy, po stabilizaci MetaForge.Ai.

Vazby: PROP-027, IDEA-012 (Self-Healing — Healing segment), PROP-019 (IAiTranslator)
