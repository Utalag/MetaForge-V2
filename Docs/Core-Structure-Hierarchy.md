# Core Layer — Struktura Hierarchie

> Kompletní přehled třídní hierarchie v `MetaForge.Core`.
> AppRoot → ProjectElement → RootElement (Class, Interface, Enum, Struct) → Members → Types

---

## 1. AppRoot — Vstupní bod dokumentu

```
AppRoot
 ├── List<ProjectElement> Projects
 └── int TotalCoin  (suma všech TotalCoin napříč projekty)
```

- **Soubor:** `Src/MetaForge.Core/Abstractions/AppRoot.cs`
- Vlastnosti: `Projects` (seznam projektů), `TotalCoin` (agregovaná cena v kreditech).

---

## 2. ProjectElement — Projekt v solution

```
ProjectElement
 ├── string Name
 ├── string? DefaultNamespace
 └── List<RootElement> RootElements
```

- **Soubor:** `Src/MetaForge.Core/Abstractions/ProjectElement.cs`
- Obsahuje top-level elementy (třídy, interfacy, enumy, struktury).

---

## 3. RootElement — Bázová abstrakce (abstract)

```
RootElement (abstract)
 ├── Guid Id
 ├── string Name
 ├── abstract string Kind
 ├── List<string> Usings
 ├── List<AttributeElement> Attributes
 ├── int Coin
 └── virtual int TotalCoin
```

- **Soubor:** `Src/MetaForge.Core/Abstractions/RootElement.cs`
- Bázová třída pro všechny top-level deklarace.

---

## 4. Konkrétní RootElement potomci

```
                    ┌──────────────┐
                    │  RootElement │ (abstract)
                    └──────┬───────┘
                           │
            ┌──────────────┼──────────────┬──────────────┐
            ▼              ▼              ▼              ▼
      ClassElement   InterfaceElement  EnumElement   StructElement
```

### 4.1 ClassElement

```
ClassElement : RootElement
 ├── Kind = "class"
 ├── string? BaseClassName
 ├── List<string> ImplementedInterfaces
 ├── AccessModifier AccessModifier
 ├── bool IsAbstract | IsSealed | IsStatic | IsPartial
 ├── List<PropertyElement> Properties
 ├── List<MethodElement> Methods
 └── override int TotalCoin  (Coin + Properties + Methods)
```

- **Soubor:** `Src/MetaForge.Core/Elements/Types/ClassElement.cs`

### 4.2 InterfaceElement

```
InterfaceElement : RootElement
 ├── Kind = "interface"
 ├── AccessModifier AccessModifier
 ├── List<PropertyElement> Properties
 ├── List<MethodElement> Methods
 └── override int TotalCoin
```

- **Soubor:** `Src/MetaForge.Core/Elements/Types/InterfaceElement.cs`

### 4.3 EnumElement

```
EnumElement : RootElement
 ├── Kind = "enum"
 ├── AccessModifier AccessModifier
 ├── DataType UnderlyingType (default: Int32)
 ├── bool IsFlags
 ├── List<EnumMemberElement> Members
 └── override int TotalCoin  (Coin + Members)
```

- **Soubor:** `Src/MetaForge.Core/Elements/Types/EnumElement.cs`

### 4.4 StructElement

```
StructElement : RootElement
 ├── Kind = "struct"
 ├── AccessModifier AccessModifier
 ├── bool IsReadOnly | IsRecord
 ├── List<PropertyElement> Properties
 ├── List<MethodElement> Methods
 └── override int TotalCoin
```

- **Soubor:** `Src/MetaForge.Core/Elements/Types/StructElement.cs`

---

## 5. Member Elementy

```
Member elementy (nezávislé na RootElement — jsou child elementy v Class/Interface/Struct)
 ├── PropertyElement
 ├── MethodElement
 │    └── List<ParameterElement> Parameters
 ├── ParameterElement
 └── EnumMemberElement  (child EnumElementu)
```

### 5.1 PropertyElement

```
PropertyElement
 ├── string Name
 ├── TypeModel Type
 ├── AccessModifier AccessModifier
 ├── bool HasGetter | HasSetter | IsInitOnly | IsRequired | IsStatic
 ├── string? DefaultValue
 └── int Coin (default: 2)
```

