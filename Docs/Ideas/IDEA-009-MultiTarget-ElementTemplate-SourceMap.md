# IDEA-009 MultiTarget Emitter + ElementTemplate + SourceMap + AssemblyMeta

Stav: Idea
Oblast: Generators, Core
Zdroj: Koumák — Perplexity konverzace e0609fe1
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

Perplexity navrhlo několik nápadů odložených na později. MultiTarget emitter (jeden model → C#/.d.ts/OpenAPI), ElementTemplate (parametrizované šablony), SourceMap (mapování kódu zpět na element), AssemblyMetaElement (AssemblyInfo.cs).

## 2. Problém dnes

- Generátor produkuje jen C# — žádné další formáty.
- Žádné šablony pro opakované vzory (CRUD controller, DTO).
- Chybí mapování vygenerovaného kódu → původní element.
- Assembly metadata nejsou modelována.

## 3. Předběžný směr řešení

- MultiTarget: `IEmitTarget` interface, C#/TypeScript/OpenAPI implementace.
- ElementTemplate: `ElementTemplate<CrudController>(entityType: TypeRef)` expandující do MethodElement[].
- SourceMap: `LineMapping` kolekce mapující řádek výstupu → `ElementPath`.
- AssemblyMetaElement: verze, InternalsVisibleTo, CLSCompliant.

## 4. Signál hodnoty

- MultiTarget: jeden model pro C#, TypeScript, OpenAPI.
- ElementTemplate: eliminuje boilerplate.
- SourceMap: debugování generovaného kódu.

## 5. Rizika a nejasnosti

- Nízká priorita — nejdřív musí být stabilní Core.
- MultiTarget může být overengineering, pokud TypeScript/Python nejsou priorita.

## 6. Doporučený další krok

- Zatím jen zapisovat, neaktivovat.
- Po dokončení PROP-037 (Roslyn importer) znovu zvážit.
