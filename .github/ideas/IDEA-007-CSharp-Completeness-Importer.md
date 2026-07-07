# IDEA-007 C# Completeness — Chybějící konstrukty + Projektová metadata + Roslyn Importer

Stav: Candidate (převedeno na PROP-037)
Oblast: Core, Infrastructure
Zdroj: GitHub task "Implement the plan" — kroky 4, 5, 6
Datum vytvoření: 2026-07-07
Poslední revize: 2026-07-07

## 1. Kontext

GitHub task definoval 8 kroků k dokončení C# pokrytí. Kroky 1-3 a 7-8 jsou pokryty existujícími PROP. Kroky 4 (delegate/event/operator), 5 (projektová metadata, framework vrstva) a 6 (Roslyn importer) vyžadují nový PROP.

## 2. Problém dnes

- Chybí DelegateElement, EventElement, OperatorElement
- ProjectElement je chudý (jen Name, DefaultNamespace)
- Žádný importer C# → Core
- Framework metadata (DI, ASP.NET, EF) nejsou modelována

## 3. Předběžný směr řešení

5 fází: member typy → projektová metadata → framework vrstva → Roslyn importer → integrace/docs. Samostatný projekt MetaForge.Importer s Roslyn závislostí mimo Core.

## 4. Signál hodnoty

- Možnost importovat existující C# kód do Core
- Plnohodnotná reprezentace .NET projektů
- Framework metadata umožňují generování boilerplate (DI, ASP.NET)

## 5. Rizika a nejasnosti

- Roslyn závislost musí být mimo Core
- Framework metadata scope creep
- Import nebude 100% — diagnostika musí být informativní

## 6. Doporučený další krok

- ✅ Převedeno na Candidate → PROP-037
