namespace MetaForge.FrameworkMetadata.Hosting;

/// <summary>
/// Metadata pro dlouho běžící službu registrovanou přes .NET Generic Host
/// (`IHostedService` nebo `BackgroundService`).
/// </summary>
public sealed record HostedServiceMetadata
{
    /// <summary>Název typu implementujícího službu.</summary>
    public string TypeName { get; init; } = string.Empty;

    /// <summary>Druh hostované služby.</summary>
    public HostedServiceKind Kind { get; init; } = HostedServiceKind.BackgroundService;

    /// <summary>
    /// Respektuje service konvenci předávání <c>CancellationToken</c> do dlouho běžících
    /// operací (`ExecuteAsync(CancellationToken stoppingToken)`)?
    /// </summary>
    public bool UsesCancellationToken { get; init; } = true;
}

/// <summary>Druh hostované služby v .NET Generic Host.</summary>
public enum HostedServiceKind
{
    /// <summary>Implementuje `IHostedService` přímo (StartAsync/StopAsync).</summary>
    HostedService,

    /// <summary>Dědí z `BackgroundService` (ExecuteAsync).</summary>
    BackgroundService,
}

/// <summary>
/// Konvence pro použití <c>CancellationToken</c> v asynchronních metodách —
/// zaznamenává, zda metoda respektuje standardní .NET konvenci
/// (poslední parametr, výchozí hodnota `default`, propagace do volaných async metod).
/// </summary>
public sealed record CancellationTokenConvention
{
    /// <summary>Název metody, ke které se konvence vztahuje.</summary>
    public string MethodName { get; init; } = string.Empty;

    /// <summary>Název parametru CancellationToken (konvenčně "cancellationToken" nebo "stoppingToken").</summary>
    public string ParameterName { get; init; } = "cancellationToken";

    /// <summary>Je CancellationToken posledním parametrem metody (doporučená konvence)?</summary>
    public bool IsLastParameter { get; init; } = true;

    /// <summary>Má parametr výchozí hodnotu `default`?</summary>
    public bool HasDefaultValue { get; init; } = true;
}