- **Soubor:** `Src/MetaForge.Core/Elements/Members/PropertyElement.cs`

### 5.2 MethodElement

```
MethodElement
 ├── string Name
 ├── TypeModel ReturnType
 ├── AccessModifier AccessModifier
 ├── bool IsStatic | IsAsync | IsAbstract | IsVirtual | IsOverride
 ├── List<ParameterElement> Parameters
 ├── List<AttributeElement> Attributes
 ├── string? Body
 ├── int Coin (default: 5)
 └── int TotalCoin  (Coin + Parameters)
```

- **Soubor:** `Src/MetaForge.Core/Elements/Members/MethodElement.cs`

### 5.3 ParameterElement

```
ParameterElement
 ├── string Name
 ├── TypeModel Type
 ├── bool HasDefaultValue
 ├── string? DefaultValue
 ├── ParameterModifier Modifier (None | Ref | Out | In | Params)
 └── int Coin (default: 1)
```

- **Soubor:** `Src/MetaForge.Core/Elements/Members/ParameterElement.cs`

### 5.4 EnumMemberElement

```
EnumMemberElement
 ├── string Name
 ├── object? Value
 ├── List<AttributeElement> Attributes
 └── int Coin (default: 1)
```

- **Soubor:** `Src/MetaForge.Core/Elements/Types/EnumMemberElement.cs`

---

## 6. Pomocné Abstrakce

### 6.1 AttributeElement

```
AttributeElement
 ├── string Name (např. "Obsolete", "JsonProperty")
 └── List<object?> Arguments
```

- **Soubor:** `Src/MetaForge.Core/Abstractions/AttributeElement.cs`

### 6.2 AccessModifier (enum)

```
AccessModifier: Public | Internal | Protected | Private
               | ProtectedInternal | PrivateProtected
```

- **Soubor:** `Src/MetaForge.Core/Abstractions/AccessModifier.cs`

### 6.3 SemanticCollection<T>

```
SemanticCollection<T> : List<T>
 └── event Action? Changed  (vyvolá se při Add, Remove, Clear)
```

- **Soubor:** `Src/MetaForge.Core/Abstractions/SemanticCollection.cs`

---

## 7. Typový systém

### 7.1 DataType (enum — 32 C# primitiv)

```
DataType: Bool | Byte | SByte | Int16 | UInt16 | Int32 | UInt32
         | Int64 | UInt64 | Int128 | Half | Single | Double | Decimal
         | NInt | NUInt | Char | String | Binary
         | DateOnly | TimeOnly | DateTime | DateTimeOffset | TimeSpan
         | Guid | Uri | Version
         | Entity | EnumValue | Object | Dynamic | Void
         | Array | Nullable | Struct | Record
```

- **Soubor:** `Src/MetaForge.Core/DataTypes/DataType.cs`

### 7.2 TypeModel (sealed record)

```
TypeModel (record)
 ├── DataType BaseType
 ├── bool IsNullable
 ├── bool IsCollection
 ├── string? CustomTypeName
 ├── List<TypeModel> GenericArguments
 ├── bool IsVoid  (derived)
 │
 ├── Statické factory: Void, String, Int32, Bool, Object, Decimal, Guid, DateTime
 ├── static Of(DataType) → TypeModel
 ├── MakeNullable() → TypeModel
 ├── MakeCollection() → TypeModel
 ├── WithCustomName(string) → TypeModel
 └── WithGenericArg(TypeModel) → TypeModel
```

- **Soubor:** `Src/MetaForge.Core/DataTypes/TypeModel.cs`

---

## 8. Expresní systém

```
Expression (abstract)
 └── abstract string Kind

    ┌─────────────────────────┐
    │  Expression (abstract)  │
    └──────────┬──────────────┘
               │
               ▼
     ComputedExpression
       ├── ComputedOperation Operation
       └── List<Expression> Operands

ComputedOperation (record)
 ├── string OperationId
 ├── string DisplayName
 └── string? Description
```

