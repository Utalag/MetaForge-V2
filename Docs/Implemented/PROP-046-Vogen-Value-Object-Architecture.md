# PROP-046: Vogen Value Object Architecture — StrongType metareprezentace a codegen

> **Status:** ✅ Implementováno (2026-07-10)
> **Projekt:** MetaForge-V2
> **Oblast:** Core, Generators
> **Priorita:** Vysoká — Vogen je #1 externí ForgeBlock kandidát

---

## Souhrn

Implementace architektury Vogen value objectů v MetaForge:
- **StrongType** = čistá metareprezentace (Name, UnderlyingType, ValidationRules) — **bez** konverzních flags
- **ValueObjectElement** = codegen element dědící z `StructElement`, nese `VogenConversions` (default `None`)
- **VogenValueObject.scriban** = Scriban template generující `[ValueObject]`-annotated C# kód
- **VogenConversions** = flags enum mapující Vogen `Conversions` (11+ flagů)
- Konverze se **nevolí** v business modelu, ale **až při výběru infrastruktury** (EF Core, Dapper, JSON...)

---

## Architektonický model

```
BusinessModel (CustomTypeDefinition)     ← Name, UnderlyingType, ValidationRules
    ↓
MetaForge.Core (StrongType)              ← čistá metareprezentace, BEZ conversion flags
    ↓
ValueObjectElement (codegen layer)       ← Conversions = None (default)
    ↓
VogenValueObject.scriban                 ← generuje [ValueObject]
    ↓
[User selects infrastructure]            ← EF Core? Dapper? JSON? BSON?
    ↓
Infrastructure ForgeBlock                ← nastaví Conversions na ValueObjectElement
    ↓
Final codegen                            ← [ValueObject(conversions: ...)]
    ↓
Vogen source-generator (compile time)    ← dotvoří EfCoreValueConverter, DapperTypeHandler, ...
```

---

## Změněné / nové soubory

| Soubor | Akce | Popis |
|--------|------|-------|
| `Src/MetaForge.Core/ValueObjects/VogenConversions.cs` | ✨ Nový | Flags enum: `None`, `TypeConverter`, `SystemTextJson`, `NewtonsoftJson`, `EfCoreValueConverter`, `DapperTypeHandler`, `Bson`, `MessagePack`, `Orleans`, `XmlSerializable`, `LinqToDbValueConverter`, `ServiceStackDotText` |
| `Src/MetaForge.Core/ValueObjects/ConversionOptions.cs` | ❌ Odstraněn | Nahrazen `VogenConversions` |
| `Src/MetaForge.Core/ValueObjects/StrongType.cs` | ✏️ Upraven | Odstraněna `Conversion` property (konverze není součást business modelu) |
| `Src/MetaForge.Core/Elements/Types/StructElement.cs` | ✏️ Upraven | Odstraněn `sealed` (umožňuje dědičnost `ValueObjectElement`) |
| `Src/MetaForge.Core/Elements/Types/ValueObjectElement.cs` | ✨ Nový | Dědí z `StructElement`, přidává `Conversions` (default `None`), `ThrowsExceptionType`, `FromStrongType()`, fluent API |
| `Src/MetaForge.Generators/Templates/VogenValueObject.scriban` | ✨ Nový | Scriban template generující `[global::Vogen.ValueObject] readonly partial struct` |
| `Src/MetaForge.Generators/CodeGenerator.cs` | ✏️ Upraven | Přidán `ValueObjectElement` case ve switch, `GenerateValueObject()`, `FormatVogenConversions()`, oprava inline strong types |
| `Tests/MetaForge.Core.Tests/ValueObjects/ConversionOptionsTests.cs` | ❌ Odstraněn | Test odstraněn spolu s `ConversionOptions` |
| `Tests/MetaForge.Core.Tests/ValueObjects/StrongTypeTests.cs` | ✏️ Upraven | Odstraněn test `Constructor_NullConversion_Allowed` |
| `Tests/MetaForge.Generators.Tests/CSharp/EndToEndScenariosTests.cs` | ✏️ Upraven | Scenario6 přepsán — používá `ValueObjectElement`, očekává `[ValueObject]` output |
| `New_Architecture/27-ForgeBlock-External-Libraries.md` | ✏️ Upraven | Aktualizován Vogen popis, capability YAML, priority tabulky |

