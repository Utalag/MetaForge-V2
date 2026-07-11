# PROP-033: Core — Element Factory Metody a CoreValidator

> **Stav:** 🟢 Schváleno
> **Datum:** 2026-07-05
> **Autor:** Copilot (C# Implementer)
> **Návaznost:** PROP-002 (Core), PROP-031 (Statement System), PROP-032 (Integrační testy)
> **Matice:** [`Docs/Integration/01-Integration-Test-Matrix.md`](../Integration/01-Integration-Test-Matrix.md)

---

## Cíl

Přidat do Core elementů **statické factory metody** pro každou validní modifikátorovou kombinaci (dle matice) a vytvořit **`CoreValidator`** pro validaci nevalidních kombinací.

Zajistit, aby:
- Translator nemusel znát detailní pravidla kombinací modifikátorů
- Core elementy se daly sestavit bez rizika nevalidního stavu
- Matice (74 ✅ + 23 ❌ + 5 ⚠️) byla přímo vtělena do kódu
- Žádná nová vrstva — vše v `MetaForge.Core`

---

## Princip: Hybridní přístup

| Mechanismus | Použití |
|-------------|---------|
| **Statické factory metody** | Modifikátorové kombinace s konflikty (Class C1-C8, Method M1-M8, atd.) |
| **Fluent `With*` metody** | Vlastnosti bez konfliktů (access modifier, dědičnost, base class) |
| **`Validate()` metoda** | Explicitní ověření — pro testy, debugging, CI |

```
┌──────────────────────────────────────────────┐
│  Translator / ForgeBlock / Test              │
│      │                                        │
│      ▼                                        │
│  Statická factory (ClassElement.Abstract())  │  ← VŽDY validní výstup
│      │                                        │
│      ▼                                        │
│  Fluent rozšíření (.WithAccess(Internal))    │  ← Bez konfliktů
│      │                                        │
│      ▼                                        │
│  .Validate() → IReadOnlyList<ValidationIssue>│  ← Volitelné ověření
└──────────────────────────────────────────────┘
```

---

## 1. ClassElement — factory metody

### Modifikátorové kombinace (C1-C8 matice)

```csharp
// Každá metoda = právě jedna validní kombinace
public static ClassElement Basic(string name)          // C1: public class
public static ClassElement Abstract(string name)       // C2: public abstract class
public static ClassElement Sealed(string name)         // C3: public sealed class
public static ClassElement Static(string name)         // C4: public static class
public static ClassElement Partial(string name)        // C5: public partial class
public static ClassElement Record(string name)         // C6: public record class
public static ClassElement AbstractRecord(string name) // C7: public abstract record class
public static ClassElement SealedRecord(string name)   // C8: public sealed record class
```

### Fluent rozšíření (bez konfliktů)

```csharp
public ClassElement WithAccess(AccessModifier access)       // A1,A2,A6
public ClassElement WithBaseClass(string? baseClassName)    // I1-I4
public ClassElement WithInterfaces(params string[] interfaces)
public ClassElement WithUsings(params string[] usings)
```

### Co NENÍ potřeba

- ❌ `AsAbstract().AsSealed()` fluent API — nahrazeno factory metodami
- ❌ `IsAbstract`, `IsSealed`, ... properties **zůstávají public** pro serializaci a pokročilé scénáře
- ❌ Žádné tiché přepisování — factory metoda nastaví správné hodnoty atomicky

---

## 2. EnumElement — factory metody

```csharp
public static EnumElement Basic(string name)           // E1: int32
public static EnumElement ByteEnum(string name)        // E2: byte
public static EnumElement Int64Enum(string name)       // E3: long
public static EnumElement Flags(string name)           // E4: [Flags] int32
public static EnumElement Flags(Type underlyingType, string name)  // E4 varianta

public EnumElement WithAccess(AccessModifier access)
```

---

## 3. StructElement — factory metody

```csharp
public static StructElement Basic(string name)             // S1
public static StructElement ReadOnly(string name)          // S2
public static StructElement Record(string name)            // S3
public static StructElement ReadOnlyRecord(string name)    // S4

public StructElement WithAccess(AccessModifier access)
```

---

## 4. PropertyElement — factory metody

```csharp
public static PropertyElement GetSet(string name, TypeModel type)       // P1
public static PropertyElement GetOnly(string name, TypeModel type)      // P2
public static PropertyElement InitOnly(string name, TypeModel type)     // P3
public static PropertyElement Required(string name, TypeModel type)     // P4
public static PropertyElement Static(string name, TypeModel type)       // P5
public static PropertyElement RequiredGetOnly(string name, TypeModel type) // P8

public PropertyElement WithAccess(AccessModifier access)
public PropertyElement WithDefault(string defaultValue)
```

---

## 5. MethodElement — factory metody

```csharp
public static MethodElement Basic(string name)                         // M1: void
public static MethodElement Static(string name, TypeModel returnType)  // M2
public static MethodElement Async(string name, TypeModel returnType)   // M3,M4,M8
public static MethodElement Abstract(string name, TypeModel returnType)// M5
public static MethodElement Virtual(string name, TypeModel returnType) // M6
public static MethodElement Override(string name, TypeModel returnType)// M7

public MethodElement WithAccess(AccessModifier access)
public MethodElement WithParameters(params ParameterElement[] parameters)
public MethodElement WithBody(BlockStatement body)
```

> **Poznámka:** `Async` nastaví `IsAsync = true`. Pokud `ReturnType.IsTask`, nechá se tak. Pro `Task<List<T>>` předá volající správný TypeModel.

---

## 6. InterfaceElement — factory metody

```csharp
public static InterfaceElement Basic(string name)

public InterfaceElement WithAccess(AccessModifier access)
```

---

## 7. CoreValidator

```csharp
namespace MetaForge.Core.Validation;

/// <summary>
/// Validátor Core elementů — kontroluje nevalidní kombinace dle matice.
/// Použití: volitelné ověření pro testy, debugging, CI/CD.
/// </summary>
public static class CoreValidator
{
    /// <summary>Vrátí seznam validačních problémů. Prázdný seznam = validní.</summary>
    public static IReadOnlyList<ValidationIssue> Validate(RootElement element);

    /// <summary>Vyhodí výjimku při prvním problému. Pro fail-fast scénáře.</summary>
    public static void EnsureValid(RootElement element);

    // Specializované validátory
    private static IReadOnlyList<ValidationIssue> ValidateClass(ClassElement c);
    private static IReadOnlyList<ValidationIssue> ValidateEnum(EnumElement e);
    private static IReadOnlyList<ValidationIssue> ValidateMethod(MethodElement m);
    private static IReadOnlyList<ValidationIssue> ValidateProperty(PropertyElement p);
    private static IReadOnlyList<ValidationIssue> ValidateStatement(Statement stmt, MethodElement context);
}
```

### Pokryté ❌ řádky matice

| Kód | Popis | Detekce |
|:---:|-------|---------|
| C9 | `abstract sealed` | `ValidateClass` |
| C10 | `abstract static` | `ValidateClass` |
| C12 | `static record` | `ValidateClass` |
| A3-A5 | private/protected top-level | `ValidateClass` (dle kontextu) |
| I5 | dědění od `string` (sealed) | `ValidateClass` — v MVP jen varování |
| E5-E6 | nevalidní underlying type | `ValidateEnum` |
| P7 | property bez getteru i setteru | `ValidateProperty` |
| T19-T21 | void jako property type | `ValidateProperty` |
| M9-M12 | konfliktní modifikátory | `ValidateMethod` |
| B11-B13 | typové chyby ve statementech | `ValidateStatement` |
| K7-K8 | nevalidní konstruktor | `ValidateConstructor` (až bude ConstructorElement) |

---

## Struktura nových souborů

```
Src/MetaForge.Core/
├── Validation/
│   ├── CoreValidator.cs          ← Nový
│   └── ValidationIssue.cs        ← Nový
```

### Upravené soubory

| Soubor | Změna |
|--------|-------|
| `Src/MetaForge.Core/Elements/Types/ClassElement.cs` | Přidat factory metody + `With*` fluent |
| `Src/MetaForge.Core/Elements/Types/EnumElement.cs` | Přidat factory metody + `WithAccess` |
| `Src/MetaForge.Core/Elements/Types/StructElement.cs` | Přidat factory metody + `WithAccess` |
| `Src/MetaForge.Core/Elements/Types/InterfaceElement.cs` | Přidat factory metody + `WithAccess` |
| `Src/MetaForge.Core/Elements/Members/PropertyElement.cs` | Přidat factory metody + `With*` |
| `Src/MetaForge.Core/Elements/Members/MethodElement.cs` | Přidat factory metody + `With*` |

---

## Co se nemění

- Properties zůstávají `{ get; set; }` — factory metody jsou doplněk, ne náhrada
- `RootElement` a `AppRoot` beze změny
- `ParameterElement` beze změny
- `Expression` a `Statement` hierarchie beze změny

---

## Pořadí implementace

| # | Krok | Odhad |
|:-:|------|------:|
| 1 | Vytvořit `ValidationIssue` record | 5 min |
| 2 | Implementovat `CoreValidator` | 1-2 h |
| 3 | Přidat factory metody do `ClassElement` | 20 min |
| 4 | Přidat factory metody do `EnumElement` | 15 min |
| 5 | Přidat factory metody do `StructElement` | 10 min |
| 6 | Přidat factory metody do `InterfaceElement` | 5 min |
| 7 | Přidat factory metody do `PropertyElement` | 20 min |
| 8 | Přidat factory metody do `MethodElement` | 20 min |
| 9 | `dotnet build` ověření | 5 min |
| 10 | Aktualizovat New_Architecture/04-Core-Elements.md | 20 min |
| 11 | Aktualizovat New_Architecture/05-Core-Behaviors.md | 10 min |
| 12 | Revidovat PROP-032 | 15 min |
| **Celkem** | | **~4 h** |

---

## Verifikace

1. `dotnet build Src/MetaForge.Core/` → OK
2. `dotnet build` (celá solution) → OK
3. Factory metody produkují elementy s očekávanými hodnotami properties
4. `CoreValidator.Validate()` vrací správné issues pro nevalidní kombinace
5. `CoreValidator.EnsureValid()` nehází výjimku pro validní kombinace

---

## Rozhodnutí

| Aspekt | Volba |
|--------|-------|
| Přístup | Hybrid: factory pro modifikátory + fluent pro zbytek |
| Properties | Zůstávají public `{ get; set; }` — zpětná kompatibilita |
| Builder vrstva | ❌ Ne — vše v Core, žádná nová assembly |
| Fluent auto-korekce | ❌ Ne — factory metody jsou atomické, ne "přepínače" |
| Validate() | Volitelná — pro testy, CI, debugging |

---

## Legenda

- Status: 🟢 Schváleno
- Vrstva: Core
- Návaznost: PROP-002, PROP-031, PROP-032
- Priorita: 🟡 Vysoká — před implementací PROP-032
