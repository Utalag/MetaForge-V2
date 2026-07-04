# Security Model

> Bezpečnostní model pro MetaForge — MCP, WebApi a AI provider komunikace.

---

## Principy

1. **MCP používá lokální transport** — stdio pro lokální vývoj, žádná síťová komunikace.
2. **WebApi je určeno pro lokální síť** — žádná built-in autentizace pro MVP. Přidat v případě potřeby.
3. **AI API klíče jdou do proměnných prostředí** — nikdy do kódu nebo appsettings.json v repu.
4. **CommandLog je append-only** — žádné mazání ani úprava historie.
5. **Exportovaný kód je artifact** — žádné zpětné ovlivňování business modelu z vygenerovaného kódu.

---

## MCP — Transport a autentizace

### Lokální transport (stdio)

Pro lokální vývoj a AI klienty (např. Claude Desktop, VS Code extensions):

```
MCP Server (MetaForge.Mcp)
    ↓ komunikace přes stdio (JSON-RPC)
AI klient (Claude Desktop, VS Code)
```

- Žádná autentizace — transport je lokální.
- Žádná síťová komunikace — žádný exposure risk.

### Vzdálený MCP transport (future)

Pro budoucí hosted scénář:

| Metoda | Zabezpečení |
|--------|------------|
| HTTP+SSE | API key v headeru `x-api-key` |
| WebSocket | OAuth 2.0 Bearer token |

---

## WebApi — Autentizace a autorizace

### MVP režim (žádná autentizace)

- WebApi běží na `localhost` — přístup pouze z lokálního stroje.
- Žádná autentizace, žádná autorizace.
- Všechny endpointy jsou veřejné (pouze pro lokální vývoj).

### Produkční režim (volitelný)

```csharp
// Program.cs — volitelná autentizace
if (builder.Configuration.GetValue<bool>("MetaForge:Security:EnableAuth"))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["MetaForge:Security:Authority"];
            options.Audience = "metaforge-webapi";
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Authoring", policy =>
            policy.RequireClaim("scope", "metaforge:authoring"));
    });
}
```

### CORS

```csharp
// Program.cs — povolit frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
```

---

## AI Provider — API klíče

### Konfigurace přes proměnné prostředí

```bash
# Windows PowerShell
$env:MetaForge__AI__ApiKey = "sk-..."
$env:MetaForge__AI__Endpoint = "https://api.openai.com/v1"

# Linux/macOS
export MetaForge__AI__ApiKey="sk-..."
export MetaForge__AI__Endpoint="https://api.openai.com/v1"
```

### Ochrana API klíčů

- API klíče se nikdy neukládají do `appsettings.json` v repozitáři.
- `appsettings.Development.json` je v `.gitignore`.
- Pro GitHub Actions: použít `secrets.NUGET_API_KEY` a podobně.

---

## CommandLog — Integrita dat

| Hrozba | Mitigace |
|--------|----------|
| Ruční smazání command.log souboru | Pravidelné zálohy, git tagy checkpointů |
| Korupce JSON souboru | Deserializace s validací, auto-backup před zápisem |
| Append mimo PatchEngine | Facade je jediný vstup — žádný přímý přístup k CommandLogStore z host surfaces |
| Neautorizovaný přístup k souboru | OS-level file permissions |

---

## ForgeBlock registry — bezpečnost

- ForgeBlock registrace je možná pouze programaticky — žádný runtime download.
- FileSystem catalog provider čte pouze z povolené cesty (`MetaForge:Catalog:FileSystemCatalogPath`).
- Marketplace provider (future): podepsané balíčky, hash verifikace.

---

## Security checklist pro nový host surface

- [ ] Určit transport (stdio/HTTP/WebSocket)
- [ ] Rozhodnout auth requirements (none/api key/oauth)
- [ ] Zajistit že neexponuje CommandLogStore nebo PatchEngine přímo
- [ ] Logovat všechny přístupy (audit log)
- [ ] Omezit CORS pokud je to WebApi
- [ ] Validovat všechny vstupy (DTO validation)
