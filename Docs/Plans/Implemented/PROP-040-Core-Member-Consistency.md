# PROP-040 Core Member Consistency — IMemberElement, PropertyElement Attributes, XmlSummary

Typ výsledku: Candidate Proposal
Zdroj podnětu: AI — Perplexity Deep Research (konverzace 2293d4a6)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-08

Priorita: High
Oblast: Core
Owner:
Datum vytvoření: 2026-07-08
Aktualizováno: 2026-07-08

Navazuje na:
- PROP-037 (C# Completeness) — EventElement, OperatorElement
- PROP-035 (C#-First Core Migration)
- Perplexity revize: https://www.perplexity.ai/search/2293d4a6-aca7-4219-aeac-8d3285213a71

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Core/Elements/Members/MethodElement.cs`
- `Src/MetaForge.Core/Elements/Members/PropertyElement.cs`
- `Src/MetaForge.Core/Elements/Members/EventElement.cs`
- `Src/MetaForge.Core/Elements/Members/OperatorElement.cs`
- `Src/MetaForge.Core/Abstractions/RootElement.cs`

## 1. Kontext

Perplexity Deep Research identifikoval tři architektonické nekonzistence v Core vrstvě:

1. **MethodElement a PropertyElement nemají společný základ** — ClassElement dědí z RootElement, ale členské elementy jsou samostatné třídy bez společného interface/base. To znemožňuje polymorfní zpracování členů a chybí jim Id, XmlSummary, Attributes (u PropertyElement).

2. **PropertyElement postrádá Attributes** — RootElement a MethodElement Attributes mají, PropertyElement ne. To je problém pro generování EF Core, ASP.NET modelů kde `[Key]`, `[Required]` jsou na properties.

3. **XmlSummary chybí na MethodElement a PropertyElement** — RootElement.XmlSummary existuje, ale MethodElement to nemá. Generátor XML dokumentace potřebuje summary pro každý člen.

## 2. Problém dnes

- **Členové bez společného základu:** Nelze iterovat přes `Všechny členy třídy` bez ohledu na typ. Generátor musí mít separátní logiku pro Method, Property, Event, Operator.
- **PropertyElement bez atributů:** `[Required]`, `[MaxLength]`, `[Key]` na property nelze modelovat. Generátor produkuje kód bez atributů na properties.
- **Chybějící XmlSummary:** Dokumentační XML komentáře nelze přidat na metody a properties — pouze na typy.

## 3. Cíl

- Vytvořit `IMemberElement` interface (případně `MemberElement` base class) společný pro MethodElement, PropertyElement, EventElement, OperatorElement.
- Přidat `Attributes` na PropertyElement (konzistentně s MethodElement a RootElement).
- Přidat `XmlSummary` na MethodElement a PropertyElement.
- Zachovat zpětnou kompatibilitu — doplnit, ne nahradit.

## 4. Architektonické invarianty

- Core nesmí nést logiku, která patří do vyšší vrstvy.
- Členské elementy zůstávají jednoduché — žádná business logika.

## 5. Scope

### In scope
- `IMemberElement` interface s `Name`, `Attributes`, `Metadata`, `XmlSummary`, `Coin`.
- Implementace na MethodElement, PropertyElement, EventElement, OperatorElement.
- Přidání `Attributes` na PropertyElement.
- Přidání `XmlSummary` na MethodElement a PropertyElement.

### Out of scope
- Changes to existing fluent API / factory methods.
- Generátorové změny (budou následovat v samostatném PROP).
- Automatická migrace existujících volajících.

## 6. Návrh řešení

### IMemberElement

```csharp
public interface IMemberElement
{
    string Name { get; }
    List<AttributeElement> Attributes { get; }
    MetadataBag Metadata { get; }
    string? XmlSummary { get; set; }
    int Coin { get; }
}
```

### PropertyElement rozšíření

```csharp
// Stávající PropertyElement dostane:
public List<AttributeElement> Attributes { get; init; } = new();
public string? XmlSummary { get; set; }
```

### MethodElement rozšíření

```csharp
// Stávající MethodElement dostane:
public string? XmlSummary { get; set; }
```

### Implementace

- `IMemberElement` je additive — všechny existující elementy ho implementují.
- Žádné stávající API se nemění.
- Factory metody zůstávají beze změny.

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Src/MetaForge.Core/Abstractions/IMemberElement.cs` — nový
- `Src/MetaForge.Core/Elements/Members/MethodElement.cs` — +XmlSummary, +IMemberElement
- `Src/MetaForge.Core/Elements/Members/PropertyElement.cs` — +Attributes, +XmlSummary, +IMemberElement
- `Src/MetaForge.Core/Elements/Members/EventElement.cs` — +IMemberElement
- `Src/MetaForge.Core/Elements/Members/OperatorElement.cs` — +IMemberElement

### Testy
- Testy na IMemberElement implementaci na všech 4 typech
- Testy na PropertyElement.Attributes (přidání, čtení)
- Testy na XmlSummary na MethodElement a PropertyElement
- 15-20 nových testů

### Dokumentace
- Update Docs/Core/04-Methods.md
- Update New_Architecture/04-Core-Elements.md

## 8. Implementační fáze

### Fáze 1 — IMemberElement interface
- Vytvořit interface
- Implementovat na MethodElement
- Implementovat na PropertyElement
- Implementovat na EventElement, OperatorElement

### Fáze 2 — PropertyElement Attributes
- Přidat `List<AttributeElement> Attributes` na PropertyElement

### Fáze 3 — XmlSummary
- Přidat `string? XmlSummary` na MethodElement a PropertyElement
