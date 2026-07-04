# Epic 2 — Core vrstva

> **Cíl:** Vytvořit projekt `MetaForge.Core` s kompletním typovým modelem, elementy, katalogem, inference a ValueObjects.
> **Výstup:** Plně funkční Core knihovna bez závislostí na vyšších vrstvách.
> **Závislosti:** Epic 1 (solution existuje).

---

## TASK-2.1.1 — Založení projektu MetaForge.Core

**Vstup:** `MetaForge.slnx` existuje.
**Výstup:** Nový class library projekt `Src/MetaForge.Core/MetaForge.Core.csproj` přidaný do solution.
**Soubory:** `Src/MetaForge.Core/MetaForge.Core.csproj`, `MetaForge.slnx`

**Kód — vytvoř `MetaForge.Core.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Core</RootNamespace>
  </PropertyGroup>
</Project>
```

**Krok 2 — Aktualizuj `MetaForge.slnx`:**

```xml
<Solution>
  <Folder Name="/Src/">
    <Project Path="Src/MetaForge.Core/MetaForge.Core.csproj" />
  </Folder>
  <Folder Name="/Tests/">
  </Folder>
</Solution>
```

**Ověření:** `dotnet build Src/MetaForge.Core/MetaForge.Core.csproj` projde bez chyb.
**Riziko:** Nízké.
**Rollback:** Odeber `<Project>` řádek ze slnx, smaž složku `Src/MetaForge.Core/`.

---

## TASK-2.2.1 — DataType enum (32 C# typů)

**Vstup:** Projekt `MetaForge.Core` existuje.
**Výstup:** Soubor `Src/MetaForge.Core/DataTypes/DataType.cs`.
**Soubory:** `Src/MetaForge.Core/DataTypes/DataType.cs`

**Kód — vytvoř přesně:**

```csharp
namespace MetaForge.Core.DataTypes;

/// <summary>
/// Výčet 32 datových typů mapovaných na C# typy.
/// Používá se v TypeModel jako základní typová informace.
/// </summary>
public enum DataType : int
{
    // === Číselné ===
    Bool,       // System.Boolean
    Byte,       // System.Byte
    SByte,      // System.SByte
    Int16,      // System.Int16
    UInt16,     // System.UInt16
    Int32,      // System.Int32
    UInt32,     // System.UInt32
    Int64,      // System.Int64
    UInt64,     // System.UInt64
    Int128,     // System.Int128
    Half,       // System.Half
    Single,     // System.Single
    Double,     // System.Double
    Decimal,    // System.Decimal
    NInt,       // System.IntPtr
    NUInt,      // System.UIntPtr

    // === Textové ===
    Char,       // System.Char
    String,     // System.String

    // === Binární ===
    Binary,     // System.Byte[]

    // === Časové ===
    DateOnly,   // System.DateOnly
    TimeOnly,   // System.TimeOnly
    DateTime,   // System.DateTime
    DateTimeOffset,
    TimeSpan,

    // === Speciální ===
    Guid,
    Uri,
    Version,

    // === Placeholder pro komplexní typy ===
    Entity,     // Odkaz na jinou entitu
    EnumValue,  // Odkaz na hodnotu enumu
    Object,     // System.Object — fallback
    Dynamic,    // dynamic — otevřený typ
    Void,       // System.Void
    Array,
    Nullable,
    Struct,
    Record,
}
```

**Ověření:** `dotnet build` projde. Enum obsahuje přesně 32 hodnot.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-2.2.2 — TypeModel sealed record

**Vstup:** TASK-2.2.1 (DataType existuje).
**Výstup:** Soubor `Src/MetaForge.Core/DataTypes/TypeModel.cs`.
**Soubory:** `Src/MetaForge.Core/DataTypes/TypeModel.cs`

**Kód — vytvoř přesně:**

```csharp
namespace MetaForge.Core.DataTypes;

/// <summary>
/// Immutable popis typu — základní stavební kámen typového modelu.
/// Kombinuje BaseType, nullable, kolekci, custom název a generické argumenty.
/// </summary>
public sealed record TypeModel
{
    /// <summary>Základní datový typ.</summary>
    public DataType BaseType { get; init; }

    /// <summary>Je typ nullable?</summary>
    public bool IsNullable { get; init; }

    /// <summary>Je typ kolekce (List, Array, IEnumerable)?</summary>
    public bool IsCollection { get; init; }

    /// <summary>Vlastní název typu (pro Entity, EnumValue, Struct, Record).</summary>
    public string? CustomTypeName { get; init; }

    /// <summary>Generické argumenty (např. List&lt;T&gt; má jeden GenericArgument T).</summary>
    public List<TypeModel> GenericArguments { get; init; } = [];

    /// <summary>Je to void (bez návratové hodnoty)?</summary>
    public bool IsVoid => BaseType == DataType.Void
                          && !IsNullable && !IsCollection
                          && GenericArguments.Count == 0;

    // === Factory metody pro často používané typy ===

    public static TypeModel Void { get; } = new() { BaseType = DataType.Void };
    public static TypeModel String { get; } = new() { BaseType = DataType.String };
    public static TypeModel Int32 { get; } = new() { BaseType = DataType.Int32 };
    public static TypeModel Bool { get; } = new() { BaseType = DataType.Bool };
    public static TypeModel Object { get; } = new() { BaseType = DataType.Object };
    public static TypeModel Decimal { get; } = new() { BaseType = DataType.Decimal };
    public static TypeModel Guid { get; } = new() { BaseType = DataType.Guid };
    public static TypeModel DateTime { get; } = new() { BaseType = DataType.DateTime };

    /// <summary>Vytvoří TypeModel s daným BaseType.</summary>
    public static TypeModel Of(DataType baseType) => new() { BaseType = baseType };

    /// <summary>Vytvoří nullable variantu tohoto typu.</summary>
    public TypeModel MakeNullable() => this with { IsNullable = true };

    /// <summary>Vytvoří kolekční variantu tohoto typu.</summary>
    public TypeModel MakeCollection() => this with { IsCollection = true };

    /// <summary>Nastaví vlastní název typu.</summary>
    public TypeModel WithCustomName(string name) => this with { CustomTypeName = name };

    /// <summary>Přidá generický argument.</summary>
    public TypeModel WithGenericArg(TypeModel arg) => this with
    {
        GenericArguments = [..GenericArguments, arg]
    };
}
```

