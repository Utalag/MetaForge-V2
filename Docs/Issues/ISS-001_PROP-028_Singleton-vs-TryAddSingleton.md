# ISS-001 Singleton vs TryAddSingleton v InfrastructureServiceRegistration

Datum: 2026-04-07
PROP: PROP-028
Soubor: `Src/MetaForge.Infrastructure/InfrastructureServiceRegistration.cs`
Závažnost: ⚠️ Střední
Stav: Open
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-028 (Infrastructure — Persistence, konfigurace, caching).

## 2. Popis problému

`InfrastructureServiceRegistration.cs` používá `AddSingleton` místo `TryAddSingleton`. Při vícenásobném volání `AddMetaForgeInfrastructure()` vzniknou duplicitní DI registrace, což může vést k neočekávanému chování (vytvoří se nová instance při každém resolve, poslední registrace vyhrává).

## 3. Dopad

- Při vícenásobném volání `AddMetaForgeInfrastructure()` (např. z různých composition root modulů) vznikají duplicitní registrace.
- Může vést k vytvoření více instancí singleton služeb.
- Ohrožuje konzistenci DI kontejneru.

## 4. Doporučené řešení

Nahradit `services.AddSingleton<T>()` → `services.TryAddSingleton<T>()` pro všechny registrace uvnitř `AddMetaForgeInfrastructure()`. Vyžaduje přidání `using Microsoft.Extensions.DependencyInjection.Extensions;`.

## 5. Otevřené otázky

- Žádné.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*

---

## Související

- Vazby: `PROP-028`
- Blokuje: —
