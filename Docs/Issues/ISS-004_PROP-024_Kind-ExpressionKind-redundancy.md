# ISS-004 Redundantní Kind string a ExpressionKind enum

Datum: 2026-04-07
PROP: PROP-024
Soubor: `Src/MetaForge.Core/Expressions/Expression.cs`
Závažnost: 💡 Návrh
Stav: Open
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-024 (Core — StrongType, Expression, Record).

## 2. Popis problému

Abstraktní třída `Expression` má `Kind` jako `string` i `ExpressionKind` jako `enum` — obě vlastnosti nesou redundantní informaci. Časem by se mělo sjednotit pouze na `ExpressionKind` enum a `Kind` string odstranit, ale jedná se o breaking change vyžadující migraci.

## 3. Dopad

- Mírná redundance v modelu.
- Udržování dvou paralelních vlastností zvyšuje nároky na konzistenci.
- Breaking change — nelze opravit bez migrace volajícího kódu.

## 4. Doporučené řešení

Ponechat obojí pro zpětnou kompatibilitu. Při další major verzi (nebo vyhrazeném breaking release) odstranit `string Kind` a ponechat pouze `ExpressionKind` enum.

## 5. Otevřené otázky

- Kdy bude vhodný okamžik pro breaking release?

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*

---

## Související

- Vazby: `PROP-024`