**Ověření:** `dotnet build` projde. Record lze vytvořit: `TypeModel.String`, `TypeModel.Int32`, `TypeModel.Of(DataType.Guid)`.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-2.1.2 — AccessModifier enum, AppRoot, ProjectElement, RootElement, AttributeElement, SemanticCollection

**Vstup:** TASK-2.1.1 (projekt existuje).
**Výstup:** 6 souborů v `Abstractions/`.
**Soubory:**
- `Src/MetaForge.Core/Abstractions/AccessModifier.cs`
- `Src/MetaForge.Core/Abstractions/AppRoot.cs`
- `Src/MetaForge.Core/Abstractions/ProjectElement.cs`
- `Src/MetaForge.Core/Abstractions/RootElement.cs`
- `Src/MetaForge.Core/Abstractions/AttributeElement.cs`
- `Src/MetaForge.Core/Abstractions/SemanticCollection.cs`

**Kód — `AccessModifier.cs`:**

```csharp
namespace MetaForge.Core.Abstractions;

/// <summary>
/// Modifikátory přístupu pro typy a členy.
/// </summary>
public enum AccessModifier
{
    Public,
    Internal,
    Protected,
    Private,
    ProtectedInternal,
    PrivateProtected,
}
```

**Kód — `AppRoot.cs`:**

```csharp
namespace MetaForge.Core.Abstractions;

/// <summary>
/// Vstupní bod dokumentu — obsahuje projekty.
/// Celková cena exportu = suma Coinů všech elementů.
/// </summary>
public sealed class AppRoot
{
    /// <summary>Seznam projektů v solution.</summary>
    public List<ProjectElement> Projects { get; } = new();

    /// <summary>Celková cena exportu v kreditech.</summary>
    public int TotalCoin =>
        Projects.Sum(p => p.RootElements.Sum(e => e.TotalCoin));
}
```

**Kód — `ProjectElement.cs`:**

```csharp
namespace MetaForge.Core.Abstractions;

/// <summary>
/// Reprezentuje jeden projekt v solution.
/// Obsahuje RootElementy (třídy, interfacy, enumy, struktury).
/// </summary>
public sealed class ProjectElement
{
    /// <summary>Název projektu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Výchozí namespace projektu.</summary>
    public string? DefaultNamespace { get; set; }

    /// <summary>Top-level elementy v projektu.</summary>
    public List<RootElement> RootElements { get; } = new();
}
```

**Kód — `RootElement.cs`:**

```csharp
namespace MetaForge.Core.Abstractions;

/// <summary>
/// Bázová třída pro top-level deklarace (Class, Interface, Enum, Struct).
/// Nese Id, název, usingy, atributy a kreditovou cenu.
/// </summary>
public abstract class RootElement
{
    /// <summary>Unikátní identifikátor elementu.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Název elementu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Druh elementu — implementuje potomek.</summary>
    public abstract string Kind { get; }

    /// <summary>Using direktivy potřebné pro tento element.</summary>
    public List<string> Usings { get; } = new();

    /// <summary>Atributy na tomto elementu.</summary>
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Cena elementu v kreditech. Výchozí dle typu.</summary>
    public int Coin { get; set; }

    /// <summary>Coin tohoto elementu + children. Přetěžují potomci.</summary>
    public virtual int TotalCoin => Coin;
}
```

**Kód — `AttributeElement.cs`:**

```csharp
namespace MetaForge.Core.Abstractions;

/// <summary>
/// Reprezentuje C# atribut — název a argumenty.
/// </summary>
public sealed class AttributeElement
{
    /// <summary>Název atributu (např. "Obsolete", "JsonProperty").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Argumenty atributu — mohou být null pro bezparametrové atributy.</summary>
    public List<object?> Arguments { get; } = new();
}
```

**Kód — `SemanticCollection.cs`:**

```csharp
namespace MetaForge.Core.Abstractions;

/// <summary>
/// Kolekce child elementů s notifikací o změnách.
/// Používá se pro Properties, Methods, Members — kdekoliv kde child elementy ovlivňují TotalCoin.
/// </summary>
public class SemanticCollection<T> : List<T>
{
    /// <summary>Událost vyvolaná při jakékoliv změně kolekce.</summary>
    public event Action? Changed;

    /// <summary>Přidá prvek a vyvolá Changed.</summary>
    public new void Add(T item)
    {
        base.Add(item);
        Changed?.Invoke();
    }

    /// <summary>Odebere prvek a vyvolá Changed.</summary>
    public new void Remove(T item)
    {
        base.Remove(item);
        Changed?.Invoke();
    }

    /// <summary>Vyčistí kolekci a vyvolá Changed.</summary>
    public new void Clear()
    {
        base.Clear();
        Changed?.Invoke();
    }
}
```

