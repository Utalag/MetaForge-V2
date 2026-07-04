using MetaForge.Ai.Configuration;
using MetaForge.Ai.Runtime;
using MetaForge.Core.Configuration;

namespace MetaForge.Translator;

internal static class AuthoringAiHealthProbe
{
    private const int ProbeTimeoutCapMs = 5000;

    private static readonly (string Name, AiSegment Segment, string ApiKeyEnvironmentVariable)[] SegmentDefinitions =
    [
        ("AI-Conversation", AiSegment.Conversation, "METAFORGE_CONVERSATION_API_KEY"),
        ("AI-AuthoringTranslation", AiSegment.AuthoringTranslation, "METAFORGE_AUTHORING_TRANSLATION_API_KEY"),
    ];

    public static IReadOnlyList<AuthoringAiApiKeyPromptRequest> GetMissingApiKeyPromptRequests(
        AiPlatformConfiguration? configuration = null)
    {
        var effectiveConfiguration = configuration ?? AiPlatformConfiguration.Load();

        return SegmentDefinitions
            .Select(segmentDefinition => GetMissingApiKeyPromptRequest(segmentDefinition, effectiveConfiguration))
            .Where(request => request is not null)
            .Cast<AuthoringAiApiKeyPromptRequest>()
            .ToArray();
    }

    public static async Task<IReadOnlyList<AuthoringAiHealthStatus>> ProbeAsync(
        AiPlatformConfiguration? configuration = null,
        IAiRuntimeAdapterFactory? runtimeAdapterFactory = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveConfiguration = configuration ?? AiPlatformConfiguration.Load();
        var effectiveFactory = runtimeAdapterFactory ?? new AiRuntimeAdapterFactory();

        var tasks = SegmentDefinitions
            .Select(segmentDefinition => ProbeSegmentAsync(segmentDefinition, effectiveConfiguration, effectiveFactory, cancellationToken));

        return await Task.WhenAll(tasks);
    }

    private static AuthoringAiApiKeyPromptRequest? GetMissingApiKeyPromptRequest(
        (string Name, AiSegment Segment, string ApiKeyEnvironmentVariable) segmentDefinition,
        AiPlatformConfiguration configuration)
    {
        var settings = configuration.GetSettingsForSegment(segmentDefinition.Segment);
        if (!settings.Enabled
            || !ProviderRequiresApiKey(settings.Provider)
            || !string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return null;
        }

        return new AuthoringAiApiKeyPromptRequest(
            segmentDefinition.Name,
            segmentDefinition.ApiKeyEnvironmentVariable);
    }

    private static async Task<AuthoringAiHealthStatus> ProbeSegmentAsync(
        (string Name, AiSegment Segment, string ApiKeyEnvironmentVariable) segmentDefinition,
        AiPlatformConfiguration configuration,
        IAiRuntimeAdapterFactory runtimeAdapterFactory,
        CancellationToken cancellationToken)
    {
        var settings = configuration.GetSettingsForSegment(segmentDefinition.Segment);
        var status = new AuthoringAiHealthStatus(
            segmentDefinition.Name,
            IsOk: false,
            IsEnabled: settings.Enabled,
            RequiresApiKey: ProviderRequiresApiKey(settings.Provider),
            HasApiKeyConfigured: !string.IsNullOrWhiteSpace(settings.ApiKey),
            ApiKeyEnvironmentVariable: segmentDefinition.ApiKeyEnvironmentVariable);

        try
        {
            if (!settings.Enabled)
                return status;

            using var probeCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            probeCancellation.CancelAfter(GetProbeTimeout(settings.TimeoutMs));

            var adapter = runtimeAdapterFactory.Create(settings);
            var isOk = await adapter.TestConnectionAsync(probeCancellation.Token);
            return status with { IsOk = isOk };
        }
        catch
        {
            return status;
        }
    }

    private static bool ProviderRequiresApiKey(AIProvider provider)
    {
        return provider is AIProvider.OpenAI or AIProvider.Azure or AIProvider.MiniMax;
    }

    private static TimeSpan GetProbeTimeout(int configuredTimeoutMs)
    {
        var timeoutMs = configuredTimeoutMs <= 0
            ? ProbeTimeoutCapMs
            : Math.Min(configuredTimeoutMs, ProbeTimeoutCapMs);

        return TimeSpan.FromMilliseconds(timeoutMs);
    }
}

internal sealed record AuthoringAiApiKeyPromptRequest(
    string Name,
    string ApiKeyEnvironmentVariable);

internal sealed record AuthoringAiHealthStatus(
    string Name,
    bool IsOk,
    bool IsEnabled,
    bool RequiresApiKey,
    bool HasApiKeyConfigured,
    string ApiKeyEnvironmentVariable);