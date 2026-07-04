# Atomické tasky pro malý model

> Každý task je navržen tak, aby ho zvládl Gemma 4 12B bez ztráty kontextu.
> Maximální scope: 1–3 soubory, jasný vstup/výstup, explicitní definition of done.

---

## Pravidla atomizace

1. Jeden task = jedna odpovědnost v jedné vrstvě.
2. Maximálně 3 dotčené soubory na task.
3. Jasný vstup (co existuje), výstup (co má vzniknout), ověření (jak poznat hotovo).
4. Žádný task nevyžaduje znalost celého repa.
5. Každý task je rollback-friendly (revert jednoho commitu).
6. Závislosti jsou explicitní — task říká, co musí existovat před ním.

---

## Task formát

```
### TASK-{epic}.{slice}.{číslo} — {název}
- Vstup: co musí existovat
- Výstup: co task vytvoří nebo změní
- Soubory: seznam dotčených souborů
- Závislosti: předchozí tasky
- Ověření: jak poznat, že je hotovo
- Riziko: co se může pokazit
- Rollback: jak vrátit změnu
```

---

## Epic 1 — Governance

### TASK-1.1.1 — Vytvoření solution souboru

- Vstup: Prázdný adresář
- Výstup: `MetaForge.slnx` (prázdný solution)
- Soubory: `MetaForge.slnx`
- Závislosti: Žádné
- Ověření: `dotnet build MetaForge.slnx` projde bez chyb (0 projektů)
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-1.1.2 — Vytvoření PROPOSALS.md

- Vstup: Kořen repa
- Výstup: `PROPOSALS.md` s hlavičkou a skeleton strukturou
- Soubory: `PROPOSALS.md`
- Závislosti: TASK-1.1.1
- Ověření: Soubor existuje, obsahuje sekce dle governance šablony
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-1.1.3 — Vytvoření Progress.md

- Vstup: Kořen repa
- Výstup: `Progress.md` s hlavičkou
- Soubory: `Progress.md`
- Závislosti: TASK-1.1.1
- Ověření: Soubor existuje, má správnou strukturu
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-1.1.4 — Vytvoření Memories.md

- Vstup: Kořen repa
- Výstup: `Memories.md` s hlavičkou a šablonou záznamu
- Soubory: `Memories.md`
- Závislosti: TASK-1.1.1
- Ověření: Soubor existuje, obsahuje šablonu
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-1.1.5 — Vytvoření PROPOSALS_NEXT.md

- Vstup: Kořen repa
- Výstup: `PROPOSALS_NEXT.md` se zásobníkem kandidátních návrhů
- Soubory: `PROPOSALS_NEXT.md`
- Závislosti: TASK-1.1.1
- Ověření: Soubor existuje
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-1.2.1 — Markdown-first workflow instrukční soubor

- Vstup: TASK-1.1.2 až TASK-1.1.5
- Výstup: `Docs/workflow-markdown-first.md`
- Soubory: `Docs/workflow-markdown-first.md`
- Závislosti: TASK-1.1.5
- Ověření: Soubor existuje, popisuje celý workflow
- Riziko: Nízké
- Rollback: Smazat soubor

---

## Epic 2 — Core

### TASK-2.1.1 — Založení projektu MetaForge.Core

- Vstup: Solution soubor
- Výstup: `Src/MetaForge.Core/MetaForge.Core.csproj` (net9.0, nullable, implicit usings)
- Soubory: `Src/MetaForge.Core/MetaForge.Core.csproj`, `MetaForge.slnx`
- Závislosti: TASK-1.1.1
- Ověření: `dotnet build Src/MetaForge.Core/MetaForge.Core.csproj` projde
- Riziko: Nízké
- Rollback: Odebrat projekt ze solution, smazat složku

### TASK-2.1.2 — RootElement abstrakce

- Vstup: Projekt MetaForge.Core existuje
- Výstup: `Src/MetaForge.Core/Abstractions/RootElement.cs`
- Soubory: `Src/MetaForge.Core/Abstractions/RootElement.cs`
- Závislosti: TASK-2.1.1
- Ověření: Build projde, třída má Id, Name, Namespace, Kind (bez závislosti na jazykové abstrakci)
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-2.2.1 — BaseType enum

- Vstup: Projekt MetaForge.Core existuje
- Výstup: `Src/MetaForge.Core/DataTypes/BaseType.cs`
- Soubory: `Src/MetaForge.Core/DataTypes/BaseType.cs`
- Závislosti: TASK-2.1.1
- Ověření: Build projde, enum obsahuje String, Int, Bool, DateTime, Decimal, Guid, Object
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-2.2.2 — TypeModel record

