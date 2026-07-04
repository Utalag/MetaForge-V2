namespace MetaForge.Translator;

public interface IAuthoringAiClient
{
    bool IsAvailable { get; }

    Task<AuthoringResponseEnvelope?> CompleteAuthoringAsync(
        AuthoringPromptRequest request,
        CancellationToken cancellationToken = default);
}