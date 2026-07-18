# ForgeBlock — Externí knihovny pro integraci

> Katalog nejpoužívanějších C#/.NET knihoven vhodných pro zapouzdření jako ForgeBlock balíčky.
> Každá knihovna je hodnocena z hlediska vhodnosti integrace (capability metadata, codegen, katalog/presety, discovery, host surface).

**Aktualizace:** PROP-029 (2026-07-04) — Implementovány 3 nové ForgeBlocky: AutoMapper, EF Core, FluentValidation. Všechny implementují `IForgeBlockCapabilityPackage` s tier omezením.

---

## Aktuální stav implementace (PROP-029)

| ForgeBlock | Handle | Tier | Stav | Capabilities |
|---|---|---|---|---|
| **Math** | `math` | — | ✅ Implementováno (PROP-008) | 8 aritmetických operací |
| **String** | `string` | — | ✅ Implementováno (PROP-008) | 8 textových operací |
| **Validation** | `validation` | — | ✅ Implementováno (PROP-008) | 8 validačních operací |
| **AutoMapper** | `mapping-automapper` | 🟡 Domain (Tier 1+) | ✅ Implementováno (PROP-029) | generate-mapping-profile, generate-dto, generate-mapping-config |
| **EF Core** | `orm-ef-core` | 🔴 Infrastructure (Tier 2+) | ✅ Implementováno (PROP-029) | generate-dbcontext, generate-entity-config, generate-repository, generate-migration, generate-di |
| **FluentValidation** | `validation-fluent` | 🔴 Infrastructure (Tier 2+) | ✅ Implementováno (PROP-029) | generate-validator, generate-validation-rules, generate-di |

Všechny ForgeBlocky implementují `IForgeBlockCapabilityPackage` z `MetaForge.Core` a registrují se do `ForgeBlockRegistry`. AutoMapper, EF Core a FluentValidation odkazují `MetaForge.Generators` pro `GeneratorTier` monetizaci.

### Struktura adresářů

```
Src/ForgeBlocks/
├── Math/                    ← basic, bez tier omezení
│   ├── MetaForge.ForgeBlocks.Math.csproj
│   └── MathForgeBlock.cs
├── String/                  ← basic, bez tier omezení
│   ├── MetaForge.ForgeBlocks.String.csproj
│   └── StringForgeBlock.cs
├── Validation/              ← basic, bez tier omezení
│   ├── MetaForge.ForgeBlocks.Validation.csproj
│   └── ValidationForgeBlock.cs
├── AutoMapper/              ← Domain tier
│   ├── MetaForge.ForgeBlocks.AutoMapper.csproj
│   └── AutoMapperForgeBlock.cs
├── EntityFramework/         ← Infrastructure tier
│   ├── MetaForge.ForgeBlocks.EntityFrameworkCore.csproj
│   └── EfCoreForgeBlock.cs
└── FluentValidation/        ← Infrastructure tier
    ├── MetaForge.ForgeBlocks.FluentValidation.csproj
    └── FluentValidationForgeBlock.cs
```

### Příklad: EfCoreForgeBlock

```csharp
public sealed class EfCoreForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "orm-ef-core";
    public string Version => "1.0.0";
    public ForgeBlockPackageDescriptor Descriptor { get; } = new()
    {
        DisplayName = "Entity Framework Core",
        Description = "Generuje DbContext, entity konfiguraci, migrace a repository vrstvu",
        Tags = ["orm", "ef-core", "database", "sql", "migration"],
        Tier = GeneratorTier.Infrastructure,  // ← PAID TIER
    };
    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = [
        new("generate-dbcontext", "Generuje AppDbContext s DbSet<T> pro každou entitu"),
        new("generate-entity-config", "Generuje IEntityTypeConfiguration<T> pro každou entitu"),
        new("generate-repository", "Generuje repository interface + implementaci"),
        new("generate-migration", "Generuje EF Core migraci"),
    ];
    // ...
}
```

Všechny implementované ForgeBlocky jsou **metadata-only** — definují capability a catalog entries. (codegen je v PROP-025).

---

## Kritéria hodnocení

