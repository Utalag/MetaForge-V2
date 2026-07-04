# PROP-018: Translator — ExpertProjection a ProjectionOptions

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot

## Cíl

Rozšířit aktuální jednoduchou ProjectionView o bohatší projekci (ExpertProjection) a volitelné sekce (ProjectionOptions), aby host surfaces (CLI, MCP) mohly získat detailní náhled na business model včetně diagnostiky, workflow stavů a authoring kontextu.

## Odůvodnění

Aktuální ProjectionReadService vrací jen plochý seznam entit s atributy. Pro AI-assisted authoring, workflow binding a discovery je potřeba:
- Diagnostické informace (počet otevřených otázek, atributy s constrainty, atd.)
- Workflow binding stavy
- Authoring kontext pro AI
- Discovery výsledky

## Obsah

### 1. ExpertProjection

Bohatší projekce business modelu s diagnostikou:

`csharp
public sealed class ExpertProjectionView
{
    public string SchemaVersion { get; init; }
    public string ProjectName { get; init; }
    public int EntityCount { get; init; }
    public int RelationCount { get; init; }
    public int OpenQuestionCount { get; init; }
    public IReadOnlyList<ExpertEntityProjection> Entities { get; init; }
    public IReadOnlyList<ExpertRelationProjection> Relations { get; init; }
    public IReadOnlyList<ExpertPendingQuestionProjection> PendingQuestions { get; init; }
    public ExpertProjectionDiagnostics Diagnostics { get; init; }
}
`

| Model | Popis |
|-------|-------|
| ExpertEntityProjection | Id, name, presetId, noteCount, atributy + chování |
| ExpertAttributeProjection | Id, name, businessType, coreType, constraints, computed expression |
| ExpertBehaviorProjection | Id, name, inputs, returns, constraints |
| ExpertRelationProjection | From → To, type, navigace |
| ExpertPendingQuestionProjection | Nezodpovězené otázky |
| ExpertProjectionDiagnostics | Statistiky — počty atributů s constrainty, computed výrazy, presety, atd. |
| ExpertReplayProjectionInfo | Informace o replayi (počet commandů, poslední timestamp) |

**Builder:** ExpertProjectionBuilder — staví ExpertProjectionView z BusinessAuthoringDocument.

### 2. WorkflowProjection

Projekce workflow binding stavů:

| Model | Popis |
|-------|-------|
| WorkflowProjectionView | Seznam workflow bindingů s jejich stavy |
| WorkflowBindingState | Název, status (pending/active/completed), přiřazené entity |

### 3. AuthoringContext

Kontext pro AI authoring:

| Model | Popis |
|-------|-------|
| AuthoringContextView | Projekt info + seznam entit s atributy |
| AuthoringEntityContext | Entita s atributy pro AI prompt |
| AuthoringAttributeContext | Atribut s typem pro AI prompt |
| DiscoveryContext | Objevené ForgeBlock capability |

**Builder:** AuthoringContextBuilder — staví kontext z dokumentu + discovery session.

### 4. ProjectionOptions

Volitelné sekce projekce:

`csharp
public sealed record ProjectionOptions
{
    public static ProjectionOptions Basic();
    public static ProjectionOptions Full();

    public bool Expert { get; init; }       // ExpertProjection
    public bool Workflow { get; init; }     // WorkflowProjection
    public bool AuthoringContext { get; init; }  // Authoring context
    public bool DiscoveryContext { get; init; }  // Discovery results
}
`

### 5. NodePath

Navigace v dokumentu:

`csharp
public sealed record NodePath(string Path)  // např. "entities/Customer/attributes/Email"
{
    public static NodePath ForEntity(string entityId);
    public static NodePath ForAttribute(string entityId, string attributeId);
}
`

## Výstup

| Soubor | Umístění |
|--------|----------|
| ExpertProjection.cs (modely) | Src/MetaForge.Translator/Projections/ |
| ExpertProjectionBuilder.cs | Src/MetaForge.Translator/Projections/ |
| WorkflowProjection.cs (modely + builder) | Src/MetaForge.Translator/Projections/ |
| AuthoringContext modely + builder | Src/MetaForge.Translator/Projections/ |
| ProjectionOptions.cs | Src/MetaForge.Translator/Host/ |
| NodePath.cs | Src/MetaForge.Translator/Host/ |
| Rozšíření ProjectionReadService | Async, volitelné sekce |
| Testy | Tests/MetaForge.Translator.Tests/Projections/ |

## Závislosti

| Komponenta | Stav |
|------------|------|
| BusinessAuthoringDocument (BusinessModel) | ✅ Hotovo |
| ProjectionView (aktuální Translator) | ✅ Hotovo |
| ForgeBlockRegistry, IDiscoverySession (Core) | ✅ Hotovo |

## Odhad

| Fáze | Dny |
|------|-----|
| ExpertProjection modely + builder | 0,5 dne |
| WorkflowProjection | 0,5 dne |
| AuthoringContext | 0,5 dne |
| ProjectionOptions + rozšíření ProjectionReadService | 0,5 dne |
| NodePath | 0,25 dne |
| Testy | 0,5 dne |
| **Celkem** | **2,5 dne** |
