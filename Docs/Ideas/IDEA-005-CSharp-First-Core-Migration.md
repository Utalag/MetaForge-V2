# IDEA-005 C#-First Core Migration

Stav: Candidate (převedeno na PROP-035)
Oblast: Core, Translator, Generators, Docs
Zdroj: Koumák — Perplexity konverzace 05663298 (5 dotazů o C#-first paradigmatu)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Perplexity analýza potvrdila, že MetaForge Core je fakticky už napůl C#-first a setrvávání v "language-agnostic" narativu jen prodlužuje architektonický dluh. Core by se měl explicitně deklarovat jako C# semantic model.

## 2. Problém dnes

- Elementy předstírají language-agnostic, ale reálně nesou C# sémantiku (IsRecord, IsAsync, IsSealed...)
- Chybí C# koncepty: Namespace, PrimaryConstructor, ExpressionBody, TypeParameters
- Translator dělá heuristické mapování místo přímé projekce
- AI prompting je složitější, když interní model neodpovídá výstupu 1:1

## 3. Předběžný směr řešení

7-commit migrace: RootElement → ClassElement → MethodElement → Expressions → Translator → Tests → Docs. Aditivní změny, nic se nemaže. Core = C# sémantický, ne syntaktický.

## 4. Signál hodnoty

- Jednodušší Translator (míň heuristik)
- Lepší AI prompting (model = výstup 1:1)
- Čistší architektura (explicitní místo implicitního)
- Snadnější rozšiřování o nové C# featury

## 5. Rizika a nejasnosti

- False economy pro budoucí TypeScript/Python generátory
- Semantic leakage: C#-first nesmí znamenat "C# syntax v Core"

## 6. Doporučený další krok

- ✅ Převedeno na Candidate → PROP-035
