# PROP-049 Test Framework Consolidation — Sdílená knihovna a Snapshot DX

Typ výsledku: Candidate Proposal
Zdroj podnětu: IDEA-028 (Koumák — analýza testů + PROP-032 code review)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-11

Priorita: High
Oblast: Tests, Infrastructure
Owner:
Datum vytvoření: 2026-07-11
Aktualizováno: 2026-07-11

Navazuje na:
- PROP-032 (Integration Tests Core+Generators — hotovo)
- PROP-042 (Core Test Expansion — hotovo)
- PROP-048 (Generator Render Core Tests — kandidát)

Blokuje:
- —

Související soubory:
- `Tests/MetaForge.Core.Integration.Tests/SyntaxValidator.cs`
- `Tests/MetaForge.Generators.Tests/SyntaxValidator.cs`
- `Tests/MetaForge.Core.Integration.Tests/Snapshots/SnapshotComparer.cs`
- `Tests/MetaForge.Core.Tests/`
- `Docs/Core/00-Support-Matrix.md`

## 1. Kontext

Při analýze testů (IDEA-028) a code review PROP-032 bylo identifikováno několik infrastrukturních problémů: `SyntaxValidator` je duplicitní ve dvou test projektech, `SnapshotComparer` nepodporuje `UPDATE_SNAPSHOTS` env var, property P6 a P7 chybí v snapshot matici, a neexistuje centrální testovací knihovna pro sdílené helpery.

## 2. Problém dnes

