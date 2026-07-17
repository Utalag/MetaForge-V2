# CODE-004: MapType TODO vyčištění + typová diagnostika

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-17
> **Autor:** Copilot
> **Oblast:** Generators
> **Odhad:** ~1 den
> **Zdroj:** B10 (Stavová analýza 2026-07-12/17)
> **Motivace:** 3 inline `/* TODO */` komentáře v generovaném C# kódu + 2 tiché degradace → nahradit funkčním mapováním + diagnostickými varováními.

## Cíl

1. **Odstranit `/* TODO */` komentáře** z `MapDataType` — už žádné TODO v generovaném kódu
2. **Přidat správné mapování** pro `DataType.Array` (`T[]` syntax) a `DataType.Nullable` (`T?` syntax)
3. **Přidat diagnostická varování** pro nerozpoznané typy — reportovat přes `DiagnosticInfo` do `GeneratedCodeArtifact.Diagnostics`
4. **Nulové signature changes** — `MapType` zůstává `internal static string MapType(TypeModel)`, žádné změny 22 volacích míst

## Motivace

### Dnešní stav

```csharp
// CodeGenerator.MapDataType() — řádky 543-549
DataType.Entity   => "object /* TODO: Replace with actual entity type */",
DataType.Array    => "object[]/* TODO: Replace with actual array type */",
DataType.Nullable => "object /* TODO: Replace with actual nullable type */",
DataType.Struct   => "object /* Resolved via CustomTypeName in MapType */",   // tichá degradace
DataType.Record   => "object /* Resolved via CustomTypeName in MapType */",   // tichá degradace
```

**Důsledky:**
- TODO komentáře jsou **embedované do generovaného C# kódu** — uživatel vidí `object /* TODO... */` ve vygenerovaném souboru
- `DataType.Array` je konfliktní s `IsCollection` (`List<T>` vs `T[]`) — není jasné, co má být výsledek
- `DataType.Nullable` se nikdy nepoužije (existuje `IsNullable`), ale kdyby ano, vyrobí `"object"`
- `DataType.Struct`/`Record` bez `CustomTypeName` tiše degradují na `"object"` — žádné varování, žádné TODO
- **Žádná diagnostika** — `MapType`/`MapDataType` jsou statické čisté funkce bez postranního kanálu

### Cílový stav

```csharp
// CodeGenerator.MapDataType() — čisté mapování, žádné komentáře
DataType.Entity   => "object",   // fallback, diagnostika v MapType
DataType.Array    => "object",   // pole se řeší v MapType (T[] syntax)
DataType.Nullable => "object",   // nullable se řeší v MapType (T? syntax)
DataType.Struct   => "object",   // fallback, diagnostika v MapType
DataType.Record   => "object",   // fallback, diagnostika v MapType
```

A v `MapType()` přibudou explicitní cesty pro Array a Nullable + diagnostika pro nerozpoznané typy.

### Proč nezměnit signaturu MapType

`MapType` je volána z **22 míst** (20× CodeGenerator + 2× ExpressionRenderer). Přidání parametru `List<DiagnosticInfo>` by:
- Vynutilo si změnu všech 22 volání
- Vyžádalo protažení diagnostického listu přes Scriban šablony (které volají `MapType` jako `Func<TypeModel, string>` delegát)
- Rozbilo čisté rozhraní `internal static string MapType(TypeModel)`

**Místo toho: `AsyncLocal<List<DiagnosticInfo>>`** — ambientní, thread-safe, zero signature changes.

## Soubor změn: `Src/MetaForge.Generators/CodeGenerator.cs`

Všechny změny v jediném souboru. Čtyři editační místa:

| # | Místo | Řádky | Operace |
|---|-------|-------|---------|
| 1 | Nové pole + metody před `Generate()` | za ř. 17 | **INSERT** (3 nové metody + 1 pole) |
| 2 | `Generate()` — wire-up ambientního kolektoru | 22–59 | **EDIT** (begin/end kolem dispatche) |
| 3 | `MapType()` — nové cesty + diagnostika | 482–509 | **EDIT** (Array, Nullable, diag před return) |
| 4 | `MapDataType()` — odstranění TODO | 511–549 | **EDIT** (5 řádků změněno) |
| 5 | Nová `IsKnownPrimitive` za `MapDataType()` | za ř. 549 | **INSERT** (pomocná metoda) |

