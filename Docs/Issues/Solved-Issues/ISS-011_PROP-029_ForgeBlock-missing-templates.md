# ISS-011 ForgeBlocky nemají Scriban šablony

Datum: 2026-04-07
PROP: PROP-029
Soubor: ForgeBlock projekty (EF Core, AutoMapper, FluentValidation)
Závažnost: ⚠️ Nízká
Stav: Resolved (2026-07-12) — Plugin templates
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
- 2026-07-12: **Plugin systém implementován** — ForgeBlocky nyní nesou vlastní Scriban šablony:
  1. Nové rozhraní `IForgeBlockTemplateProvider` v Core — ForgeBlock implementuje `GetTemplates()`
  2. `ForgeBlockRegistry` automaticky sbírá šablony při registraci
  3. `TemplateManager.RegisterInlineTemplate()` — registrace šablony z textového obsahu
  4. **EF Core**: 2 šablony (DbContext, EntityTypeConfig)
  5. **AutoMapper**: 1 šablona (AutoMapperProfile)
  6. **FluentValidation**: 1 šablona (FluentValidator)
- Při registraci ForgeBlocku se šablony automaticky zaregistrují do TemplateManageru (plugin pattern).
- Šablony jsou inline v C# kódu (připraveno na embedded resources při NuGet distribuci).

---

## Související

- Vazby: `PROP-029`
