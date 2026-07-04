# PROPOSALS_NEXT — Zásobník kandidátních návrhů

> Návrhy, které jsou identifikované, ale zatím neschválené k implementaci.
> Nikdy neimplementovat přímo z tohoto souboru — vždy přesunout do PROPOSALS.md.

## Kandidátní návrhy

| ID | Název | Priorita | Odhad | Poznámka |
|----|-------|----------|-------|----------|
| PROP-010 | Infrastructure — persistence CommandLogu | 🟡 Vysoká | 3-5 dní | JSON souborová persistence, auto-save, load při startu |
| PROP-011 | WebApi host surface | 🟢 Nízká | 3-5 dní | ASP.NET Core REST API endpointy |
| PROP-012 | Payload escaping — JSON místo pipe-delimited | 🟡 Vysoká | 1 den | Oprava korupce dat v ReplayEngine |
| PROP-013 | Integrační testy celé pipeline | 🟢 Nízká | 2-3 dny | CLI → Facade → Patch → Log → Replay → Projection |
| PROP-014 | AI test project | 🟢 Nízká | 2 dny | MetaForge.Ai.Tests s mockovaným HttpClient |
| PROP-015 | ForgeBlock → CatalogManager propojení | 🟡 Vysoká | 2 dny | Automatická registrace CatalogEntries při startu |
| PROP-016 | Ollama konfigurace přes DI/IOptions | 🟢 Nízká | 1 den | URL, model, temperature z appsettings.json |

## Odložené návrhy

| ID | Název | Důvod odložení | Datum |
|----|-------|----------------|-------|
| —  | —     | —              | —     |

---

## Legenda priorit

- 🔴 Kritická — musí se implementovat co nejdříve
- 🟡 Vysoká — důležité pro další vývoj
- 🟢 Nízká — nice to have
- ⚪ Odloženo — zatím se neimplementuje
