namespace MetaForge.FrameworkMetadata.AspNetCore;

/// <summary>
/// Metadata pro ASP.NET Core minimal API nebo controller endpoint.
/// </summary>
public sealed record EndpointMetadata
{
    /// <summary>HTTP metoda (GET, POST, PUT, DELETE, PATCH, ...).</summary>
    public string HttpMethod { get; init; } = "GET";

    /// <summary>Route šablona, např. "/api/orders/{id}".</summary>
    public string Route { get; init; } = string.Empty;

    /// <summary>Název handler metody nebo action metody na controlleru.</summary>
    public string HandlerName { get; init; } = string.Empty;

    /// <summary>Parametry vázané z route/query/body.</summary>
    public List<EndpointParameterBinding> Parameters { get; init; } = new();

    /// <summary>Název typu odpovědi (návratový typ endpointu), např. "OrderDto".</summary>
    public string? ResponseTypeName { get; init; }

    /// <summary>Vyžaduje endpoint autorizaci (`[Authorize]` / `.RequireAuthorization()`)?</summary>
    public bool RequiresAuthorization { get; init; }
}

/// <summary>Vazba jednoho parametru endpointu na zdroj HTTP requestu.</summary>
public sealed record EndpointParameterBinding
{
    /// <summary>Název parametru.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Název typu parametru.</summary>
    public string TypeName { get; init; } = string.Empty;

    /// <summary>Zdroj vazby (route/query/body/header/service).</summary>
    public EndpointBindingSource Source { get; init; } = EndpointBindingSource.Route;
}

/// <summary>Zdroj, ze kterého se hodnota parametru endpointu váže.</summary>
public enum EndpointBindingSource
{
    Route,
    Query,
    Body,
    Header,

    /// <summary>Vstřikovaná služba přes DI (`[FromServices]`).</summary>
    Service,
}