- Vstup: BaseType existuje
- Výstup: `Src/MetaForge.Core/DataTypes/TypeModel.cs`
- Soubory: `Src/MetaForge.Core/DataTypes/TypeModel.cs`
- Závislosti: TASK-2.2.1
- Ověření: Build projde, record má BaseType, IsNullable, IsCollection, GenericArgs
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-2.3.1 — ClassElement

- Vstup: RootElement existuje
- Výstup: `Src/MetaForge.Core/Elements/Types/ClassElement.cs`
- Soubory: `Src/MetaForge.Core/Elements/Types/ClassElement.cs`
- Závislosti: TASK-2.1.2
- Ověření: Build projde, třída dědí z RootElement
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-2.3.2 — PropertyElement

- Vstup: ClassElement existuje, TypeModel existuje
- Výstup: `Src/MetaForge.Core/Elements/Members/PropertyElement.cs`
- Soubory: `Src/MetaForge.Core/Elements/Members/PropertyElement.cs`
- Závislosti: TASK-2.3.1, TASK-2.2.2
- Ověření: Build projde, třída má Name, Type (TypeModel), Modifiers
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-2.3.3 — MethodElement

- Vstup: ClassElement existuje, TypeModel existuje
- Výstup: `Src/MetaForge.Core/Elements/Members/MethodElement.cs`
- Soubory: `Src/MetaForge.Core/Elements/Members/MethodElement.cs`
- Závislosti: TASK-2.3.1, TASK-2.2.2
- Ověření: Build projde, třída má Name, ReturnType, Parameters
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-2.4.1 — CatalogManager základ

- Vstup: TypeModel existuje
- Výstup: `Src/MetaForge.Core/Catalog/CatalogManager.cs`
- Soubory: `Src/MetaForge.Core/Catalog/CatalogManager.cs`
- Závislosti: TASK-2.2.2
- Ověření: Build projde, třída má ResolveType(), RegisterPreset()
- Riziko: Střední — musí být rozšiřitelný
- Rollback: Smazat soubor

### TASK-2.8.1 — Expression abstrakce

- Vstup: Projekt MetaForge.Core existuje
- Výstup: `Src/MetaForge.Core/Elements/Expressions/Expression.cs`
- Soubory: `Src/MetaForge.Core/Elements/Expressions/Expression.cs`
- Závislosti: TASK-2.1.1
- Ověření: Build projde, abstraktní třída reprezentuje výraz použitelný v computed properties/behaviors
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-2.8.2 — ComputedExpression a ComputedOperation

- Vstup: Expression existuje
- Výstup: `Src/MetaForge.Core/Elements/Expressions/ComputedExpression.cs`, `Src/MetaForge.Core/Elements/Expressions/ComputedOperation.cs`
- Soubory: `Src/MetaForge.Core/Elements/Expressions/ComputedExpression.cs`, `Src/MetaForge.Core/Elements/Expressions/ComputedOperation.cs`
- Závislosti: TASK-2.8.1
- Ověření: Build projde, ComputedExpression skládá operace nad Expression stromem
- Riziko: Střední — základ pro computed properties
- Rollback: Smazat soubory

### TASK-2.8.3 — Statement a Comment

- Vstup: Expression existuje
- Výstup: `Src/MetaForge.Core/Elements/Expressions/Statement.cs`, `Src/MetaForge.Core/Elements/Expressions/Comment.cs`
- Soubory: `Src/MetaForge.Core/Elements/Expressions/Statement.cs`, `Src/MetaForge.Core/Elements/Expressions/Comment.cs`
- Závislosti: TASK-2.8.1
- Ověření: Build projde, Statement reprezentuje jeden příkaz v těle behavioru
- Riziko: Nízké
- Rollback: Smazat soubory

### TASK-2.8.4 — IExpressionRenderer a ExpressionRendererRegistry

- Vstup: Expression, Statement existují
- Výstup: `Src/MetaForge.Core/Elements/Expressions/IExpressionRenderer.cs`, `Src/MetaForge.Core/Elements/Expressions/ExpressionRendererRegistry.cs`
- Soubory: `Src/MetaForge.Core/Elements/Expressions/IExpressionRenderer.cs`, `Src/MetaForge.Core/Elements/Expressions/ExpressionRendererRegistry.cs`
- Závislosti: TASK-2.8.2, TASK-2.8.3
- Ověření: Build projde, registry umožňuje registraci rendereru pro daný typ výrazu
- Riziko: Nízké
- Rollback: Smazat soubory

### TASK-2.8.5 — SemanticMath a SemanticStandardLibrary

