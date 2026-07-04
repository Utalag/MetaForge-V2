# PROP-004: Translator vrstva

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-04
> **Autor:** Copilot (C# Implementer)

## Cíl

Vytvořit projekt `MetaForge.Translator` s Facade, ProjectionReadService, DefaultBusinessTranslator a WriteBackService.

## Výstup

- `Src/MetaForge.Translator/MetaForge.Translator.csproj` — reference na Core + BusinessModel
- `Translation/IBusinessTranslator.cs` — rozhraní + EnrichmentResult
- `Translation/DefaultBusinessTranslator.cs` — deterministický překlad business atribut → TypeModel
- `Translation/WriteBackService.cs` — zápis enrichment dat zpět do business modelu
- `Host/ProjectionView.cs` — projekce business modelu pro čtení
- `Host/ProjectionReadService.cs` — vytváří projekci přes replay + překlad
- `Host/BusinessAuthoringHostFacade.cs` — JEDINÝ vstupní bod pro host surfaces

## Invarianty

- Facade je jediný entry point pro host surfaces
- Write path jde přes PatchEngine → CommandLog
- Read path jde přes ProjectionReadService (replay)
- Facade je surface-agnostic
- Translator je deterministický (AI je volitelný overlay)

## Zpětná vazba / Poznámky

DefaultBusinessTranslator používá CatalogManager pro resolvování typů s fallbackem.
