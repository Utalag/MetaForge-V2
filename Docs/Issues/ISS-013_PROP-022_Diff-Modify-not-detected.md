# ISS-013 BusinessDocumentDiffer nezachycuje Modify operace

Datum: 2026-04-07
PROP: PROP-022
Soubor: `Src/MetaForge.Infrastructure/BusinessDocumentDiffer.cs`
Závažnost: ⚠️ Nízká
Stav: Open
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-022 (Observabilita — OpenTelemetry tracing a BusinessModel diff).

## 2. Popis problému

`BusinessDocumentDiffer` porovnává jen entity a atributy na úrovni Add/Remove. Nezachycuje změny vlastností (Modify), relace ani workflow stav. Diff je tak neúplný a může poskytovat zavádějící informace o rozsahu změn.

## 3. Dopad

- Observabilita je neúplná — chybí detekce Modify, relací a workflow změn.
- Uživatel nevidí úplný přehled změn v dokumentu.
- Při auditu může uniknout důležitá informace.

## 4. Doporučené řešení

Rozšířit `BusinessDocumentDiffer` o detekci `Modified` stavu a podporu pro další typy uzlů (relace, workflow stav). Implementovat `IEquatable<T>` nebo custom comparer pro PropertyElement a další uzly.

## 5. Otevřené otázky

- Jak granularitu diffu požadujeme (property-level, node-level)?
- Zda má diff podporovat i změny v CommandLogu nebo jen finální dokument?

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*

---

## Související

- Vazby: `PROP-022`
