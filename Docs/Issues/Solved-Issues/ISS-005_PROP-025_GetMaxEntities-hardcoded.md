# ISS-005 GetMaxEntities vrací hardcodované 3 místo hodnoty z licence

Datum: 2026-04-07
PROP: PROP-025
Soubor: `Src/MetaForge.Generators/IncrementalCodeGenerator.cs`
Závažnost: ⚠️ Střední
Stav: Resolved (2026-07-08)
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-025 (Generators — Incremental, monetization).

## 2. Popis problému

`GetMaxEntities()` v `IncrementalCodeGenerator.cs` vrací hardcodované číslo 3, místo aby četlo hodnotu z `GeneratorLicense.MaxEntities`. Sandbox limit se nekontroluje správně pro vyšší tiery — licence s vyšším limitem entity budou stále omezeny na 3.

## 3. Dopad

- Vyšší licenční tiery (umožňující více entit) nefungují správně.
- Monetizační model není správně vynucen.
- Uživatelé s vyšší licencí budou frustrováni omezením.

## 4. Doporučené řešení

Předat `GeneratorLicense` do metody nebo číst `_license.MaxEntities` místo hardcodované hodnoty 3.

## 5. Otevřené otázky

- Žádné.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*
- 2026-07-08: `GetMaxEntities()` nyní čte `_license.MaxEntities`. `_license` field přidán do `IncrementalCodeGenerator`.

---

## Související

- Vazby: `PROP-025`
