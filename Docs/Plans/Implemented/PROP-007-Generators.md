# PROP-007: Generators (C#-first)

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-04
> **Autor:** Copilot (C# Implementer)

## Cíl

Vytvořit projekt `MetaForge.Generators` s CSharpGenerator jako jediným aktivním generátorem.

## Výstup

- `Src/MetaForge.Generators/MetaForge.Generators.csproj` — class library
- `BaseCodeGenerator.cs` — abstraktní bázová třída pro generátory
- `CSharp/CSharpGenerator.cs` — generátor C# kódu (třídy, interfacy, enumy, struktury)
- `GeneratedCodeArtifact.cs` — výstup generátoru
- `DiagnosticInfo.cs` — diagnostické informace (varování/chyby)
- `LanguageMapping.cs` — metadata o cílovém jazyce

## Generované typy

| Element | Výstup |
|---------|--------|
| ClassElement | `public class Name { ... }` — properties, metody, dědičnost, atributy |
| InterfaceElement | `public interface IName { ... }` — properties, metody |
| EnumElement | `public enum Name { ... }` — members, Flags atribut, underlying type |
| StructElement | `public readonly record struct Name { ... }` — properties, metody |

## Zpětná vazba / Poznámky

C# je jediný podporovaný výstupní jazyk. Generátor mapuje všech 36 DataType na C# klíčová slova.