**Ověření:** `dotnet build` projde. Všechny třídy jsou v namespace `MetaForge.Core.Abstractions`.
**Riziko:** Nízké.
**Rollback:** Smaž všech 6 souborů.

---

## TASK-2.3.1 — ClassElement, InterfaceElement, EnumElement, StructElement, EnumMemberElement

**Vstup:** TASK-2.1.2 (RootElement, AccessModifier, AttributeElement existují).
**Výstup:** 5 souborů v `Elements/Types/`.
**Soubory:**
- `Src/MetaForge.Core/Elements/Types/ClassElement.cs`
- `Src/MetaForge.Core/Elements/Types/InterfaceElement.cs`
- `Src/MetaForge.Core/Elements/Types/EnumElement.cs`
- `Src/MetaForge.Core/Elements/Types/EnumMemberElement.cs`
- `Src/MetaForge.Core/Elements/Types/StructElement.cs`

**POZNÁMKA:** Všechny elementy používají `List<PropertyElement>` a `List<MethodElement>`. Tyto třídy zatím neexistují — vytvoří se v TASK-2.3.2 a TASK-2.3.3. Použij forward reference — PropertyElement a MethodElement budou v namespace `MetaForge.Core.Elements.Members`.

**Kód — `ClassElement.cs`:**

```csharp
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# třídu — dědí z RootElement.
/// </summary>
public sealed class ClassElement : RootElement
{
    public override string Kind => "class";

    /// <summary>Název bázové třídy (pokud dědí).</summary>
    public string? BaseClassName { get; set; }

    /// <summary>Seznam implementovaných interfaců.</summary>
    public List<string> ImplementedInterfaces { get; } = new();

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public bool IsStatic { get; set; }
    public bool IsPartial { get; set; }

    /// <summary>Vlastnosti (property) třídy.</summary>
    public List<PropertyElement> Properties { get; } = new();

    /// <summary>Metody třídy.</summary>
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
```

**Kód — `InterfaceElement.cs`:**

```csharp
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# interface.
/// </summary>
public sealed class InterfaceElement : RootElement
{
    public override string Kind => "interface";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
```

**Kód — `EnumElement.cs`:**

```csharp
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# enum.
/// </summary>
public sealed class EnumElement : RootElement
{
    public override string Kind => "enum";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>Podkladový typ enumu (výchozí Int32).</summary>
    public DataType UnderlyingType { get; set; } = DataType.Int32;

    /// <summary>Má atribut [Flags]?</summary>
    public bool IsFlags { get; set; }

    /// <summary>Členové enumu.</summary>
    public List<EnumMemberElement> Members { get; } = new();

    public override int TotalCoin => Coin + Members.Sum(m => m.Coin);
}
```

**Kód — `EnumMemberElement.cs`:**

```csharp
using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Jeden člen enumu — název a volitelná hodnota.
/// </summary>
public sealed class EnumMemberElement
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Explicitní hodnota (null = automatická).</summary>
    public object? Value { get; set; }

    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 1;
}
```

**Kód — `StructElement.cs`:**

```csharp
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# struct (včetně record struct).
/// </summary>
public sealed class StructElement : RootElement
{
    public override string Kind => "struct";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsReadOnly { get; set; }
    public bool IsRecord { get; set; }

    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
```

**Ověření:** `dotnet build` **NESELŽE**, i když PropertyElement a MethodElement ještě neexistují — C# kompilátor to dovolí, pokud jsou ve stejném assembly. Pokud selže, vytvoř nejprve stub soubory pro PropertyElement a MethodElement (viz další task).
**Riziko:** Střední — závislost na PropertyElement a MethodElement.
**Rollback:** Smaž všech 5 souborů.

---

## TASK-2.3.2 — PropertyElement

**Vstup:** TypeModel existuje (TASK-2.2.2).
**Výstup:** Soubor `Src/MetaForge.Core/Elements/Members/PropertyElement.cs`.
**Soubory:** `Src/MetaForge.Core/Elements/Members/PropertyElement.cs`

**Kód — vytvoř přesně:**

```csharp
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Reprezentuje C# property (vlastnost) na třídě, interfacu nebo structu.
/// </summary>
public sealed class PropertyElement
{
    /// <summary>Název property.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Datový typ property.</summary>
    public TypeModel Type { get; set; } = TypeModel.Object;

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool HasGetter { get; set; } = true;
    public bool HasSetter { get; set; } = true;
    public bool IsInitOnly { get; set; }
    public bool IsRequired { get; set; }
    public bool IsStatic { get; set; }

    /// <summary>Výchozí hodnota jako string (např. "0", "null", "\"hello\"").</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 2;
}
```

**Ověření:** `dotnet build` projde. Všechny třídy z TASK-2.3.1 nyní kompilují.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-2.3.3 — MethodElement, ParameterElement, ParameterModifier

**Vstup:** TypeModel existuje (TASK-2.2.2).
**Výstup:** 2 soubory v `Elements/Members/`.
**Soubory:**
- `Src/MetaForge.Core/Elements/Members/MethodElement.cs`
- `Src/MetaForge.Core/Elements/Members/ParameterElement.cs`

**Kód — `MethodElement.cs`:**

