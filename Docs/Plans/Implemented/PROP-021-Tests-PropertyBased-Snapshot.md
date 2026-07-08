# PROP-021: Testování — Property-based a Snapshot testy

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot (Orchestrator)
> **Návaznost:** PROP-020 (závisí na dokončení BusinessModel upgradu)

---

## Cíl

Zavést dvě pokročilé testovací techniky pro ověření správnosti BusinessModel vrstvy a jejích invariantů:
1. **Property-based testování (FsCheck)** — automatické generování náhodných command sekvencí a ověřování invariantů
2. **Snapshot testování (Verify)** — regresní testování replay výstupů proti schváleným snapshotům

## Odůvodnění

Současné testy (PROP-009) pokrývají jen jednotkové testy s konkrétními vstupy. Pro BusinessModel s event sourcingem to nestačí:

- **Invarianty musí platit pro VŠECHNY možné sekvence commandů** — nejen pro ty, které vývojáře napadnou
- **Replay je deterministický** — musí platit pro libovolnou kombinaci operací
- **Regrese v replay logice** — změna v PatchEngine může nenápadně změnit výstup pro existující command logy

---

## 1. Property-based testování (FsCheck)

### Rozsah

```csharp
public class BusinessModelProperties
{
    // Generátor náhodných command sekvencí
    private static Gen<CommandEnvelope> CommandGenerator();
    private static Gen<IReadOnlyList<CommandEnvelope>> CommandSequenceGenerator();

    [Property]
    public Property Replay_je_deterministicky()
    {
        // Pro libovolnou sekvenci commandů:
        //   replayEngine.Replay(commands) == replayEngine.Replay(commands)
    }

    [Property]
    public Property CommandLog_Count_nikdy_neklesa()
    {
        // Pro libovolnou sekvenci Append() volání:
        //   Count po každém Append >= Count před Append
    }

    [Property]
    public Property Replay_po_Append_dava_stejny_vysledek_jako_ApplyPatch()
    {
        // replayEngine.Replay(log.GetAll()) == patchEngine.Apply(dokument, operace)
    }

    [Property]
    public Property Validator_propusti_kazdy_replay_vystup()
    {
        // Pro libovolnou sekvenci commandů:
        //   validator.Validate(replayEngine.Replay(commands)) neobsahuje Errors
    }

    [Property]
    public Property MutationId_zajistuje_idempotenci()
    {
        // Dvojité Append se stejným MutationId → Count se zvýší jen o 1
    }
}
```

### Nástroje

| Nástroj | Účel |
|---------|------|
| `FsCheck.Xunit` | Property-based test framework pro .NET |
| Vlastní `Gen<>` generátory | Generování validních CommandEnvelope, BusinessAttributeNode, atd. |
| `Arbitrary<T>` instance | Custom arbitraries pro doménové typy |

### Výstup

| Soubor | Umístění |
|--------|----------|
| `BusinessModelGenerators.cs` | `Tests/MetaForge.BusinessModel.Tests/Generators/` |
| `BusinessModelProperties.cs` | `Tests/MetaForge.BusinessModel.Tests/Properties/` |
| `CommandSequenceGenerators.cs` | `Tests/MetaForge.BusinessModel.Tests/Generators/` |

---

## 2. Snapshot testování (Verify)

### Rozsah

```csharp
public class ReplaySnapshotTests
{
    [Fact]
    public Task Replay_PayrollSample_MatchesSnapshot()
    {
        var commands = PayrollSampleCommands.Create();
        var document = replayEngine.Replay(commands);
        var json = BusinessDocumentJsonSerializer.Serialize(document);
        return Verify(json); // Verify.Json()
    }

    [Fact]
    public Task Replay_PrazdnyLog_VraciPrazdnyDokument()
    {
        var document = replayEngine.Replay([]);
        return Verify(document);
    }

    [Fact]
    public Task PatchEngine_AddEntity_VytvoriStejnyDokumentJakoReplay()
    {
        // Ověří, že Apply a Replay dávají identický výstup
    }
}
```

### Workflow

1. Vývojář napíše test s `Verify()`
2. První spuštění vytvoří `*.verified.json` — vývojář zkontroluje a schválí
3. Další spuštění porovnávají — pokud se výstup liší, test selže
4. Vývojář buď opraví kód (regrese), nebo aktualizuje snapshot (záměrná změna)

### Nástroje

| Nástroj | Účel |
|---------|------|
| `Verify.Xunit` | Snapshot test framework pro .NET |
| `Verify.Json` | JSON snapshoty s normovaným formátováním |
| `DiffEngine` | Vizuální porovnání změn při selhání |

---

## Odhad

| Fáze | Dny |
|------|-----|
| FsCheck — generátory a property testy | 1 den |
| Verify — snapshot testy | 0,5 dne |
| Integrace do CI | 0,25 dne |
| **Celkem** | **1,75 dne** |

---

## Závislosti

| Závislost | Stav |
|-----------|------|
| PROP-020 (BusinessModel upgrade) | 🟢 Schváleno — musí být hotovo před PROP-021 |
| PROP-009 (Testovací infrastruktura) | ✅ Hotovo |
| `FsCheck.Xunit` NuGet | Třeba přidat |
| `Verify.Xunit` NuGet | Třeba přidat |