| Úroveň | Význam |
|--------|--------|
| ⭐⭐⭐⭐⭐ | Imediátní kandidát — přirozený partner MetaForge modelu, high adoption, silný codegen potenciál |
| ⭐⭐⭐⭐ | Silný kandidát — široce používaný, codegen dává smysl |
| ⭐⭐⭐ | Niche nebo klesající adopce — implementovat později |
| ⭐⭐ | Okrajový — spíše reference |

---

## 1. Data / ORM

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Entity Framework Core** | ⭐⭐⭐⭐⭐ | Nejpoužívanější ORM. ForgeBlock generuje `DbContext`, entity mapping, migrační config, repository vrstvu z business modelu. |
| **Dapper** | ⭐⭐⭐⭐ | Micro-ORM. Generování SQL dotazů, mapování výsledků, repository vrstva. |
| **Marten** | ⭐⭐⭐⭐ | PostgreSQL + Event Store. Přirozeně zapadá do MetaForge CommandLog konceptu — generování event-sourcing repository. |
| **MongoDB.Driver** | ⭐⭐⭐⭐ | Document DB. Generování repository vrstvy pro NoSQL, index definitions. |
| **NHibernate** | ⭐⭐⭐ | Starší, stále používaný v enterprise projektech. Nižší priorita. |
| **Cosmos DB SDK** | ⭐⭐⭐ | Azure-specifický, relevantní pro enterprise Azure ekosystém. |

### Capability návrh

```yaml
capability: orm-ef-core
family: data-access
handles: [orm, entity-framework, ef-core, dbcontext, migration]
generator:
  csharp: DbContext + entity mapping + repository interface
  typescript: TypeORM/MikroORM analog
  python: SQLAlchemy analog
catalog:
  presets: [ef-core-config, ef-core-audit, ef-core-soft-delete]
```

```yaml
capability: orm-dapper
family: data-access
handles: [dapper, micro-orm, sql-mapping, query]
generator:
  csharp: Dapper query extensions + repository implementation
```

---

## 2. Objektové mapování

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **AutoMapper** | ⭐⭐⭐⭐⭐ | Extrémně rozšířený. ForgeBlock generuje `Profile` třídy, mapping konfiguraci z atributových mapování. |
| **Mapperly** (Riok.Mapperly) | ⭐⭐⭐⭐⭐ | Roslyn source-generator. Perfektní pro codegen — vygenerovat mapper jako partial class přímo. |
| **Mapster** | ⭐⭐⭐⭐ | Rychlejší alternativa AutoMapperu. Generování mapping configu. |

### Capability návrh

```yaml
capability: model-mapping
family: mapping
handles: [mapper, automapper, mapster, mapperly, object-mapping, map]
generator:
  csharp: MappingProfile třída nebo Mapperly partial mapper
  typescript: mapper funkce
  python: dataclass mapping
  java: MapStruct mapper
catalog:
  presets: [mapping-default, mapping-flatten, mapping-reverse]
```

---

## 3. Validace

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **FluentValidation** | ⭐⭐⭐⭐⭐ | Standard pro validační pravidla. ForgeBlock generuje `AbstractValidator<T>` třídy z atributových constraintů v business modelu. |
| **System.ComponentModel.DataAnnotations** | ⭐⭐⭐⭐ | Vestavěný atributový systém. ForgeBlock rozšiřuje o custom atributy a generuje data annotations. |

### Capability návrh

```yaml
capability: validation-fluent
family: validation
handles: [validation, fluent-validation, validator, rule, constraint]
generator:
  csharp: FluentValidation AbstractValidator třídy
  typescript: zod/yup schema
  python: pydantic model
  java: Jakarta Validation
catalog:
  presets: [validation-default, validation-guid, validation-email, validation-phone]
```

---

## 4. Value Objects / Doménové typy

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Vogen** | ⭐⭐⭐⭐⭐ | ValueObject source-generator s integrací do EF Core, Dapper, JSON, BSON, Orleans, MessagePack ad. MetaForge generuje Vogen-annotated kód z `CustomTypeDefinition` — StrongType je metareprezentace, `ValueObjectElement` nese Vogen-specific properties. Vogen source-generuje konvertory při kompilaci target projektu. Konverzní flags se volí až při výběru infrastruktury (default: `None`). |
| **Ardalis.SmartEnum** | ⭐⭐⭐⭐⭐ | Typované enumy. ForgeBlock generuje SmartEnum třídy z business enumů (s výčtem hodnot, popisem, implicitní konverzí). |
| **OneOf** | ⭐⭐⭐⭐ | Discriminované unie. Užitečné pro výsledkové typy (Success/Error/ValidationResult). |
| **LanguageExt** | ⭐⭐⭐⭐ | Funkcionální knihovna (Option, Either, Chain, Reader). ForgeBlock generuje funkční wrapper patterny. |

