namespace MetaForge.Core.Abstractions;

/// <summary>
/// Reference na NuGet balíček — `&lt;PackageReference Include="Name" Version="X.Y.Z" /&gt;`.
/// </summary>
public sealed record PackageReferenceInfo(string Name, string Version);

/// <summary>
/// Reference na Roslyn analyzer balíček (source generator nebo diagnostický analyzer)
/// — typicky přidaný jako `&lt;PackageReference&gt;` s `PrivateAssets="all"` nebo `&lt;Analyzer&gt;`.
/// </summary>
public sealed record AnalyzerReferenceInfo(string Name, string Version);

/// <summary>
/// Reference na jiný projekt v rámci stejné solution — `&lt;ProjectReference&gt;`.
/// </summary>
public sealed record ProjectReferenceInfo(string ProjectName);
