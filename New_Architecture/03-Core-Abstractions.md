# Core — Abstractions and Types

> AppRoot, ProjectElement, RootElement, TypeModel, DataType

---

## AppRoot — vstupní bod dokumentu

```csharp
public sealed class AppRoot
{
    public List<ProjectElement> Projects { get; } = new();

    /// <summary>Celková cena exportu = suma Coinů všech elementů v projektu.</summary>
    public int TotalCoin =>
        Projects.Sum(p => p.RootElements.Sum(e => e.TotalCoin));
}

public sealed class ProjectElement
{
    public string Name { get; set; } = string.Empty;
    public string? DefaultNamespace { get; set; }
    public List<RootElement> RootElements { get; } = new();
}
```

## RootElement — bázová třída pro top-level deklarace

```csharp
public abstract class RootElement
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public abstract string Kind { get; }

    /// <summary>Namespace elementu (C#-first). Null = global::.</summary>
    public string? Namespace { get; set; }

    /// <summary>XML documentation summary pro AI prompting a generování docs.</summary>
    public string? XmlSummary { get; set; }

    public List<string> Usings { get; } = new();
    public List<AttributeElement> Attributes { get; } = new();

    /// <summary>Cena elementu v kreditech. Výchozí dle typu, lze změnit v metaforge.coins.json.</summary>
    public int Coin { get; set; }

    /// <summary>Coin tohoto elementu + children. Přetěžují potomci.</summary>
    public virtual int TotalCoin => Coin;

    /// <summary>
    /// Univerzální key-value anotace (dokumentace, validace, generátorové hinty, AI kontext).
    /// Komplementární k Attributes — C# [Attribute] jde do Attributes, vše ostatní sem.
    /// </summary>
    public MetadataBag Metadata { get; init; } = new();

    // === Fluent setters ===
    // PROP-035: XmlSummary, Namespace
    // PROP-038: Metadata přístupné přes element.Metadata.Set(...)
}

public sealed class AttributeElement
{
    public string Name { get; set; } = string.Empty;
    public List<object?> Arguments { get; } = new();
}

// Konkrétní elementy viz 04-Core-Elements.md
```

## MetadataBag — univerzální anotační systém (PROP-038)

```csharp
// Komplementární k AttributeElement (C#-specific [Attribute]).
// Každý element má MetadataBag pro key-value anotace.
// Standardizované klíče: Validation.*, Docs.*, Generation.*, Ai.*, Domain.*

public enum MetadataScope { Domain, Validation, Generation, Ai, Documentation }
public enum MergeStrategy { Override, Skip, Throw }

public sealed record MetadataEntry(string Key, object? Value, MetadataScope Scope);

public sealed class MetadataBag
{
    public MetadataBag Set<T>(string key, T value, MetadataScope scope = MetadataScope.Domain);
    public T? Get<T>(string key);
    public bool Has(string key);
    public MetadataBag Merge(MetadataBag other, MergeStrategy strategy = MergeStrategy.Override);

    public static class Keys
    {
        // === Validation ===
        public const string ValidationRequired = "Validation.Required";
        public const string ValidationMinLength = "Validation.MinLength";
        public const string ValidationMaxLength = "Validation.MaxLength";
        public const string ValidationRangeMin = "Validation.Range.Min";
        public const string ValidationRangeMax = "Validation.Range.Max";

        // === Documentation ===
        public const string DocsSummary = "Docs.Summary";
        public const string DocsReturns = "Docs.Returns";
        public const string DocsRemarks = "Docs.Remarks";

        // === Generation ===
        public const string GenerationIgnore = "Generation.Ignore";
        public const string GenerationUsePartial = "Generation.UsePartial";
        public const string GenerationJsonIgnore = "Generation.JsonIgnore";

        // === AI ===
        public const string AiContext = "Ai.Context";
        public const string AiExample = "Ai.Example";

        // === Domain ===
        public const string DomainBusinessName = "Domain.BusinessName";
        public const string DomainGlossary = "Domain.Glossary";
    }
}

// Integrace:
// - RootElement.Metadata    ← PROP-038
// - PropertyElement.Metadata ← PROP-038
// - MethodElement.Metadata   ← PROP-038 (nové)
```

## DataType — hotová hodnota (enum, 32 C# typů)

```csharp
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
    Entity,     // odkaz na jinou entitu
    EnumValue,  // odkaz na hodnotu enumu
    Object,     // System.Object — fallback
    Dynamic,    // dynamic — open
    Void,       // System.Void
    Array,
    Nullable,
    Struct,
    Record,
}
```

## TypeModel — sealed record s factory metodami

```csharp
public sealed record TypeModel
{
    public DataType BaseType { get; init; }
    public bool IsNullable { get; init; }
    public bool IsCollection { get; init; }
    public string? CustomTypeName { get; init; }
    public List<TypeModel> GenericArguments { get; init; } = [];

    public bool IsVoid => BaseType == DataType.Void
                          && !IsNullable && !IsCollection
                          && GenericArguments.Count == 0;

    // Factory methods
    public static TypeModel Void { get; } = new() { BaseType = DataType.Void };
    public static TypeModel String { get; } = new() { BaseType = DataType.String };
    public static TypeModel Int32 { get; } = new() { BaseType = DataType.Int32 };
    public static TypeModel Bool { get; } = new() { BaseType = DataType.Bool };
    public static TypeModel Object { get; } = new() { BaseType = DataType.Object };
    public static TypeModel Decimal { get; } = new() { BaseType = DataType.Decimal };
    public static TypeModel Guid { get; } = new() { BaseType = DataType.Guid };
    public static TypeModel DateTime { get; } = new() { BaseType = DataType.DateTime };

    public static TypeModel Of(DataType baseType) => new() { BaseType = baseType };
    public TypeModel MakeNullable() => this with { IsNullable = true };
    public TypeModel MakeCollection() => this with { IsCollection = true };
    public TypeModel WithCustomName(string name) => this with { CustomTypeName = name };
    public TypeModel WithGenericArg(TypeModel arg) => this with {
        GenericArguments = [..GenericArguments, arg]
    };
}
```

## AccessModifier — enum

```csharp
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

## SemanticCollection<T>

```csharp
// Kontejner pro child elementy (např. Properties, Methods) s možností rozšíření.
public class SemanticCollection<T> : List<T>
{
    public event Action? Changed;
    public new void Add(T item) { base.Add(item); Changed?.Invoke(); }
    public new void Remove(T item) { base.Remove(item); Changed?.Invoke(); }
    public new void Clear() { base.Clear(); Changed?.Invoke(); }
}
```