### Capability návrh

```yaml
capability: value-object-vogen
family: domain-types
handles: [value-object, vogen, strong-type, custom-type, primitive-obsession,
          ef-core-converter, dapper-handler, json-converter, bson-serializer]
parameters:
  - name: conversions
    type: string[]
    description: Target integrace pro Vogen Conversions flags. Default: None (┼ż├ídn├ę konvertory). Konverze se vol├ş a┼ż p┼Öi v├Żb─Ťru infrastruktury.
    options: [ef-core, dapper, system-text-json, newtonsoft-json, bson,
              messagepack, orleans, xml, linq2db, servicestack, type-converter]
    default: []
generator:
  csharp: >
    Vogen [ValueObject] readonly partial struct/class.
    MetaForge generuje atribut s Conversions flags,
    Vogen source-generuje konvertory p┼Öi kompilaci.
  typescript: branded types
  python: dataclass + validace
catalog:
  presets:
    - vogen-no-conversions: []
    - vogen-ef-core: [ef-core, system-text-json]
    - vogen-fullstack: [ef-core, dapper, system-text-json, bson, messagepack]
    - vogen-api-only: [system-text-json, type-converter]
```

```yaml
capability: smart-enum
family: domain-types
handles: [smart-enum, typed-enum, ardalis, enum-class]
generator:
  csharp: SmartEnum třídy
  typescript: const enum + mapper
  python: Enum + description
catalog:
  presets: [smart-enum-default, smart-enum-with-description]
```

---

## 5. CQRS / Mediator / Messaging

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **MediatR** | ⭐⭐⭐⭐⭐ | Nejpoužívanější CQRS mediator. ForgeBlock generuje `IRequest`/`IRequestHandler` páry z business behaviors (command/query dělení). |
| **MassTransit** | ⭐⭐⭐⭐⭐ | Message bus. Generování consumerů, message typů, routing configu, saga state machine. |
| **Brighter** | ⭐⭐⭐⭐ | Alternativa s command pipeline a podporou outbox. |
| **Rebus** | ⭐⭐⭐⭐ | Jednodušší service bus. |

### Capability návrh

```yaml
capability: cqrs-mediatr
family: messaging
handles: [cqrs, mediator, mediatr, command, query, handler, pipeline]
generator:
  csharp: MediatR IRequest + IRequestHandler třídy
  typescript: ts-cqry/cqrs třídy
  python: command pattern + handler
catalog:
  presets: [cqrs-default, cqrs-with-validation, cqrs-with-logging]
```

```yaml
capability: messaging-masstransit
family: messaging
handles: [masstransit, message-bus, consumer, event-bus, saga, rabbitmq]
generator:
  csharp: MassTransit consumer + message contracts
  typescript: event emitter patterns
```

---

## 6. Logging / Observabilita

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Serilog** | ⭐⭐⭐⭐⭐ | Standard pro strukturované logování. ForgeBlock generuje logger config, enrichery, sink setup. |
| **OpenTelemetry .NET** | ⭐⭐⭐⭐⭐ | Rychle rostoucí standard pro observabilitu. Generování tracing setup, metrics, exporter config. |
| **NLog** | ⭐⭐⭐⭐ | Alternativa k Serilogu. |

### Capability návrh

```yaml
capability: logging-serilog
family: observability
handles: [serilog, logging, structured-logging, log, enricher, sink]
generator:
  csharp: Serilog config + enricher setup
catalog:
  presets: [logging-console, logging-file, logging-seq, logging-elk]
```

```yaml
capability: telemetry-opentelemetry
family: observability
handles: [opentelemetry, tracing, metrics, observability, otel, distributed-tracing]
generator:
  csharp: OpenTelemetry setup + exporter config
```

---

## 7. API klienti / HTTP

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Refit** | ⭐⭐⭐⭐⭐ | REST client generátor. Z business modelu vygenerovat HTTP API rozhraní — ideální codegen scénář. |
| **RestSharp** | ⭐⭐⭐ | Starší, stále používaný. |
| **NSwag** | ⭐⭐⭐⭐ | OpenAPI generování client/server kódu. Lze propojit business model → OpenAPI spec → generated code. |

