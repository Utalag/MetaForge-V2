# ISS-002 AppendAsync používá synchronní I/O

Datum: 2026-04-07
PROP: PROP-028
Soubor: `Src/MetaForge.Infrastructure/JsonCommandLogRepository.cs`
Závažnost: ⚠️ Nízká
Stav: Resolved (2026-07-08)
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-028 (Infrastructure — Persistence).

## 2. Popis problému

`AppendAsync` v `JsonCommandLogRepository.cs` používá synchronní `File.AppendAllText` uvnitř `lock` a vrací `Task.CompletedTask`. Není to pravá async operace — při velkém objemu dat může blokovat vlákno.

## 3. Dopad

- Při velkém objemu záznamů (stovky tisíc commandů) může synchronní I/O blokovat thread pool.
- V reasonable scénářích (desítky commandů) je dopad zanedbatelný.
- Async signature slibuje asynchronní chování, ale nedodržuje ho.

## 4. Doporučené řešení

Pro produkci: použít `await File.AppendAllTextAsync(...)` (pokud API existuje) nebo obalit do `Task.Run(...)`. Pro MVP účely je současné řešení akceptovatelné.

## 5. Otevřené otázky

- Zda má smysl řešit teď nebo až při produkčním nasazení.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*
- 2026-07-08: `AppendAsync` používá `Task.Run` pro offload sync I/O z volajícího vlákna.

---

## Související

- Vazby: `PROP-028`
