# ISS-008 TryEnrichAsync chybí v IBusinessTranslator rozhraní

Datum: 2026-04-07
PROP: PROP-019
Soubor: `Src/MetaForge.Translator/Services/DefaultBusinessTranslator.cs`
Závažnost: ⚠️ Nízká
Stav: Resolved (2026-07-12)
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-019 (Translator — IAiTranslator a AI-assisted překlad).

## 2. Popis problému

`TryEnrichAsync` je nová async metoda v `DefaultBusinessTranslator`, ale `IBusinessTranslator` rozhraní má pouze synchronní `TryEnrich`. Volající musí explicitně používat async verzi, což obchází rozhraní a porušuje Liskov substitution principle.

## 3. Dopad

- Volající nemohou používat `TryEnrichAsync` přes rozhraní — musí castovat na konkrétní typ.
- Porušuje programování proti rozhraní (Dependency Inversion).
- Zvyšuje riziko, že někdo použije synchronní variantu tam, kde by měla být async.

## 4. Doporučené řešení

Přidat `TryEnrichAsync` do `IBusinessTranslator` nebo vytvořit samostatné `IAsyncBusinessTranslator` rozhraní.

## 5. Otevřené otázky

- Zda přidat do stávajícího rozhraní (breaking change pro implementace) nebo vytvořit samostatné rozhraní.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*
- 2026-07-12: `TryEnrichAsync` přidáno do `IBusinessTranslator` rozhraní. Signatura odpovídá implementaci v `DefaultBusinessTranslator`.

---

## Související

- Vazby: `PROP-019`
