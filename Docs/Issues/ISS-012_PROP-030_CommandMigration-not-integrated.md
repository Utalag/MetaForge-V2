# ISS-012 CommandMigrationEngine není integrován do ReplayEngine

Datum: 2026-04-07
PROP: PROP-030
Soubor: `Src/MetaForge.BusinessModel/Replay/ReplayEngine.cs`
Závažnost: ⚠️ Střední
Stav: Open
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-030 (Bezpečnost a stabilita — Schema migration, validace, health).

## 2. Popis problému

`CommandMigrationEngine` není integrován do `ReplayEngine` — migrace se musí volat ručně před replayem. To zvyšuje riziko, že uživatel spustí replay bez provedení migrace, což může vést k chybám při deserializaci starších command formátů.

## 3. Dopad

- Replay bez předchozí migrace může selhat na starších command formátech.
- Zvyšuje kognitivní zátěž na uživatele — musí si pamatovat volat migraci.
- Ohrožuje stabilitu replay pipeline.

## 4. Doporučené řešení

Přidat `CommandMigrationEngine` jako závislost `ReplayEngine` (přes DI nebo konstruktor) a volat jej automaticky před každým replayem.

## 5. Otevřené otázky

- Zda má být migrace volána vždy, nebo jen při detekci starého formátu.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*

---

## Související

- Vazby: `PROP-030`
