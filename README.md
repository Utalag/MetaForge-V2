# MetaForge V2

> C#-first platforma pro modelování, generování a správu business aplikací.
> Architektura: Event Sourcing + CommandLog + deterministický překlad do C#.

## Koncept

MetaForge je nástroj pro:
1. **Modelování** business entit, atributů a chování v strukturovaném dokumentu (BusinessAuthoringDocument).
2. **Překlad** business modelu do C# typového modelu přes Translator vrstvu.
3. **Generování** C# kódu z typového modelu přes CSharpGenerator.
4. **Správu** změn přes append-only CommandLog a replay mechanismsus.

## Architektura

```
Host Surface (CLI, MCP, WebApi)
    ↓
Facade (BusinessAuthoringHostFacade)
    ↓
BusinessModel (BusinessAuthoringDocument + CommandLog)
    ↓
Translator (DefaultBusinessTranslator)
    ↓
Core (typový model, katalog, ForgeBlock metadata)
    ↓
Generators (CSharpGenerator)
```

## Požadavky

- .NET SDK 9.0
- Visual Studio Code nebo JetBrains Rider

## Rychlý start

```bash
# Build všech projektů
dotnet build MetaForge.slnx

# Spuštění testů
dotnet test MetaForge.slnx

# CLI — přidání entity
dotnet run --project Src/MetaForge.Cli -- entity add Osoba
```
