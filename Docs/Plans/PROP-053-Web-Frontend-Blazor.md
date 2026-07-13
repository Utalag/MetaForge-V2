# PROP-053 — Web Frontend (Blazor Server)

> Návrh jednoduchého frontendu pro MetaForge — strom modelu, konfigurace, ForgeBlock výběr.
> **Stav:** 🟡 Draft
> **Vrstva:** Frontend, Host Surfaces
> **Priorita:** ⚪ Na zvážení
> **Odhad:** ~5 dní
> **Závislosti:** PROP-044 (hotovo), PROP-045 (hotovo)

---

## 1. Motivace

MetaForge dnes umí modelovat business entity a generovat C# kód, ale pouze přes CLI a MCP rozhraní. Chybí **přehledný vizuální nástroj**, který by umožnil:

- Zobrazit celý model ve **stromové struktuře**
- **Konfigurovat** jednotlivé vrstvy (Storage, AI, Generator) přes formulář
- **Vybrat a konfigurovat ForgeBlocky** (toggle enable/disable, nastavení)
- **Generovat C# kód** jedním tlačítkem
- Vidět **SyncState** atributů (co je přeloženo, co čeká na enrichment)

---

## 2. Rozhodnutí

### 2.1 Technologie: Blazor Server

| Kritérium | Blazor Server | WASM + WebApi | Electron |
|-----------|--------------|---------------|----------|
| Jednoduchost | ✅ **Nejjednodušší** | ❌ Nutný WebApi | ❌ Složitý setup |
| Sdílení typů | ✅ **Ano, přímo** | ✅ Přes shared lib | ❌ Nutná TS/JS |
| Perzistence stavu | ✅ Přes DI scoped | ✅ Přes WebApi | ✅ Přes IPC |
| Rychlost UI | 🟢 Nízká latence | 🟢 Rychlý po načtení | 🟢 Nativní |
| Nasazení | ✅ **Jeden projekt** | ❌ Dva projekty | ❌ Nutný builder |

**Zdůvodnění:** Blazor Server je jediná varianta, která nevyžaduje novou API vrstvu. Může přímo injectovat `BusinessAuthoringHostFacade` a všechny stávající služby. Všechny C# typy jsou dostupné bez duplikace.

### 2.2 UI Framework: MudBlazor

Lehký, moderní, MIT licence. Obsahuje vše potřebné:
- `MudTreeView` — strom modelu
- `MudTable` — seznam atributů
- `MudCard` — ForgeBlock karty
- `MudTextField`, `MudSelect`, `MudSwitch` — formuláře
- `MudDialog` — dialogy
- `MudSnackbar` — notifikace

---

## 3. Struktura projektu

```
Src/MetaForge.Web/
├── MetaForge.Web.csproj           (Blazor Server, net10.0, MudBlazor)
├── Program.cs                     (DI setup + middleware)
├── appsettings.json               (config)
├── Components/
│   ├── App.razor
│   ├── _Host.cshtml
│   ├── Layout/
│   │   ├── MainLayout.razor       (sidebar + main area)
│   │   └── NavMenu.razor          (navigace)
│   ├── Pages/
│   │   ├── Dashboard.razor        (přehled projektu, generovat)
│   │   ├── ModelTree.razor        (strom celého modelu)
│   │   ├── EntityDetail.razor     (detail entity + atributy)
│   │   ├── ForgeBlocks.razor      (katalog ForgeBlocků)
│   │   ├── Configuration.razor    (nastavení)
│   │   └── Workflows.razor        (workflow editor)
│   └── Shared/
│       ├── SyncStateBadge.razor
│       ├── ConfirmDialog.razor
│       └── StatusBar.razor
└── wwwroot/css/
```

---

## 4. Hlavní obrazovky

### 4.1 Dashboard
- **Project info**: název, verze, schema verze
- **Statistiky**: počet entit, atributů, workflow, commandů
- **SyncState přehled**: kolik atributů je synced/pending/unsynced
- **Rychlé akce**: Generate Code, Export, Save

### 4.2 Model Tree

Stromová struktura celého modelu:

```
📦 ProjectName
├── 📁 Entities
│   ├── 📄 Customer
│   │   ├── 🔷 Id (int)
│   │   ├── 🔷 Name (string, required) [✓ Synced]
│   │   ├── 🔷 Email (string, required)
│   │   └── 🔗 Relations: → Order
│   ├── 📄 Order
│   └── ...
├── 🔗 Relations
├── ⚙️ Workflows
├── 📦 Custom Types
└── ❓ Pending Questions
```

Implementace: `MudTreeView` s lazy loading (každý expand volá Facade pro detail).

### 4.3 Entity Detail
- **Záhlaví**: název entity, ID, created date
- **Tabulka atributů** (`MudTable`):
  - Sloupce: jméno, typ, required, SyncState badge, akce
  - SyncState badge: Synced=zelený, Pending=žlutý, None=šedý
  - Akce na řádek: Edit, Delete, Enrich with AI