### Capability návrh

```yaml
capability: api-client-refit
family: http
handles: [refit, rest-client, http-api, api-interface, http-client]
generator:
  csharp: Refit interface + HttpClient setup
  typescript: axios/angular http service
  python: httpx/aiohttp client
```

---

## 8. Background Jobs / Scheduling

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Hangfire** | ⭐⭐⭐⭐⭐ | Nejpoužívanější job scheduler. ForgeBlock generuje job definice, cron schedule, dashboard config z business behaviors. |
| **Quartz.NET** | ⭐⭐⭐⭐ | Enterprise scheduler s pokročilým cron modelováním. |
| **Coravel** | ⭐⭐⭐⭐ | Jednoduchý, lehký scheduler — vhodný pro menší projekty. |

### Capability návrh

```yaml
capability: background-jobs-hangfire
family: scheduling
handles: [hangfire, background-job, scheduled-job, fire-and-forget, cron, recurring]
generator:
  csharp: Hangfire job třídy + startup config
catalog:
  presets: [hangfire-inmemory, hangfire-sqlserver, hangfire-postgres]
```

---

## 9. Caching

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **FusionCache** | ⭐⭐⭐⭐⭐ | Moderní caching (memory + distributed + hybrid). Generování cache policy, cache-aside/repository pattern. |
| **StackExchange.Redis** | ⭐⭐⭐⭐ | Redis klient. Generování repository caching layer, redis config. |
| **Microsoft.Extensions.Caching** | ⭐⭐⭐⭐ | Abstrakce — MemoryCache + IDistributedCache. |

### Capability návrh

```yaml
capability: caching-fusion
family: caching
handles: [fusioncache, cache, caching, hybrid-cache, memory-cache, distributed-cache]
generator:
  csharp: FusionCache setup + cache policy třídy
catalog:
  presets: [cache-memory, cache-redis, cache-hybrid]
```

---

## 10. Testování / Auto-fixture

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Bogus** | ⭐⭐⭐⭐⭐ | Generování falešných dat. ForgeBlock generuje `Faker<T>` definice z business modelu (dle atributů a constraintů). |
| **AutoFixture** | ⭐⭐⭐⭐ | Auto-generování anonymních testovacích objektů. |
| **NSubstitute / Moq** | ⭐⭐⭐⭐ | Mocking knihovny. ForgeBlock generuje mock setup z interface definic. |
| **FluentAssertions** | ⭐⭐⭐⭐ | Assertion knihovna. |
| **Testcontainers** | ⭐⭐⭐⭐ | Integrační testy s reálnými DB/container images. |

### Capability návrh

```yaml
capability: test-data-bogus
family: testing
handles: [bogus, fake-data, test-data, faker, seed-data, mock-data]
generator:
  csharp: Bogus Faker<T> definice
  typescript: faker.js definice
  python: faker definice
catalog:
  presets: [bogus-default, bogus-localized-cs, bogus-localized-en]
```

---

## 11. PDF / Reporting / Excel

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **QuestPDF** | ⭐⭐⭐⭐⭐ | Moderní fluent PDF API. Generování report layoutu z business modelu. |
| **ClosedXML** | ⭐⭐⭐⭐ | Excel bez Excelu. Generování Excel reportů. |
| **EPPlus** | ⭐⭐⭐⭐ | Excel (commercial license pro komerční užití). |

### Capability návrh

```yaml
capability: reporting-pdf
family: reporting
handles: [questpdf, pdf, report, document-export]
generator:
  csharp: QuestPDF document definition
```

---

## 12. Email / Notification

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **MailKit / MimeKit** | ⭐⭐⭐⭐⭐ | Standard pro SMTP/IMAP. Generování email template, SMTP config, notifikační služby. |
| **FluentEmail** | ⭐⭐⭐⭐ | Fluent API pro sestavení emailů s templatingem. |
| **SendGrid SDK** | ⭐⭐⭐ | Cloud-specifický, ale široce používaný. |

---

## 13. State Machines / Workflow

