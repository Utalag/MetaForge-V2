# PROP-005: Host Surfaces (CLI + MCP)

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-04
> **Autor:** Copilot (C# Implementer)

## Cíl

Vytvořit tenké CLI a MCP host surfaces — žádná business logika, jen volání Facade.

## Výstup

- `Src/MetaForge.Cli/MetaForge.Cli.csproj` — konzolová aplikace
- `Src/MetaForge.Cli/Program.cs` — Composition Root + CLI dispatcher + command handlers
- `Src/MetaForge.Cli/appsettings.json` — konfigurace
- `Src/MetaForge.Mcp/MetaForge.Mcp.csproj` — konzolová aplikace (stdio server)
- `Src/MetaForge.Mcp/Program.cs` — JSON-RPC stdio server
- `Src/MetaForge.Mcp/Models/JsonRpcModels.cs` — JSON-RPC 2.0 modely

## CLI příkazy

| Příkaz | Popis |
|--------|-------|
| `add-entity <název>` | Přidá novou entitu |
| `update-entity <id> <nový-název>` | Přejmenuje entitu |
| `delete-entity <id>` | Smaže entitu |
| `add-attribute <entity-id> <název> [typ] [required]` | Přidá atribut |
| `projection` | Zobrazí aktuální projekci |
| `list-entities` | Vypíše všechny entity |

## MCP tools

| Tool | Popis |
|------|-------|
| `add_entity` | Přidá novou business entitu |
| `add_attribute` | Přidá atribut k entitě |
| `get_projection` | Vrátí aktuální projekci |
| `list_entities` | Vypíše všechny entity |

## Zpětná vazba / Poznámky

Po code review JSON-RPC modely extrahovány do samostatného souboru s namespace.