---

### Místo 1 — INSERT za řádek 17 (za `_renderer` pole, před `Generate()`)

```csharp
// === EXISTUJÍCÍ KÓD (ř. 17) ===
    private readonly ExpressionRenderer _renderer = new();

// === NOVÝ KÓD (INSERT) ===
    // Ambientní diagnostický kolektor pro MapType — aktivní pouze během Generate()
    private static readonly AsyncLocal<List<DiagnosticInfo>?> _typeDiags = new();

    private static List<DiagnosticInfo> BeginTypeDiagnostics()
    {
        var list = new List<DiagnosticInfo>();
        _typeDiags.Value = list;
        return list;
    }

    private static List<DiagnosticInfo>? EndTypeDiagnostics()
    {
        var list = _typeDiags.Value;
        _typeDiags.Value = null;
        return list;
    }

// === EXISTUJÍCÍ KÓD (ř. 19) ===
    public override GeneratedCodeArtifact Generate(RootElement element)
```

---

### Místo 2 — EDIT `Generate()` (řádky 22–59)

**PŘED:**
```csharp
    public override GeneratedCodeArtifact Generate(RootElement element)
    {
        if (string.IsNullOrWhiteSpace(element.Name))
        {
            return new GeneratedCodeArtifact(
                FileName: "error.cs",
                SourceCode: "// ERROR: Element bez názvu",
                Diagnostics: new[] { new DiagnosticInfo("Element nemá nastavený název.", DiagnosticSeverity.Error, element.Id.ToString()) }
            );
        }

        var code = element switch
        {
            ValueObjectElement vo => GenerateValueObject(vo),
            ClassElement cls => GenerateClass(cls),
            InterfaceElement iface => GenerateInterface(iface),
            EnumElement enm => GenerateEnum(enm),
            StructElement str => GenerateStruct(str),
            DelegateElement del => GenerateDelegate(del),
            _ => string.IsNullOrWhiteSpace(element.Name)
                ? "// ERROR: Element bez názvu"
                : $"// Nepodporovaný element typu: {element.GetType().Name} ({element.Kind})"
        };

        var diagnostics = new List<DiagnosticInfo>();
        if (string.IsNullOrWhiteSpace(code) || code.StartsWith("// Nepodporovaný"))
        {
            diagnostics.Add(new DiagnosticInfo(
                $"Nepodporovaný element: {element.Kind}",
                DiagnosticSeverity.Warning,
                element.Id.ToString(),
                element.Name));
        }

        return new GeneratedCodeArtifact(
            FileName: $"{element.Name}{FileExtension}",
            SourceCode: code,
            Diagnostics: diagnostics.Count > 0 ? diagnostics.AsReadOnly() : null
        );
    }
```

**PO:**
```csharp
    public override GeneratedCodeArtifact Generate(RootElement element)
    {
        if (string.IsNullOrWhiteSpace(element.Name))
        {
            return new GeneratedCodeArtifact(
                FileName: "error.cs",
                SourceCode: "// ERROR: Element bez názvu",
                Diagnostics: new[] { new DiagnosticInfo("Element nemá nastavený název.", DiagnosticSeverity.Error, element.Id.ToString()) }
            );
        }

        BeginTypeDiagnostics();                         // ← NOVÉ: aktivuje ambientní kolektor

        var code = element switch
        {
            ValueObjectElement vo => GenerateValueObject(vo),
            ClassElement cls => GenerateClass(cls),
            InterfaceElement iface => GenerateInterface(iface),
            EnumElement enm => GenerateEnum(enm),
            StructElement str => GenerateStruct(str),
            DelegateElement del => GenerateDelegate(del),
            _ => string.IsNullOrWhiteSpace(element.Name)
                ? "// ERROR: Element bez názvu"
                : $"// Nepodporovaný element typu: {element.GetType().Name} ({element.Kind})"
        };

        var diagnostics = new List<DiagnosticInfo>();

        var typeDiags = EndTypeDiagnostics();            // ← NOVÉ: vyzvedne varování z MapType
        if (typeDiags is { Count: > 0 })
            diagnostics.AddRange(typeDiags);

        if (string.IsNullOrWhiteSpace(code) || code.StartsWith("// Nepodporovaný"))
        {
            diagnostics.Add(new DiagnosticInfo(
                $"Nepodporovaný element: {element.Kind}",
                DiagnosticSeverity.Warning,
                element.Id.ToString(),
                element.Name));
        }

        return new GeneratedCodeArtifact(
            FileName: $"{element.Name}{FileExtension}",
            SourceCode: code,
            Diagnostics: diagnostics.Count > 0 ? diagnostics.AsReadOnly() : null
        );
    }
```