> ⚠️ Explicitní workflow model byl z platformy odstraněn (PROP-063, 2026-07-18).
> Náhrada: `FlowGraphSection` — odvozená grafová vizualizace z entit a relací (PROP-062).
> State machine generování (Stateless, WorkflowCore) zůstává jako potenciální ForgeBlock capability,
> ale jejím vstupem již není `BusinessWorkflowNode` — byl by jím `FlowGraphSection` nebo budoucí
> query vrstva nad ním.

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Stateless** | ⭐⭐⭐⭐⭐ | Lehký state machine. Vstup: FlowGraphSection nebo explicitní model v budoucnu. |
| **WorkflowCore** | ⭐⭐⭐⭐ | Plnohodnotný workflow engine s podporou kroků, branching, retry. |

### Capability návrh (revidováno PROP-063)

```yaml
capability: state-machine
family: workflow
handles: [stateless, state-machine, fsm, workflow, state, transition, trigger]
generator:
  csharp: Stateless state machine config
  typescript: XState machine
note: Vstupem již není BusinessWorkflowNode. FlowGraphSection nebo budoucí explicitní model.
```

---

## 14. Dependency Injection / Modularita

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Scrutor** | ⭐⭐⭐⭐ | Batch DI registrace (assembly scanning, decoration). |
| **Autofac** | ⭐⭐⭐⭐ | Alternativní DI container s module support. |
| **Lamar** | ⭐⭐⭐ | Rychlejší alternativa, méně rozšířená. |

---

## 15. Serialization

| Knihovna | ForgeBlock vhodnost | Zdůvodnění |
|----------|-------------------|------------|
| **Newtonsoft.Json** | ⭐⭐⭐⭐ | Stále široce používaný. Generování JsonConverter, contract resolver. |
| **YamlDotNet** | ⭐⭐⭐⭐ | YAML serializace — užitečné pro konfigurační soubory. |
| **CsvHelper** | ⭐⭐⭐⭐ | CSV export/import — generování ClassMap definic. |

---

## 16. Vestavěné .NET knihovny (System.*)

> Standard Library Wrappers — zabalují built-in `System.*` API za sémantické handles `mf.*.*`.
> ForgeBlock vhodnost: ⭐⭐⭐⭐⭐ — nulová závislost na externích NuGet balíčcích, nejvyšší stabilita.

| # | ForgeBlock | .NET zdroj | Capability ID | Sémantické handles (příklady) |
|---|-----------|-----------|---------------|-------------------------------|
| 1 | **Text** | `System.Text.RegularExpressions`, `System.String`, `StringBuilder` | `text-processing` | `mf.text.regex-match`, `mf.text.regex-replace`, `mf.text.format`, `mf.text.concat`, `mf.text.split`, `mf.text.join`, `mf.text.trim`, `mf.text.pad` |
| 2 | **Collections** | `System.Linq`, `System.Collections.Generic`, `System.Collections.Immutable` | `collection-operations` | `mf.collection.filter`, `mf.collection.sort`, `mf.collection.group`, `mf.collection.distinct`, `mf.collection.aggregate`, `mf.collection.first`, `mf.collection.last` |
| 3 | **Json** | `System.Text.Json` | `json-processing` | `mf.json.serialize`, `mf.json.deserialize`, `mf.json.merge`, `mf.json.select`, `mf.json.pretty` |
| 4 | **Xml** | `System.Xml.Linq` | `xml-processing` | `mf.xml.parse`, `mf.xml.select`, `mf.xml.transform`, `mf.xml.serialize`, `mf.xml.validate` |
| 5 | **Http** | `System.Net.Http` | `http-client` | `mf.http.get`, `mf.http.post`, `mf.http.put`, `mf.http.delete`, `mf.http.download`, `mf.http.headers` |
| 6 | **FileSystem** | `System.IO` | `file-system` | `mf.fs.read`, `mf.fs.write`, `mf.fs.copy`, `mf.fs.move`, `mf.fs.delete`, `mf.fs.watch`, `mf.fs.list`, `mf.fs.info` |
| 7 | **Crypto** | `System.Security.Cryptography` | `cryptography` | `mf.crypto.hash`, `mf.crypto.encrypt`, `mf.crypto.decrypt`, `mf.crypto.sign`, `mf.crypto.random`, `mf.crypto.key` |
| 8 | **Compression** | `System.IO.Compression` | `compression` | `mf.compress.gzip`, `mf.compress.deflate`, `mf.compress.zip`, `mf.compress.tar`, `mf.compress.brotli` |
| 9 | **Encoding** | `System.Text.Encoding`, `System.Web` | `encoding` | `mf.encode.base64`, `mf.encode.url`, `mf.encode.html`, `mf.encode.uri`, `mf.encode.hex` |
| 10 | **DateTime** | `System.DateTime`, `System.TimeZoneInfo`, `System.Calendar` | `date-time` | `mf.time.now`, `mf.time.parse`, `mf.time.format`, `mf.time.add`, `mf.time.diff`, `mf.time.timezone`, `mf.time.utc` |
| 11 | **Concurrency** | `System.Threading.Tasks`, `System.Threading.Channels`, `System.Threading.Locks` | `concurrency` | `mf.thread.parallel`, `mf.thread.delay`, `mf.thread.channel`, `mf.thread.lock`, `mf.thread.task` |
| 12 | **Numerics** | `System.Numerics` (Vector, Matrix, Complex, BigInteger) | `numerics` | `mf.num.vector`, `mf.num.matrix`, `mf.num.complex`, `mf.num.bigint`, `mf.num.quaternion` |
| 13 | **Reflection** | `System.Reflection` | `reflection` | `mf.reflect.type`, `mf.reflect.property`, `mf.reflect.method`, `mf.reflect.attribute`, `mf.reflect.activate` |
| 14 | **Environment** | `System.Environment`, `System.Runtime.InteropServices`, `System.PlatformID` | `system-info` | `mf.sys.os`, `mf.sys.env`, `mf.sys.process`, `mf.sys.machine`, `mf.sys.runtime` |

