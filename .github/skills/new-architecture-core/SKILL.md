---
name: new-architecture-core
description: "Pouzij pri: praci s Core vrstvou Nove Architektury — AppRoot, RootElement, DataType/TypeModel, ClassElement, PropertyElement, MethodElement, EnumElement, StructElement, Expression, IConstraintInferencer, IStandardLibraryTranslator, CatalogManager, ForgeBlockRegistry, Discovery, StrongType."
---

# new-architecture-core

Udržet změny v Core vrstvě bezpečné, konzistentní s C#-first architekturou a v souladu s dokumenty `03-Core-Abstractions.md`, `04-Core-Elements.md`, `05-Core-Behaviors.md` a `06-Core-Services.md`.

## Kdy použít

- Při práci se soubory v `Src/MetaForge.Core/`
- Při přidávání nebo změně Core abstrakcí, datových typů, elementů
- Při práci s CatalogManager, ForgeBlockRegistry, Discovery
- Při implementaci Expression, constraint inference, standard library translator

## Architektonické guardraily

| # | Invariant | Vysvětlení |
|---|-----------|------------|
| 1 | **C#-first** (ne jazykově agnostické) | Core může obsahovat C#-specifické typy — `DataType` enum obsahuje 32 C# typů |
| 2 | **AppRoot → ProjectElement → RootElement** | AppRoot je vstupní bod, obsahuje projekty, projekt obsahuje RootElement |
| 3 | **Core nesmí záviset na vyšších vrstvách** | Žádná reference na BusinessModel, Translator, Generators |
| 4 | **DataType je sealed enum** | Nikdy se nerozšiřuje děděním — nové typy se přidávají do enumu |
| 5 | **TypeModel je immutable record** | Všechny properties jsou init-only, používá factory metody |
| 6 | **RootElement je abstract** | Konkrétní elementy dědí (ClassElement, InterfaceElement atd.) |
| 7 | **Expression je otevřený k rozšíření** | Nové druhy výrazů se přidávají jako nové třídy dědící z `Expression` |

## Klíčové typy

### AppRoot a ProjectElement

```csharp
public sealed class AppRoot
{
    public List<ProjectElement> Projects { get; } = new();
}

public sealed class ProjectElement
{
    public string Name { get; set; } = string.Empty;
    public string? DefaultNamespace { get; set; }
    public List<RootElement> RootElements { get; } = new();
}
```

### DataType enum (32 C# typů)

Obsahuje: Bool, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Int128, Half, Single, Double, Decimal, NInt, NUInt, Char, String, Binary, DateOnly, TimeOnly, DateTime, DateTimeOffset, TimeSpan, Guid, Uri, Version, Entity, EnumValue, Object, Dynamic, Void, Array, Nullable, Struct, Record.

### TypeModel (factory metody)

```csharp
public sealed record TypeModel
{
    public DataType BaseType { get; init; }
    public bool IsNullable { get; init; }
    public bool IsCollection { get; init; }
    public string? CustomTypeName { get; init; }
    public List<TypeModel> GenericArguments { get; init; } = [];

    public static TypeModel Void { get; }
    public static TypeModel String { get; }
    public static TypeModel Int32 { get; }
    public static TypeModel Bool { get; }
    public static TypeModel Object { get; }
    public static TypeModel Decimal { get; }
    public static TypeModel Guid { get; }
    public static TypeModel DateTime { get; }

    public static TypeModel Of(DataType baseType);
    public TypeModel MakeNullable();
    public TypeModel MakeCollection();
    public TypeModel WithCustomName(string name);
    public TypeModel WithGenericArg(TypeModel arg);
}
```

### Core elementy

| Třída | Base | Klíčové properties |
|-------|------|--------------------|
| `ClassElement` | `RootElement` | Kind="class", BaseClassName, ImplementedInterfaces, AccessModifier, IsAbstract, IsSealed, IsStatic, IsPartial, Properties[], Methods[] |
| `InterfaceElement` | `RootElement` | Kind="interface", Properties[], Methods[] |
| `EnumElement` | `RootElement` | Kind="enum", UnderlyingType, IsFlags, Members[] |
| `StructElement` | `RootElement` | Kind="struct", IsReadOnly, IsRecord, Properties[], Methods[] |
| `PropertyElement` | — (member) | Name, Type(TypeModel), AccessModifier, HasGetter, HasSetter, IsInitOnly, IsRequired, IsStatic |
| `MethodElement` | — (member) | Name, ReturnType(TypeModel), Parameters[], IsAsync, IsAbstract, IsVirtual, IsOverride, IsStatic |

### Behaviors a Services

| Typ | Účel |
|-----|-------|
| `Expression` | Abstraktní báze pro výrazy v computed properties/behaviors |
| `ComputedExpression` | Expression s Operation + Operands |
| `IConstraintInferencer` | Inferuje constraints podle názvu atributu a typu |
| `IStandardLibraryTranslator` | Překládá sémantické operace na standardní knihovnu |
| `CatalogManager` | Registrace a resolve presetů, vyhledávání |
| `IForgeBlockPackage` | ForgeBlock kontrakt — Handle, Version, Capabilities, Discovery |
| `ForgeBlockRegistry` | Centrální registr ForgeBlock balíků |
| `StrongType` | record(Name, Underlying TypeModel, ValidationRules, Conversion) |

## Workflow

1. Identifikuj dotčenou Core oblast (abstrakce, elementy, datové typy, chování, služby)
2. Najdi odpovídající dokument v `New_Architecture/03-06`
3. Implementuj dle specifikace v dokumentu
4. Ověř architektonické guardraily
5. Přidej/uprav testy v `MetaForge.Core.Tests`

## Anti-patterny

- ❌ Přidávání C#-specifické logiky mimo Core
- ❌ Obcházení AppRoot → ProjectElement → RootElement hierarchie
- ❌ Mutace TypeModel po vytvoření (TypeModel je immutable)
- ❌ Přidání závislosti Core na vyšší vrstvě

## Výstupní checklist

- [ ] C#-first architektura je dodržena
- [ ] AppRoot → ProjectElement → RootElement hierarchie respektována
- [ ] Core nezávisí na vyšších vrstvách
- [ ] TypeModel používá factory metody
- [ ] DataType enum je použit
- [ ] Testy v MetaForge.Core.Tests jsou aktuální