---

### Místo 3 — EDIT `MapType()` (řádky 482–509)

**PŘED:**
```csharp
    internal static string MapType(TypeModel type)
    {
        var nullable = type.IsNullable ? "?" : "";
        var baseType = MapDataType(type.BaseType);

        // Custom název (např. ValueTuple, Task, user-defined typy)
        if (!string.IsNullOrWhiteSpace(type.CustomTypeName))
        {
            var typeName = type.CustomTypeName;
            if (type.GenericArguments.Count > 0)
            {
                var genericArgs = string.Join(", ", type.GenericArguments.Select(MapType));
                return $"{typeName}<{genericArgs}>{nullable}";
            }
            return $"{typeName}{nullable}";
        }

        // Kolekce
        if (type.IsCollection)
        {
            var innerType = type.GenericArguments.Count > 0
                ? MapType(type.GenericArguments[0])
                : "object";
            return $"List<{innerType}>";
        }

        return $"{baseType}{nullable}";
    }
```

**PO:**
```csharp
    internal static string MapType(TypeModel type)
    {
        var nullable = type.IsNullable ? "?" : "";
        var baseType = MapDataType(type.BaseType);

        // Custom název (např. ValueTuple, Task, user-defined typy)
        if (!string.IsNullOrWhiteSpace(type.CustomTypeName))
        {
            var typeName = type.CustomTypeName;
            if (type.GenericArguments.Count > 0)
            {
                var genericArgs = string.Join(", ", type.GenericArguments.Select(MapType));
                return $"{typeName}<{genericArgs}>{nullable}";
            }
            return $"{typeName}{nullable}";
        }

        // Array — T[] syntax (před IsCollection, které vrací List<T>)
        if (type.BaseType == DataType.Array)
        {
            var elementType = type.GenericArguments.Count > 0
                ? MapType(type.GenericArguments[0])
                : "object";
            return $"{elementType}[]";
        }

        // Nullable jako BaseType — T? syntax
        if (type.BaseType == DataType.Nullable)
        {
            var innerType = type.GenericArguments.Count > 0
                ? MapType(type.GenericArguments[0])
                : "object";
            return $"{innerType}?";
        }

        // Kolekce
        if (type.IsCollection)
        {
            var innerType = type.GenericArguments.Count > 0
                ? MapType(type.GenericArguments[0])
                : "object";
            return $"List<{innerType}>";
        }

        // Diagnostika: nerozpoznaný typ → varování (jen pokud běží ambientní kolektor)
        if (baseType == "object" && !IsKnownPrimitive(type.BaseType))
        {
            _typeDiags.Value?.Add(new DiagnosticInfo(
                $"Nelze namapovat typ '{type.BaseType}' na C# typ. Používám 'object'.",
                DiagnosticSeverity.Warning,
                ElementName: type.CustomTypeName ?? type.BaseType.ToString()
            ));
        }

        return $"{baseType}{nullable}";
    }
```

---

### Místo 4 — EDIT `MapDataType()` (řádky 511–549)