```csharp
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Reprezentuje C# metodu na třídě, interfacu nebo structu.
/// </summary>
public sealed class MethodElement
{
    /// <summary>Název metody.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Návratový typ (výchozí void).</summary>
    public TypeModel ReturnType { get; set; } = TypeModel.Void;

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsStatic { get; set; }
    public bool IsAsync { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }

    /// <summary>Parametry metody.</summary>
    public List<ParameterElement> Parameters { get; } = new();

    /// <summary>Atributy na metodě.</summary>
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Tělo metody jako string (volitelné — pro codegen).</summary>
    public string? Body { get; set; }

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 5;

    /// <summary>Celková cena včetně parametrů.</summary>
    public int TotalCoin => Coin + Parameters.Sum(p => p.Coin);
}
```

**Kód — `ParameterElement.cs`:**

```csharp
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Elements.Members;

/// <summary>
/// Reprezentuje parametr metody.
/// </summary>
public sealed class ParameterElement
{
    /// <summary>Název parametru.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Datový typ parametru.</summary>
    public TypeModel Type { get; set; } = TypeModel.Object;

    /// <summary>Má parametr výchozí hodnotu?</summary>
    public bool HasDefaultValue { get; set; }

    /// <summary>Výchozí hodnota jako string.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Modifikátor parametru (ref, out, in, params).</summary>
    public ParameterModifier Modifier { get; set; } = ParameterModifier.None;

    /// <summary>Cena v kreditech.</summary>
    public int Coin { get; set; } = 1;
}

/// <summary>
/// Modifikátor parametru metody.
/// </summary>
public enum ParameterModifier
{
    None,
    Ref,
    Out,
    In,
    Params,
}
```

**Ověření:** `dotnet build` projde. MethodElement.TotalCoin správně sčítá Coin parametrů.
**Riziko:** Nízké.
**Rollback:** Smaž oba soubory.

---

## TASK-2.4.1 — CatalogManager + PresetDefinition + ICatalogProvider + BuiltInCatalogProvider

**Vstup:** TypeModel existuje (TASK-2.2.2).
**Výstup:** 4 soubory v `Catalog/`.
**Soubory:**
- `Src/MetaForge.Core/Catalog/PresetDefinition.cs`
- `Src/MetaForge.Core/Catalog/ICatalogProvider.cs`
- `Src/MetaForge.Core/Catalog/BuiltInCatalogProvider.cs`
- `Src/MetaForge.Core/Catalog/CatalogManager.cs`

**Kód — `PresetDefinition.cs`:**

```csharp
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Předdefinovaný typ — mapuje název na TypeModel.
/// Např. "Email" → TypeModel.String, "Price" → TypeModel.Decimal.
/// </summary>
public sealed record PresetDefinition(
    string Name,
    TypeModel Type,
    string? Description = null,
    IReadOnlyList<string>? Tags = null
);
```

**Kód — `ICatalogProvider.cs`:**

```csharp
namespace MetaForge.Core.Catalog;

/// <summary>
/// Poskytovatel presetů — umožňuje více zdrojů (built-in, filesystem, marketplace).
/// </summary>
public interface ICatalogProvider
{
    /// <summary>Název providera pro logování.</summary>
    string ProviderName { get; }

    /// <summary>Vrátí všechny presety z tohoto providera.</summary>
    IReadOnlyList<PresetDefinition> GetAllPresets();

    /// <summary>Vyhledá preset podle názvu. Vrací null pokud nenajde.</summary>
    PresetDefinition? ResolveType(string typeName);
}
```

**Kód — `BuiltInCatalogProvider.cs`:**

```csharp
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Catalog;

/// <summary>
/// Vestavěný katalog — obsahuje základní mapování běžných názvů na TypeModel.
/// </summary>
public sealed class BuiltInCatalogProvider : ICatalogProvider
{
    public string ProviderName => "BuiltIn";

    private readonly Dictionary<string, PresetDefinition> _presets;

    public BuiltInCatalogProvider()
    {
        _presets = new Dictionary<string, PresetDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            // Číselné
            ["int"] = new("int", TypeModel.Int32, "32bitové celé číslo"),
            ["long"] = new("long", TypeModel.Of(DataType.Int64), "64bitové celé číslo"),
            ["decimal"] = new("decimal", TypeModel.Decimal, "Desetinné číslo s pevnou řádovou čárkou"),
            ["double"] = new("double", TypeModel.Of(DataType.Double), "Desetinné číslo s plovoucí řádovou čárkou"),
            ["float"] = new("float", TypeModel.Of(DataType.Single), "32bitové desetinné číslo"),

            // Textové
            ["string"] = new("string", TypeModel.String, "Textový řetězec"),
            ["text"] = new("text", TypeModel.String, "Textový řetězec (alias)"),

            // Logické
            ["bool"] = new("bool", TypeModel.Bool, "Pravdivostní hodnota"),
            ["boolean"] = new("boolean", TypeModel.Bool, "Pravdivostní hodnota (alias)"),

            // Časové
            ["datetime"] = new("datetime", TypeModel.DateTime, "Datum a čas"),
            ["date"] = new("date", TypeModel.Of(DataType.DateOnly), "Pouze datum"),
            ["time"] = new("time", TypeModel.Of(DataType.TimeOnly), "Pouze čas"),

            // Speciální
            ["guid"] = new("guid", TypeModel.Guid, "Globálně unikátní identifikátor"),
            ["uuid"] = new("uuid", TypeModel.Guid, "Globálně unikátní identifikátor (alias)"),
            ["email"] = new("email", TypeModel.String, "Emailová adresa", new[] { "contact", "validation" }),
            ["phone"] = new("phone", TypeModel.String, "Telefonní číslo", new[] { "contact" }),
            ["url"] = new("url", TypeModel.Of(DataType.Uri), "URL adresa"),
            ["uri"] = new("uri", TypeModel.Of(DataType.Uri), "URI adresa (alias)"),
            ["money"] = new("money", TypeModel.Decimal, "Peněžní částka", new[] { "finance" }),
            ["price"] = new("price", TypeModel.Decimal, "Cena", new[] { "finance" }),
        };
    }

    public IReadOnlyList<PresetDefinition> GetAllPresets() =>
        _presets.Values.ToList().AsReadOnly();

    public PresetDefinition? ResolveType(string typeName) =>
        _presets.TryGetValue(typeName, out var preset) ? preset : null;
}
```

