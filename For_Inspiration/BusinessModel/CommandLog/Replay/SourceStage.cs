using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MetaForge.BusinessModel;

/// <summary>
/// Nacte vsechny commandy z command logu a vybere ty relevantni pro pozadovany stream.
/// Interni pipeline krok pro <see cref="ReplayProjectionQueryService"/>.
/// </summary>
internal static class SourceStage
{
    /// <summary>
    /// Nacte commandy z readeru, urci effectiveStreamId a vrati filtrovany seznam pro replay.
    /// Vyhodi <see cref="InvalidOperationException"/> pokud log obsahuje vice streamu a streamId neni zadano.
    /// </summary>
    public static (IReadOnlyList<CommandEnvelope> Commands, string? EffectiveStreamId) Load(
        JsonlShadowCommandReader commandReader,
        string? requestedStreamId)
    {
        var allCommands = commandReader.ReadAll();
        var effectiveStreamId = ResolveStreamId(allCommands, requestedStreamId);
        var commands = FilterCommands(allCommands, effectiveStreamId);
        return (commands, effectiveStreamId);
    }

    private static string? ResolveStreamId(IReadOnlyList<CommandEnvelope> allCommands, string? requestedStreamId)
    {
        if (!string.IsNullOrWhiteSpace(requestedStreamId))
            return requestedStreamId.Trim();

        var streamIds = allCommands
            .Select(command => command.StreamId)
            .Where(streamId => !string.IsNullOrWhiteSpace(streamId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return streamIds.Length switch
        {
            0 => null,
            1 => streamIds[0],
            _ => throw new InvalidOperationException("Shadow log obsahuje vice streamu; pro nacteni business projekce je potreba zadat streamId."),
        };
    }

    private static IReadOnlyList<CommandEnvelope> FilterCommands(IReadOnlyList<CommandEnvelope> allCommands, string? streamId)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            return allCommands;

        return allCommands
            .Where(command => string.Equals(command.StreamId, streamId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    /// <summary>
    /// Vraci true pokud soubor command logu prekrocil streaming threshold.
    /// </summary>
    public static bool ShouldUseStreaming(string filePath, StreamingThresholdOptions? threshold = null)
        => (threshold ?? StreamingThresholdOptions.Default).ShouldUseStreaming(filePath);

    /// <summary>
    /// Rychle urceni effectiveStreamId bez plne deserializace — cte pouze pole "streamId" z JSONL.
    /// Pouziva se jako predkrok streamingoveho rezimu kdyz requestedStreamId neni znamo.
    /// </summary>
    public static async ValueTask<string?> ResolveStreamIdAsync(
        string filePath,
        string? requestedStreamId,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(requestedStreamId))
            return requestedStreamId.Trim();

        if (!File.Exists(filePath))
            return null;

        var streamIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await foreach (var line in File.ReadLinesAsync(filePath, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var doc = JsonDocument.Parse(line);
            if (doc.RootElement.TryGetProperty("streamId", out var streamIdElem))
            {
                var sid = streamIdElem.GetString();
                if (!string.IsNullOrWhiteSpace(sid))
                    streamIds.Add(sid);
            }
        }

        return streamIds.Count switch
        {
            0 => null,
            1 => streamIds.First(),
            _ => throw new InvalidOperationException("Shadow log obsahuje vice streamu; pro nacteni business projekce je potreba zadat streamId."),
        };
    }

    /// <summary>
    /// Streamuje commandy ze souboru jeden po druhem bez nacitani vsech do pameti.
    /// Filtruje podle effectiveStreamId (null = vsechny). Vyzaduje predreselovane effectiveStreamId.
    /// </summary>
    public static async IAsyncEnumerable<CommandEnvelope> StreamAsync(
        JsonlShadowCommandReader commandReader,
        string? effectiveStreamId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(commandReader.FilePath))
            yield break;

        await foreach (var line in File.ReadLinesAsync(commandReader.FilePath, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var envelope = JsonSerializer.Deserialize<CommandEnvelope>(line);
            if (envelope is null)
                continue;

            if (string.IsNullOrWhiteSpace(effectiveStreamId)
                || string.Equals(envelope.StreamId, effectiveStreamId, StringComparison.OrdinalIgnoreCase))
            {
                yield return envelope;
            }
        }
    }
}