- **Duplicita `SyntaxValidator`**: stejná třída (Roslyn C# parsování) existuje v `Tests/MetaForge.Core.Integration.Tests/SyntaxValidator.cs` i `Tests/MetaForge.Generators.Tests/SyntaxValidator.cs` — změna logiky vyžaduje změnu na 2 místech.
- **Chybí `UPDATE_SNAPSHOTS=true`**: při přidávání nových snapshot testů první spuštění vždy failne — vývojář musí ručně vytvořit expected soubor, nebo spustit test, dohledat generated output a přejmenovat ho.
- **Chybí P6 a P7 v property matici**: dokumentace matice (PROP-032, PROP-034) počítá s 8 variantami (P1-P8), ale snapshot testy pokrývají jen P1-P5 a P8.
- **Žádná sdílená testovací knihovna**: helpery (`SyntaxValidator`, `SnapshotComparer`, test data builders) jsou rozptýlené — nelze je sdílet mezi test projekty.

## 3. Cíl

- Jeden zdroj pravdy pro `SyntaxValidator` — sdílený přes project reference.
- `UPDATE_SNAPSHOTS=true` env var — při nastavení místo failu přepíše expected soubor aktuálním výstupem.
- Property snapshoty kompletní (P1-P8).
- Sdílená testovací knihovna `MetaForge.Testing.Common` pro helpery napříč test projekty.

## 4. Architektonické invarianty

- Testovací infrastruktura nesmí zavádět závislosti do produkčního kódu.
- Sdílená knihovna je pouze pro test projekty — žádná závislost na produkčních NuGet balících mimo xUnit/FluentAssertions.

## 5. Scope

### In scope
- Vytvoření `Tests/MetaForge.Testing.Common/` s:
  - `SyntaxValidator` — Roslyn C# syntax validation (migrovat z Integration.Tests, odstranit duplikát z Generators.Tests)
  - `SnapshotComparer` — migrovat z Integration.Tests
  - `UPDATE_SNAPSHOTS` podpora v SnapshotComparer
  - `TestDataBuilder` helpery (volitelné)
- Přepojení `Integration.Tests` a `Generators.Tests` na sdílenou knihovnu
- Doplnění P6 a P7 snapshot testů

### Out of scope
- Změny v produkčním kódu
- Změny E2E test scénářů (PROP-045)
- Nové testovací frameworky (Verify.NET, FsCheck — již existují)
- Automatické generování support matrix z kódu

## 6. Návrh řešení

### Sdílená testovací knihovna

```
Tests/MetaForge.Testing.Common/
├── MetaForge.Testing.Common.csproj
├── SyntaxValidator.cs           # Roslyn C# parsování
├── SnapshotComparer.cs          # Snapshot engine s UPDATE_SNAPSHOTS
└── TestDataBuilder.cs           # (volitelně) helpery pro konstrukci elementů
```

`MetaForge.Testing.Common.csproj`:
- Cílový framework: `net9.0`
- Závislosti: `Microsoft.CodeAnalysis.CSharp`, `xunit`, `FluentAssertions`
- Project reference na produkční projekty: NE — je to test-only

### UPDATE_SNAPSHOTS podpora

```csharp
public static class SnapshotComparer
{
    private static readonly bool UpdateSnapshots =
        Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "true";

    public static void Verify(string category, string name, string actualSource)
    {
        var expectedPath = FindExpectedFile(category, name);

        if (UpdateSnapshots)
        {
            File.WriteAllText(expectedPath, actualSource);
            return; // no assert, just update
        }

        var expected = File.ReadAllText(expectedPath);
        Assert.Equal(expected, actualSource);
    }
}
```

Chování:
- Bez proměnné: fail při neshodě (současné chování)
- `UPDATE_SNAPSHOTS=true`: tichý přepis expected souborů

Bezpečnostní opatření: proměnná se kontroluje jen při neshodě, ne při prvním načtení.

### Duplicita SyntaxValidator — odstranění

1. Vytvořit `SyntaxValidator.cs` v `MetaForge.Testing.Common`
2. V `Tests/MetaForce.Generators.Tests/SyntaxValidator.cs` nahradit duplicitní třídu pomocí `using MetaForge.Testing.Common;`
3. V `Tests/MetaForce.Core.Integration.Tests/SyntaxValidator.cs` — buď odstranit a používat sdílenou, nebo ponechat s odkazem

### Chybějící snapshoty P6 a P7

| Varianta | Factory metoda | Popis | Stav |
|----------|---------------|-------|------|
| P6 | `PropertyElement.RequiredInitOnly(...)` | `required init;` | ❌ Chybí |
| P7 | `PropertyElement.GetSetWithDefault(...)` | `get; set;` s výchozí hodnotou | ❌ Chybí |
| P1-P5, P8 | — | Existují | ✅ Hotovo |

Nutné ověřit, zda `PropertyElement` má factory metody pro P6 a P7 — pokud ne, přidat jako součást tohoto PROP.

## 7. Implementační dopad

### Změněné projekty nebo soubory
- `Tests/MetaForge.Testing.Common/MetaForge.Testing.Common.csproj` — nový
- `Tests/MetaForge.Testing.Common/SyntaxValidator.cs` — nový
- `Tests/MetaForge.Testing.Common/SnapshotComparer.cs` — nový (migrovat)
- `Tests/MetaForge.Generators.Tests/SyntaxValidator.cs` — odstranit duplicitu
- `Tests/MetaForge.Generators.Tests/MetaForge.Generators.Tests.csproj` — přidat project reference
- `Tests/MetaForge.Core.Integration.Tests/MetaForge.Core.Integration.Tests.csproj` — přidat project reference
- `Tests/MetaForge.Core.Integration.Tests/Scenarios/PropertyModifierSnapshots.cs` — přidat P6, P7
- `Tests/MetaForge.Core.Tests/Validation/PropertyModifierValidationTests.cs` — možné přejmenování (P7 již existuje jako validační test, ne snapshot)

### API a kontrakty
- Žádné změny v produkčním API.

### Testy
- 2 nové snapshot testy (P6, P7)
- Stávající testy beze změny (pouze změna reference)

### Dokumentace
- Aktualizace `Docs/Core/00-Support-Matrix.md` — P6 a P7 stav na ✅ Supported

## 8. Implementační fáze

### Fáze 1: Sdílená knihovna (~1 den)
- Vytvoření projektu `MetaForge.Testing.Common`
- Migrace `SyntaxValidator` a `SnapshotComparer`
- Přepojení Integration.Tests a Generators.Tests

### Fáze 2: UPDATE_SNAPSHOTS (~0.5 dne)
- Implementace env var v SnapshotComparer
- Test ověření chování

### Fáze 3: P6 a P7 snapshoty (~0.5 dne)
- Ověření factory metod na PropertyElement
- Přidání snapshot testů
- Aktualizace Support Matrix

## 9. Otevřené otázky

- **OQ-049-01**: Existují factory metody `PropertyElement.RequiredInitOnly()` a `PropertyElement.GetSetWithDefault()`? Pokud ne, přidat do Core v rámci tohoto PROP nebo založit ISS?
- **OQ-049-02**: Má `SyntaxValidator` zůstat i v Integration.Tests jako kopie (pro zamezení přímé závislosti) nebo plně migrovat do sdílené knihovny?

## 10. Rizika a trade-offy

- **Riziko změny cesty k snapshotům**: migrace SnapshotComparer může změnit relativní cestu — nutné otestovat, zda snapshoty stále nachází.
- **Riziko `UPDATE_SNAPSHOTS` zneužití**: proměnná by neměla být nastavena v CI — dokumentovat jako lokální vývojářský nástroj.
- **Vědomý kompromis**: TestDataBuilder není prioritou — odložit na později.

## 11. Validace

- Build: `dotnet build` bez chyb
- Testy: `dotnet test` — všechny stávající testy prochází
- Smoke: `UPDATE_SNAPSHOTS=true dotnet test` — tichý update
- Ruční kontrola: P6 a P7 snapshoty generují korektní C#
- Jak poznáme, že je návrh hotový: SyntaxValidator existuje jen jednou v repo; `UPDATE_SNAPSHOTS` funguje; P1-P8 kompletní

## 12. Výsledek po dokončení

*— vyplnit při uzavření —*
