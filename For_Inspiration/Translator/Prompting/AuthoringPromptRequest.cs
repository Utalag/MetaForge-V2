using MetaForge.BusinessModel;

namespace MetaForge.Translator;

public sealed class AuthoringPromptRequest
{
    public string UserMessage { get; init; } = string.Empty;
    public bool IsManualTranslateCommand { get; init; }

    public BusinessAuthoringDocument Document { get; init; } = new();

    public BusinessTreeDetailLevel TreeDetailLevel { get; init; } = BusinessTreeDetailLevel.Extended;

    public string? CurrentTree { get; init; }

    public SemanticBriefJson? SemanticBrief { get; init; }

    public bool AutoApplyModeApply { get; init; }

    public bool RequireConfirmationForPropose { get; init; }

    public AuthoringContextView? AuthoringContext { get; init; }
}