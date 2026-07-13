# ISS-010 RequiredTier není vynuceno kompilátorem

Datum: 2026-04-07
PROP: PROP-029
Soubor: `Src/ForgeBlocks/EfCoreForgeBlock/EfCoreForgeBlock.cs`
Závažnost: ⚠️ Nízká
Stav: Resolved (2026-07-08)
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-029 (ForgeBlocks — Rozšíření a marketplace).

## 2. Popis problému

`RequiredTier` je custom property na ForgeBlocku, ale `IForgeBlockCapabilityPackage` ho nedefinuje. Není vynucováno kompilátorem — každý ForgeBlock musí manuálně implementovat tuto property, a pokud tak neučiní, není kontrolováno.

## 3. Dopad

- ForgeBlock může vzniknout bez správně nastaveného `RequiredTier`.
- Chybí compile-time bezpečnost — chyba se projeví až za běhu nebo při validaci.
- Snižuje robustnost marketplace systému.

## 4. Doporučené řešení

Přidat `RequiredTier` do `IForgeBlockCapabilityPackage` (jako povinnou vlastnost) nebo použít atribut `[RequiredTier(GeneratorTier.Professional)]`.

## 5. Otevřené otázky

- Zda použít rozhraní nebo atribut — atribut je flexibilnější, ale hůře se vynucuje.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*
- 2026-07-08: `RequiredTier` je implementován na `EfCoreForgeBlock` (GeneratorTier.Infrastructure). `IForgeBlockCapabilityPackage` rozhraní je definováno. Chybějící `RequiredTier` na rozhraní je akceptováno — vynucení probíhá přes konvenci.

---

## Související

- Vazby: `PROP-029`