- Vstup: Expression existuje
- Výstup: `Src/MetaForge.Core/Elements/Expressions/SemanticMath.cs`, `Src/MetaForge.Core/Elements/Expressions/SemanticStandardLibrary.cs`
- Soubory: `Src/MetaForge.Core/Elements/Expressions/SemanticMath.cs`, `Src/MetaForge.Core/Elements/Expressions/SemanticStandardLibrary.cs`
- Závislosti: TASK-2.8.1
- Ověření: Build projde, obsahují jazykově agnostické sémantické operace (math, standardní knihovna)
- Riziko: Nízké
- Rollback: Smazat soubory

### TASK-2.8.6 — Primitives (Field, Property, Parameter, Variable)

- Vstup: TypeModel existuje
- Výstup: `Src/MetaForge.Core/Elements/Primitives/Field.cs`, `Property.cs`, `Parameter.cs`, `Variable.cs`
- Soubory: `Src/MetaForge.Core/Elements/Primitives/Field.cs`, `Src/MetaForge.Core/Elements/Primitives/Property.cs`, `Src/MetaForge.Core/Elements/Primitives/Parameter.cs`, `Src/MetaForge.Core/Elements/Primitives/Variable.cs`
- Závislosti: TASK-2.2.2
- Ověření: Build projde, každý primitiv má Name a Type (TypeModel)
- Riziko: Nízké
- Rollback: Smazat soubory

### TASK-2.9.1 — IConstraintInferencer a RuleBasedConstraintInferencer

- Vstup: TypeModel existuje
- Výstup: `Src/MetaForge.Core/Inference/IConstraintInferencer.cs`, `Src/MetaForge.Core/Inference/RuleBasedConstraintInferencer.cs`
- Soubory: `Src/MetaForge.Core/Inference/IConstraintInferencer.cs`, `Src/MetaForge.Core/Inference/RuleBasedConstraintInferencer.cs`
- Závislosti: TASK-2.2.2
- Ověření: Build projde, inferencer odvozuje constraints z názvu/typu atributu
- Riziko: Střední — pravidla musí být deterministická
- Rollback: Smazat soubory

### TASK-2.9.2 — IDomainAnalyzer, BoundaryRule, MethodBoundaryAnalyzer

- Vstup: RootElement existuje
- Výstup: `Src/MetaForge.Core/Inference/Boundary/IDomainAnalyzer.cs`, `BoundaryRule.cs`, `MethodBoundaryAnalyzer.cs`
- Soubory: `Src/MetaForge.Core/Inference/Boundary/IDomainAnalyzer.cs`, `Src/MetaForge.Core/Inference/Boundary/BoundaryRule.cs`, `Src/MetaForge.Core/Inference/Boundary/MethodBoundaryAnalyzer.cs`
- Závislosti: TASK-2.1.2
- Ověření: Build projde, analyzer odvozuje doménové hranice metody z jejího podpisu
- Riziko: Střední — heuristika ovlivňuje kvalitu generovaného API
- Rollback: Smazat soubory

### TASK-2.10.1 — IStandardLibraryTranslator a Registry

- Vstup: Projekt MetaForge.Core existuje
- Výstup: `Src/MetaForge.Core/StandardLibraries/IStandardLibraryTranslator.cs`, `IStandardLibraryTranslatorRegistry.cs`, `StandardLibraryTranslatorRegistry.cs`
- Soubory: `Src/MetaForge.Core/StandardLibraries/IStandardLibraryTranslator.cs`, `Src/MetaForge.Core/StandardLibraries/IStandardLibraryTranslatorRegistry.cs`, `Src/MetaForge.Core/StandardLibraries/StandardLibraryTranslatorRegistry.cs`
- Závislosti: TASK-2.1.1
- Ověření: Build projde, registry mapuje sémantickou operaci na C# standardní knihovnu
- Riziko: Nízké
- Rollback: Smazat soubory

### TASK-2.10.2 — StandardLibraryRequirements a Resolver

- Vstup: IStandardLibraryTranslator existuje
- Výstup: `Src/MetaForge.Core/StandardLibraries/StandardLibraryRequirements.cs`, `StandardLibraryRequirementResolver.cs`
- Soubory: `Src/MetaForge.Core/StandardLibraries/StandardLibraryRequirements.cs`, `Src/MetaForge.Core/StandardLibraries/StandardLibraryRequirementResolver.cs`
- Závislosti: TASK-2.10.1
- Ověření: Build projde, resolver vrací seznam potřebných using direktiv pro daný element
- Riziko: Nízké
- Rollback: Smazat soubory

### TASK-2.11.1 — StrongType a ConversionOptions

