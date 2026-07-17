# 2026-07-17 Analýza implementace proti auditům

> Ověření, že dnešní změny (PROP-060, PROP-055, PROP-054) adresují zjištění z Overkill Auditu a Stavové Analýzy.

---

## 1. Overkill Audit (2026-07-16) — verifikace

| Zjištění | Riziko | Dnešní implementace | Verdikt |
|----------|--------|---------------------|---------|
| **PROP-039**: 8 typů, 1 přežil — framework bez consumera | Framework před konzumentem | PROP-060: používá 1 interface (`IElementIdResolver`), 1 record (`SyncState`), 1 class (`ElementIdMapping`) — vše rovnou zapojeno do pipeline. | ✅ **Čistý** |
| **PROP-036**: shelfware — nikdo nevolá `Evaluate()` | Typy bez volajícího kódu | PROP-055: `ReferenceGraph.Build()` ihned volá `diagnostics.Report()`. Každý typ má konzumenta v `Build()`. | ✅ **Čistý** |
| **PROP-022**: ActivitySource stuby bez StartActivity | Infrastruktura bez konzumenta | PROP-054: `DiRegistrationAttribute` + `ApplyToDi()` — 1 atribut, 1 metoda. Žádná abstrakce bez volání. | ✅ **Čistý** |
| **PROP-046**: test harness bez benchmarku | Plán 4 fáze, dodána 1 | PROP-055: dodáno 5 typů, všechny rovnou v `Build()` factory. Žádné "Fáze 2" závislosti. | ✅ **Čistý** |

### Vzorec overkill vs worth it — znovu potvrzen

| 🔴 Overkill pattern (minulost) | 🟢 Worth it pattern (dnes) |
|-------------------------------|---------------------------|
| Framework bez konzumenta (PROP-039) | Konkrétní typ s okamžitým konzumentem (ReferenceGraph → DiagnosticBag) |
| Abstrakce před potřebou (PROP-022) | Jednoduchý atribut + 1 metoda (DiRegistrationAttribute → ApplyToDi) |
| Více fází bez Fáze 1 (PROP-046) | Jeden kompletní celek (ElementIdMapping → Map/Resolve/IsConsistent) |

---

## 2. Stavová Analýza (2026-07-12) — verifikace

| ID | Problém | Stav dnes |
|----|---------|-----------|
| B25 | Chybí generování DI registrací — `Program.cs` je ruční. PROP-054 naplánován. | ✅ **Hotovo** — `DiRegistrationAttribute` + `ForgeBlockRegistry.ApplyToDi()` |
| B10 | MapType TODO (3 místa) v `CodeGenerator.cs` | ❌ Stále otevřeno — není v rozsahu |
| B11 | Operator generování stub v `CodeGenerator.cs` | ❌ Stále otevřeno — není v rozsahu |
| B16 | Docker konfigurace chybí | ❌ Stále otevřeno — není v rozsahu |
| B21 | Chybí health checks | ❌ Stále otevřeno — není v rozsahu |
| B22 | Chybí setup/instalační skript | ❌ Stále otevřeno — není v rozsahu |

### Nově přidané capability (mimo Stavovou Analýzu)

| Co | Kde | Hodnota |
|----|-----|---------|
| `IMemberElement.Id` (Guid) | Core/Abstractions | Stabilní identita pro všechny member elementy — prerekvizita pro kontrakty, projekci, graf |
| `BusinessParameterNode.Id` | BusinessModel | Konzistence — všechny BusinessModel typy mají Id |
| `ElementIdMapping` | Translator | Business→Core traceabilita. `Resolve()` pro PROP-055/056 |
| `IElementIdResolver` | Core/Abstractions | Core nezávisí na Translatoru |
| `SyncState` record | BusinessModel | Typovaný state machine místo enum. Exhaustive switch. `Conflict` nese kontext |
| `ReferenceGraph` (5 souborů) | Core/ReferenceGraph | Detekce cyklů, unresolved, topologické řazení, vrstvy |
| `DiRegistrationAttribute` | Core/ForgeBlockPackages | Deklarativní DI registrace pro ForgeBlocky |
| `ForgeBlockRegistry.ApplyToDi()` | Core | Reflection-based registrace služeb |

---

## 3. Shrnutí

| Metrika | Před změnami | Po změnách |
|---------|-------------|------------|
| Aktivní PROPy | 8 | **5** (057, 058, 056, 053, 023) |
| Hotovost kód | 33 PROPů | **36 PROPů** |
| Build | 0 chyb | 0 chyb ✅ |
| Testy | ~603 | **603/603** ✅ |
| Přidané soubory | — | **10 nových** (5 ReferenceGraph + ElementIdMapping + SyncState + IElementIdResolver + DiRegistrationAttribute + ForgeBlockRegistry změna) |
| Závislost PROP-060 → PROP-055/056 | Neevidovaná | 🔴 HARD — zdokumentováno |
