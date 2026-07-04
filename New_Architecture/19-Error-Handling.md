# Error Handling a Logging

> Strategie pro zpracování chyb a strukturované logování napříč všemi vrstvami.

---

## Principy

1. **Výjimky se chytají na hranici vrstev** — nikdy nepropadají napříč vrstvami bez zpracování.
2. **BusinessModel a Core nikdy nelogují přímo** — logování je odpovědnost host vrstvy.
3. **Facade zapouzdřuje chyby do strukturovaných výsledků** — žádné raw exception propagation.
4. **AI selhání = graceful fallback, ne výjimka** — AI vrací null, host surface rozhodne o chybové hlášce.
5. **Strukturované logování přes `Microsoft.Extensions.Logging`** — žádný Serilog/NLog jako povinná závislost.

---

## Vrstvy a odpovědnost za chyby

| Vrstva | Chyby zachycuje | Výstup |
|--------|----------------|--------|
| Core | Nikdy — Core je stabilní, žádné runtime chyby by neměly nastat | Assertion/exception = programátorská chyba |
| BusinessModel | Validace před mutací | `ValidationResult` s kolekcí chyb |
| Translator (Facade) | Chyby z Translate/Enrichment | Strukturovaná odpověď pro host |
| CLI | `try/catch` v Program.cs | Exit code + chybová hláška |
| MCP | `try/catch` v Tool handlerech | JSON-RPC error response |
| WebApi | `ExceptionHandlingMiddleware` | `ErrorResponse` JSON |

---

## Exception typy

| Typ | Vrstva | Kdy |
|-----|--------|-----|
| `InvalidOperationException` | BusinessModel | Mutace na neexistující entitě/atributu |
| `ArgumentException` | Core/BusinessModel | Nevalidní vstup (prázdný název, null) |
| `InvalidModelException` | Translator | Export/translate selhal kvůli nevalidnímu modelu |
| `ForgeBlockRegistrationException` | Core | Duplicitní nebo nevalidní ForgeBlock registrace |

---

## WebApi — ExceptionHandlingMiddleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Nevalidní vstup: {Message}", ex.Message);
            context.Response.StatusCode = 400;
            await WriteErrorResponse(context, 400, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Neplatná operace: {Message}", ex.Message);
            context.Response.StatusCode = 409;
            await WriteErrorResponse(context, 409, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neošetřená výjimka: {Message}", ex.Message);
            context.Response.StatusCode = 500;
            await WriteErrorResponse(context, 500, "Došlo k neošetřené chybě.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        var response = new ErrorResponse(statusCode, message);
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}

public record ErrorResponse(int StatusCode, string Message, string? Details = null);
```

---

## CLI — Error handling

```csharp
try
{
    facade.AddEntity(args[0]);
    Console.WriteLine($"Entita '{args[0]}' přidána.");
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Chyba: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Neošetřená chyba: {ex.Message}");
    return 2;
}
```

---

## Logování — konvence

| Úroveň | Kdy použít |
|--------|-----------|
| `Trace` | Volání metod s parametry (ladění) |
| `Debug` | DI registrace, inicializace, lifecycle |
| `Information` | Command aplikován, entita přidána, export dokončen |
| `Warning` | Nevalidní vstup, AI fallback, chybějící preset |
| `Error` | Neošetřená výjimka, selhání persistence, selhání exportu |
| `Critical` | Nemožnost startu, corrupt command log |

### Příklad — Facade logging

```csharp
public class BusinessAuthoringHostFacade
{
    private readonly ILogger<BusinessAuthoringHostFacade> _logger;

    public void AddEntity(string name)
    {
        _logger.LogInformation("Přidávám entitu: {Name}", name);
        // ... operace ...
        _logger.LogDebug("Entita {Name} přidána s Id: {Id}", name, newEntity.Id);
    }
}
```

### Příklad — Command logging

```csharp
public class CommandLogStore
{
    private readonly ILogger<CommandLogStore> _logger;

    public void Append(CommandEnvelope envelope)
    {
        _logger.LogDebug("Append command: {Type} at {Timestamp}", envelope.CommandType, envelope.Timestamp);
        // ... append ...
        _logger.LogInformation("Command {Id} appended. Total: {Count}", envelope.Id, Count);
    }
}
```

---

## AI fallback — logování

```csharp
public class AiTranslationService
{
    private readonly ILogger<AiTranslationService> _logger;

    public async Task<EnrichmentResult?> TranslateAsync(BusinessAttributeNode attribute, ContextProjection context)
    {
        try
        {
            var result = await _provider.SendAsync(request);
            if (result is null)
            {
                _logger.LogWarning("AI vrátil null pro atribut {Name}, používám fallback", attribute.Name);
                return null;
            }
            _logger.LogInformation("AI enrichment pro {Name} úspěšný", attribute.Name);
            return MapResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI selhal pro atribut {Name}, používám fallback", attribute.Name);
            return null;
        }
    }
}
```
