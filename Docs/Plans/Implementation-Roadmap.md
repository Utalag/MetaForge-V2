# Implementation Roadmap — MetaForge Platform

> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Účel:** Pořadí implementace PROP-020 až PROP-030 dle architektonických závislostí a business value.

---

## 🗺️ Dependencies Graph

```
                      PROP-020 ──── BusinessModel upgrade (JINÝ AGENT)
                     /    |    \
                    ▼     ▼     ▼
      ┌─────────┐  ┌──────┐  ┌─────────┐
      │PROP-028 ◄──┤CORE  │  │PROP-027 │
      │Infra    │  │024   │  │AI Layer │
      │persist  │  │Strong│  │Ollama   │
      │config   │  │Type  │  │Prompts  │
      └────┬────┘  │Expr  │  └────┬────┘
           │       └──────┘       │
           │                      │
           ▼                      ▼
      ┌───────────────────────────────────┐
      │         PROP-025                   │
      │  GENERATORS + MONETIZATION        │
      │  Incremental, scaffolding, tiers   │
      │  Sandbox, license middleware        │
      └──────────────┬────────────────────┘
                     │
           ┌─────────┴──────────┐
           ▼                    ▼
     ┌────────────┐      ┌──────────┐
     │PROP-026    │      │PROP-019  │
     │Host        │      │AI Trans- │
     │CLI/MCP/Web │      │lator     │
     │REPL        │      │Enrichm.  │
     └────────────┘      └──────────┘
           │
           ▼
     ┌──────────────────────────────────┐
     │         PROP-029                  │
     │  ForgeBlocks — EF Core, AutoMapper│
     │  marketplace, NuGet distribuce    │
     └──────────────┬───────────────────┘
                    │
     ┌──────────────┼──────────────┐
     ▼              ▼              ▼
┌────────┐   ┌──────────┐   ┌──────────┐
│PROP-030│   │PROP-021  │   │PROP-022  │
│Security│   │Tests     │   │Observab. │
│Migrat. │   │FsCheck   │   │Tracing   │
│Valid.  │   │Verify    │   │Diff      │
│Health  │   │          │   │          │
└────────┘   └──────────┘   └──────────┘

      PROP-023 ──── Future / na zvážení (po všem)
```

---

## 📋 Implementační sekvence

### FÁZE 0 — AKTUÁLNĚ BĚŽÍ

| Pořadí | Návrh | Vrstva | Odhad | Zdůvodnění |
|--------|-------|--------|-------|------------|
| **0** | **PROP-020** | BusinessModel | 10 dní | **Právě implementuje jiný agent.** Základ všeho — CoreDetail, SyncState, CommandProvenance, immutabilita |

---

### FÁZE 1 — FOUNDATION (paralelní / ihned po PROP-020)

Tři nezávislé streamy, mohou běžet paralelně:

| Pořadí | Návrh | Vrstva | Odhad | Proč právě teď |
|--------|-------|--------|-------|----------------|
| **1a** | **PROP-024** | Core | 6 dní | **Nezávislý na PROP-020.** Core potřebuje StrongType pro ForgeBlock presety a Expression pro computed properties. Source Generator integrace je klíčová pro monetizační model (free tier). |
| **1b** | **PROP-027** | AI Layer | 6 dní | **Částečně nezávislý.** OllamaAdapter nepotřebuje PROP-020. PromptRegistry může existovat samostatně. AiConstraintInferencer a AiTranslationService až po PROP-019. |
| **1c** | **PROP-028** | Infrastructure | 5,25 dne | **Závisí na PROP-020** (upgraded CommandEnvelope). Potřebujeme JSONL persistence, aby se commandy ukládaly na disk. Bez toho je platforma "zapomnětlivá". Konfigurace přes IOptions je nutná pro všechny ostatní vrstvy. |

**Parallelizační poznámka:** Všechny tři streamy jsou na sobě nezávislé. Můžou běžet současně s PROP-020 nebo hned po něm.

---

### FÁZE 2 — CORE FLOW (po PROP-020 + INFRA)

| Pořadí | Návrh | Vrstva | Odhad | Závislosti |
|--------|-------|--------|-------|------------|
| **2a** | **PROP-019** | Translator | 2,25 dne | PROP-020 (CoreDetail), PROP-027 (Ollama) |
| **2b** | **PROP-025** | Generators | 8 dní | PROP-020 (dokument), PROP-024 (Expression), PROP-028 (config), PROP-026 (sandbox API) |

**Zdůvodnění:**
- **PROP-019** (AI Translator enrichment) zapojuje AI-2 do flow — CoreDetail vzniká AI enrichmentem. To je klíčový krok před generováním kódu.
- **PROP-025** je nejdelší (8 dní) a nejkomplexnější — implementuje celý monetizační model. Potřebuje, aby vše ostatní stálo.

---

### FÁZE 3 — HOST + EKOSYSTÉM (paralelní)

| Pořadí | Návrh | Vrstva | Odhad | Závislosti |
|--------|-------|--------|-------|------------|
| **3a** | **PROP-026** | Host | 7,5 dne | PROP-020 (Facade API), PROP-028 (config) |
| **3b** | **PROP-029** | ForgeBlocks | 8 dní | PROP-024 (StrongType), PROP-028 (config) |

