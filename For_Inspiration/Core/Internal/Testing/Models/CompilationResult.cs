using System.Reflection;
using Microsoft.CodeAnalysis;

namespace MetaForge.Core.Internal.Testing.Models;

internal sealed class CompilationResult
{
    public bool Success { get; init; }
    public Assembly? Assembly { get; init; }
    public IEnumerable<Diagnostic>? Diagnostics { get; init; }
}
