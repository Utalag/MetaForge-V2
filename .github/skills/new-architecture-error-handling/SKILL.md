---
name: new-architecture-error-handling
description: "Pouzij pri: navrhu nebo implementaci error handling strategie — exception typy, vrstveni, logging, middleware, graceful degradation."
---

# new-architecture-error-handling

Zajistit konzistentní error handling napříč všemi vrstvami dle `19-Error-Handling.md`.

## Kdy použít

- Při implementaci exception handlingu v jakékoli vrstvě
- Při návrhu nových exception typů
- Při implementaci middleware (WebApi), try/catch (CLI), error response (MCP)
- Při rozhodování o logging strategii

## Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **Výjimky se chytají na hranici vrstev** | Nikdy nepropadají napříč vrstvami bez zpracování |
| 2 | **BusinessModel a Core nikdy nelogují přímo** | Logování je odpovědnost host vrstvy |
| 3 | **Facade zapouzdřuje chyby do strukturovaných výsledků** | Žádné raw exception propagation |
| 4 | **AI selhání = graceful fallback, ne výjimka** | AI vrací null, host surface rozhodne o chybové hlášce |
| 5 | **Strukturované logování přes Microsoft.Extensions.Logging** | Žádný Serilog/NLog jako povinná závislost |

## Exception typy

| Typ | Vrstva | Kdy |
|-----|--------|-----|
| `InvalidOperationException` | BusinessModel | Mutace na neexistující entitě/atributu |
| `ArgumentException` | Core/BusinessModel | Nevalidní vstup (prázdný název, null) |
| `InvalidModelException` | Translator | Export/translate selhal kvůli nevalidnímu modelu |
| `ForgeBlockRegistrationException` | Core | Duplicitní nebo nevalidní ForgeBlock registrace |

## Error handling per vrstva

| Vrstva | Chyby zachycuje | Výstup |
|--------|----------------|--------|
| **Core** | Nikdy — Core je stabilní | Assertion/exception = programátorská chyba |
| **BusinessModel** | Validace před mutací | `ValidationResult` s kolekcí chyb |
| **Translator (Facade)** | Chyby z Translate/Enrichment | Strukturovaná odpověď pro host |
| **CLI** | `try/catch` v Program.cs | Exit code + chybová hláška |
| **MCP** | `try/catch` v Tool handlerech | JSON-RPC error response |
| **WebApi** | `ExceptionHandlingMiddleware` | `ErrorResponse` JSON |

## Vzor — CLI error handling

```csharp
try
{
    facade.AddEntity(args[0]);
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

## Vzor — WebApi middleware

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (ArgumentException ex) { /* 400 Bad Request */ }
        catch (InvalidOperationException ex) { /* 409 Conflict */ }
        catch (Exception ex) { /* 500 Internal Server Error */ }
    }
}

public record ErrorResponse(int StatusCode, string Message, string? Details = null);
```

## Logování — konvence

| Úroveň | Kdy použít |
|--------|-----------|
| `Information` | Command aplikován, entita přidána, export dokončen |
| `Warning` | Nevalidní vstup, AI fallback, chybějící preset |
| `Error` | Neošetřená výjimka, selhání persistence |

## Anti-patterny

- ❌ BusinessModel nebo Core volající `ILogger` přímo
- ❌ AI výjimka propagovaná do host vrstvy (AI vrací null)
- ❌ Raw exception detaily v HTTP response (production)
- ❌ Různé formáty chybových odpovědí napříč host surfaces

## Výstupní checklist

- [ ] Výjimky se chytají na hranici vrstev
- [ ] BusinessModel a Core nelogují přímo
- [ ] AI selhání = gracefully degradováno (ne výjimka)
- [ ] Každý host surface má konzistentní error response
- [ ] Logování je strukturované (Microsoft.Extensions.Logging)
