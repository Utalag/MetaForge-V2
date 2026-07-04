using MetaForge.BusinessModel;

namespace MetaForge.Translator;

public sealed class AuthoringResponseEnvelope
{
    public AuthoringResponseMode Mode { get; init; } = AuthoringResponseMode.Answer;

    public string AssistantMessage { get; init; } = string.Empty;

    public IReadOnlyList<string> Questions { get; init; } = [];

    public IReadOnlyList<string> Warnings { get; init; } = [];

    public IReadOnlyList<BusinessPatchOperation> Patches { get; init; } = [];
}

public enum AuthoringResponseMode
{
    Answer,
    Ask,
    Propose,
    Apply,
}