- **Soubory:** `Src/MetaForge.Core/Elements/Expressions/Expression.cs`, `ComputedExpression.cs`, `ComputedOperation.cs`

---

## 9. Value Objects

### 9.1 StrongType

```
StrongType (record)
 ├── string Name
 ├── TypeModel Underlying
 ├── IReadOnlyList<ValueObjectValidationRule>? ValidationRules
 └── ConversionOptions? Conversion
```

- **Soubor:** `Src/MetaForge.Core/ValueObjects/StrongType.cs`

### 9.2 ValueObjectValidationRule

```
ValueObjectValidationRule (record)
 ├── string RuleName
 ├── string? Parameter
 └── string? ErrorMessage
```

### 9.3 ConversionOptions

```
ConversionOptions (record)
 ├── bool GenerateImplicitConversion
 ├── bool GenerateExplicitConversion
 ├── bool GenerateToString (default: true)
 ├── bool GenerateEquals (default: true)
 └── bool GenerateGetHashCode (default: true)
```

---

## 10. Services (katalog, inference, standard libraries, forge blocks)

### 10.1 Catalog — Typový katalog

```
CatalogManager
 ├── RegisterProvider(ICatalogProvider)
 ├── RegisterPreset(PresetDefinition)
 ├── ResolveType(string) → PresetDefinition?
 ├── SearchPresets(string) → IReadOnlyList<PresetDefinition>
 └── GetAllPresets() → IReadOnlyList<PresetDefinition>

ICatalogProvider
 ├── string ProviderName
 ├── GetAllPresets()
 └── ResolveType(string)

BuiltInCatalogProvider : ICatalogProvider  (vestavěná mapování: int, string, email, money...)

PresetDefinition (record)
 ├── string Name
 ├── TypeModel Type
 ├── string? Description
 └── IReadOnlyList<string>? Tags
```

- **Soubory:** `Src/MetaForge.Core/Catalog/`

### 10.2 Inference — Odvozování constraintů

```
IConstraintInferencer
 └── Infer(string attributeName, TypeModel type) → IReadOnlyList<string>

RuleBasedConstraintInferencer : IConstraintInferencer
  (pravidla: email → ["email_format","not_empty","max_length:254"], ...)
```

- **Soubory:** `Src/MetaForge.Core/Inference/`

### 10.3 Standard Libraries — Překlad sémantických operací

```
IStandardLibraryTranslator
 ├── string OperationId
 └── Translate(string) → StandardLibraryRequirements?

IStandardLibraryTranslatorRegistry
 ├── Register(IStandardLibraryTranslator)
 ├── Resolve(string) → IStandardLibraryTranslator?
 └── GetAll() → IReadOnlyList<IStandardLibraryTranslator>

StandardLibraryRequirements (record)
 ├── string OperationId
 ├── IReadOnlyList<string> RequiredNamespaces
 ├── IReadOnlyList<string>? RequiredPackages
 └── string? CSharpExpressionTemplate

StandardLibraryTranslatorRegistry : IStandardLibraryTranslatorRegistry
StandardLibraryRequirementResolver
```

- **Soubory:** `Src/MetaForge.Core/StandardLibraries/`

### 10.4 ForgeBlock Packages — Externí balíčky

```
ForgeBlockRegistry
 ├── Register(IForgeBlockPackage)
 ├── GetPackage(string) → IForgeBlockPackage?
 ├── SearchByTag(string) → IReadOnlyList<IForgeBlockPackage>
 └── GetAllCapabilities() → IReadOnlyList<ForgeBlockCapability>

IForgeBlockPackage
 ├── string Handle
 ├── string Version
 ├── IReadOnlyList<ForgeBlockCapability> Capabilities
 ├── DiscoveryMetadata Discovery
 └── Register(ForgeBlockRegistry)

DiscoveryMetadata (record)
 ├── string DisplayName
 ├── string Description
 ├── string? Author
 ├── string? Website
 ├── IReadOnlyList<string>? Tags
 └── IReadOnlyList<string>? Categories

ForgeBlockCapability
ForgeBlockPackageDescriptor
```

- **Soubory:** `Src/MetaForge.Core/ForgeBlockPackages/`

---

## 11. Hierarchie — Kompletní graf

