# IDEA-028 Snapshot & Integration Test Framework — Konsolidace a rozšíření

Stav: Idea
Oblast: Tests, Generators
Zdroj: Koumák — analýza testů + PROP-032 code review
Datum vytvoření: 2026-07-11
Poslední revize: 2026-07-11

## 1. Kontext

Při analýze testů bylo identifikováno několik infrastrukturních problémů:
- `SyntaxValidator` je duplikovaný v `Generators.Tests` a `Integration.Tests` (bylo již zaznamenáno v PROP-032 code review).
- `SnapshotComparer` hledá složku `Snapshots/` v bin adresáři — křehké při změně build struktury.
- Snapshot testy failují na prvním spuštění s `Assert.Fail()` — chybí `UPDATE_SNAPSHOTS=true` přepínač pro tichý update.
- Chybí centrální testovací knihovna pro sdílené helpery napříč test projekty.
- Property P6 a P7 nejsou v snapshot matici pokryty (mezera v PROP-032).

## 2. Problém dnes

- **Duplicita kódu**: `SyntaxValidator` (Roslyn parsování) je zkopírovaný do 2 projektů — změna logiky vyžaduje změnu na 2 místech.
- **Křehké snapshoty**: cesta k snapshotům závisí na `CopyToOutputDirectory` — při změně build konfigurace se snapshoty nenajdou.
- **Špatný DX při přidávání snapshotů**: první spuštění vždy failne — vývojář musí ručně vytvořit expected soubor, nebo spustit test a přejmenovat generated output.
- **Chybějící P6, P7 v property matici**: dokumentace matice počítá s 8 variantami (P1-P8), ale snapshot testy pokrývají jen P1-P5 a P8.

## 3. Předběžný směr řešení

### 3.1 Sdílená testovací knihovna
Vytvořit `Tests/MetaForge.Testing.Common/` s:
- `SyntaxValidator` — Roslyn C# syntax validation (jediný zdroj)
- `SnapshotComparer` — snapshot engine s podporou `UPDATE_SNAPSHOTS` env var
- `TestDataBuilder` — helpery pro konstrukci Core elementů v testech
- Případně `FakeHttpMessageHandler` (přesunout z Core.Tests)

Ostatní test projekty referencují `MetaForge.Testing.Common` místo duplikace.

### 3.2 SnapshotComparer vylepšení
- `UPDATE_SNAPSHOTS=true` proměnná prostředí: místo failu přepíše expected soubor aktuálním výstupem.
- Snapshot cesta: hledat relativně k projektu (`../../Snapshots/`) místo bin adresáře.

### 3.3 Doplnění chybějících property snapshotů
Dohledat, co je P6 a P7 v matici:

| Varianta | Status | 
|----------|--------|
| P1 GetSet | ✅ Hotovo |
| P2 GetOnly | ✅ Hotovo |
| P3 InitOnly | ✅ Hotovo |
| P4 Required | ✅ Hotovo |
| P5 StaticProperty | ✅ Hotovo |
| P6 ? (pravděpodobně Required InitOnly) | ❌ Chybí |
| P7 ? (pravděpodobně GetSet s default value) | ❌ Chybí |
| P8 RequiredGetOnly | ✅ Hotovo |

(Dohledat v `PropertyElement.cs` factory metody nebo v PROP-032 matici.)

## 4. Signál hodnoty

- **Údržba**: změna v SyntaxValidatoru se propaguje do všech test projektů automaticky.
- **Vývojářská zkušenost**: `UPDATE_SNAPSHOTS=true` umožní rychlé přegenerování snapshotů při refactoringu.
- **Konzistence**: všechny snapshoty používají stejný engine.
- **Úplnost matice**: property snapshoty budou kompletní.
- Zapadá do Epic 9 (testovací infrastruktura).

## 5. Rizika a nejasnosti

- Sdílená testovací knihovna vyžaduje rozhodnutí: NuGet balík (interní) nebo project reference?
- `UPDATE_SNAPSHOTS=true` musí být bezpečný — neměl by přepsat expected soubory při náhodném spuštění.
- P6 a P7 je třeba dohledat v architektonické dokumentaci nebo v PROP-032 — možná byly pouze vynechány.

## 6. Aktuální stav

✅ Převedeno na Candidate → PROP-049

## 7. Doporučený další krok

**Candidate Proposal** — odhad:
- Vytvoření `MetaForge.Testing.Common`: 1 den
- Migrace existujících testů na sdílenou knihovnu: 1 den
- SnapshotComparer vylepšení: 0.5 dne
- Doplnění P6, P7 snapshotů: 0.5 dne
- Celkem: ~3 dny

Navazuje na: `PROP-032 code review` (známé issues), `15-Test-Scaffold.md`
