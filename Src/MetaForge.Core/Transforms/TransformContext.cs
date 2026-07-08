using MetaForge.Core.Diagnostics;

namespace MetaForge.Core.Transforms;

/// <summary>
/// Kontext pro běh transformační pipeline.
/// </summary>
public sealed class TransformContext
{
    /// <summary>Sdílený diagnostický sběrač napříč všemi transformy.</summary>
    public IDiagnosticCollector Diagnostics { get; }

    /// <summary>Volby pipeline.</summary>
    public PipelineOptions Options { get; }

    /// <summary>Sdílený stav mezi transformy (cross-transform communication).</summary>
    public Dictionary<string, object> State { get; } = [];

    public TransformContext(IDiagnosticCollector diagnostics, PipelineOptions? options = null)
    {
        Diagnostics = diagnostics;
        Options = options ?? new PipelineOptions();
    }
}
