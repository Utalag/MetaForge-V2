# ISS-006 CodeGenerator změněn z sealed na class kvůli dědičnosti

Datum: 2026-04-07
PROP: PROP-025
Soubor: `Src/MetaForge.Generators/CodeGenerator.cs`
Závažnost: ⚠️ Nízká
Stav: Open
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-025 (Generators — Incremental, monetization).

## 2. Popis problému

`CodeGenerator` byl změněn z `sealed` na `class` kvůli dědičnosti `TieredCodeGenerator`. Tato změna umožňuje nechtěné přepsání metod (nejsou virtual, ale otevření třídy otevírá dveře k dalším nevhodným patternům).

## 3. Dopad

- Ztráta záruky, že `CodeGenerator` nebude rozšiřován nevhodným způsobem.
- Otevření třídy může vést k těsně provázanému kódu.
- Snižuje čistotu architektury.

## 4. Doporučené řešení

Zvážit kompozici místo dědičnosti — např. `TieredCodeGenerator` by mohl wrapovat `CodeGenerator` místo dědění. Tím zůstane `CodeGenerator` uzavřený.

## 5. Otevřené otázky

- Zda je refactoring na kompozici prioritou nebo až při příští úpravě.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*

---

## Související

- Vazby: `PROP-025`
