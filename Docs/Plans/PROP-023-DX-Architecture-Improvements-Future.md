# PROP-023: DX a architektonická vylepšení na zvážení

> **Stav:** ⚪ Na zvážení (neimplementovat bez schválení)
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-020 (BusinessModel upgrade)

---

## Přehled

Tento dokument obsahuje **4 vylepšení na zvážení** — nejsou kritická pro core flow, ale mohla by výrazně zlepšit architekturu a developer experience. Každé z nich vyžaduje samostatné rozhodnutí před implementací.

---

## 1. Typový SyncState machine

### Současný stav (PROP-020)

```csharp
public enum AttributeSyncState { New, Synced, BusinessEdited, CoreEdited, Conflict }
```

Kontroluje se `if`/`switch` — kompilátor nehlídá pokrytí všech přechodů.

### Návrh

```csharp
public abstract record SyncState
{
    public sealed record New : SyncState;
    public sealed record Synced(DateTimeOffset SyncedAt) : SyncState;
    public sealed record BusinessEdited(SyncState Previous) : SyncState;
    public sealed record CoreEdited(SyncState Previous) : SyncState;
    public sealed record Conflict(string Reason, SyncState Business, SyncState Core) : SyncState;

    public SyncState OnBusinessEdit() => this switch
    {
        Synced s => new BusinessEdited(s),
        CoreEdited c => new Conflict("both edited", new BusinessEdited(c.Previous), c),
        _ => this
    };

    public SyncState OnCoreEdit() => this switch
    {
        Synced s => new CoreEdited(s),
        BusinessEdited b => new Conflict("both edited", b, new CoreEdited(b.Previous)),
        _ => this
    };
}
```

### Výhody
- Kompilátor vynutí pokrytí všech přechodů (exhaustive switch)
- `Conflict` nese kontext — která strana co změnila
- `Synced` nese timestamp — kdy naposledy synchronizováno

### Nevýhody
- Více kódu
- Složitější serializace (JSON polymorfismus)
- Možný over-engineering pro aktuální fázi

### Odhad: 1 den

---

## 2. Layer stack — Git-style vrstvení dokumentu

### Současný stav (PROP-020)

```csharp
public sealed class BusinessAttributeNode
{
    // Uživatelská vrstva
    public string Name { get; init; }
    public string Type { get; init; }

    // Core vrstva (připíchnutá jako sub-objekt)
    public BusinessAttributeCoreDetail? CoreDetail { get; init; }
}
```

### Návrh

Místo sub-objektu — dokument jako **stack vrstev**:

```
BusinessAuthoringDocument
├── Layer 0: UserRaw        ← "potřebuju entitu Customer s emailem"
├── Layer 1: AiTranslated   ← BusinessEntityNode { Name="Customer", ... }
├── Layer 2: CoreMapped     ← TypeModel, ClassElement, PropertyElement
└── Layer 3: AiEnriched     ← CoreDetail { Source=Generated, ... }
```

Každá vrstva je **neměnná** a odkazuje na předchozí.

```csharp
public sealed record LayeredDocument(
    DocumentLayer UserRaw,
    DocumentLayer? AiTranslated,
    DocumentLayer? CoreMapped,
    DocumentLayer? AiEnriched
);

public sealed record DocumentLayer(
    string LayerId,
    DateTimeOffset CreatedAt,
    CoreInfoSource Source,
    JsonObject Content
);
```

### Výhody
- Plný audit trail — kdo, kdy, jakou vrstvu vytvořil
- Rollback na libovolnou vrstvu
- "Rebase" — přepočítat vrstvy 2-3 když se změní vrstva 1
- Příprava na kolaborativní editaci

### Nevýhody
- Zásadní změna architektury — nekompatibilní s PROP-020
- Vyšší paměťová náročnost
- Složitější dotazování (nutnost "flatten" vrstev)

### Odhad: 3-5 dní (zásadní přepracování)

---

## 3. YAML DSL pro BusinessModel

