namespace MetaForge.Generators;

/// <summary>
/// Metadata o cílovém jazyce — pro C#-first architekturu existuje jen jedna instance.
/// </summary>
public sealed record LanguageMapping(
    string LanguageId,
    string FileExtension,
    string CommentPrefix,
    bool SupportsPartialClasses
)
{
    /// <summary>Výchozí mapping pro C#.</summary>
    public static LanguageMapping CSharp { get; } = new(
        LanguageId: "csharp",
        FileExtension: ".cs",
        CommentPrefix: "//",
        SupportsPartialClasses: true
    );
}