---

## VogenConversions — flags enum

```csharp
[Flags]
public enum VogenConversions
{
    None                  = 0,     // Žádné konvertory (default)
    TypeConverter         = 1,     // System.ComponentModel.TypeConverter
    SystemTextJson        = 2,     // System.Text.Json.JsonConverter
    NewtonsoftJson        = 4,     // Newtonsoft.Json.JsonConverter
    EfCoreValueConverter  = 8,     // EF Core ValueConverter + ValueComparer
    DapperTypeHandler     = 16,    // Dapper SqlMapper.TypeHandler
    Bson                  = 32,    // MongoDB BSON serializer
    MessagePack           = 64,    // MessagePack formatter
    Orleans               = 128,   // Microsoft Orleans codec + copier
    XmlSerializable       = 256,   // IXmlSerializable
    LinqToDbValueConverter = 512,  // LINQ to DB converter
    ServiceStackDotText   = 1024,  // ServiceStack.Text serializer
}
```

---

## Klíčová rozhodnutí

| Rozhodnutí | Zdůvodnění |
|-----------|------------|
| **Konverze nejsou v StrongType** | Konverze je volba infrastruktury, ne business modelu. Uživatel si vybere "chci EF Core" a teprve pak se přidá `EfCoreValueConverter`. |
| **Default: `VogenConversions.None`** | Vogen negeneruje žádné konvertory, dokud není vybrána infrastruktura. To je záměrné — uživatel platí jen za to, co potřebuje. |
| **ValueObjectElement dědí ze StructElement** | Čistší než přidávat Vogen props do generického `StructElement`. |
| **Samostatný VogenValueObject.scriban** | Separace od `Struct.scriban` — Vogen typy nejsou record structy, nemají primary constructor. |
| **Vogen typy nejsou record** | Vogen generuje `Value` property a `Equals`/`GetHashCode` sám — není potřeba `record`. Typ je `readonly partial struct`. |
| **CodeGenerator switch: ValueObjectElement před StructElement** | `ValueObjectElement` je `StructElement`, musí být v pattern match dřív. |

---

## Generovaný kód (příklad)

Pro `[ValueObject] public readonly partial struct CustomerId` vygeneruje Vogen defaultně:

```csharp
[global::System.Text.Json.Serialization.JsonConverter(typeof(CustomerIdSystemTextJsonConverter))]
[global::System.ComponentModel.TypeConverter(typeof(CustomerIdTypeConverter))]
public readonly partial struct CustomerId
    : IEquatable<CustomerId>, IComparable<CustomerId>, IParsable<CustomerId>, ...
{
    // Vogen source-generuje Value property, From(), Validate(), konvertory...
}
```

Pro `Conversions.None` se nevygeneruje žádný konvertor — jen `IEquatable`, `IComparable` atd.

---

## Testy

| Projekt | Testů | Výsledek |
|---------|-------|----------|
| `MetaForge.Core.Tests` | 402 | ✅ Všechny prochází |
| `MetaForge.Generators.Tests` | 14 | ✅ Všechny prochází (vč. Scenario6) |
| `MetaForge.BusinessModel.Tests` | 22 | ✅ Všechny prochází |
| `MetaForge.Translator.Tests` | 31 | ✅ Všechny prochází |

---

## Budoucí kroky

1. **Infrastructure ForgeBlock integrace** — EF Core, Dapper, JSON ForgeBlocky nastaví `VogenConversions` na `ValueObjectElement` před codegenem
2. **Vogen validační pravidla** — `ValueObjectValidationRule` → generování Vogen `Validate()` metody
3. **Vogen NuGet dependency** — přidat referenci na Vogen balíček do vygenerovaného projektu
4. **SmartEnum ForgeBlock** — obdobný pattern pro Ardalis.SmartEnum

---

## Související

- [27-ForgeBlock-External-Libraries.md](../../New_Architecture/27-ForgeBlock-External-Libraries.md) — Vogen capability návrh
- [Vogen GitHub](https://github.com/SteveDunn/Vogen) — oficiální Vogen dokumentace
- [PROP-008](../../New_Architecture/) — původní ForgeBlock implementace (Math, String, Validation)
- [PROP-029](../../New_Architecture/27-ForgeBlock-External-Libraries.md) — AutoMapper, EF Core, FluentValidation ForgeBlocky
