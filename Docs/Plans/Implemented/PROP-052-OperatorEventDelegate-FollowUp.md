# PROP-052 Operator/Event/Delegate — Follow-up: Snapshot testy a contract status

Typ výsledku: Follow-up
Zdroj podnětu: IDEA-027 (Koumák + Perplexity)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-11

Priorita: Low
Oblast: Core, Generators, Tests
Owner:
Datum vytvoření: 2026-07-11
Aktualizováno: 2026-07-11

Navazuje na:
- PROP-037 (C# Completeness — hotovo; přidal DelegateElement, EventElement, OperatorElement do Core)
- PROP-043 (Generator Completeness — hotovo; přidal Event+Operator+Delegate generování)
- PROP-045 (Generator E2E Completeness — hotovo; 13/13 scénářů)

Blokuje:
- —

Související soubory:
- `Tests/MetaForge.Core.Integration.Tests/Scenarios/DelegateSnapshots.cs` — nový
- `Tests/MetaForge.Core.Integration.Tests/Snapshots/Delegate/*.expected.cs` — nový
- `Docs/Core/00-Support-Matrix.md`

## 1. Kontext

IDEA-027 identifikovala nekonzistenci mezi Core modelem a generátorem pro Delegate, Event a Operator. Analýza aktuálního kódu (2026-07-11) ukázala, že tento gap je již **téměř zcela vyřešen**:

| Element | Core model | Generátor | Snapshot test | Unit test |
|---------|-----------|-----------|---------------|-----------|
| DelegateElement | ✅ PROP-037 | ✅ PROP-043 | ❌ Chybí | ❌ Chybí |
| EventElement | ✅ PROP-037 | ✅ PROP-043 | ❌ Chybí | ❌ Chybí |
| OperatorElement | ✅ PROP-037 | ✅ PROP-043 | ❌ Chybí | ❌ Chybí |

Zbývá pouze:
1. Dopsat snapshot testy pro Delegate (generátor umí, test chybí)
2. Ověřit, že Event a Operator E2E prochází (PROP-045)
3. Aktualizovat Support Matrix — Delegate, Event, Operator z "Planned" na "Supported"

## 2. Problém dnes

- **Delegate** má generování v CodeGeneratoru (`GenerateDelegate`), ale neexistuje snapshot test, který by ověřil korektní C# výstup.
- **Event a Operator** jsou pokryty PROP-045 E2E scénáři, ale Support Matrix je stále uvádí jako "Planned" (zastaralé).
- **Falešný signál**: Core.Testy testují Delegate/Event/Operator factory metody, což vytváří dojem plné podpory — ale snapshot testy chybí.

## 3. Cíl

- Snapshot testy pro Delegate (basic, generic, s parametry)
- Ověření Event a Operator E2E pokrytí
- Aktualizace Support Matrix

## 4. Scope

### In scope
- Snapshot test: `DelegateSnapshots.cs` (basic, generic, s parametry)
- Aktualizace `Docs/Core/00-Support-Matrix.md` — Delegate, Event, Operator → ✅ Supported

### Out of scope
- Nové implementace v generátoru (vše již hotovo)
- Unit testy ExpressionRendereru pro Event/Operator (řeší PROP-048)
- Změny v Core modelu

## 5. Návrh řešení

### Delegate snapshot testy

```csharp
public class DelegateSnapshots
{
    private readonly CodeGenerator _generator = new();

    [Fact]
    public void D1_BasicDelegate()
    {
        var del = new DelegateElement
        {
            Name = "ActionHandler",
            ReturnType = TypeModel.Void,
            Parameters = { new ParameterElement { Name = "message", Type = TypeModel.String } }
        };
        var result = _generator.Generate(del);
        SnapshotComparer.Verify("Delegate", nameof(D1_BasicDelegate), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
    }

    [Fact]
    public void D2_GenericDelegate() { /* delegate T Transformer<T>(T input) */ }

    [Fact]
    public void D3_DelegateWithMultipleParams() { /* delegate bool Filter(int id, string name) */ }
}
```

### Aktualizace Support Matrix

V `Docs/Core/00-Support-Matrix.md` změnit:
- Delegate: `🟡 Planned` → `✅ Supported`
- Event: `🟡 Planned` → `✅ Supported`
- Operator: `🟡 Planned` → `✅ Supported`

## 6. Implementační dopad

### Změněné projekty nebo soubory
- `Tests/MetaForge.Core.Integration.Tests/Scenarios/DelegateSnapshots.cs` — nový (3 testy)
- `Tests/MetaForge.Core.Integration.Tests/Snapshots/Delegate/D1_BasicDelegate.expected.cs` — nový
- `Tests/MetaForge.Core.Integration.Tests/Snapshots/Delegate/D2_GenericDelegate.expected.cs` — nový
- `Tests/MetaForge.Core.Integration.Tests/Snapshots/Delegate/D3_DelegateWithMultipleParams.expected.cs` — nový
- `Docs/Core/00-Support-Matrix.md` — oprava 3 stavů

### API a kontrakty
- Žádné změny.

### Testy
- 3 nové snapshot testy.

### Dokumentace
- Oprava Support Matrix.

## 7. Odhad

- Snapshot testy: ~1 hodina
- Oprava Support Matrix: ~15 minut
- **Celkem: ~1.5 hodiny**

## 8. Otevřené otázky

- Žádné — jednoduchý follow-up.

## 9. Validace

- Build: `dotnet build` bez chyb
- Testy: 3 nové snapshot testy prochází
- Support Matrix: Delegate, Event, Operator jsou ✅ Supported
