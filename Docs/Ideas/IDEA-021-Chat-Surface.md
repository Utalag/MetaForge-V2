# IDEA-021 Chat Surface — MetaForge.Chat

Stav: Idea
Oblast: AI, Host, Product
Zdroj: For_Inspiration/Architecture-Define/00-Platform-Overview.md
Datum vytvoření: 2026-07-08
Poslední revize: 2026-07-08

## 1. Kontext

Původní koncept zmiňoval `MetaForge.Chat` jako samostatný projekt — server-side AI konverzační surface. Současná implementace má AI integrovanou do MCP a CLI, ale chybí dedikovaný chat režim, kde by uživatel mohl s platformou komunikovat přirozeným jazykem.

Nápad vychází z `00-Platform-Overview.md`, kde je chat zmíněn jako jeden z host surfaces vedle MCP a CLI.

## 2. Problém dnes

- AI konverzace probíhá přes MCP (externí AI klient) nebo CLI — není vestavěný chat.
- Uživatel musí mít externího AI klienta (Claude Desktop, ChatGPT) pro přirozenou konverzaci.
- Chybí "vibe-coding" režim — otevřít chat a modelovat přirozeným jazykem.
- `AuthoringConversationService` existuje v Translator, ale není vystavená jako samostatný host surface.

## 3. Předběžný směr řešení

- `MetaForge.Chat` — nový projekt v Src/
- Režimy: interactive (REPL-like), one-shot (otázka → odpověď), pipe (stdin → stdout)
- Využívá `AuthoringConversationService` pro orchestraci AI + patching
- Integrace s PromptRegistry pro prompt templaty
- Volitelně: web-based chat UI místo terminálu

Alternativa: Rozšířit CLI o `metaforge chat` příkaz místo samostatného projektu.

Dotčené vrstvy: AI (konverzace), Translator (AuthoringConversationService), Host (Chat).

## 4. Signál hodnoty

- Vibe-coding bez externích nástrojů — jeden příkaz a modeluješ.
- Nižší bariéra vstupu — není potřeba instalovat AI klienta.
- Lepší integrace s PromptRegistry — prompty jsou verzované a testovatelné.
- Možnost monetizovat chat jako premium feature.

## 5. Rizika a nejasnosti

- OQ-xxx: Samostatný projekt nebo CLI subcommand?
- OQ-xxx: Jaký AI provider pro chat? (lokální Ollama pro offline, cloud API pro výkon)
- Chat bez grafického rozhraní je omezený — textové CLI chatu může být nepřehledné.

## 6. Doporučený další krok

Zatím jen zapisovat, neaktivovat. Chat surface dává smysl až po stabilizaci AI vrstvy a PromptRegistry.

Vazby: PROP-027 (MetaForge.Ai, PromptRegistry), PROP-026 (CLI), AuthoringConversationService
