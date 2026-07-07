# IDEA-004 Method Expression Boundary Clarification

Stav: Idea
Oblast: Core, Translator
Zdroj: Koumák — analýza Perplexity konverzace (d773bf6a)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Perplexity analýza upozornila, že chybí jasná hranice mezi tím, co je v Core reprezentované strukturovaně (AST/expression model) a co už je jen text, AI body nebo mimo model.

## 2. Problém dnes

- `MethodElement` v Core řeší signaturu, ale není jasné, co z těla metody je strukturované.
- Expression system (`ComputedExpression`, `ComputedOperation`, Statement AST) existuje, ale není explicitně vymezené, co do něj patří.
- Translator neví, zda má tělo metody překládat strukturovaně, nebo ho nechat jako text/AI body.
- To způsobuje nekonzistence: někdy je tělo metody prázdné, jindy obsahuje text, jindy strukturovaný AST.

## 3. Předběžný směr řešení

- Vytvořit dokument `Docs/Core/05-Expressions-and-AST.md`, který definuje:
  - Jaké výrazy a statementy jsou reprezentované strukturovaně.
  - Co je "method body boundary" — kdy končí strukturovaný AST a začíná text/AI body.
  - Mapování mezi C# výrazy a Core expression model.
  - Pravidla pro future rozšíření (jak přidat nový typ výrazu).
- Doplnit do `MethodElement` property `MethodBodyKind`: `None | Structured | Text | AiBody`.

## 4. Signál hodnoty

- Translator ví, jak s tělem metody nakládat.
- AI vrstva ví, kdy může generovat strukturovaný AST a kdy jen text.
- Core je čistší — explicitní hranice místo implicitního předpokladu.

## 5. Rizika a nejasnosti

- Přidání `MethodBodyKind` do `MethodElement` je breaking change pro existující kód.
- Je potřeba rozhodnout, jestli jít cestou "všechno je text dokud není strukturované" nebo "všechno je strukturované dokud není označeno jako text".
- Expression model je stále ve vývoji — dokumentace může zastarat.

## 6. Doporučený další krok

- Open Question: Jaký je default `MethodBodyKind` pro nově parsované metody?
- Po rozhodnutí: Candidate Proposal, pravděpodobně jako součást PROP-031 (Core Statement System) nebo follow-up.
