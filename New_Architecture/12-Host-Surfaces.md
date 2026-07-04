# Host Surfaces

> CLI, MCP, WebApi — tenké vrstvy bez business logiky

---

## Princip

- Host surfaces volají pouze `BusinessAuthoringHostFacade`.
- Žádná business logika v host vrstvě.

## CLI (MetaForge.Cli)

```
Src/MetaForge.Cli/
├── MetaForge.Cli.csproj
├── Program.cs
├── Commands/
│   ├── AddEntityCommand.cs
│   ├── ProjectionCommand.cs
│   ├── TranslateCommand.cs
│   └── ExportCommand.cs
└── Formatting/
    └── CliOutputFormatter.cs
```

## MCP (MetaForge.Mcp)

```
Src/MetaForge.Mcp/
├── MetaForge.Mcp.csproj
├── Program.cs
└── Tools/
    ├── AddEntityTool.cs
    ├── GetProjectionTool.cs
    ├── TranslateTool.cs
    └── ExportTool.cs
```

## WebApi (MetaForge.WebApi)

```
Src/MetaForge.WebApi/
├── MetaForge.WebApi.csproj
├── Program.cs
├── Controllers/
│   ├── AuthoringController.cs
│   ├── ProjectionController.cs
│   └── ExportController.cs
├── Dtos/
│   ├── AddEntityRequest.cs
│   ├── AddEntityResponse.cs
│   ├── ProjectionResponse.cs
│   └── ErrorResponse.cs
└── Middleware/
    ├── ExceptionHandlingMiddleware.cs
    └── RequestLoggingMiddleware.cs
```