**PŘED:**
```csharp
    internal static string MapDataType(DataType dataType) => dataType switch
    {
        DataType.Bool => "bool",
        DataType.Byte => "byte",
        DataType.SByte => "sbyte",
        DataType.Int16 => "short",
        DataType.UInt16 => "ushort",
        DataType.Int32 => "int",
        DataType.UInt32 => "uint",
        DataType.Int64 => "long",
        DataType.UInt64 => "ulong",
        DataType.Int128 => "Int128",
        DataType.Half => "Half",
        DataType.Single => "float",
        DataType.Double => "double",
        DataType.Decimal => "decimal",
        DataType.NInt => "nint",
        DataType.NUInt => "nuint",
        DataType.Char => "char",
        DataType.String => "string",
        DataType.Binary => "byte[]",
        DataType.DateOnly => "DateOnly",
        DataType.TimeOnly => "TimeOnly",
        DataType.DateTime => "DateTime",
        DataType.DateTimeOffset => "DateTimeOffset",
        DataType.TimeSpan => "TimeSpan",
        DataType.Guid => "Guid",
        DataType.Uri => "Uri",
        DataType.Version => "Version",
        DataType.Object => "object",
        DataType.Void => "void",
        DataType.Dynamic => "dynamic",
        DataType.Entity => "object /* TODO: Replace with actual entity type */",
        DataType.EnumValue => "int",
        DataType.Array => "object[]/* TODO: Replace with actual array type */",
        DataType.Nullable => "object /* TODO: Replace with actual nullable type */",
        DataType.Struct => "object /* Resolved via CustomTypeName in MapType */",
        DataType.Record => "object /* Resolved via CustomTypeName in MapType */",
        _ => "object"
    };
```

**PO:**
```csharp
    internal static string MapDataType(DataType dataType) => dataType switch
    {
        DataType.Bool => "bool",
        DataType.Byte => "byte",
        DataType.SByte => "sbyte",
        DataType.Int16 => "short",
        DataType.UInt16 => "ushort",
        DataType.Int32 => "int",
        DataType.UInt32 => "uint",
        DataType.Int64 => "long",
        DataType.UInt64 => "ulong",
        DataType.Int128 => "Int128",
        DataType.Half => "Half",
        DataType.Single => "float",
        DataType.Double => "double",
        DataType.Decimal => "decimal",
        DataType.NInt => "nint",
        DataType.NUInt => "nuint",
        DataType.Char => "char",
        DataType.String => "string",
        DataType.Binary => "byte[]",
        DataType.DateOnly => "DateOnly",
        DataType.TimeOnly => "TimeOnly",
        DataType.DateTime => "DateTime",
        DataType.DateTimeOffset => "DateTimeOffset",
        DataType.TimeSpan => "TimeSpan",
        DataType.Guid => "Guid",
        DataType.Uri => "Uri",
        DataType.Version => "Version",
        DataType.Object => "object",
        DataType.Void => "void",
        DataType.Dynamic => "dynamic",
        DataType.Entity => "object",
        DataType.EnumValue => "int",
        DataType.Array => "object",
        DataType.Nullable => "object",
        DataType.Struct => "object",
        DataType.Record => "object",
        _ => "object"
    };
```

> **Změna souhrn:** 5 řádků — odstraněno `/* TODO: Replace with actual entity type */`, `[]/* TODO: Replace with actual array type */`, `/* TODO: Replace with actual nullable type */`, `/* Resolved via CustomTypeName in MapType */` (2×). Všechny vracejí čisté `"object"`.

---

### Místo 5 — INSERT za řádek 549 (za `MapDataType()`, před koncem třídy)

```csharp
// === EXISTUJÍCÍ KÓD (ř. 549) ===
    };
// === NOVÝ KÓD (INSERT) ===

    /// <summary>Vrátí true pro primitivní typy, které MapType umí přeložit bez CustomTypeName.</summary>
    private static bool IsKnownPrimitive(DataType dt) => dt switch
    {
        DataType.Bool or DataType.Byte or DataType.SByte or DataType.Int16
            or DataType.UInt16 or DataType.Int32 or DataType.UInt32 or DataType.Int64
            or DataType.UInt64 or DataType.Int128 or DataType.Half or DataType.Single
            or DataType.Double or DataType.Decimal or DataType.NInt or DataType.NUInt
            or DataType.Char or DataType.String or DataType.Binary or DataType.DateOnly
            or DataType.TimeOnly or DataType.DateTime or DataType.DateTimeOffset
            or DataType.TimeSpan or DataType.Guid or DataType.Uri or DataType.Version
            or DataType.Object or DataType.Void or DataType.Dynamic
            or DataType.EnumValue => true,
        _ => false
    };

// === EXISTUJÍCÍ KÓD (ř. 550) ===
}
```