### Capability návrh — Standard Library Wrapper pattern

```yaml
capability: text-processing
family: system-library
handles: [text, string, regex, regexp, format, concat, split, join, trim, pad, substring]
generator:
  csharp: System.String + Regex extension methods
  typescript: String.prototype + RegExp
  python: str + re module
  java: String + Pattern/Matcher
catalog:
  presets: [text-default, text-regex, text-format, text-concat]
```

```yaml
capability: json-processing
family: system-library
handles: [json, serialize, deserialize, parse, stringify]
generator:
  csharp: System.Text.Json (JsonSerializer, JsonDocument, JsonNode)
  typescript: JSON.parse/stringify
  python: json module
  java: Jackson / Gson
catalog:
  presets: [json-default, json-camelcase, json-snakecase, json-ignore-null]
```

```yaml
capability: http-client
family: system-library
handles: [http, rest, api, get, post, put, delete, download, web-request]
generator:
  csharp: System.Net.HttpClient
  typescript: fetch / axios
  python: httpx / requests / aiohttp
  java: java.net.http.HttpClient / OkHttp
catalog:
  presets: [http-default, http-auth-bearer, http-retry, http-timeout]
```

### Capability návrh — Architectural Patterns

```yaml
capability: caching
family: architecture
handles: [cache, memory-cache, distributed-cache, imemorycache, idistributedcache]
generator:
  csharp: Microsoft.Extensions.Caching (IMemoryCache, IDistributedCache)
  typescript: in-memory Map / lru-cache
  python: functools.lru_cache / cachetools
catalog:
  presets: [cache-memory-default, cache-distributed-redis, cache-hybrid]
```

```yaml
capability: logging
family: architecture
handles: [logger, ilogger, log, logging, structured-logging]
generator:
  csharp: Microsoft.Extensions.Logging.ILogger
  typescript: console / winston / pino
  python: logging / structlog
catalog:
  presets: [logging-console, logging-file, logging-structured]
```

```yaml
capability: dependency-injection
family: architecture
handles: [di, ioc, service-provider, iserviceprovider, service-collection, dependency-injection]
generator:
  csharp: Microsoft.Extensions.DependencyInjection
  typescript: tsyringe / inversify / nestjs DI
  python: dependency-injector / fastapi Depends
  java: Spring DI / Jakarta Inject
catalog:
  presets: [di-default, di-scoped, di-singleton, di-decorator]
```

### Přehled kategorií

