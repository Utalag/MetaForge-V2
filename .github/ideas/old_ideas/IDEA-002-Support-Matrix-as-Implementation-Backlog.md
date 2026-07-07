# IDEA-002 Support Matrix as Implementation Backlog

Stav: Idea
Oblast: Core, Governance, Workflow
Zdroj: Koumák — rozšíření IDEA-001 (Perplexity konverzace d773bf6a)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Pokud vznikne Core Reference s podporovou maticí (IDEA-001), samotná matice může sloužit jako živý backlog pro další implementaci. Prvky označené jako `Planned` nebo `Partial` jsou přímými kandidáty na nové PROP návrhy.

## 2. Problém dnes

- Backlog v `PROPOSALS_NEXT.md` a `PROPOSALS.md` není přímo provázaný s tím, co Core umí a co ne.
- Není snadné zjistit, která C# konstrukce chybí a jaká je její priorita.
- Rozhodování o tom, co implementovat dál, je ad-hoc, ne datově podložené.

## 3. Předběžný směr řešení

- Vytvořit v `Docs/Core/` indexovou matici (např. `Docs/Core/00-Support-Matrix.md`), která bude obsahovat přehled všech konstrukcí a jejich stav.
- Každý řádek matice může odkazovat na existující `PROP-xxx` nebo být podkladem pro nový.
- Při pravidelné revizi (např. měsíčně) se matice vyhodnotí a chybějící prvky se prioritizují.

Formát matice:

| Konstrukce | Stav podpory | Core reprezentace | PROP vazba | Priorita |
|-----------|-------------|------------------|------------|----------|
| Class | Supported | TypeKind.Class | PROP-0xx | Hotovo |
| Record Class | Partial | — | PROP-xxx | High |
| Primary Constructors | Unsupported | — | — | Medium |
| ... | ... | ... | ... | ... |

## 4. Signál hodnoty

- Governance: rozhodování o dalším směru je podložené daty.
- Planning agent může přímo číst matici a navrhovat PROP dokumenty.
- Product owner vidí jedním pohledem, co chybí.

## 5. Rizika a nejasnosti

- Matice musí být udržovaná — hrozí zastarání.
- Kdo matici reviduje a jak často?
- Má být matice ručně psaná, nebo generovaná z kódu (např. z `SupportedCSharpConstructs` atributů)?

## 6. Doporučený další krok

- Open Question: Jak matici udržovat živou? (ručně vs generovaně)
- Po rozhodnutí: Follow-up na PROP-034 nebo samostatný PROP.