**Zdůvodnění:**
- **PROP-026** (CLI upgrade, WebApi, REPL) dělá platformu použitelnou. System.CommandLine + Spectre.Console je viditelný výstup pro uživatele.
- **PROP-029** (EF Core, AutoMapper, FluentValidation) jsou ukázkové ForgeBlocky — demonstrují sílu platformy. Každý je cca 2-3 dny, takže půjdou iterativně.

---

### FÁZE 4 — KVALITA A STABILITA

| Pořadí | Návrh | Vrstva | Odhad | Závislosti |
|--------|-------|--------|-------|------------|
| **4a** | **PROP-030** | Průřezové | 4 dny | PROP-028 (migrace persistence), PROP-026 (sandbox guard) |
| **4b** | **PROP-021** | Tests | 1,75 dne | PROP-020 (upgraded modely) |
| **4c** | **PROP-022** | Infrastr. | 2,5 dne | PROP-020 (modely pro diff), PROP-028 (persistence pro tracing) |

**Zdůvodnění:**
- **PROP-030** je průřezový — schema migration chrání před corrupt data, sandbox guard chrání free tier.
- **PROP-021** (FsCheck + Verify) dává jistotu, že celý event sourcing je korektní.
- **PROP-022** (OpenTelemetry + diff) je monitoring — nasadit až když platforma stojí.

---

### FÁZE ∞ — BUDOUCNOST

| Pořadí | Návrh | Vrstva | Odhad | Zdůvodnění |
|--------|-------|--------|-------|------------|
| **—** | **PROP-023** | Průřezové | 5-9 dní | Všechna vylepšení jsou "na zvážení" — typový SyncState, Layer stack, YAML DSL, Undo/redo. Neimplementovat dokud nebude reálný use case. |

---

## 📊 Časová osa

```
TÝDEN 1-2     PROP-020 (BusinessModel upgrade) ← agent implementuje
              ──────────────────────────────────────────────
TÝDEN 2-3     PROP-024 (Core)     PROP-027 (AI)    PROP-028 (Infra)
              ──────────────────────────────────────────────
TÝDEN 3-4     PROP-019 (AI Translator)
              PROP-025 (Generators + Monetization)
              ──────────────────────────────────────────────
TÝDEN 4-5     PROP-026 (Host Surfaces)
              PROP-029 (ForgeBlocks)
              ──────────────────────────────────────────────
TÝDEN 5-6     PROP-030 (Security)  PROP-021 (Tests)  PROP-022 (Observ.)
              ──────────────────────────────────────────────
TÝDEN 6+      PROP-023 (Future — pouze pokud rozhodnuto)
```

---

## 🎯 Klíčová rozhodnutí

### Milník 1: "Už to něco dělá" (konec Fáze 1)
- PROP-020 + PROP-028 → CommandLog se ukládá na disk, dokument přežije restart
- PROP-024 → StrongType presety fungují (Money, Email, ...)
- PROP-027 → Ollama je připojená, AI může odpovídat

### Milník 2: "Generujeme kód" (konec Fáze 2)
- PROP-025 → TieredCodeGenerator generuje C# kód podle licence
- PROP-019 → AI enrichment vyplňuje CoreDetail

### Milník 3: "Produkční platforma" (konec Fáze 3)
- PROP-026 → CLI s bohatým výstupem, WebApi s license middleware
- PROP-029 → EF Core generování pro paying custromers

### Milník 4: "Enterprise ready" (konec Fáze 4)
- PROP-030 → Schema migrace, health checks
- PROP-021 → Property-based ověření invariantů
- PROP-022 → Monitoring a diff

---

## 📝 Co implementovat jako první?

**Doporučení:** Po dokončení PROP-020 spustit paralelně **PROP-028 (Infrastructure)** a **PROP-024 (Core)** — oba jsou krátké a odemknou závislosti pro vše ostatní. PROP-027 (AI) může počkat o týden déle, protože jeho výstup (AiTranslationService) stejně potřebuje PROP-019.

```
1. PROP-028 Infra     → persistence + config   5 dní
2. PROP-024 Core      → StrongType + Expression 6 dní  ← paralelně s 1
3. PROP-027 AI Layer  → Ollama + Prompty       6 dní  ← paralelně s 1
4. PROP-025 Generators→ MONETIZACE             8 dní  ← NEJDŮLEŽITĚJŠÍ
5. PROP-026 Host      → CLI/MCP/WebApi/REPL   7,5 dne ← poté
6. PROP-029 ForgeBlocks→ EF Core + AutoMapper  8 dní  ← poté
7. PROP-019 AI Transl.→ enrichment            2 dny   ← poté
8. PROP-030 Security  → migrace, health       4 dny   ← před release
9. PROP-021/022       → kvalita               4 dny   ← průběžně
```

---

## Legenda

| Ikona | Význam |
|-------|--------|
| 🔴 Fáze 0 | Právě běží |
| 🟡 Fáze 1 | Foundation — paralelní streamy |
| 🟢 Fáze 2 | Core flow — generování kódu |
| 🔵 Fáze 3 | Host + ekosystém |
| ⚪ Fáze 4 | Kvalita a stabilita |
