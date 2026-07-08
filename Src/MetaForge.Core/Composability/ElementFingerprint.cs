// ---------------------------------------------------------------------------
// MetaForge.Core — ElementFingerprint
// Incremental dirty-tracking via structural hashing for build optimization.
// Vrstva: Core / Composability
// 
// PROPOSAL: PROP-039 — Core Composability
// ---------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;

namespace MetaForge.Core.Composability;

/// <summary>
/// Lightweight fingerprint of an element's content, used for dirty-tracking.
/// When two fingerprints are equal, the element hasn't changed and can be skipped.
/// </summary>
public sealed class ElementFingerprint : IEquatable<ElementFingerprint>
{
    /// <summary>SHA256 hash of the element's structural content.</summary>
    public string StructuralHash { get; }

    /// <summary>Version of the pipeline that last processed this element.</summary>
    public int PipelineVersion { get; }

    /// <summary>UTC timestamp when this fingerprint was computed.</summary>
    public DateTimeOffset ComputedAt { get; }

    /// <summary>
    /// Creates a fingerprint with the given hash and pipeline version.
    /// </summary>
    public ElementFingerprint(string structuralHash, int pipelineVersion)
    {
        StructuralHash = structuralHash ?? throw new ArgumentNullException(nameof(structuralHash));
        PipelineVersion = pipelineVersion;
        ComputedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Computes a SHA256 fingerprint from one or more content strings.
    /// Thread-safe — uses a new SHA256 instance each time.
    /// </summary>
    public static ElementFingerprint Compute(IEnumerable<string> contentParts, int pipelineVersion)
    {
        var parts = string.Join("|", contentParts);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(parts));
        var hashString = Convert.ToHexStringLower(hashBytes);
        return new ElementFingerprint(hashString, pipelineVersion);
    }

    /// <summary>
    /// Computes a fingerprint from a single content string.
    /// </summary>
    public static ElementFingerprint Compute(string content, int pipelineVersion)
    {
        return Compute(new[] { content }, pipelineVersion);
    }

    /// <summary>Creates an empty (zero) fingerprint — used for unprocessed elements.</summary>
    public static ElementFingerprint Empty { get; } = new("0000000000000000000000000000000000000000000000000000000000000000", 0);

    /// <inheritdoc />
    public bool Equals(ElementFingerprint? other)
    {
        if (other is null) return false;
        return StructuralHash == other.StructuralHash && PipelineVersion == other.PipelineVersion;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ElementFingerprint other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(StructuralHash, PipelineVersion);

    /// <inheritdoc />
    public override string ToString() => $"{StructuralHash[..12]}...@v{PipelineVersion}";

    public static bool operator ==(ElementFingerprint? left, ElementFingerprint? right) =>
        Equals(left, right);

    public static bool operator !=(ElementFingerprint? left, ElementFingerprint? right) =>
        !Equals(left, right);
}

/// <summary>
/// Extension methods for computing fingerprints on Core elements.
/// </summary>
public static class FingerprintExtensions
{
    /// <summary>
    /// Computes a fingerprint for a ClassElement based on its structural properties.
    /// </summary>
    public static ElementFingerprint ComputeFingerprint(this Elements.Types.ClassElement cls, int pipelineVersion)
    {
        var parts = new List<string>
        {
            $"class:{cls.Name}",
            $"ns:{cls.Namespace ?? ""}",
            $"access:{cls.AccessModifier}",
            $"abstract:{cls.IsAbstract}",
            $"sealed:{cls.IsSealed}",
            $"static:{cls.IsStatic}",
            $"record:{cls.IsRecord}",
            $"partial:{cls.IsPartial}",
            $"base:{cls.BaseClassName ?? ""}",
            $"interfaces:{string.Join(",", cls.ImplementedInterfaces.OrderBy(x => x))}",
            $"typeParams:{string.Join(",", cls.TypeParameters)}",
            $"props:{string.Join(",", cls.Properties.Select(p => $"{p.Name}:{p.Type}"))}",
            $"methods:{string.Join(",", cls.Methods.Select(m => $"{m.Name}:{m.ReturnType}"))}"
        };

        return ElementFingerprint.Compute(parts, pipelineVersion);
    }

    /// <summary>
    /// Computes a fingerprint for a MethodElement.
    /// </summary>
    public static ElementFingerprint ComputeFingerprint(this Elements.Members.MethodElement method, int pipelineVersion)
    {
        var parts = new List<string>
        {
            $"method:{method.Name}",
            $"return:{method.ReturnType}",
            $"access:{method.AccessModifier}",
            $"static:{method.IsStatic}",
            $"async:{method.IsAsync}",
            $"abstract:{method.IsAbstract}",
            $"virtual:{method.IsVirtual}",
            $"override:{method.IsOverride}",
            $"extension:{method.IsExtension}",
            $"typeParams:{string.Join(",", method.TypeParameters)}",
            $"params:{string.Join(",", method.Parameters.Select(p => $"{p.Name}:{p.Type}"))}"
        };

        return ElementFingerprint.Compute(parts, pipelineVersion);
    }
}
