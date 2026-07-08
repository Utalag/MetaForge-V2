using MetaForge.Core.Common;

namespace MetaForge.Core.StandardLibraries;

/// <summary>
/// Agregované importy a balíčky vyžadované použitými standard-library wrappery.
/// </summary>
public sealed record StandardLibraryRequirements
{
    public IReadOnlyCollection<string> Imports { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<CodePackageDependency> Packages { get; init; } = Array.Empty<CodePackageDependency>();
}