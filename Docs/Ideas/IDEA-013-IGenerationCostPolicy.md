# IDEA-013 IGenerationCostPolicy — Pluggable Billing Gate

Stav: Idea
Oblast: Monetizace, Core, Generators
Zdroj: For_Inspiration/Architecture-Define/07-Monetization-Credits.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept monetizace obsahoval propracovaný kreditový systém s `IGenerationCostPolicy` rozhraním. Současná implementace (PROP-025) má pouze `Coin` property na elementech a `TotalCoin` sumu, ale chybí gate před generací — nikde se nekontroluje, zda má uživatel dostatek kreditů.

Nápad vychází z `07-Monetization-Credits.md`, kde je popsáno:
- `IGenerationCostPolicy` s `CalculateCost()`, `CanGenerateAsync()`, `DeductAsync()`
- 4 implementace: `AlwaysAllowGenerationPolicy`, `CloudBillingGenerationPolicy`, `LocalLicenseGenerationPolicy`, `EnterpriseLicenseGenerationPolicy`
- Jazykový multiplikátor (C#=1×, TS=2×, Python=2×, Java=2×, Go=3×)
- Kreditová hodnota per element: Class=10, Struct=15, Property=2, Method=5, Enum=5 (chybí), Interface=8 (chybí)
- Gate v `BusinessAuthoringHostFacade.GenerateCodeAsync()`

## 2. Problém dnes

- PROP-025 implementoval Coin model a license tiers, ale chybí **kontrola před generací**.
- `IGenerationCostPolicy` rozhraní neexistuje — Core a Generators přímo nevědí o billing modelu.
- Deployment varianty (standalone, cloud, enterprise licence) nelze rozlišit — vše je `AlwaysAllow`.
- Enum a Interface nemají `CreditScore` — monetizace je neúplná.
- Marketplace presety (`CatalogItem.CreditCost`) nejsou napojeny na generační cenu.

## 3. Předběžný směr řešení

Pluggable rozhraní:

- `IGenerationCostPolicy` v Core (nebo Generators) — 3 metody: `CalculateCost`, `CanGenerateAsync`, `DeductAsync`
- Implementace per deployment variantu (AlwaysAllow, CloudBilling, LocalLicense, EnterpriseLicense)
- Gate volaný z `BusinessAuthoringHostFacade.GenerateCodeAsync()` nebo z `BaseCodeGenerator`
- Doplnění `CreditScore` na `EnumElement` (5) a `InterfaceElement` (8)
- Napojení `CatalogItem.CreditCost` na celkovou cenu generace

Dotčené vrstvy: Core (rozhraní), Generators (gate), Infrastructure (lokální licence), Host (injekce policy).

## 4. Signál hodnoty

- MetaForge může být nasazeno v různých deployment variantách bez změny kódu.
- OSS/trial → standalone licence → cloud billing — jedna codebase.
- Uživatel vidí cenu před generací → transparentní monetizace.
- Nezbytný krok pro produktizaci MVP.

## 5. Rizika a nejasnosti

- `IGenerationCostPolicy` musí být jednoduché — nemá být general-purpose billing engine.
- Zeměpisná závislost: CloudBilling vyžaduje REST API — kdo ho hostuje?
- Lokální licence: jak zabránit falšování?
- OQ-xxx: Patří `IGenerationCostPolicy` do Core, Generators, nebo samostatného projektu `MetaForge.Billing`?

## 6. Doporučený další krok

Candidate Proposal — follow-up k PROP-025 (Monetizace). Měl by být plánován jako další fáze monetizace po stabilizaci Coin modelu.

Vazby: PROP-025, PROP-035 (C#-first — doplnění Enum/Interface CreditScore), PROP-029 (marketplace CreditCost)