## Testy

### `Tests/MetaForge.Generators.Tests/CSharp/MapTypeDiagnosticsTests.cs` (nový soubor, ~8 testů)

| Test | Vstup | Očekávaný výstup |
|------|-------|-----------------|
| `MapType_Array_ProducesBracketSyntax` | `TypeModel { BaseType = Array, GenericArguments = [int] }` | `"int[]"` |
| `MapType_Array_WithoutGeneric_ProducesObjectArray` | `TypeModel { BaseType = Array, GenericArguments = [] }` | `"object[]"` |
| `MapType_Nullable_ProducesQuestionMark` | `TypeModel { BaseType = Nullable, GenericArguments = [int] }` | `"int?"` |
| `MapType_Nullable_WithoutGeneric_ProducesObjectNullable` | `TypeModel { BaseType = Nullable, GenericArguments = [] }` | `"object?"` |
| `MapType_Entity_WithoutName_ProducesObject` | `TypeModel { BaseType = Entity }` | `"object"` (čisté, bez TODO) |
| `MapType_Struct_WithoutName_ProducesObject` | `TypeModel { BaseType = Struct }` | `"object"` (čisté, bez TODO) |
| `Generate_UnresolvedType_EmitsDiagnostic` | `PropertyElement.Type = TypeModel { BaseType = Entity }` → `Generate()` | `artifact.Diagnostics` obsahuje varování |
| `Generate_ResolvedType_NoDiagnostic` | `PropertyElement.Type = TypeModel { BaseType = Entity, CustomTypeName = "Customer" }` → `Generate()` | `artifact.Diagnostics` je null/prázdné |
| `MapType_Collection_Unchanged` | `TypeModel { IsCollection = true, GenericArguments = [int] }` | `"List<int>"` (IsCollection beze změny) |
| `MapType_DataTypeCommentFree` | Všechny DataType hodnoty přes `MapType` | Žádný výstup neobsahuje `/* TODO */` |

## Rizika

1. **AsyncLocal overhead** — `AsyncLocal` má minimální overhead (~1 ns), ale je to ExecutionContext operace. Akceptovatelné — voláno max 1× per `Generate()`.
2. **Scriban delegate capture** — `MapType` je předávána jako `Func<TypeModel, string>` do Scriban modelu. AsyncLocal VALUE je zachycena při volání `BeginTypeDiagnostics()` a přečtena při volání `MapType()` uvnitř `.Render()`. Ověřeno: Scriban volá delegáta synchronně v rámci `.Render()`.
3. **Testy bez BeginTypeDiagnostics** — MapType volaná mimo `Generate()` (např. v unit testech) nebude mít ambientní kolektor → diagnostika se NEemituje. To je správné chování — unit testy testují jen mapování, ne diagnostiku.

## Otevřené otázky

- **OQ-004-01**: Má se `DataType.Array` s `IsCollection = true` chovat jako `List<T>` nebo `T[]`?  
  → **Rozhodnuto**: Array cesta se vyhodnocuje PŘED `IsCollection`. Pokud někdo nastaví `BaseType = Array` i `IsCollection = true`, Array cesta vyhraje a výstupem je `T[]`. Je to edge case — v praxi se Array a IsCollection kombinovat nebudou.

- **OQ-004-02**: Má CLI zobrazovat diagnostická varování z `GeneratedCodeArtifact.Diagnostics`?  
  → **Ne v CODE-004**. To je samostatný follow-up task. CLI dnes ignoruje `artifact.Diagnostics` — to se nemění.

- **OQ-004-03**: Má `IsKnownPrimitive` zahrnovat i `DataType.EnumValue`?  
  → **Ano**. `EnumValue` je mapován na `"int"`, což je známý primitiv. Zahrnuto v implementaci.

## Verifikace

```bash
dotnet build                    # 0 chyb
dotnet test --filter "MapType"  # 10 nových testů projde
dotnet test                     # všech ~613 testů projde, žádný regres
```