| Facet | Typické ForgeBlocky |
|-------|-------------------|
| **StandardLibraryWrapper** (Standard Library/Wrappers) | Text, Collections, Json, Xml, Http, FileSystem, Crypto, Compression, Encoding, DateTime, Concurrency, Numerics, Reflection, Environment |
| **ArchitecturalPattern** (Architecture/…) | Logging, Caching, DI, Configuration, Serialization, Validation, Result, Specification, Repository |
| **FrameworkIntegration** (Framework/Wrappers) | Orm (EF Core), Vogen, Api (ASP.NET), Auth, Hosting, Telemetry |

### Vazba na existující ForgeBlocky

| Existující | Příbuzný built-in kandidát | Konflikt? |
|-----------|---------------------------|-----------|
| `Math` (standard-math) | Numerics (System.Numerics) | Rozšíření — komplexní čísla, vektory, matice |
| `Random` (random-values) | Crypto (System.Security.Cryptography.RandomNumberGenerator) | Doplněk — kryptograficky bezpečný random |
| `Mapper` (model-mapping) | — | Externí knihovna, žádný built-in equivalent |

| Priorita | ForgeBlock | Strategický důvod |
|----------|-----------|-------------------|
| **1** | **Vogen** | P┼Ö├şm├Ż vztah k `CustomTypeDefinition` — MetaForge generuje `[ValueObject]` k├│d z `ValueObjectElement`, Vogen source-generuje konvertory (EF Core, Dapper, JSON, BSON, ...) p┼Öi kompilaci. Konverze se vol├ş a┼ż p┼Öi v├Żb─Ťru infrastruktury. |
| **2** | **Ardalis.SmartEnum** | Typovan├ę enumy jsou v┼íudyp┼Ö├ştomn├ę, lehk├í integrace |
| **3** | **FluentValidation** | Validace je univerz├íln├ş pr┼»┼Öezov├í pot┼Öeba, p┼Ö├şmo navazuje na atributy/constraints |
| **4** | **Entity Framework Core** | ORM je nej─Źast─Ťj┼í├ş po┼żadavek, demonstruje hodnotu platformy |
| **5** | **MediatR** | CQRS pattern p┼Öirozen─Ť odpov├şd├í business behaviors (command/query) |
| **6** | **Mapperly** | Source-generator → MetaForge generuje mapper jako codegen, ne runtime |
| **7** | **Bogus** | Test data generovan├í z business modelu — vysok├í viditelnost hodnoty |
| **8** | **Refit** | REST client generovan├Ż z API entit — siln├Ż codegen use-case |
| **9** | **Serilog** | Logov├ín├ş je pr┼»┼Öezov├ę, jednoduch├í integrace |
| **10** | **Hangfire** | Background jobs z business behaviors |
| **11** | **MassTransit** | Messaging pro event-driven architekturu |
| **12** | **QuestPDF** | Report generov├ín├ş — ─Źast├Ż po┼żadavek |
| **13** | **FusionCache** | Caching vrstva |
| **14** | **OpenTelemetry** | Observabilita — rostouc├ş standard |
| **15** | **Stateless** | State machine pro business workflow |

---

## Prioritní pořadí implementace

> ### Priorita — Vestavěné System.* knihovny (okamžitá hodnota, nulová závislost)
>
> | Priorita | ForgeBlock | Strategický důvod |
> |----------|-----------|-------------------|
> | **1** | **Text** | Nejpoužívanější System.* API, 15+ sémantických toolů (regex, format, split, join, concat, trim, pad) |
> | **2** | **Collections** | Univerzální dotazovací jazyk nad kolekcemi (LINO — Language-Integrated Natural Operations) |
> | **3** | **Json** | Klíčový pro serializační profil platformy, přirozený vstup/výstup authoring dokumentů |
> | **4** | **Http** | Pro integrační scénáře, API klienty a webhooky v codegenu |
> | **5** | **FileSystem** | Základ pro persistence a Infrastructure vrstvu |

