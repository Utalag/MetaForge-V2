# IDEA-008 Incremental Dirty-Tracking pro Source Generator

Stav: Idea
Oblast: Core, Generators
Zdroj: Koumák — Perplexity konverzace e0609fe1
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Perplexity navrhlo incremental dirty-tracking pro Core elementy — každý ClassElement nese verzi/hash, generátor přeskočí nezměněné prvky. Kritické pro source generator performance.

## 2. Problém dnes

- Generátor vždy generuje všechny elementy znovu.
- Pro velké modely (100+ tříd) je to pomalé.
- Chybí mechanismus pro detekci změn.

## 3. Předběžný směr řešení

- `RootElement` získá `ContentHash` (SHA256 nebo rychlejší hash).
- `TypeModel` si udržuje `Version` counter.
- Generátor porovnává hash před generováním.
- MVP: content hash na elementu. Plná verze: dirty-tracking graph.

## 4. Signál hodnoty

- Výkon: generování přeskočí nezměněné elementy.
- Source generator kompatibilita.

## 5. Rizika a nejasnosti

- Hash musí být deterministický.
- Jak hashovat Expression/Statement stromy?
- Potřebujeme plný dirty-tracking graph, nebo stačí content hash?

## 6. Doporučený další krok

- Open Question: Content hash, nebo plný dirty-tracking graph?
- Po rozhodnutí: Follow-up na PROP-038 nebo samostatný PROP.