**Kód — `CatalogManager.cs`:**

```csharp
namespace MetaForge.Core.Catalog;

/// <summary>
/// Centrální správce katalogu — agreguje všechny ICatalogProvider.
/// Registrace presetů, vyhledávání, resolve typů.
/// </summary>
public sealed class CatalogManager
{
    private readonly List<ICatalogProvider> _providers = new();
    private readonly Dictionary<string, PresetDefinition> _customPresets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Zaregistruje catalog providera (built-in, filesystem, marketplace).</summary>
    public void RegisterProvider(ICatalogProvider provider)
    {
        _providers.Add(provider);
    }

    /// <summary>Zaregistruje vlastní preset (z ForgeBlocku nebo uživatele).</summary>
    public void RegisterPreset(PresetDefinition preset)
    {
        _customPresets[preset.Name] = preset;
    }

    /// <summary>Vyhledá typ podle názvu — prohledá custom presety, pak providery.</summary>
    public PresetDefinition? ResolveType(string typeName)
    {
        // 1. Vlastní presety
        if (_customPresets.TryGetValue(typeName, out var custom))
            return custom;

        // 2. Providery v pořadí registrace
        foreach (var provider in _providers)
        {
            var result = provider.ResolveType(typeName);
            if (result is not null)
                return result;
        }

        return null;
    }

    /// <summary>Vyhledá presety podle dotazu (hledá v názvu a tazích).</summary>
    public IReadOnlyList<PresetDefinition> SearchPresets(string query)
    {
        var results = new List<PresetDefinition>();

        // Z custom presetů
        results.AddRange(_customPresets.Values
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                        || (p.Tags?.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)) ?? false)));

        // Z providerů
        foreach (var provider in _providers)
        {
            results.AddRange(provider.GetAllPresets()
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                            || (p.Tags?.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)) ?? false)));
        }

        return results.DistinctBy(p => p.Name).ToList().AsReadOnly();
    }

    /// <summary>Vrátí všechny dostupné presety.</summary>
    public IReadOnlyList<PresetDefinition> GetAllPresets()
    {
        var all = new List<PresetDefinition>();
        all.AddRange(_customPresets.Values);

        foreach (var provider in _providers)
            all.AddRange(provider.GetAllPresets());

        return all.DistinctBy(p => p.Name).ToList().AsReadOnly();
    }
}
```

**Ověření:** `dotnet build` projde. CatalogManager má metody ResolveType, SearchPresets, GetAllPresets, RegisterPreset, RegisterProvider.
**Riziko:** Střední — CatalogManager musí být thread-safe pro Singleton použití? Pro teď ne — přidá se později pokud třeba.
**Rollback:** Smaž všechny 4 soubory.

---

## TASK-2.5.1 — ForgeBlock registrační infrastruktura

**Vstup:** Projekt existuje.
**Výstup:** 6 souborů v `ForgeBlockPackages/`.
**Soubory:**
- `Src/MetaForge.Core/ForgeBlockPackages/ForgeBlockCapability.cs`
- `Src/MetaForge.Core/ForgeBlockPackages/DiscoveryMetadata.cs`
- `Src/MetaForge.Core/ForgeBlockPackages/IForgeBlockPackage.cs`
- `Src/MetaForge.Core/ForgeBlockPackages/IForgeBlockCapabilityPackage.cs`
- `Src/MetaForge.Core/ForgeBlockPackages/ForgeBlockPackageDescriptor.cs`
- `Src/MetaForge.Core/ForgeBlockPackages/ForgeBlockRegistry.cs`

**Kód — `ForgeBlockCapability.cs`:**

```csharp
namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Jedna capability (schopnost) ForgeBlocku — co umí poskytnout.
/// </summary>
public sealed record ForgeBlockCapability(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<string>? Tags = null
);
```

**Kód — `DiscoveryMetadata.cs`:**

```csharp
namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Federovaná discovery metadata — každý ForgeBlock nese vlastní.
/// </summary>
public sealed record DiscoveryMetadata(
    string DisplayName,
    string Description,
    string? Author = null,
    string? Website = null,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyList<string>? Categories = null
);
```

**Kód — `IForgeBlockPackage.cs`:**

```csharp
namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Minimální kontrakt pro ForgeBlock balík.
/// Každý ForgeBlock musí mít Handle, Version, Capabilities a Discovery metadata.
/// </summary>
public interface IForgeBlockPackage
{
    /// <summary>Unikátní identifikátor (např. "math", "string", "validation").</summary>
    string Handle { get; }

    /// <summary>Sémantická verze.</summary>
    string Version { get; }

    /// <summary>Seznam capabilities, které balík poskytuje.</summary>
    IReadOnlyList<ForgeBlockCapability> Capabilities { get; }

    /// <summary>Discovery metadata pro katalog.</summary>
    DiscoveryMetadata Discovery { get; }

    /// <summary>Zaregistruje balík do registru.</summary>
    void Register(ForgeBlockRegistry registry);
}
```

