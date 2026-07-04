using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MetaForge.Translator.Telemetry;

/// <summary>
/// Centralni platformni telemetry meter pro MetaForge.
/// Drzi jeden sdileny <see cref="Meter" />, <see cref="ActivitySource" /> pro tracing a prvni sadu counteru/histogramu.
/// </summary>
public static class MetaForgeTelemetry
{
    public const string MeterName = "MetaForge.Platform";
    public const string MeterVersion = "0.1.0";

    public static readonly Meter Meter = new(MeterName, MeterVersion);

    /// <summary>
    /// ActivitySource pro OpenTelemetry tracing. Pouziva se pro trasovani prubehu requestu
    /// pres facade a host boundary.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(MeterName, MeterVersion);

    // Authoring
    public static readonly Counter<long> AuthoringTurns = Meter.CreateCounter<long>(
        "metaforge.authoring.turns",
        description: "Pocet authoring turnu.");

    public static readonly Histogram<double> AuthoringTurnDurationMs = Meter.CreateHistogram<double>(
        "metaforge.authoring.turn.duration.ms",
        unit: "ms",
        description: "Delka zpracovani authoring turnu.");

    public static readonly Counter<long> AuthoringOperations = Meter.CreateCounter<long>(
        "metaforge.authoring.operations",
        description: "Pocet explicitnich operaci.");

    public static readonly Histogram<double> AuthoringOperationDurationMs = Meter.CreateHistogram<double>(
        "metaforge.authoring.operation.duration.ms",
        unit: "ms",
        description: "Delka explicitni operace.");

    // Projection
    public static readonly Counter<long> ProjectionRequests = Meter.CreateCounter<long>(
        "metaforge.projection.requests",
        description: "Pocet projekcnich dotazu.");

    public static readonly Histogram<double> ProjectionDurationMs = Meter.CreateHistogram<double>(
        "metaforge.projection.duration.ms",
        unit: "ms",
        description: "Delka projection read path.");

    // Discovery
    public static readonly Counter<long> DiscoveryRequests = Meter.CreateCounter<long>(
        "metaforge.discovery.requests",
        description: "Pocet discovery dotazu.");

    public static readonly Histogram<double> DiscoveryDurationMs = Meter.CreateHistogram<double>(
        "metaforge.discovery.duration.ms",
        unit: "ms",
        description: "Delka discovery dotazu.");

    // Export
    public static readonly Counter<long> ExportRequests = Meter.CreateCounter<long>(
        "metaforge.export.requests",
        description: "Pocet exportnich operaci.");

    public static readonly Histogram<double> ExportDurationMs = Meter.CreateHistogram<double>(
        "metaforge.export.duration.ms",
        unit: "ms",
        description: "Delka exportu.");

    // Node Assist
    public static readonly Counter<long> NodeAssistPresetsSuggested = Meter.CreateCounter<long>(
        "metaforge.nodeassist.presets.suggested",
        description: "Pocet preset suggestion dotazu.");

    public static readonly Counter<long> NodeAssistRequests = Meter.CreateCounter<long>(
        "metaforge.nodeassist.requests",
        description: "Pocet node assist requestu.");

    public static readonly Histogram<double> NodeAssistDurationMs = Meter.CreateHistogram<double>(
        "metaforge.nodeassist.duration.ms",
        unit: "ms",
        description: "Delka node assist requestu vcetne AI volani.");

    public static readonly Counter<long> NodeAssistAiCalls = Meter.CreateCounter<long>(
        "metaforge.nodeassist.ai.calls",
        description: "Pocet AI volani v ramci node assist.");

    public static readonly Counter<long> NodeAssistApplyRequests = Meter.CreateCounter<long>(
        "metaforge.nodeassist.apply.requests",
        description: "Pocet explicitnich apply pozadavku pro node assist operace.");

    public static readonly Counter<long> NodeAssistApplyOperations = Meter.CreateCounter<long>(
        "metaforge.nodeassist.apply.operations",
        description: "Pocet operaci aplikovanych pres node assist apply.");

    public static readonly Counter<long> NodeAssistApplyRejected = Meter.CreateCounter<long>(
        "metaforge.nodeassist.apply.rejected",
        description: "Pocet zamitnutych node assist apply pozadavku (validace selhala).");

    // AI Health
    public static readonly Counter<long> AiHealthProbes = Meter.CreateCounter<long>(
        "metaforge.ai.health.probes",
        description: "Pocet AI health probe behu.");

    public static readonly Histogram<double> AiHealthDurationMs = Meter.CreateHistogram<double>(
        "metaforge.ai.health.duration.ms",
        unit: "ms",
        description: "Delka AI health probe.");
}
