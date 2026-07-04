namespace MetaForge.BusinessModel;

public sealed record BusinessValidationIssue(
    string Code,
    string Message,
    string Severity,
    string? Path = null);