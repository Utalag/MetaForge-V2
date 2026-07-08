# ISS-011 ForgeBlocky nemají Scriban šablony

Datum: 2026-04-07
PROP: PROP-029
Soubor: ForgeBlock projekty (EF Core, AutoMapper, FluentValidation)
Závažnost: ⚠️ Nízká
Stav: Open
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-029 (ForgeBlocks — Rozšíření a marketplace).

## 2. Popis problému

Nové ForgeBlocky (EF Core, AutoMapper, FluentValidation) nemají Scriban šablony — pouze metadata. Generování kódu zatím není implementováno a bude řešeno v budoucnu.

## 3. Dopad

- ForgeBlocky jsou v aktuálním stavu pouze metadata-kontejnery bez generační schopnosti.
- Uživatelé nemohou ForgeBlocky používat k reálnému generování kódu.
- Funkcionalita je odložena — není to blocker, ale chybějící feature.

## 4. Doporučené řešení

Implementovat Scriban šablony pro EF Core, AutoMapper a FluentValidation v další samostatné iteraci. Každý ForgeBlock by měl mít alespoň základní generační scénář.

## 5. Otevřené otázky

- Jaká je priorita jednotlivých ForgeBlocků pro generování?
- Jaký bude formát šablon (inline vs embedded resources)?

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*

---

## Související

- Vazby: `PROP-029`
