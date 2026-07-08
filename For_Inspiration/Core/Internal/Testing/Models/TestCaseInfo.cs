namespace MetaForge.Core.Internal.Testing.Models;

internal sealed class TestCaseInfo
{
    public string MethodName { get; init; } = "";
    public string Description { get; init; } = "";
    public string CallCode { get; init; } = "";
    public string? ExpectedException { get; init; }
}
