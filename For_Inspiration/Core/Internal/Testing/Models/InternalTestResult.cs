namespace MetaForge.Core.Internal.Testing.Models;

internal sealed class InternalTestResult
{
    public List<DetectedBoundary> Boundaries { get; init; } = new();
    public int PassedCount { get; init; }
    public int FailedCount { get; init; }
}