```
AppRoot
 └── List<ProjectElement> Projects
      └── List<RootElement> RootElements
           │
           ├── ClassElement : RootElement
           │    ├── List<PropertyElement> Properties
           │    │    └── TypeModel Type
           │    │         ├── DataType BaseType
           │    │         └── List<TypeModel> GenericArguments
           │    ├── List<MethodElement> Methods
           │    │    ├── TypeModel ReturnType
           │    │    ├── List<ParameterElement> Parameters
           │    │    │    └── TypeModel Type
           │    │    └── List<AttributeElement> Attributes
           │    └── List<AttributeElement> Attributes
           │
           ├── InterfaceElement : RootElement
           │    ├── List<PropertyElement> Properties
           │    ├── List<MethodElement> Methods
           │    └── List<AttributeElement> Attributes
           │
           ├── EnumElement : RootElement
           │    ├── DataType UnderlyingType
           │    ├── List<EnumMemberElement> Members
           │    │    ├── object? Value
           │    │    └── List<AttributeElement> Attributes
           │    └── List<AttributeElement> Attributes
           │
           └── StructElement : RootElement
                ├── List<PropertyElement> Properties
                ├── List<MethodElement> Methods
                └── List<AttributeElement> Attributes
```

---

## 12. Coin systém — Propagace ceny

```
AppRoot.TotalCoin
 └── ∑ ProjectElement.RootElements.Sum(e.TotalCoin)
      ├── ClassElement.TotalCoin = Coin + ∑Properties.Coin + ∑Methods.TotalCoin
      │                                              └── Coin + ∑Parameters.Coin
      ├── InterfaceElement.TotalCoin = Coin + ∑Properties.Coin + ∑Methods.TotalCoin
      ├── EnumElement.TotalCoin = Coin + ∑Members.Coin
      └── StructElement.TotalCoin = Coin + ∑Properties.Coin + ∑Methods.TotalCoin
```

---

## 13. Adresářová struktura

```
Src/MetaForge.Core/
 ├── Abstractions/
 │    ├── AccessModifier.cs
 │    ├── AppRoot.cs
 │    ├── AttributeElement.cs
 │    ├── ProjectElement.cs
 │    ├── RootElement.cs
 │    └── SemanticCollection.cs
 │
 ├── Catalog/
 │    ├── BuiltInCatalogProvider.cs
 │    ├── CatalogManager.cs
 │    ├── ICatalogProvider.cs
 │    └── PresetDefinition.cs
 │
 ├── DataTypes/
 │    ├── DataType.cs
 │    └── TypeModel.cs
 │
 ├── Elements/
 │    ├── Expressions/
 │    │    ├── ComputedExpression.cs
 │    │    ├── ComputedOperation.cs
 │    │    └── Expression.cs
 │    ├── Members/
 │    │    ├── MethodElement.cs
 │    │    ├── ParameterElement.cs
 │    │    └── PropertyElement.cs
 │    └── Types/
 │         ├── ClassElement.cs
 │         ├── EnumElement.cs
 │         ├── EnumMemberElement.cs
 │         ├── InterfaceElement.cs
 │         └── StructElement.cs
 │
 ├── ForgeBlockPackages/
 │    ├── DiscoveryMetadata.cs
 │    ├── ForgeBlockCapability.cs
 │    ├── ForgeBlockPackageDescriptor.cs
 │    ├── ForgeBlockRegistry.cs
 │    ├── IForgeBlockCapabilityPackage.cs
 │    └── IForgeBlockPackage.cs
 │
 ├── Inference/
 │    ├── IConstraintInferencer.cs
 │    └── RuleBasedConstraintInferencer.cs
 │
 ├── StandardLibraries/
 │    ├── IStandardLibraryTranslator.cs
 │    ├── IStandardLibraryTranslatorRegistry.cs
 │    ├── StandardLibraryRequirementResolver.cs
 │    ├── StandardLibraryRequirements.cs
 │    └── StandardLibraryTranslatorRegistry.cs
 │
 └── ValueObjects/
      ├── ConversionOptions.cs
      ├── StrongType.cs
      └── ValueObjectValidationRule.cs
```
