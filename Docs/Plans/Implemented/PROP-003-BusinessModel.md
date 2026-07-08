# PROP-003: BusinessModel vrstva

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-04
> **Autor:** Copilot (C# Implementer)

## Cíl

Vytvořit projekt `MetaForge.BusinessModel` s BusinessAuthoringDocument, CommandLog, ReplayEngine a PatchEngine.

## Výstup

- `Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj` — class library (bez závislostí na Core)
- `Models/` — BusinessEntityNode, BusinessAttributeNode, BusinessBehaviorNode, BusinessRelationNode, BusinessNoteNode, PendingQuestionNode, BusinessAuthoringDocument, CustomTypeDefinition
- `CommandLog/` — CommandEnvelope (immutable record), CommandLogStore (append-only), ReplayEngine (autoritativní rekonstrukce)
- `Patches/` — IPatchOperation, PatchEngine, Operations: AddEntityOp, UpdateEntityOp, DeleteEntityOp, AddAttributeOp, UpdateAttributeOp

## Invarianty

- BusinessAuthoringDocument je source of truth
- CommandLog je append-only (Count nikdy neklesá)
- Replay je autoritativní rekonstrukce stavu
- Žádná přímá mutace dokumentu — vše přes PatchEngine

## Zpětná vazba / Poznámky

Po code review přidána thread safety do CommandLogStore (lock). BusinessAuthoringDocument je sealed. SchemaVersion je konstanta CurrentSchemaVersion.