- **Panel chování** (behaviors) — seznam s popisem
- **Panel relací** — odkazy na související entity
- **Tlačítka**: Add Attribute (+ inline formulář), Enrich All, Generate Code for Entity

### 4.4 ForgeBlocks

Karty seskupené do kategorií:
- **Core** (Math, String, Validation) — vždy zapnuté, nelze vypnout
- **Domain** (AutoMapper) — toggle
- **Infrastructure** (EF Core, FluentValidation) — toggle

Každá karta:
- Název, popis, verze
- Tier badge (Free/Pro/Enterprise)
- Seznam capability s ikonkami
- Enable/Disable toggle (jen pro Domain/Infrastructure)
- Expandovatelný **config panel** (např. EF Core: connection string, namespace)

### 4.5 Configuration

Formulář rozdělený do sekcí:

| Sekce | Pole | Typ |
|-------|------|-----|
| **Storage** | CommandLogPath, DocumentPath, AutoSave, Interval | text, toggle, number |
| **AI** | Provider (None/Ollama), Endpoint, Model, Temperature, MaxTokens | select, text, slider, number |
| **Generator** | License Tier (Sandbox/Domain/Infrastructure/Full) | select |
| **Catalog** | BuiltInPresetsPath, EnableFileSystemProvider | text, toggle |
| **Logging** | Level (Debug/Info/Warning/Error), Console | select, toggle |

Uložení: `appsettings.user.json` přes `IOptions<T>` + JSON config provider.

### 4.6 Workflows
- Seznam workflow s kroky a přechody
- Vizuální reprezentace (MudStepper nebo vlastní diagram)
- Add Workflow, Add Step, Add Transition formuláře
- Delete workflow/kroku/přechodu

---

## 5. DI registrace

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

// MetaForge — Scoped (per-SignalR circuit)
builder.Services.AddScoped<BusinessAuthoringDocument>();
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();
builder.Services.AddScoped<BusinessAuthoringHostFacade>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<IBusinessTranslator, DefaultBusinessTranslator>();
builder.Services.AddScoped<WriteBackService>();

// MetaForge — Singleton
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();
```

---

## 6. Data flow

```
User akce v prohlížeči
    → Blazor Server (SignalR)
    → C# event handler v komponentě
    → BusinessAuthoringHostFacade.AddEntity(...)
    → PatchEngine.Apply(document, new AddEntityOp(...))
    → CommandLogStore.TryAppend(envelope)
    → _document = updated
    → StateHasChanged()
    → UI re-render (pouze změněné části díky Blazor diffing)
```

Bez HTTP, bez JSON, bez duplicit — přímo v paměti serveru.

---

## 7. Perzistence configu

Uživatelské nastavení se ukládá do `appsettings.user.json` (přidáno do `.gitignore`):

```json
{
  "MetaForge": {
    "Storage": { "CommandLogPath": "data/commands.jsonl", "AutoSave": true },
    "AI": { "Provider": "Ollama", "Endpoint": "http://localhost:11434", "Model": "gemma4" }
  }
}
```

Načítání: `builder.Configuration.AddJsonFile("appsettings.user.json", optional: true, reloadOnChange: true)`

---

## 8. Realizace — kroky

| Krok | Popis | Odhad |
|------|-------|-------|
| 1 | Vytvořit `Src/MetaForge.Web` projekt + MudBlazor NuGet | 0.5 dne |
| 2 | Zapojit DI (MetaForge služby) | 0.5 dne |
| 3 | Dashboard page | 0.5 dne |
| 4 | Model Tree (MudTreeView + lazy loading) | 1 den |
| 5 | Entity Detail + atributy (MudTable + formuláře) | 1 den |
| 6 | ForgeBlocks page (karty + toggle + config) | 1 den |
| 7 | Configuration page (formulář → appsettings.user.json) | 0.5 dne |
| 8 | Workflow editor | 1 den |
| **Celkem** | | **~5 dní** |

---

## 9. Co je mimo scope

- **Autentizace / autorizace** — frontend je lokální nástroj
- **Multi-user** — jeden uživatel na instanci
- **Dark mode** — MudBlazor podporuje, ale není priorita
- **Mobilní responzivita** — primárně desktopové rozhraní
- **Export do PDF/DOCX** — pouze C# code generation

---

## 10. Otevřené otázky

| # | Otázka | Doporučení |
|---|--------|------------|
| 1 | Blazor Server vs WASM+WebApi? | **Server** — jednodušší, žádná nová API |
| 2 | MudBlazor vs Radzen? | **MudBlazor** — lehčí, open source |
| 3 | Sdílet config s CLI (appsettings.json) nebo vlastní? | **Vlastní** `appsettings.user.json` — CLI a Web mohou koexistovat |
| 4 | ForgeBlock config ukládat do dokumentu nebo zvlášť? | Do `BusinessAuthoringDocument.Metadata` — jeden zdroj pravdy |
