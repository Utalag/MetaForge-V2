# IDEA-019 Desktop Authoring Studio

Stav: Idea
Oblast: Host, Product, UI
Zdroj: For_Inspiration/Architecture-Define/11-Frontend-Authoring-Studio.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept definoval desktop authoring studio v Avalonii jako plnohodnotný host surface pro MetaForge. Obsahoval `MetaForge.WorkspacePrototype` (návrhové prostředí) a `MetaForge.Studio` (odvozený desktop host). Současná implementace má pouze CLI a MCP — chybí grafické rozhraní.

Nápad vychází z `11-Frontend-Authoring-Studio.md`, kde je popsáno:
- Desktop-first workspace v Avalonii (ne web-first SPA)
- Čtyři pracovní režimy: Document Overview, Model Studio, Workflow Studio, Review Surface
- Vicepanelový layout s persistentními regiony
- Node-level assist s preview a explicit apply
- Guardrails: žádná druhá read/write pipeline, AI pouze preview + apply

## 2. Problém dnes

- MetaForge je ovladatelné pouze přes CLI a MCP — chybí grafické rozhraní pro modelování.
- Business model nelze vizuálně prohlížet — jen přes textové projekce.
- Uživatelé zvyklí na GUI nástroje (Enterprise Architect, draw.io) nemigrují.
- Node-level assist existuje, ale jen přes MCP — není integrován do pracovního prostředí.
- Workflow modelování v textu je nepřehledné.

## 3. Předběžný směr řešení

Desktop-first host surface:

- `MetaForge.Studio` — Avalonia desktop aplikace
- Čtyři režimy: Document Overview, Model Studio, Workflow Studio, Review Surface
- Vicepanelový layout (Project Tree, Canvas, Inspector, Assistant)
- Read přes `BusinessAuthoringHostFacade`, write přes `ApplyOperations`
- Node-level assist s preview panelem a explicitním apply tlačítkem
- Workflow vizualizace (kroky, přechody, capability bindingy)

Alternativa: WebApi + SPA (React/Angular) místo Avalonia — ale původní koncept už rozhodl desktop-first.

Dotčené vrstvy: Host (Studio), Translator (Facade), Core (elementy pro vizualizaci).

## 4. Signál hodnoty

- První grafické rozhraní pro MetaForge — dramaticky zlepšuje DX.
- Vizuální modelování entit, atributů a workflow.
- Uživatel vidí model, ne jen text.
- Klíčový produktový milník pro MVP.

## 5. Rizika a nejasnosti

- Avalonia je nová technologie — riziko neznalosti.
- Desktop-first znamená omezenou distribuci (Windows-only? Cross-platform?).
- OQ-xxx: Má studio běžet in-process (přímá vazba na Facade) nebo out-of-process (HTTP)?
- OQ-xxx: Jaký je minimální životaschopný rozsah pro první verzi?

## 6. Doporučený další krok

Zatím jen zapisovat, neaktivovat. Desktop studio je dlouhodobý cíl — před ním musí být stabilní authoring kernel, workflow a readiness model.

Vazby: Všechny PROP — studio je konzument všech vrstev.
