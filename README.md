# MetaForge V2

> **Váš AI agent modeluje, vy dostáváte hotový kód.**
> MetaForge je engine pro AI agenty: agent přes MCP zadá business model → platforma vrátí strukturovaný C# kód. Bez vendor lock-inu, bez magie, s plnou kontrolou.

[![Build](https://img.shields.io/badge/build-passing-brightgreen)](#)
[![Tests](https://img.shields.io/badge/tests-500%2B-brightgreen)](#)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](#)
[![License](https://img.shields.io/badge/license-MIT-lightgrey)](#)

---

## Proč MetaForge?

### Hlavní scénář: AI agent orchestruje, vy rozhodujete

Největší síla MetaForge není v CLI — je v tom, že **AI agent (Copilot, Claude, GPT) může přes MCP protokol automaticky modelovat a generovat kód**. Vy jen komunikujete s agentem, agent dělá práci.

```
┌───────────────────────────────────────────────────────────────┐
│  VY (uživatel)                                                │
│  "Potřebuju aplikaci pro správu autoservisu."                 │
│  "Zákazník má jméno, email, telefon. Auto má SPZ, značku."    │
│  "Objednávka má datum, cenu, stav."                           │
└─────────────────────────┬─────────────────────────────────────┘
                          ↓ hovor s agentem
┌───────────────────────────────────────────────────────────────┐
│  AI AGENT (Copilot, Claude, GPT)                              │
│  Rozumí vašemu záměru → volá MCP tooly                       │
└─────────────────────────┬─────────────────────────────────────┘
                          ↓ MCP protokol
┌───────────────────────────────────────────────────────────────┐
│  METAFORGE ENGINE                                            │
│  add-entity → add-attribute → generate                        │
│  Vrací strukturovaný přehled: co bylo vytvořeno, jaký kód    │
└─────────────────────────┬─────────────────────────────────────┘
                          ↓
┌───────────────────────────────────────────────────────────────┐
│  VY dostanete:                                                │
│  ✅ Přehled modelu (entity, atributy, vztahy)                │
│  ✅ Vygenerovaný C# kód (třídy, validace, DbContext…)        │
│  ✅ CommandLog (kdo, kdy, co změnil — audit trail)           │
│  ✅ Žádný vendor lock-in (kód je váš)                        │
└───────────────────────────────────────────────────────────────┘
```

**Vy se soustředíte na business需求, agent obstará technickou implementaci.**

### Problém tradičního vývoje (a proč agent bez enginu nestačí)

AI agenti umí psát kód, ale:
- ❌ **Nemají kontext** — neví, jakou máte architekturu, jaké patterny používáte
- ❌ **Halucinují** — vygenerují `DbContext` bez connection stringu, zapomenou DI registraci
- ❌ **Jsou nekonzistentní** — každý prompt = jiný výstup, jiné názvy, jiné uspořádání
- ❌ **Nemají paměť** — příště neví, co už vygenerovali
- ❌ **Chybí audit** — kdo a kdy změnil model? Co byl předchozí stav?

MetaForge řeší všechny tyto problémy:

| Problém agenta | MetaForge řešení |
|----------------|------------------|
| ❌ Nemá kontext architektury | ✅ Engine má pevnou architekturu (Core → Translator → Generators) |
| ❌ Halucinace | ✅ Deterministický překlad — stejný vstup = stejný výstup |
| ❌ Nekonzistentní výstup | ✅ Šablony garantují jednotný pattern |
| ❌ Žádná paměť | ✅ CommandLog = kompletní historie změn |
| ❌ Chybí audit | ✅ Každý command je zalogovaný s provenance |

### Výhody oproti čistě ručnímu psaní

| MetaForge | Ruční psaní |
|-----------|-------------|
| ✅ **AI agent modeluje za vás** — stačí popsat business | ❌ Musíte psát každou třídu ručně |
| ✅ **Změna na jednom místě** — upravíte atribut v modelu, vše se přegeneruje | ❌ Změna = opravit 5+ souborů ručně |
| ✅ **Konzistentní výstup** — stejný pattern pro všechny entity | ❌ Každý vývojář píše jinak |
| ✅ **Generování za sekundy** — celý model → C# kód | ❌ Hodiny boilerplatu |
| ✅ **Audit trail** — každá změna je v CommandLogu | ❌ Kdo, kdy a proč změnil? |
| ✅ **Migrace zadarmo** — replay command logu = nový model | ❌ Ruční migrační skripty |
| ✅ **Bez vendor lock-inu** — generovaný C# kód je váš | ✅ (stejné) |
| ✅ **AI-ready** — MCP protokol pro agenty | ❌ Agent nemá kde brát kontext |

### A co nevýhody?

- 🟡 **Není vhodný pro UI/generický kód** — generujeme domain vrstvu, ne ASP.NET controllery (zatím)
- 🟡 **Křivka učení** — musíte pochopit, jak agent pracuje s MetaForge modelem
- 🟡 **Generátor není debugger** — chyby v modelu se projeví až v generovaném kódu
- ✅ **Vendor lock-in: NENÍ** — generovaný kód jsou standardní C# třídy bez závislosti na MetaForge runtime

---

## K čemu MetaForge je?

**MetaForge je engine pro AI agenty, kteří generují doménovou vrstvu .NET aplikací.**

Dva režimy použití:

### Režim 1: AI Agent (doporučeno — hlavní scénář)

Uživatel komunikuje s AI agentem (např. GitHub Copilot, Claude, GPT). Agent volá MetaForge přes MCP protokol:

```bash
# Agent volá automaticky — vy nevidíte tyto příkazy
add-entity "Customer"
add-attribute <id> "Email" --type email --required true
add-attribute <id> "Phone" --type phone
generate --output ./Domain
```

Vy dostanete:
- ✅ **Přehledný strukturovaný zápis** — co bylo vytvořeno, jaké atributy, jaké typy
- ✅ **Hotový C# kód** — připravený k použití
- ✅ **Možnost dále model upravovat** — přes agenta nebo přímo CLI

### Režim 2: CLI (přímé použití)

Bez agenta — přímo přes command line:

```bash
dotnet run --project Src/MetaForge.Cli -- add-entity "Customer"
dotnet run --project Src/MetaForge.Cli -- generate --output ./Domain
```

---

## Co MetaForge umí generovat? ✅

### Domain vrstva (produkční kvalita)

| Výstup | Popis | Ukázka |
|--------|-------|--------|
| **Třídy** | public/abstract/sealed/static/partial/record | `public class Customer { … }` |
| **Rozhraní** | s async metodami, generiky | `public interface IUserRepository { Task<User?> GetByIdAsync(Guid id); }` |
| **Enumy** | vč. [Flags] | `public enum OrderStatus { Draft, Confirmed, Shipped }` |
| **Structy** | readonly, record | `public readonly record struct Position { … }` |
| **Value Objects** | Vogen [ValueObject] | `[ValueObject] public readonly partial struct CustomerId { }` |
| **Properties** | get/set, get-only, init, required, static | `public required string Name { get; init; }` |
| **Methods** | async, static, expression-bodied, params | `public async Task<User?> GetByIdAsync(Guid id) => …` |
| **Constructors** | basic, s parametry, private, static | `public Customer(string name) { Name = name; }` |
| **Fields** | readonly, const, static | `private readonly IUserRepository _repository;` |
| **Delegáty** | basic, generic, internal | `public delegate void ActionHandler(string message);` |
| **Expressions** | 14 typů (binary, lambda, switch, await, null-coalescing…) | `(a + b)`, `x => x.Name`, `await task` |
| **Statements** | 13 typů (if, for, foreach, while, try-catch, switch…) | `if (x > 0) { return x; }` |

### S ForgeBlock pluginy

| Plugin | Generuje |
|--------|----------|
| **EF Core** 🔧 | DbContext, entity konfigurace |
| **AutoMapper** 🔧 | Profile třídy s CreateMap |
| **FluentValidation** 🔧 | AbstractValidator s validačními pravidly |

---

## Jak MetaForge funguje?

### Architektura

```
┌───────────────────────────────────────────────────────────────┐
│  AI AGENT (Copilot, Claude, GPT)                             │
│  Komunikuje s vámi → volá MCP tooly                          │
│  "add-entity", "add-attribute", "generate"                   │
└─────────────────────────┬─────────────────────────────────────┘
                          ↓ MCP protokol
┌───────────────────────────────────────────────────────────────┐
│  METAFORGE ENGINE                                            │
│                                                               │
│  BusinessAuthoringDocument  ←  source of truth                │
│  • Entity, atributy, vztahy, chování                          │
│  • Append-only CommandLog (každá změna je zaznamenaná)       │
│                                                               │
│  Translator → Core elementy                                   │
│  • Deterministický — stejný vstup = stejný výstup            │
│  • Atribut "email" → TypeModel.String + validační pravidla   │
│                                                               │
│  CodeGenerator → C# soubory                                   │
│  • Scriban šablony (Class, Interface, Enum, Struct…)          │
│  • Expression/Statement renderer (14+13 typů)                 │
└─────────────────────────┬─────────────────────────────────────┘
                          ↓
┌───────────────────────────────────────────────────────────────┐
│  VÝSTUP PRO UŽIVATELE                                        │
│  ✅ Strukturovaný přehled modelu                             │
│  ✅ Standardní C# .cs soubory (bez závislosti)               │
│  ✅ CommandLog historie (kdo, kdy, co změnil)                │
│  ✅ Žádný vendor lock-in                                     │
└───────────────────────────────────────────────────────────────┘
```

### Životní cyklus (pohledem uživatele)

```
1. Řeknete agentovi: "Chci aplikaci pro správu autoservisu."
2. Agent přes MCP:
     add-entity "Customer"
     add-attribute "Name" --type string --required true
     add-attribute "Email" --type email
     add-entity "Car"
     add-attribute "LicensePlate" --type string
3. Vygeneruje:
     generate --output ./Domain
4. Vy dostanete:
     ✅ "Vytvořeny entity: Customer (3 atributy), Car (1 atribut)"
     ✅ "Vygenerováno 5 souborů do ./Domain/"
     ✅ "CommandLog: 6 commandů"
5. Můžete dál upravovat:
     "Přidej k objednávce atribut Cena."
     → Agent provede add-attribute, vygeneruje znovu
```

---

## Není to vendor lock-in?

**Není.** Generovaný kód je standardní C# bez jakékoli závislosti na MetaForge:

```csharp
// Toto je váš kód. Žádná závislost na MetaForge.
// Můžete ho upravit, rozšířit, commitnout — je váš.
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

- ✅ **Žádný MetaForge runtime** v produkci
- ✅ **Žádné magické atributy** nebo base třídy
- ✅ **Standardní .cs soubory** — fungují s jakýmkoli .NET projektem
- ✅ **Git-friendly** — generovaný kód commitnete, diff funguje
- ✅ **Můžete přestat používat MetaForge** kdykoli — kód zůstane

---

## Srovnání s alternativami

| | MetaForge + AI agent | Ruční psaní | Roslyn source generators |
|---|---|---|---|
| **Vstup** | Konverzace s agentem | C# kód | C# code attributes |
| **AI integrace** | ✅ **Nativní (MCP)** | ❌ | ❌ |
| **Změna** | ✅ Jedno místo (řeknete agentovi) | ❌ 5+ míst | ✅ Jedno místo |
| **Kontrola výstupu** | ✅ Plná (kód je váš) | ✅ Plná | ❌ Černá skříňka |
| **Vendor lock-in** | ❌ Žádný | ❌ Žádný | ⚠️ Roslyn API |
| **Audit trail** | ✅ CommandLog | ❌ | ❌ |
| **Rychlost nasazení** | 🟢 Minuty | 🔴 Hodiny | 🟡 Střední |

---

## Rychlý start — CLI (pro vývojáře a testing)

```bash
# Build a testy
dotnet build MetaForge.slnx
dotnet test MetaForge.slnx       # ~500+ testů, vše zelené ✅

# Modelování
dotnet run --project Src/MetaForge.Cli -- add-entity "Customer"
dotnet run --project Src/MetaForge.Cli -- add-attribute <ID> "Email" --type email --required true

# Generování
dotnet run --project Src/MetaForge.Cli -- generate --output ./MyDomain

# Uložení
dotnet run --project Src/MetaForge.Cli -- save
```

### CLI reference

| Command | Význam |
|---------|--------|
| `add-entity <name>` | Nová business entita |
| `list-entities` | Seznam všech entit |
| `projection [--entity]` | Detail modelu |
| `add-attribute <id> <nazev> [--type] [--required]` | Přidá atribut |
| `delete-entity <id>` | Smaže entitu |
| `info` | Stav projektu |
| `generate [--output]` | **Vygeneruje C# kód** |
| `save` | Uloží model na disk |

---

## ForgeBlock pluginy

| Plugin | Generuje | Stav |
|--------|----------|------|
| EF Core | DbContext, entity konfigurace | ✅ Šablony, 🔧 DI |
| AutoMapper | Profile s mapováními | ✅ Šablony, 🔧 DI |
| FluentValidation | Validátory s pravidly | ✅ Šablony, 🔧 DI |

---

## Kdy MetaForge (ne)použít

### ✅ Použijte když

- Chcete AI agentovi říct "udělej mi aplikaci" a dostat hotový kód
- Vytváříte novou .NET business aplikaci
- Máte mnoho entit s podobnou strukturou
- Chcete konzistentní domain vrstvu napříč týmem
- Potřebujete audit trail změn v modelu

### ❌ Nepoužívejte když

- Píšete UI, prezentaci nebo infrastrukturní kód
- Máte velmi specifickou netriviální business logiku (generátor vytvoří stub)
- Preferujete čistě ruční psaní bez nástrojů
- Potřebujete podporu pro jiný jazyk než C# (zatím)

---

## Kde najít víc

| Kam | Co tam je |
|-----|-----------|
| [`New_Architecture/00-Index.md`](New_Architecture/00-Index.md) | Kompletní architektonická dokumentace (30 dokumentů) |
| [`Progress.md`](Progress.md) | Chronologický log všech změn |
| [`PROPOSALS.md`](PROPOSALS.md) | Plánované a dokončené featury |
| [`AuditLog/2026-07-12-Stavova-Analyza.md`](AuditLog/2026-07-12-Stavova-Analyza.md) | Detailní stavová analýza platformy |

---

## Licence

MIT. Generovaný kód je váš — můžete ho používat pod libovolnou licencí.
