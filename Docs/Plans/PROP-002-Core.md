# PROP-002: Core vrstva

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-04
> **Autor:** Copilot (C# Implementer)

## Cíl

Vytvořit projekt `MetaForge.Core` s kompletním typovým modelem, elementy, katalogem, ForgeBlock registry, inference a ValueObjects.

## Výstup

- `Src/MetaForge.Core/MetaForge.Core.csproj` — class library
- `DataTypes/DataType.cs` — 36 C# datových typů
- `DataTypes/TypeModel.cs` — immutable record s factory metodami
- `Abstractions/` — AppRoot, ProjectElement, RootElement, AccessModifier, AttributeElement, SemanticCollection
- `Elements/Types/` — ClassElement, InterfaceElement, EnumElement, EnumMemberElement, StructElement
- `Elements/Members/` — PropertyElement, MethodElement, ParameterElement
- `Elements/Expressions/` — Expression, ComputedExpression, ComputedOperation
- `Catalog/` — PresetDefinition, ICatalogProvider, BuiltInCatalogProvider, CatalogManager
- `ForgeBlockPackages/` — IForgeBlockPackage, ForgeBlockRegistry, DiscoveryMetadata
- `Inference/` — IConstraintInferencer, RuleBasedConstraintInferencer
- `StandardLibraries/` — IStandardLibraryTranslator, registry, resolver
- `ValueObjects/` — StrongType, ValueObjectValidationRule, ConversionOptions

## Architektonické guardraily

- C#-first (ne jazykově agnostické) — 36 C# typů v DataType enumu
- AppRoot → ProjectElement → RootElement hierarchie
- Core nesmí záviset na vyšších vrstvách
- TypeModel je immutable record

## Zpětná vazba / Poznámky

Po code review přidána thread safety (ConcurrentDictionary, lock). SemanticCollection je sealed.
