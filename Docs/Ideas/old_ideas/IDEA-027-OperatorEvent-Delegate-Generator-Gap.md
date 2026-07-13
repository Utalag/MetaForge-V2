# IDEA-027 Operator, Event a Delegate — Gap mezi Core modelem a Generátorem

Stav: Idea
Oblast: Core, Generators
Zdroj: Koumák + Perplexity konverzace db7f49c1-8f18-4f2e-8da2-b9b6e1f5c122
Datum vytvoření: 2026-07-11
Poslední revize: 2026-07-11

## 1. Kontext

Perplexity si všimla architektonického rozporu: *"Pokud `OperatorElement` a `EventElement` existují v Core, ale generátor je neumí, je otázka, jestli je Core napřed před contractem, nebo generator pozadu."*

Analýza kódu potvrdila:
- **DelegateElement** — existuje v Core (včetně factory metod, TypeParameters, GenericConstraints), CodeGenerator ho umí generovat (`GenerateDelegate`), ale neexistuje snapshot test.
- **OperatorElement** — existuje v Core (včetně `OperatorKind` enumu, `IMemberElement` implementace), ale CodeGenerator ho **nerenderuje**.
- **EventElement** — existuje v Core (včetně factory metod, Add/Remove accessorů), ale CodeGenerator ho **nerenderuje**.

## 2. Problém dnes

- **Nekonzistence**: Delegate je napůl hotový (generátor umí, chybí testy), Operator a Event jsou pouze v Core.
- **Falešný signál**: Core.Tests testují Delegate/Event/Operator elements, což vytváří dojem, že jsou plně podporované. Ale generátor je neumí → uživatel si je nadefinuje v Core modelu, ale generátor vyprodukuje `// Nepodporovaný element` komentář.
- **Architektonická nejistota**: Mají být tyto elementy součástí authoring kernelu, nebo jsou to "C#-specific detaily", které by měl řešit až Translator? Dokumentace `03-Core-Abstractions.md` a `04-Core-Elements.md` o nich mlčí.
- **Chybějící testy pro Delegate**: Delegate je jediný element, který generátor umí, ale nemá snapshot test.

## 3. Předběžný směr řešení

### Varianta A: Dokončit generátor (Delegate testy + Operator + Event)
- Doplnit snapshot testy pro Delegate (basic, generic, s parametry)
- Implementovat `GenerateOperator` v CodeGenerator (štábní kultura: operátorová overload → `public static ReturnType operator Op(ParamType param) => ...`)
- Implementovat `GenerateEvent` v CodeGenerator (field-like event: `public event EventType Name;` nebo add/remove)
- Aktualizovat Scriban templaty (Class.scriban, Interface.scriban) pro operátory a eventy
- Aktualizovat dokumentaci `04-Core-Elements.md` a `10-Generators.md`

### Varianta B: Vyřadit z Core a řešit jinde
- Přesunout Operator a Event do specializovaných ForgeBlock balíků nebo do Translator vrstvy
- Označit jako "internal capability" v Support Matrix (IDEA-024)
- Zachovat Delegate jako public supported (je běžný C# konstrukt)

### Varianta C: Status quo + štítky
- Označit stávající implementaci štítkem `[Experimental]` nebo `[EditorBrowsable(Never)]`
- Nerozšiřovat, pokud o to nepožádá konkrétní PROP

## 4. Signál hodnoty

- **Konzistence**: Core model a generátor si odpovídají.
- **Důvěryhodnost platformy**: uživatel nenarazí na "definoval jsem operátor, ale generátor ho ignoruje".
- **Čistý kontrakt** (viz IDEA-024): bude jasné, co je supported a co není.
- **Perplexity tento rozpor identifikovala jako symptom širšího problému** (Core napřed před contractem).

## 5. Rizika a nejasnosti

- Operator a Event jsou okrajové C# featury — otázka, zda stojí za implementaci, pokud nejsou v produktovém backlogu.
- Delegate je běžnější — chybějící snapshot test je spíš opomenutí.
- Scriban šablony pro operátory mohou být složité (operátorová overload má specifickou syntaxi).
- Rozhodnutí závisí na tom, zda MetaForge cílí na "plný C# surface" nebo "užší authoring subset" (Perplexity otázka).

## 6. Aktuální stav

✅ Převedeno na Follow-up → PROP-052. Implementace již hotova (PROP-037+043+045), zbývají jen snapshot testy.

## 7. Doporučený další krok

**Follow-up** — minimálně:
1. Dopsat snapshot testy pro Delegate (nízká námaha, vysoký přínos).
2. Rozhodnout (v rámci IDEA-026 nebo IDEA-024) o statusu Operator a Event.

Navazuje na: `CodeGenerator.cs`, `ClassElement.cs` (operator/event listy), `NewElementTypesTests.cs`
Závisí na: rozhodnutí o šířce C# surface (IDEA-024)
