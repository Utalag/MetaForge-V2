# ISS-015 Dvojí SyncState — mrtvý kód discriminated union

Datum: 2026-07-08
PROP: PROP-044
Soubor: `Src/MetaForge.BusinessModel/Models/SyncState.cs`
Závažnost: 🟡 Střední
Stav: Resolved (2026-07-10)
Owner:
Poslední revize: 2026-07-08

## 1. Kontext

Issue zjištěno při Perplexity Deep Research konverzace e7299554 (Translator & BusinessModel revize).

## 2. Popis problému

V BusinessModel existují duplicitní reprezentace SyncState:

| Soubor | Typ | Použití |
|--------|-----|---------|
| `Models/AttributeSyncState.cs` | Enum (`New`, `Synced`, `BusinessEdited`, `CoreEdited`, `Conflict`) | ✅ Aktivně používán v `BusinessAttributeCoreDetail`, `ReplayEngine`, `WriteBackService`, `ProjectionReadService` |
| `Models/SyncState.cs` | Discriminated union (abstract record) | ❌ Nikde nepoužit, existuje izolovaně |

`SyncState.cs` deklaruje že "nahrazuje původní AttributeSyncState enum", ale ve skutečnosti ho nic nepoužívá. Je to mrtvý kód — 70+ řádků včetně JSON converteru, state transition functions a unit testů.

## 3. Dopad

- Zmatení vývojářů — který SyncState použít?
- Zbytečný kód (70+ řádků) v codebase
- Riziko, že někdo začne používat nový SyncState a vznikne nekonzistence

## 4. Doporučené řešení

Dvě možnosti:

**Varianta A (doporučeno): Odstranit SyncState.cs**
- Důvod: Enum je jednodušší, JSON serializace bez custom converteru, celý codebase ho používá
- Smazat `SyncState.cs`, `SyncStateJsonConverter.cs`
- Smazat související testy
- Dokumentovat v `AttributeSyncState.cs` že je to jediná reprezentace

**Varianta B: Migrovat na discriminated union**
- Vyměnit `AttributeSyncState` property na `BusinessAttributeCoreDetail` za `SyncState`
- Přepsat `ReplayEngine.ApplyUpdateSyncState` 
- Přepsat `WriteBackService`
- Přepsat `ProjectionReadService`
- Risk: JSON serializace je složitější, `$type` discriminator

## 5. Otevřené otázky

- Kterou variantu zvolit?

## 6. Rozhodnutí

(čeká na user/owner)
- 2026-07-10 (PROP-044): `SyncState.cs` discriminated union odstraněn — soubor neexistuje. Pouze `AttributeSyncState` enum (aktivně používán) a `WorkflowBindingSyncState` enum zůstávají.
- 2026-07-12: Potvrzeno — `SyncState.cs` není v codebase. Varianta A (odstranění) již provedena.

---

## Související

- Vazby: PROP-020, PROP-044
- Blokuje: —
