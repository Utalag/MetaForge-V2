using System.Diagnostics;

namespace MetaForge.BusinessModel.Telemetry;

/// <summary>
/// ActivitySource pro BusinessModel vrstvu — sledování commandů, replayů a patchi.
/// </summary>
public static class BusinessModelActivitySource
{
    /// <summary>Název activity source pro OpenTelemetry.</summary>
    public const string SourceName = "MetaForge.BusinessModel";

    private static readonly ActivitySource Source = new(SourceName, "1.0.0");

    /// <summary>Vytvoří span pro operaci CommandLog.</summary>
    public static Activity? StartCommandLogActivity(string operation, CommandLog.CommandEnvelope envelope)
    {
        var activity = Source.StartActivity($"CommandLog.{operation}", ActivityKind.Internal);
        activity?.SetTag("command.type", envelope.CommandType);
        activity?.SetTag("command.source", envelope.Source.ToString());
        activity?.SetTag("command.entity_id", envelope.TargetEntityId);
        return activity;
    }

    /// <summary>Vytvoří span pro ReplayEngine operaci.</summary>
    public static Activity? StartReplayActivity(int commandCount)
    {
        var activity = Source.StartActivity("ReplayEngine.Replay", ActivityKind.Internal);
        activity?.SetTag("replay.command_count", commandCount);
        return activity;
    }

    /// <summary>Vytvoří span pro PatchEngine operaci.</summary>
    public static Activity? StartPatchActivity(string operation, string? entityId = null)
    {
        var activity = Source.StartActivity($"PatchEngine.{operation}", ActivityKind.Internal);
        activity?.SetTag("patch.entity_id", entityId);
        return activity;
    }
}
