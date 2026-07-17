// ---------------------------------------------------------------------------
// MetaForge.Infrastructure — ISandboxExecutionService
// Sandbox preview runner for method execution.
// Vrstva: Infrastructure / Sandbox
//
// PROPOSAL: PROP-058 — Sandbox Preview Runner
// ---------------------------------------------------------------------------

using MetaForge.Core.Contracts;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Infrastructure.Sandbox;

public interface ISandboxExecutionService
{
    Task<SandboxExecutionResult> ExecuteAsync(SandboxExecutionRequest request, CancellationToken ct = default);
}

public enum SandboxMode { Preview, Export }

public sealed record SandboxExecutionRequest
{
    public required MethodElement Method { get; init; }
    public MethodContract? Contract { get; init; }
    public string InputJson { get; init; } = "{}";
    public SandboxMode Mode { get; init; } = SandboxMode.Preview;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
}

public sealed record SandboxExecutionResult
{
    public bool Success { get; init; }
    public string? OutputJson { get; init; }
    public string? ExceptionMessage { get; init; }
    public string? CompilationErrors { get; init; }
    public TimeSpan ExecutionTime { get; init; }
}