**Kód — `IForgeBlockCapabilityPackage.cs`:**

```csharp
namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Rozšířený kontrakt pro ForgeBlocky s catalog entries.
/// </summary>
public interface IForgeBlockCapabilityPackage : IForgeBlockPackage
{
    /// <summary>Descriptor balíku.</summary>
    ForgeBlockPackageDescriptor Descriptor { get; }

    /// <summary>Catalog entries — typy/operace které balík přidává do katalogu.</summary>
    IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; }
}
```

**Kód — `ForgeBlockPackageDescriptor.cs`:**

```csharp
namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Popisovač ForgeBlock balíku pro registraci.
/// </summary>
public sealed record ForgeBlockPackageDescriptor(
    string Handle,
    string Version,
    string DisplayName,
    string Description
);

/// <summary>
/// Popisovač jedné catalog entry z ForgeBlocku.
/// </summary>
public sealed record ForgeBlockCatalogEntryDescriptor(
    string Name,
    string TypeName,
    string? Description = null,
    IReadOnlyList<string>? Tags = null
);
```

**Kód — `ForgeBlockRegistry.cs`:**

```csharp
namespace MetaForge.Core.ForgeBlockPackages;

/// <summary>
/// Centrální registr ForgeBlock balíků.
/// Spravuje registraci, duplicitu a dotazování.
/// </summary>
public sealed class ForgeBlockRegistry
{
    private readonly Dictionary<string, IForgeBlockPackage> _packages = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Všechny registrované balíky.</summary>
    public IReadOnlyList<IForgeBlockPackage> Packages => _packages.Values.ToList().AsReadOnly();

    /// <summary>Zaregistruje ForgeBlock balík.</summary>
    /// <exception cref="InvalidOperationException">Pokud handle již existuje.</exception>
    public void Register(IForgeBlockPackage package)
    {
        if (_packages.ContainsKey(package.Handle))
            throw new InvalidOperationException(
                $"ForgeBlock s handle '{package.Handle}' je již zaregistrován.");

        _packages[package.Handle] = package;
        package.Register(this);
    }

    /// <summary>Najde balík podle handle. Vrací null pokud nenajde.</summary>
    public IForgeBlockPackage? GetPackage(string handle) =>
        _packages.TryGetValue(handle, out var package) ? package : null;

    /// <summary>Vyhledá balíky podle tagu.</summary>
    public IReadOnlyList<IForgeBlockPackage> SearchByTag(string tag) =>
        _packages.Values
            .Where(p => p.Discovery.Tags?.Contains(tag, StringComparer.OrdinalIgnoreCase) == true)
            .ToList()
            .AsReadOnly();

    /// <summary>Vrátí všechny capability napříč všemi balíky.</summary>
    public IReadOnlyList<ForgeBlockCapability> GetAllCapabilities() =>
        _packages.Values
            .SelectMany(p => p.Capabilities)
            .ToList()
            .AsReadOnly();
}
```

**Ověření:** `dotnet build` projde. ForgeBlockRegistry.Register vyhazuje výjimku při duplicitním handle.
**Riziko:** Nízké.
**Rollback:** Smaž všech 6 souborů.

---

## TASK-2.11.1 — StrongType + ValueObjectValidationRule + ConversionOptions

**Vstup:** TypeModel existuje (TASK-2.2.2).
**Výstup:** 3 soubory v `ValueObjects/`.
**Soubory:**
- `Src/MetaForge.Core/ValueObjects/StrongType.cs`
- `Src/MetaForge.Core/ValueObjects/ValueObjectValidationRule.cs`
- `Src/MetaForge.Core/ValueObjects/ConversionOptions.cs`

**Kód — `StrongType.cs`:**

```csharp
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Silně typovaná doménová hodnota — obaluje TypeModel s pojmenovaným typem.
/// Např. "Email" → TypeModel.String s validačními pravidly.
/// </summary>
public sealed record StrongType(
    string Name,
    TypeModel Underlying,
    IReadOnlyList<ValueObjectValidationRule>? ValidationRules = null,
    ConversionOptions? Conversion = null
);
```

**Kód — `ValueObjectValidationRule.cs`:**

```csharp
namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Validační pravidlo pro StrongType.
/// </summary>
public sealed record ValueObjectValidationRule(
    string RuleName,
    string? Parameter = null,
    string? ErrorMessage = null
);
```

**Kód — `ConversionOptions.cs`:**

```csharp
namespace MetaForge.Core.ValueObjects;

/// <summary>
/// Možnosti konverze pro StrongType (např. implicitní/explicitní operátory).
/// </summary>
public sealed record ConversionOptions(
    bool GenerateImplicitConversion = false,
    bool GenerateExplicitConversion = false,
    bool GenerateToString = true,
    bool GenerateEquals = true,
    bool GenerateGetHashCode = true
);
```

**Ověření:** `dotnet build` projde. StrongType lze vytvořit: `new StrongType("Email", TypeModel.String)`.
**Riziko:** Nízké.
**Rollback:** Smaž všechny 3 soubory.

---

## TASK-2.8.1 — Expression, ComputedExpression, ComputedOperation

