# IDEA-003 Roundtrip Boundary Definition

Stav: Idea
Oblast: Core, Generators, Translator
Zdroj: Koumák — analýza Perplexity konverzace (d773bf6a)
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Perplexity analýza identifikovala potřebu explicitně definovat, co jde převést C# → Core → C# bez ztráty (round-trip), co je ztrátové a co není podporované.

## 2. Problém dnes

- Není dokumentované, které C# konstrukce projdou round-trip beze ztráty.
- Generators vrstva může produkovat kód, který při zpětném parsování do Core ztratí informace (např. komentáře, formatting, usingy).
- Translator a Generators nemají společný kontrakt o tom, co je "bezpečné" mapovat.
- To způsobuje nejistotu při code generationu a při write-backu z CoreDetail.

## 3. Předběžný směr řešení

- Vytvořit dokument `Docs/Core/06-Roundtrip-Boundary.md`, který definuje:
  - **Full round-trip**: konstrukce, které jdou převést bez ztráty (např. class s property).
  - **Lossy**: konstrukce, které se převedou, ale ztratí část informace (např. tělo metody → jen signatura).
  - **Unsupported**: konstrukce, které Core neumí reprezentovat (např. některé direktivy preprocessoru).
- Propojit s testy — snapshot testy jako living dokumentace round-trip scénářů.

## 4. Signál hodnoty

- Translator a Generators mají jasný kontrakt.
- Write-back z CoreDetail ví, co může bezpečně zapsat.
- Kvalita: eliminace "tichého" znehodnocování kódu při opakovaném převodu.

## 5. Rizika a nejasnosti

- Round-trip definice se bude měnit s vývojem Core — musí být živý dokument.
- Je potřeba rozhodnout, zda je round-trip cílem, nebo jen užitečnou vlastností.
- Testování round-tripu vyžaduje property-based testing (např. FsCheck) — není triviální.

## 6. Doporučený další krok

- Open Question: Je round-trip C# → Core → C# cílový stav, nebo pouze užitečná vlastnost?
- Po rozhodnutí: Candidate Proposal, pravděpodobně jako součást PROP-034 nebo samostatný PROP.
