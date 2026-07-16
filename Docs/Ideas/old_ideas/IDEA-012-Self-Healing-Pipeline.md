# IDEA-012 Self-Healing Pipeline

Stav: Dropped — odloženo na neurčito jako příliš experimentální (2026-07-11)
Oblast: Core, AI, Generators
Zdroj: For_Inspiration/Architecture-Define/08-Methods-and-SelfHealing.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní projekt MetaForge obsahoval propracovaný koncept Self-Healing — opravné smyčky pro AI-generovaný kód. Při analýze gapů mezi původním konceptem a současnou implementací (New_Architecture) byl Self-Healing identifikován jako největší chybějící koncept — není pokryt žádným existujícím PROPem.

Nápad vychází z `08-Methods-and-SelfHealing.md`, kde je popsáno:
- `ApplyCodeGenerationActions` se 3 průchody (Guard extraction, Hoist AddVar, Emit delayed guards)
- Detekce AI chyb: špatné pořadí `AddVar`, chybějící return větve (CS0161), halucinace konstruktorů, duplicitní guard clauses
- Fallback strategie: AI selže → deterministický fallback

## 2. Problém dnes

- `MethodElement.Body` je nyní Statement AST (PROP-031), ale chybí nástroje pro opravu chybně vygenerovaného AST.
- AI generátor může produkovat strukturálně vadný kód — chybí smyčka "vygeneruj → zvaliduj → oprav".
- Roslyn kompilace selže, ale uživatel neví, co opravit.
- Fail-fast bez self-healing = špatná UX pro AI-assisted code generation.
- Současné PROPy (031, 035) řeší AST a C#-first model, ale neřeší "co když AI vygeneruje blbost".

## 3. Předběžný směr řešení

Architektonický řez nad Core + AI vrstvou:

- **Detection stage**: Roslyn kompilace AST → seznam chyb (CS0161, CS0103, CS0117, ...)
- **Healing stage**: mapování známých chyb na strukturované opravy AST (přidat return, přesunout deklaraci, opravit název proměnné)
- **Validation stage**: opakovaná kompilace po healingu — max N iterací
- **Fallback**: pokud healing selže → deterministický fallback (throw `NotImplementedException` nebo generický stub)
- **Prehealing** (volitelné): validace AST před exportem — odchytí chyby ještě před kompilací

Dotčené vrstvy: Core (AST manipulace), AI (Healing segment), Generators (renderování), Tests (ověření healing logiky).

## 4. Signál hodnoty

- Uživatel dostane validní kód i z neperfektního AI generování.
- Snižuje frustraci z "AI vygenerovalo skoro správně, ale nezkompiluje se".
- Umožňuje agresivnější AI generování (víc zkusit, opravit později).
- Zapadá do `TentativePlan` jako součást produktizace — kvalita výstupu je klíčová pro MVP.

## 5. Rizika a nejasnosti

- Healing může opravit chybu způsobem, který změní sémantiku — jak poznat, že je oprava korektní?
- Kolik iterací healingu je rozumné? (riziko nekonečné smyčky)
- Jak oddělit "AI chyba" od "uživatel napsal špatný záměr"?
- OQ-xxx: Má healing běžet automaticky (součást `GenerateCodeAsync`) nebo explicitně (tool `heal code`)?

## 6. Aktuální stav

❌ Odloženo na neurčito — příliš experimentální, nízká priorita pro MVP.
Návrh nebyl dostatečně vyzrálý; healing by vyžadoval složité AST manipulace
s rizikem sémantických změn, které by bylo obtížné testovat a auditovat.
PROP-050 archivován v `Docs/Plans/Dropped/`.

## 7. Původní doporučený další krok

Candidate Proposal — navazuje na PROP-031 (Statement AST) a PROP-035 (C#-first Core). Měl by být plánován až po stabilizaci Statement systému a Expression rendereru.

Vazby: PROP-031, PROP-035, PROP-036 (Specification Layer — invarianty mohou definovat "co je validní")

## 8. Nový směr (2026-07-16)

Původní koncept Self-Healing Pipeline byl vyhodnocen jako příliš experimentální.  
PROP-050 byl zamítnut 2026-07-11 (složité AST manipulace, riziko sémantických změn, obtížná testovatelnost).

**Nový směr** vychází z Perplexity konverzace e2801d78 (2026-07-16) a posouvá těžiště od "AST-patchingu" k **"user-facing resilience"**:

- **PROP-057** — ElementContract: elementy nesou svůj sémantický kontrakt (základ pro validní opravy)
- **PROP-058** — Sandbox Preview Runner: spouštění metod v izolaci (compile gate + execution)
- **PROP-059** — Resilience & Healing Layer: řízená resilience s audit trailem a 4 politikami

**Klíčový rozdíl**: Místo "AI magicky opraví AST" jde o "řízenou resilience vrstvu, která nepustí uživatele k zemi, ale zároveň nerozbije architektonické invarianty".

Viz [PROP-059 detail](../../Plans/PROP-059-Resilience-Healing-Layer.md) a [PROP-050 srovnání](../../Plans/Dropped/PROP-050-Self-Healing-Pipeline.md#9-nástupnický-návrh-2026-07-16).