**Vstup:** Projekt existuje.
**Výstup:** 3 soubory v `Elements/Expressions/`.
**Soubory:**
- `Src/MetaForge.Core/Elements/Expressions/Expression.cs`
- `Src/MetaForge.Core/Elements/Expressions/ComputedExpression.cs`
- `Src/MetaForge.Core/Elements/Expressions/ComputedOperation.cs`

**Kód — `Expression.cs`:**

```csharp
namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Abstraktní bázová třída pro výrazy.
/// </summary>
public abstract class Expression
{
    /// <summary>Druh výrazu — implementuje potomek.</summary>
    public abstract string Kind { get; }
}
```

**Kód — `ComputedOperation.cs`:**

```csharp
namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Sémantická operace (např. Add, Concat, Compare).
/// </summary>
public sealed record ComputedOperation(
    string OperationId,
    string DisplayName,
    string? Description = null
);
```

**Kód — `ComputedExpression.cs`:**

```csharp
namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Výraz složený z operace a operandů — stromová struktura.
/// </summary>
public sealed class ComputedExpression : Expression
{
    public override string Kind => "Computed";

    /// <summary>Sémantická operace.</summary>
    public ComputedOperation Operation { get; set; } = new("identity", "Identita");

    /// <summary>Operandy — mohou být Expression nebo listy (konstanty, reference).</summary>
    public List<Expression> Operands { get; } = new();
}
```

**Ověření:** `dotnet build` projde. Lze vytvořit strom: `new ComputedExpression { Operation = new("add", "Sčítání") }`.
**Riziko:** Nízké.
**Rollback:** Smaž všechny 3 soubory.

---

## TASK-2.9.1 — IConstraintInferencer + RuleBasedConstraintInferencer

**Vstup:** TypeModel existuje (TASK-2.2.2).
**Výstup:** 2 soubory v `Inference/`.
**Soubory:**
- `Src/MetaForge.Core/Inference/IConstraintInferencer.cs`
- `Src/MetaForge.Core/Inference/RuleBasedConstraintInferencer.cs`

**Kód — `IConstraintInferencer.cs`:**

```csharp
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Inference;

/// <summary>
/// Inferuje constrainty (omezení) pro typ na základě názvu atributu.
/// Např. atribut "Email" → constraint ["email_format", "not_empty"].
/// </summary>
public interface IConstraintInferencer
{
    /// <summary>Odvodí constrainty pro daný název atributu a typ.</summary>
    IReadOnlyList<string> Infer(string attributeName, TypeModel type);
}
```

**Kód — `RuleBasedConstraintInferencer.cs`:**

```csharp
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Inference;

/// <summary>
/// Deterministická implementace inference constraintů pomocí pravidel.
/// </summary>
public sealed class RuleBasedConstraintInferencer : IConstraintInferencer
{
    private static readonly Dictionary<string, string[]> Rules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["email"] = new[] { "email_format", "not_empty", "max_length:254" },
        ["phone"] = new[] { "phone_format", "not_empty" },
        ["url"] = new[] { "url_format" },
        ["age"] = new[] { "range:0-150", "not_negative" },
        ["price"] = new[] { "not_negative", "decimal_places:2" },
        ["quantity"] = new[] { "not_negative", "integer" },
        ["name"] = new[] { "not_empty", "max_length:200" },
        ["description"] = new[] { "max_length:4000" },
        ["password"] = new[] { "min_length:8", "not_empty" },
        ["zipcode"] = new[] { "zip_format" },
        ["color"] = new[] { "hex_color_format" },
        ["percentage"] = new[] { "range:0-100" },
    };

    public IReadOnlyList<string> Infer(string attributeName, TypeModel type)
    {
        // Přesná shoda
        if (Rules.TryGetValue(attributeName, out var constraints))
            return constraints;

        // Prefixová shoda (např. "emailAddress" → "email")
        foreach (var (key, value) in Rules)
        {
            if (attributeName.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                return value;
        }

        // Inferuje podle typu
        if (type.BaseType == DataType.String && type.IsNullable == false)
            return new[] { "not_empty" };

        return Array.Empty<string>();
    }
}
```

**Ověření:** `dotnet build` projde. `Infer("Email", TypeModel.String)` vrací `["email_format", "not_empty", "max_length:254"]`. `Infer("Foo", TypeModel.String)` vrací `["not_empty"]`.
**Riziko:** Střední — pravidla musí být deterministická, žádné AI.
**Rollback:** Smaž oba soubory.

---

## TASK-2.10.1 — StandardLibrary — IStandardLibraryTranslator, Registry, Requirements, Resolver

**Vstup:** Projekt existuje.
**Výstup:** 5 souborů v `StandardLibraries/`.
**Soubory:**
- `Src/MetaForge.Core/StandardLibraries/IStandardLibraryTranslator.cs`
- `Src/MetaForge.Core/StandardLibraries/IStandardLibraryTranslatorRegistry.cs`
- `Src/MetaForge.Core/StandardLibraries/StandardLibraryTranslatorRegistry.cs`
- `Src/MetaForge.Core/StandardLibraries/StandardLibraryRequirements.cs`
- `Src/MetaForge.Core/StandardLibraries/StandardLibraryRequirementResolver.cs`

**Kód — `IStandardLibraryTranslator.cs`:**

