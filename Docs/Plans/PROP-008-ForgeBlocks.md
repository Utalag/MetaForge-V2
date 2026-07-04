# PROP-008: ForgeBlock balíky

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-04
> **Autor:** Copilot (C# Implementer)

## Cíl

Vytvořit první ForgeBlock capability balíky — Math, String, Validation.

## Výstup

- `Src/ForgeBlocks/Math/MetaForge.ForgeBlocks.Math.csproj` + `MathForgeBlock.cs` — 8 capabilities
- `Src/ForgeBlocks/String/MetaForge.ForgeBlocks.String.csproj` + `StringForgeBlock.cs` — 8 capabilities
- `Src/ForgeBlocks/Validation/MetaForge.ForgeBlocks.Validation.csproj` + `ValidationForgeBlock.cs` — 8 capabilities

## Capabilities

| Balík | Capabilities |
|-------|-------------|
| Math | add, subtract, multiply, divide, round, abs, pow, sqrt |
| String | concat, format, trim, contains, replace, split, upper, lower |
| Validation | required, email, phone, url, range, regex, max_length, min_length |

## Zpětná vazba / Poznámky

Register() metody jsou připraveny pro propojení s CatalogManager (plánováno v Infrastructure epicu). CatalogEntries jsou dostupné přes IForgeBlockCapabilityPackage.
