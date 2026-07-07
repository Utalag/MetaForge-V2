# IDEA-006 Core Specification Layer — Invarianty, Validace, Test Generation

Stav: Candidate (převedeno na PROP-036)
Oblast: Core, Tests, BusinessModel, AI
Zdroj: Koumák — Perplexity konverzace 05663298 (5 dotazů o invariantech a test generation)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Elementy v MetaForge Core mají nevalidní kombinace vlastností (např. IsAsync && IsAbstract). Tyto invarianty nejsou nikde deklarované — jsou jen implicitně. Myšlenka: povýšit invarianty na first-class specification artifact.

## 2. Problém dnes

- Žádný zdroj pravdy pro invarianty
- Duplicitní validace napříč kódem, testy, dokumentací
- Chybějící test generation z invariantů
- AI nemá strukturovaný vstup pro návrhy pravidel

## 3. Předběžný směr řešení

`Core/Specifications/` namespace s InvariantDefinition, boolean AST (InvariantExpression), IInvariantEvaluator. StrongType zůstává pro value-level constraints. Jeden zdroj pravdy → runtime validace + test generation + AI guardraily.

## 4. Signál hodnoty

- Jeden zdroj pravdy pro invarianty
- Automatické generování testů z invariantů (FsCheck)
- AI může navrhovat nová pravidla strukturovaně
- Uživatel může definovat vlastní doménové invarianty

## 5. Rizika a nejasnosti

- Riziko rozpadu zdroje pravdy (duplicitní systémy)
- Riziko overengineeringu (general-purpose rule engine)
- AI-generated chyby (tiché zavedení špatného invariantu)

## 6. Doporučený další krok

- ✅ Převedeno na Candidate → PROP-036
