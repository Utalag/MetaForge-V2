# PROP-009: Testovací infrastruktura

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-04
> **Autor:** Copilot (C# Implementer)

## Cíl

Vytvořit testovací projekty pro všechny vrstvy — unit testy, replay testy, generator output testy.

## Výstup

- `Tests/MetaForge.Core.Tests/` — 5 test class, 32 testů
- `Tests/MetaForge.BusinessModel.Tests/` — 3 test class, 19 testů
- `Tests/MetaForge.Translator.Tests/` — 1 test class, 5 testů
- `Tests/MetaForge.Generators.Tests/` — 1 test class, 7 testů

## Testovací pokrytí

| Vrstva | Testy | Pokrytí |
|--------|-------|---------|
| Core | 32 | TypeModel, DataType, CatalogManager, ForgeBlockRegistry, ConstraintInferencer |
| BusinessModel | 19 | CommandLogStore, ReplayEngine, PatchEngine |
| Translator | 5 | DefaultBusinessTranslator |
| Generators | 7 | CSharpGenerator |

## Principy

- xUnit + FluentAssertions
- AAA pattern (Arrange-Act-Assert)
- Žádné testy závislé na AI
- Unit testy preferovány

## Zpětná vazba / Poznámky

Po code review opraveny křehké testy — DataType testuje konkrétní hodnoty místo počtu; CommandLogStore testuje nepřítomnost remove metod.
