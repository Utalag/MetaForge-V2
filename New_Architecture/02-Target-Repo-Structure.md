# Cílová struktura repozitáře

> Návrh složkové a projektové struktury pro nový C#-first MetaForge projekt.

---

## Kořenová úroveň

```
MetaForge/
├── .github/                    # CI/CD, agenty, workflow instrukce
│   └── agents/                 # Instrukce pro agenty a skills
├── Docs/                       # Architektonická dokumentace
│   ├── Architecture/           # Vrstvy, guardraily, diagramy
│   └── Plans/                  # Detailní proposal markdowny
├── Src/                        # Zdrojový kód
│   ├── MetaForge.Core/         # C#-first typový model
│   ├── MetaForge.BusinessModel/# Source of truth + CommandLog
│   ├── MetaForge.Translator/   # Překlad, enrichment, facade
│   ├── MetaForge.Cli/          # CLI host surface
│   ├── MetaForge.Mcp/          # MCP host surface
│   ├── MetaForge.Generators/   # C#-first codegen
│   ├── MetaForge.Ai/           # AI integrace (volitelná)
│   ├── MetaForge.Feedback/     # Authoring Feedback Platform (PROP-061)
│   └── ForgeBlocks/            # Capability balíky
│       ├── Math/
│       ├── String/
│       ├── Validation/
│       └── ...
├── Tests/                      # Testovací projekty
│   ├── MetaForge.Core.Tests/
│   ├── MetaForge.BusinessModel.Tests/
│   ├── MetaForge.Translator.Tests/
│   └── MetaForge.Generators.Tests/
├── PROPOSALS.md                # Master checklist návrhů
├── PROPOSALS_NEXT.md           # Zásobník kandidátních návrhů
├── Progress.md                 # Chronologický log realizovaných změn
├── Memories.md                 # Provozní knowledge file
├── README.md                   # Popis projektu
└── MetaForge.slnx              # Solution soubor
```

---

## Projekty a odpovědnosti

| Projekt | Odpovědnost |
|---------|-------------|
| `MetaForge.Core` | Typový model, výrazy (computed properties/behaviors), inference, standardní knihovny, ValueObjects, katalog, ForgeBlock metadata, discovery |
| `MetaForge.BusinessModel` | BusinessAuthoringDocument, CommandLog, replay, patches, validation |
| `MetaForge.Translator` | Facade, enrichment, projekce, write-back, AI orchestrace |
| `MetaForge.Cli` | CLI host surface |
| `MetaForge.Mcp` | MCP host surface |
| `MetaForge.Generators` | CSharpGenerator (aktivní), template engine |
| `MetaForge.Ai` | AI provider abstrakce, prompt building |
| `ForgeBlocks/*` | Jednotlivé capability balíky |

---

## Namespace konvence

```
MetaForge.Core                          # Kořen Core
MetaForge.Core.Abstractions             # RootElement a sdílené abstrakce
MetaForge.Core.Elements                 # Typové elementy
MetaForge.Core.Elements.Expressions     # Computed properties/behaviors výrazy
MetaForge.Core.Elements.Primitives      # Field, Property, Parameter, Variable
MetaForge.Core.DataTypes                # Datové typy
MetaForge.Core.Inference                # Constraint inference
MetaForge.Core.Inference.Boundary       # Domain/boundary analýza
MetaForge.Core.StandardLibraries        # Mapování na standardní knihovnu
MetaForge.Core.ValueObjects             # StrongType, conversion, validation rules
MetaForge.Core.Catalog                  # Presety, ValueObjects, catalog providers
MetaForge.Core.Discovery                # Discovery metadata
MetaForge.Core.ForgeBlockPackages       # ForgeBlock registrace, capability/discovery kontrakty
MetaForge.Core.Configuration            # AI inference settings a další konfigurace

MetaForge.BusinessModel                 # Kořen BusinessModel
MetaForge.BusinessModel.Models          # Doménové modely
MetaForge.BusinessModel.CommandLog      # CommandLog + replay
MetaForge.BusinessModel.Patches         # PatchEngine
MetaForge.BusinessModel.Validation      # Validace dokumentu
MetaForge.BusinessModel.Persistence     # Serializace/deserializace

MetaForge.Translator                    # Kořen Translator
MetaForge.Translator.Host               # Facade + projekce
MetaForge.Translator.Prompting          # AI prompt building
MetaForge.Translator.Telemetry          # Telemetrie

MetaForge.Generators                    # Kořen Generators
MetaForge.Generators.CSharp             # C# generátor

MetaForge.ForgeBlocks.{Name}            # Jednotlivé ForgeBlocky
```

---

## Governance soubory v kořenu

| Soubor | Účel |
|--------|------|
| `PROPOSALS.md` | Master checklist aktivních návrhů |
| `PROPOSALS_NEXT.md` | Zásobník kandidátních nebo odložených návrhů |
| `Progress.md` | Chronologický log realizovaných změn |
| `Memories.md` | Aktivní provozní knowledge file |
| `README.md` | Popis projektu, quick start |
