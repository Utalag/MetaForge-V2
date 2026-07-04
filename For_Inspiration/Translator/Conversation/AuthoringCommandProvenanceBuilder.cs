using MetaForge.BusinessModel;

namespace MetaForge.Translator;

internal sealed class AuthoringCommandProvenanceBuilder
{
    public BusinessPatchCommandContext CreateUserCommandContext(
        string streamId,
        string turnId,
        string commandName,
        CommandSource source = CommandSource.Chat)
    {
        return CreateContext(
            streamId,
            turnId,
            source,
            new CommandIssuedBy
            {
                ActorType = "user",
            },
            new CommandProvenance
            {
                Mode = "user-command",
                Reason = NormalizeReason(commandName) ?? "explicit-host-command",
                Producer = "authoring-chat-host",
                PromptVersion = "authoring-host/v1",
            });
    }

    public BusinessPatchCommandContext CreateDeterministicFallbackContext(
        string streamId,
        string turnId,
        CommandSource source = CommandSource.Chat)
    {
        return CreateContext(
            streamId,
            turnId,
            source,
            new CommandIssuedBy
            {
                ActorType = "user",
            },
            new CommandProvenance
            {
                Mode = "deterministic-fallback",
                Reason = "recognized-deterministic-command",
                Producer = "authoring-conversation-service",
                PromptVersion = "deterministic-fallback/v1",
            });
    }

    public BusinessPatchCommandContext CreateSystemContext(
        string streamId,
        string turnId,
        string reason)
    {
        return CreateContext(
            streamId,
            turnId,
            CommandSource.System,
            new CommandIssuedBy
            {
                ActorType = "system",
                ActorId = "authoring-runtime",
                DisplayName = "MetaForge Authoring Runtime",
            },
            new CommandProvenance
            {
                Mode = "system-enrichment",
                Reason = NormalizeReason(reason) ?? "system-enrichment",
                Producer = "authoring-enrichment-service",
                PromptVersion = "enrichment/v1",
            });
    }

    public BusinessPatchCommandContext CreateAiTranslatedContext(
        string streamId,
        string turnId,
        SemanticBriefJson? brief,
        bool manualTranslateCommand,
        CommandSource source = CommandSource.Chat)
    {
        var briefTurnId = NormalizeValue(brief?.SourceTurnId);
        var briefId = NormalizeValue(brief?.BriefId);

        return new BusinessPatchCommandContext
        {
            StreamId = streamId,
            IssuedAt = DateTimeOffset.UtcNow,
            IssuedBy = new CommandIssuedBy
            {
                ActorType = manualTranslateCommand ? "user" : "assistant",
            },
            Source = source,
            CorrelationId = manualTranslateCommand ? turnId : briefTurnId ?? turnId,
            CausationId = briefId,
            MutationId = CreateMutationId(),
            Provenance = new CommandProvenance
            {
                Mode = manualTranslateCommand ? "manual-translate-command" : "ai-translated",
                Reason = ResolveTranslationReason(brief, manualTranslateCommand),
                Producer = manualTranslateCommand ? "authoring-translation-manual" : "authoring-translation-auto",
                PromptVersion = "authoring-translation/v1",
                Confidence = ResolveConfidence(brief),
            },
        };
    }

    private static BusinessPatchCommandContext CreateContext(
        string streamId,
        string turnId,
        CommandSource source,
        CommandIssuedBy issuedBy,
        CommandProvenance provenance)
    {
        return new BusinessPatchCommandContext
        {
            StreamId = streamId,
            IssuedAt = DateTimeOffset.UtcNow,
            IssuedBy = issuedBy,
            Source = source,
            CorrelationId = turnId,
            MutationId = CreateMutationId(),
            Provenance = provenance,
        };
    }

    private static string CreateMutationId()
    {
        return $"mutation-{Guid.NewGuid():N}";
    }

    private static string? NormalizeReason(string? value)
    {
        return NormalizeValue(value)?.ToLowerInvariant();
    }

    private static string ResolveTranslationReason(SemanticBriefJson? brief, bool manualTranslateCommand)
    {
        if (manualTranslateCommand)
            return "explicit-translate-command";

        return NormalizeValue(brief?.TranslationIntent.Reason) ?? "semantic-brief-translated";
    }

    private static double? ResolveConfidence(SemanticBriefJson? brief)
    {
        if (brief is null)
            return null;

        var confidences = brief.SemanticChanges
            .Select(change => change.Confidence)
            .Where(confidence => confidence > 0)
            .ToArray();

        return confidences.Length == 0 ? null : Math.Round(confidences.Max(), 4);
    }

    private static string? NormalizeValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}