### Návrh

Alternativní reprezentace business modelu v YAML:

```yaml
# business.mf.yaml
project:
  id: payroll-calculation
  name: PayrollCalculation
  description: Výpočet čisté mzdy
  
entities:
  - name: Employee
    summary: Zaměstnanec s osobními a mzdovými údaji
    attributes:
      - name: FirstName
        type: text
        required: true
        constraints:
          - not-empty
          - max-length:100
      - name: Email
        type: email
        required: true
      - name: GrossSalary
        type: money
        required: true
        constraints:
          - greater-than:0
    behaviors:
      - name: CalculateNetSalary
        kind: query
        returns: Money
        summary: Vypočítá čistou mzdu

  - name: Employer
    attributes:
      - name: Name
        type: text
        required: true
```

### Výhody
- Čitelnější pro lidi než JSON
- Git-friendly (snazší diffování)
- Méně syntaxe (žádné `{}`, `[]`, `""`)

### Nevýhody
- Další závislost (`YamlDotNet`)
- Dva formáty = dvojí údržba parserů/serializerů
- YAML má edge cases (boolean parsing `yes`/`no`, čísla s leading zeros)

### Odhad: 1 den (YAML parser + serializer)

---

## 4. Undo/redo přes CommandLog

### Návrh

Protože máme append-only log, undo = **explicitní `UndoCommand`**:

```csharp
public sealed class UndoCommandOp : IPatchOperation
{
    public string CommandType => "Undo";
    public string TargetCommandId { get; } // ID commandu, který se vrací
}
```

Při aplikaci `UndoCommandOp` se vytvoří **inverzní command** a aplikuje:

```
CommandLog:
  [5] AddEntity "Customer"              ← user vytvořil entitu
  [6] AddAttribute "Email" type="email" ← user přidal atribut
  [7] Undo [5]                          ← user kliknul "undo create entity"
  [8] DeleteEntity "Customer"           ← inverzní command (automaticky)
```

### Alternativa: Command reverzibilita

Místo `UndoCommand` — každý `IPatchOperation` implementuje `Invert()`:

```csharp
public interface IPatchOperation
{
    string CommandType { get; }
    void Apply(BusinessAuthoringDocument document);
    CommandEnvelope ToEnvelope();
    IPatchOperation? Invert(); // null = nelze vrátit
}

// AddEntityOp.Invert() → DeleteEntityOp
// DeleteEntityOp.Invert() → AddEntityOp (s původními daty)
```

### Výhody
- Plný undo/redo pro všechny operace
- Včetně AI enrichmentu
- Připraveno pro UI

### Nevýhody
- Ne všechny operace jsou reverzibilní (např. změna typu atributu)
- `Invert()` musí nést dostatek kontextu
- Přidává komplexitu do každé operace

### Odhad: 1-2 dny

---

## Souhrnné porovnání

| # | Vylepšení | Přínos | Cena | Riziko | Doporučení |
|---|-----------|--------|------|--------|------------|
| 1 | Typový SyncState | Typová bezpečnost | 1 den | Nízké | ✅ Zvážit po PROP-020 |
| 2 | Layer stack | Audit trail, rollback | 3-5 dní | 🔴 Vysoké (přepis arch.) | ⚪ Odložit na v2 |
| 3 | YAML DSL | Čitelnost pro lidi | 1 den | Nízké | ⚪ Zvážit až bude UI |
| 4 | Undo/redo | UX pro editor | 1-2 dny | Střední | ⚪ Zvážit až bude UI |

---

## Rozhodovací kritéria

Před implementací kteréhokoliv z těchto vylepšení je třeba zodpovědět:

1. **Existuje konkrétní use case?** — Kdo to bude používat a jak často?
2. **Přináší to hodnotu teď, nebo až později?** — Neimplementovat "do zásoby"
3. **Jak to ovlivní existující kód?** — Backward compatibility, migrace
4. **Je na to kapacita?** — Každé vylepšení = 1-5 dní
