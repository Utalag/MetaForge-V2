# ISS-009 CLI používá root scope pro scoped DI služby

Datum: 2026-04-07
PROP: PROP-026
Soubor: `Src/MetaForge.Cli/Program.cs`
Závažnost: ⚠️ Nízká
Stav: Open
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-026 (Host Surfaces — CLI/MCP/WebApi/REPL upgrade).

## 2. Popis problému

CLI používá `Host.CreateApplicationBuilder` a DI scoped služby, ale command handlery jsou statické lambda. `GetFacade()` vytváří nový scope? Ne — používá root `IServiceProvider` místo vytvoření nového scope.

## 3. Dopad

- Scoped služby nejsou správně uvolňovány (žijí po celou dobu života hostu).
- Při více commandech v rámci jednoho běhu může docházet ke sdílení stavu mezi commandy, kde by měl být každý command izolovaný.

## 4. Doporučené řešení

Zvážit vytvoření scope per command, nebo použít singleton pro Facade, pokud Facade sama negeneruje side-effecty napříč commandy.

## 5. Otevřené otázky

- Zda Facade a její závislosti mají být scoped nebo singleton.
- Jak velký je reálný dopad na funkcionalitu.

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*

---

## Související

- Vazby: `PROP-026`