```csharp
namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Překládá sémantickou operaci na požadavky standardní knihovny.
/// </summary>
public interface IStandardLibraryTranslator
{
    /// <summary>Identifikátor operace, kterou překladač obsluhuje.</summary>
    string OperationId { get; }

    /// <summary>Přeloží operaci na požadavky. Vrací null pokud operaci nerozumí.</summary>
    StandardLibraryRequirements? Translate(string operationId);
}
```

**Kód — `IStandardLibraryTranslatorRegistry.cs`:**

```csharp
namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Registr překladačů sémantických operací na standardní knihovnu.
/// </summary>
public interface IStandardLibraryTranslatorRegistry
{
    void Register(IStandardLibraryTranslator translator);
    IStandardLibraryTranslator? Resolve(string operationId);
    IReadOnlyList<IStandardLibraryTranslator> GetAll();
}
```

**Kód — `StandardLibraryTranslatorRegistry.cs`:**

```csharp
namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Výchozí implementace registru překladačů.
/// </summary>
public sealed class StandardLibraryTranslatorRegistry : IStandardLibraryTranslatorRegistry
{
    private readonly Dictionary<string, IStandardLibraryTranslator> _translators = new();

    public void Register(IStandardLibraryTranslator translator)
    {
        _translators[translator.OperationId] = translator;
    }

    public IStandardLibraryTranslator? Resolve(string operationId) =>
        _translators.TryGetValue(operationId, out var t) ? t : null;

    public IReadOnlyList<IStandardLibraryTranslator> GetAll() =>
        _translators.Values.ToList().AsReadOnly();
}
```

**Kód — `StandardLibraryRequirements.cs`:**

```csharp
namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Požadavky na standardní knihovnu pro danou operaci.
/// </summary>
public sealed record StandardLibraryRequirements(
    string OperationId,
    IReadOnlyList<string> RequiredNamespaces,
    IReadOnlyList<string>? RequiredPackages = null,
    string? CSharpExpressionTemplate = null
);
```

**Kód — `StandardLibraryRequirementResolver.cs`:**

```csharp
namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Resolver — pro daný element vrátí seznam potřebných using direktiv.
/// </summary>
public sealed class StandardLibraryRequirementResolver
{
    private readonly IStandardLibraryTranslatorRegistry _registry;

    public StandardLibraryRequirementResolver(IStandardLibraryTranslatorRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>Vyřeší požadavky pro danou operaci.</summary>
    public StandardLibraryRequirements? Resolve(string operationId)
    {
        var translator = _registry.Resolve(operationId);
        return translator?.Translate(operationId);
    }

    /// <summary>Vrátí všechny potřebné namespaces pro seznam operací.</summary>
    public IReadOnlyList<string> GetRequiredNamespaces(IEnumerable<string> operationIds)
    {
        var namespaces = new HashSet<string>();
        foreach (var opId in operationIds)
        {
            var req = Resolve(opId);
            if (req?.RequiredNamespaces is not null)
                foreach (var ns in req.RequiredNamespaces)
                    namespaces.Add(ns);
        }
        return namespaces.ToList().AsReadOnly();
    }
}
```

**Ověření:** `dotnet build` projde. Registry a resolver se dají použít.
**Riziko:** Nízké.
**Rollback:** Smaž všech 5 souborů.

---

## Souhrn Epic 2 — Co musí existovat po dokončení

```
Src/MetaForge.Core/
├── MetaForge.Core.csproj
├── Abstractions/
│   ├── AccessModifier.cs
│   ├── AppRoot.cs
│   ├── ProjectElement.cs
│   ├── RootElement.cs
│   ├── AttributeElement.cs
│   └── SemanticCollection.cs
├── DataTypes/
│   ├── DataType.cs              (32 hodnot)
│   └── TypeModel.cs             (sealed record, factory metody)
├── Elements/
│   ├── Types/
│   │   ├── ClassElement.cs
│   │   ├── InterfaceElement.cs
│   │   ├── EnumElement.cs
│   │   ├── EnumMemberElement.cs
│   │   └── StructElement.cs
│   ├── Members/
│   │   ├── PropertyElement.cs
│   │   ├── MethodElement.cs
│   │   └── ParameterElement.cs  (vč. ParameterModifier)
│   └── Expressions/
│       ├── Expression.cs
│       ├── ComputedExpression.cs
│       └── ComputedOperation.cs
├── Catalog/
│   ├── PresetDefinition.cs
│   ├── ICatalogProvider.cs
│   ├── BuiltInCatalogProvider.cs
│   └── CatalogManager.cs
├── ForgeBlockPackages/
│   ├── ForgeBlockCapability.cs
│   ├── DiscoveryMetadata.cs
│   ├── IForgeBlockPackage.cs
│   ├── IForgeBlockCapabilityPackage.cs
│   ├── ForgeBlockPackageDescriptor.cs
│   └── ForgeBlockRegistry.cs
├── ValueObjects/
│   ├── StrongType.cs
│   ├── ValueObjectValidationRule.cs
│   └── ConversionOptions.cs
├── Inference/
│   ├── IConstraintInferencer.cs
│   └── RuleBasedConstraintInferencer.cs
└── StandardLibraries/
    ├── IStandardLibraryTranslator.cs
    ├── IStandardLibraryTranslatorRegistry.cs
    ├── StandardLibraryTranslatorRegistry.cs
    ├── StandardLibraryRequirements.cs
    └── StandardLibraryRequirementResolver.cs
```

**Celkem souborů:** ~30
**Build:** `dotnet build Src/MetaForge.Core/` projde bez chyb.

**Checkpoint:** `git tag checkpoint/epic-2-done`
