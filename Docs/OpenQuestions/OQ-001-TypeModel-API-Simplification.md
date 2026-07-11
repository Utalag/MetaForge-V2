# OQ-001 TypeModel API Simplification for Common Patterns

Typ: Open Question
Datum: 2026-07-09
Zdroj: E2E Stress Testing
Stav: Closed (Rejected — AI je primární producent)
Vrstva: Core (DataType/TypeModel)

## 1. Kontext

Při psaní E2E scénářů se opakovaně objevuje verbose pattern pro uživatelsky definované typy:

```csharp
// Dnes:
TypeModel.Of(DataType.Entity).WithCustomName("User")
TypeModel.Of(DataType.Entity).WithCustomName("Task").WithGenericArg(TypeModel.Of(DataType.Entity).WithCustomName("User"))

// Bylo by přirozenější:
TypeModel.Custom("User")
TypeModel.TaskOf(TypeModel.Custom("User"))
```

Další problematický pattern:
```csharp
// Pro property get/set:
PropertyElement.GetSet("Name", TypeModel.String)

// Pro Task<User?>:
TypeModel.Of(DataType.Entity).WithCustomName("Task")
    .WithGenericArg(TypeModel.Of(DataType.Entity).WithCustomName("User").MakeNullable())
```

## 2. Otázka k rozhodnutí

Máme přidat convenience factory metody na `TypeModel` pro běžné vzory?

### Varianta A: Statické factory na TypeModel
```csharp
TypeModel.Custom("User")                    // user-defined typ
TypeModel.TaskOf(TypeModel.Custom("User"))   // Task<User>
TypeModel.ListOf(TypeModel.Custom("Order"))  // List<Order>
TypeModel.Nullable(TypeModel.Int32)           // int?
```
**Pro**: Jednoduché, minimální změna API
**Proti**: Další metody na už tak velkém TypeModel

### Varianta B: Nechat současný stav
**Pro**: Žádná změna, API je konzistentní
**Proti**: Verbose, opakující se pattern `DataType.Entity` pro každý user-defined typ

### Varianta C: Extension methods
```csharp
public static TypeModel Custom(this TypeModel _, string name) => ...
// Použití: TypeModel.Custom("User")
```
**Pro**: Nerozšiřuje TypeModel přímo
**Proti**: Matoucí — extension na TypeModel, který se nepoužije

## 3. Dopad

- Dotčené soubory: `Src/MetaForge.Core/DataTypes/TypeModel.cs`
- Dotčené testy: Všechny používající `TypeModel.Of(DataType.Entity).WithCustomName(...)`
- Není blocking — současné API funguje, jen je nepohodlné

## 4. Doporučení

**Varianta A** — přidat statické factory metody. Je to nejčistší řešení s minimálním dopadem.

## 5. Rozhodnutí

**2026-07-09 — ZAMÍTNUTO (Varianta B)**

Důvod: Primárním producentem Core elementů bude AI model (OllamaAdapter → AiTranslationService), ne člověk. Pro AI je verbose API výhodou:
- `DataType.Entity` explicitně specifikuje value/reference typ — AI nemusí inferovat
- `WithCustomName("User")` je jednoznačné, žádné magické konvence
- Konzistentní pattern pro všechny user-defined typy = snazší pro AI správně generovat

Lidské pohodlí při psaní testů není dostatečný důvod pro přidání convenience API. Testy si s verbose API poradí.