> ### Priorita — Externí NuGet knihovny
>
> | Priorita | ForgeBlock | Strategický důvod |
> |----------|-----------|-------------------|
> | **1** | **Vogen** | Přímý vztah k `CustomTypeDefinition` → ValueObject generování je ukázkový codegen scénář |
> | **2** | **Ardalis.SmartEnum** | Typované enumy jsou všudypřítomné, lehká integrace |
> | **3** | **FluentValidation** | Validace je univerzální průřezová potřeba, přímo navazuje na atributy/constraints |
> | **4** | **Entity Framework Core** | ORM je nejčastější požadavek, demonstruje hodnotu platformy |
> | **5** | **MediatR** | CQRS pattern přirozeně odpovídá business behaviors (command/query) |
> | **6** | **Mapperly** | Source-generator → MetaForge generuje mapper jako codegen, ne runtime |
> | **7** | **Bogus** | Test data generovaná z business modelu — vysoká viditelnost hodnoty |
> | **8** | **Refit** | REST client generovaný z API entit — silný codegen use-case |
> | **9** | **Serilog** | Logování je průřezové, jednoduchá integrace |
> | **10** | **Hangfire** | Background jobs z business behaviors |
> | **11** | **MassTransit** | Messaging pro event-driven architekturu |
> | **12** | **QuestPDF** | Report generování — častý požadavek |
> | **13** | **FusionCache** | Caching vrstva |
> | **14** | **OpenTelemetry** | Observabilita — rostoucí standard |
> | **15** | **Stateless** | State machine pro business workflow |

---

## Architektonický pattern ForgeBlocku pro externí knihovnu

Každý ForgeBlock pro externí knihovnu se řídí tímto schématem:

```csharp
// 1. Core registrace — capability metadata + katalog
public sealed class FluentValidationForgeBlockPackage : IForgeBlockPackage
{
    public ForgeBlockPackageDescriptor Descriptor { get; } = new()
    {
        Id = "forgeblock-fluentvalidation",
        DisplayName = "FluentValidation",
        Description = "Generates FluentValidation validators from business constraints",
        Tags = ["validation", "fluent", "rules"],
        Capabilities = [CapabilityIds.ValidationFluent]
    };

    public void Register(IForgeBlockRegistry registry)
    {
        registry.AddCapabilityProvider(new FluentValidationCapabilityProvider());
        registry.AddCatalogContributor(new FluentValidationCatalogContributor());
        registry.AddDiscoveryContributor(new FluentValidationDiscoveryContributor());
    }
}

// 2. Generator contributor — šablony pro cílový jazyk
public sealed class FluentValidationGeneratorContributor : IForgeBlockGeneratorContributor
{
    public void Contribute(GeneratorContext context)
    {
        // Přidá Scriban šablony pro generování validatorů
        context.AddTemplate("csharp", "FluentValidator.scriban");
    }
}
```

```yaml
# 3. Capability metadata — neutrální popis toolu
capability: validation-fluent
version: 1.0
family: validation
displayName: FluentValidation
description: >
  Generates FluentValidation AbstractValidator classes from
  business attribute constraints.
handles:
  - fluent-validation
  - validator
  - validation-rules
  - fluent-validator
parameters:
  - name: targetEntity
    type: string
    description: Entity ID or name to generate validator for
    required: false
  - name: includeRules
    type: string[]
    description: Specific rule sets to include
    required: false
returnType:
  type: string
  description: Generated validator source code
```

---

## Vazba na New_Architecture dokumenty

| Dokument | Vazba |
|----------|-------|
| `03-Core-Abstractions.md` | Definuje `IForgeBlockPackage`, `IForgeBlockRegistry` — kontrakty pro registraci |
| `06-Core-Services.md` | Popisuje CatalogManager, discovery, strong type resolution — cílové registry ForgeBlocků |
| `08-Translator.md` | Translator pravidla může ForgeBlock rozšiřovat |
| `10-Generators.md` | Generator Contributor rozhraní + Scriban template pipeline |
| `12-Host-Surfaces.md` | ForgeBlock se registruje do MCP/CLI/WebApi přes optional interfacy |
| `13-Epics-and-Slices.md` | Epic 6 — ForgeBlock tooling a package ecosystem |
| `17-Skills-and-Agents.md` | Skill `metaforge-forgeblock-package-architecture` |
| `05-ForgeBlock-Package-Model.md` | (`Architecture-Define/`) — Detailní ForgeBlock package model |

---

## Související zdroje

- [NuGet Statistics](https://www.nuget.org/stats) — adopce knihoven
- [Built-In ForgeBlocks](c:\Users\Utalag\Documents\source-git\MetaForge\Src\MetaForge.ForgeBlocks\) — existující Math, Random, Mapper
- `Architecture-Define/05-ForgeBlock-Package-Model.md` — živý dokument ForgeBlock architektury
