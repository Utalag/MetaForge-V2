# IDEA-022 Business Builders — MetaForge.Builders

Stav: Idea
Oblast: Core, DX, Testování
Zdroj: For_Inspiration/Architecture-Define/00-Platform-Overview.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní projekt obsahoval `MetaForge.Builders` — projekt s fluent buildery pro doménové objekty. Současná implementace nemá samostatný builders projekt; fluent API je částečně v PROP-038 (Core DX) jako `TypeModel.Define(...).Build()`, ale není systematické.

Nápad vychází z `00-Platform-Overview.md`, kde je `MetaForge.Builders` uveden jako samostatný projekt v Src/.

## 2. Problém dnes

- Neexistuje jednotné fluent API pro vytváření Core elementů.
- PROP-038 navrhuje `TypeModel.Define(...).Build()`, ale pouze pro TypeModel.
- Testy používají ad-hoc konstrukci elementů — chybí `ClassBuilder`, `MethodBuilder`, `ExpressionBuilder`.
- Builder API by zlepšilo DX a testovatelnost — testy by byly čitelnější.

## 3. Předběžný směr řešení

- `MetaForge.Builders` — nový projekt (nebo namespace v Core)
- `ClassBuilder`, `StructBuilder`, `EnumBuilder`, `InterfaceBuilder`
- `MethodBuilder`, `PropertyBuilder`, `ParameterBuilder`
- `ExpressionBuilder` — fluent pro skládání expression stromů
- Integrace s PROP-038 (Fluent Builder API, DiagnosticBag)

Dotčené vrstvy: Core (elementy), Testy (použití builderů), DX (fluent API).

## 4. Signál hodnoty

- Dramaticky zlepšuje DX při vytváření Core modelu v kódu.
- Testy jsou čitelnější a méně verbose.
- Konzistentní pattern napříč všemi elementy.
- PROP-038 je prvním krokem — tento nápad ho rozšiřuje na všechny elementy.

## 5. Rizika a nejasnosti

- Buildery mohou být over-engineering — stačí konstruktor + init properties?
- OQ-xxx: Mají buildery žít v samostatném projektu nebo v Core?

## 6. Doporučený další krok

Follow-up k PROP-038. Měl by být plánován jako rozšíření Fluent Builder API na všechny Core elementy.

Vazby: PROP-038 (Core DX), PROP-024 (Core elementy)