- Vstup: TypeModel existuje
- Výstup: `Src/MetaForge.Core/ValueObjects/StrongType.cs`, `ConversionOptions.cs`
- Soubory: `Src/MetaForge.Core/ValueObjects/StrongType.cs`, `Src/MetaForge.Core/ValueObjects/ConversionOptions.cs`
- Závislosti: TASK-2.2.2
- Ověření: Build projde, StrongType obaluje TypeModel s pojmenovaným doménovým typem
- Riziko: Nízké
- Rollback: Smazat soubory

### TASK-2.11.2 — ValueObjectValidationRule

- Vstup: StrongType existuje
- Výstup: `Src/MetaForge.Core/ValueObjects/ValueObjectValidationRule.cs`
- Soubory: `Src/MetaForge.Core/ValueObjects/ValueObjectValidationRule.cs`
- Závislosti: TASK-2.11.1
- Ověření: Build projde, pravidlo validuje hodnotu StrongType
- Riziko: Nízké
- Rollback: Smazat soubor

---

## Epic 3 — BusinessModel

### TASK-3.1.1 — Založení projektu MetaForge.BusinessModel

- Vstup: Solution soubor
- Výstup: `Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj`
- Soubory: `Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj`, `MetaForge.slnx`
- Závislosti: TASK-1.1.1
- Ověření: `dotnet build` projde
- Riziko: Nízké
- Rollback: Odebrat ze solution, smazat složku

### TASK-3.2.1 — BusinessEntityNode

- Vstup: Projekt existuje
- Výstup: `Src/MetaForge.BusinessModel/Models/BusinessEntityNode.cs`
- Soubory: `Src/MetaForge.BusinessModel/Models/BusinessEntityNode.cs`
- Závislosti: TASK-3.1.1
- Ověření: Build projde, třída má Id, Name, Attributes, Behaviors, Relations
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-3.2.2 — BusinessAttributeNode

- Vstup: BusinessEntityNode existuje
- Výstup: `Src/MetaForge.BusinessModel/Models/BusinessAttributeNode.cs`
- Soubory: `Src/MetaForge.BusinessModel/Models/BusinessAttributeNode.cs`
- Závislosti: TASK-3.2.1
- Ověření: Build projde, třída má Id, Name, Type, Constraints, CoreDetail
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-3.2.3 — BusinessAuthoringDocument

- Vstup: BusinessEntityNode existuje
- Výstup: `Src/MetaForge.BusinessModel/Models/BusinessAuthoringDocument.cs`
- Soubory: `Src/MetaForge.BusinessModel/Models/BusinessAuthoringDocument.cs`
- Závislosti: TASK-3.2.1
- Ověření: Build projde, třída má ProjectName, Entities, CustomTypes, SchemaVersion
- Riziko: Střední — musí být source of truth
- Rollback: Smazat soubor

### TASK-3.3.1 — CommandEnvelope

- Vstup: Projekt existuje
- Výstup: `Src/MetaForge.BusinessModel/CommandLog/CommandEnvelope.cs`
- Soubory: `Src/MetaForge.BusinessModel/CommandLog/CommandEnvelope.cs`
- Závislosti: TASK-3.1.1
- Ověření: Build projde, record má Id, Timestamp, CommandType, Payload
- Riziko: Nízké
- Rollback: Smazat soubor

### TASK-3.3.2 — CommandLogStore

- Vstup: CommandEnvelope existuje
- Výstup: `Src/MetaForge.BusinessModel/CommandLog/CommandLogStore.cs`
- Soubory: `Src/MetaForge.BusinessModel/CommandLog/CommandLogStore.cs`
- Závislosti: TASK-3.3.1
- Ověření: Build projde, třída má Append(), GetAll(), Count
- Riziko: Střední — musí být append-only
- Rollback: Smazat soubor

### TASK-3.4.1 — ReplayEngine

- Vstup: CommandLogStore, BusinessAuthoringDocument existují
- Výstup: `Src/MetaForge.BusinessModel/CommandLog/ReplayEngine.cs`
- Soubory: `Src/MetaForge.BusinessModel/CommandLog/ReplayEngine.cs`
- Závislosti: TASK-3.3.2, TASK-3.2.3
- Ověření: Build projde, metoda Replay() vrací BusinessAuthoringDocument
- Riziko: Vysoké — je to autoritativní rekonstrukce
- Rollback: Smazat soubor

---

## Poznámka

Toto je výběr prvních ~25 tasků. Kompletní seznam pokračuje v `13-Ready-to-Run-Prompts.md`, kde je každý task zformulován jako prompt pro malý model.
