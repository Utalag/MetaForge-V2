namespace MetaForge.Feedback.Models;

public sealed record ResolutionInfo(
    string ResolutionKind,
    string? Notes = null
);
