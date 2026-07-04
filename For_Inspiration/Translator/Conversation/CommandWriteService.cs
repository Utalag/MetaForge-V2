using MetaForge.BusinessModel;
using System.Text.Json;

namespace MetaForge.Translator;

/// <summary>
/// Zapouzdřuje write path: správu write dokumentu, aplikaci patchů,
/// shadow command log a persitenci. Je zodpovědný za startup sync
/// z projekce (OQ-005: startup-only, nikoli po každé mutaci).
/// </summary>
internal sealed class CommandWriteService
{
    private readonly AuthoringCommandProvenanceBuilder _commandProvenanceBuilder = new();
    private readonly BusinessPatchToCommandMapper _commandMapper;
    private readonly AuthoringConversationConfiguration _configuration;
    private readonly BusinessDocumentStore _documentStore;
    private readonly BusinessPatchEngine _patchEngine;
    private readonly IProjectionQueryService? _projectionQueryService;
    private readonly IShadowCommandStore? _shadowCommandStore;
    private BusinessAuthoringDocument _currentDocument;

    internal CommandWriteService(
        AuthoringConversationConfiguration configuration,
        BusinessDocumentStore documentStore,
        BusinessPatchEngine patchEngine,
        IShadowCommandStore? shadowCommandStore,
        BusinessPatchToCommandMapper commandMapper,
        IProjectionQueryService? projectionQueryService)
    {
        _configuration = configuration;
        _documentStore = documentStore;
        _patchEngine = patchEngine;
        _commandMapper = commandMapper;
        _shadowCommandStore = shadowCommandStore;
        _projectionQueryService = projectionQueryService;

        _currentDocument = LoadInitialDocument();
        EnsureShadowLogBootstrapForCurrentDocument();
        SyncCurrentDocumentFromProjection();
    }

    internal BusinessAuthoringDocument CurrentWriteDocument => _currentDocument;

    internal string PersistedDocumentPath => _documentStore.DocumentPath;

    internal IProjectionQueryService? ProjectionQueryService => _projectionQueryService;

    internal AuthoringResponseEnvelope? PendingProposal { get; set; }

    internal BusinessPatchCommandContext? PendingShadowCommandContext { get; set; }

    internal void ResetDocument(string projectName = "NewProject")
    {
        _currentDocument = _documentStore.CreateEmpty(projectName);
        PendingProposal = null;
        PendingShadowCommandContext = null;

        if (_configuration.Persistence.Enabled)
        {
            _currentDocument = _documentStore.Save(
                _currentDocument,
                _configuration.Persistence.ReloadPersistedDocumentAfterSave);
        }

        ResetShadowLogForCurrentDocument();
    }

    internal bool TryLoadDocument(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            var json = File.ReadAllText(filePath);
            var document = System.Text.Json.JsonSerializer.Deserialize<BusinessAuthoringDocument>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (document is null)
                return false;

            _currentDocument = document;
            PendingProposal = null;
            PendingShadowCommandContext = null;

            var directory = Path.GetDirectoryName(filePath);
            if (directory is not null)
            {
                _configuration.Persistence.DocumentPath = filePath;
            }

            ResetShadowLogForCurrentDocument();
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal WriteApplyResult ApplyPatches(IReadOnlyList<BusinessPatchOperation> patches)
    {
        var applyResult = _patchEngine.Apply(_currentDocument, patches);

        if (!applyResult.Success)
        {
            return new WriteApplyResult
            {
                Success = false,
                Issues = applyResult.Issues,
                GeneratedQuestions = applyResult.GeneratedQuestions,
                AppliedOperationCount = 0,
            };
        }

        var shadowFailure = TryAppendShadowCommands(patches);
        if (shadowFailure is not null)
        {
            return new WriteApplyResult
            {
                Success = false,
                ShadowLogErrorMessage = shadowFailure.ErrorMessage,
                ShadowLogIssue = shadowFailure.Issue,
                AppliedOperationCount = 0,
            };
        }

        _currentDocument = PersistIfEnabled(applyResult.Document);
        // Startup-only: projection sync is not re-invoked after each mutation.

        return new WriteApplyResult
        {
            Success = true,
            Issues = applyResult.Issues,
            GeneratedQuestions = applyResult.GeneratedQuestions,
            AppliedOperationCount = applyResult.AppliedOperationCount,
        };
    }

    internal WriteApplyResult ApplyEnrichment(string entityId, string attributeId, BusinessAttributeCoreDetail coreDetail)
    {
        var operation = new BusinessPatchOperation
        {
            Op = "enrich_attribute",
            EntityId = entityId,
            AttributeId = attributeId,
            Data =
            {
                ["source"] = coreDetail.Source.ToString(),
                ["resolvedPresetId"] = coreDetail.ResolvedPresetId,
                ["valueObjectName"] = coreDetail.ValueObjectName,
                ["isStrongType"] = coreDetail.IsStrongType,
            },
        };

        PendingShadowCommandContext ??= _commandProvenanceBuilder.CreateSystemContext(
            GetStreamId(), $"enrich-{Guid.NewGuid():N}", "enrich_attribute");

        return ApplyPatches([operation]);
    }

    internal string GetStreamId()
    {
        var projectId = string.IsNullOrWhiteSpace(_currentDocument.Project.Id)
            ? "unknown"
            : _currentDocument.Project.Id;

        return $"project:{projectId}";
    }

    internal BusinessProjectionView GetCurrentProjectionView()
    {
        // NOTE: async-over-sync — projection query is async but consumed synchronously
        // by GetCurrentReadDocument(). Full fix requires async read path throughout facade.
        if (_projectionQueryService is null)
            throw new InvalidOperationException("Replay business projekce neni dostupna, protoze shadow log neni zapnuty.");

        var projection = _projectionQueryService.GetProjectionAsync(GetStreamId()).GetAwaiter().GetResult();
        if (projection.TotalCommandCount == 0)
            throw new InvalidOperationException("Pro aktualni stream zatim neexistuje zadny command log k replayi.");

        return projection;
    }

    internal BusinessPatchCommandContext CreateUserCommandContext(string turnId, string commandName)
    {
        return CreateUserCommandContext(turnId, commandName, CommandSource.Chat);
    }

    internal BusinessPatchCommandContext CreateUserCommandContext(string turnId, string commandName, CommandSource source)
    {
        return _commandProvenanceBuilder.CreateUserCommandContext(GetStreamId(), turnId, commandName, source);
    }

    internal BusinessPatchCommandContext CreateDeterministicFallbackCommandContext(string turnId)
    {
        return _commandProvenanceBuilder.CreateDeterministicFallbackContext(GetStreamId(), turnId);
    }

    internal BusinessPatchCommandContext CreateAiTranslatedCommandContext(string turnId, SemanticBriefJson? brief, bool manualTranslateCommand)
    {
        return _commandProvenanceBuilder.CreateAiTranslatedContext(GetStreamId(), turnId, brief, manualTranslateCommand);
    }

    private ShadowLogAppendFailure? TryAppendShadowCommands(IReadOnlyList<BusinessPatchOperation> patches)
    {
        if (_shadowCommandStore is null || patches.Count == 0)
            return null;

        if (PendingShadowCommandContext is null)
        {
            return new ShadowLogAppendFailure(
                "Shadow log je zapnuty, ale pro aplikovanou mutaci chybi command context.",
                new BusinessValidationIssue(
                    "shadow.log.context.missing",
                    "Shadow log je zapnuty, ale pro aplikovanou mutaci chybi command context.",
                    "Error",
                    "shadowLog"));
        }

        foreach (var patch in patches)
        {
            var envelope = _commandMapper.Map(patch, PendingShadowCommandContext);
            var appendResult = _shadowCommandStore.Append(envelope);
            if (appendResult.Success)
                continue;

            var errorMessage = string.IsNullOrWhiteSpace(appendResult.ErrorMessage)
                ? "Append shadow command logu selhal."
                : appendResult.ErrorMessage;

            return new ShadowLogAppendFailure(
                errorMessage,
                new BusinessValidationIssue(
                    "shadow.log.append.failed",
                    errorMessage,
                    "Error",
                    "shadowLog"));
        }

        return null;
    }

    private void EnsureShadowLogBootstrapForCurrentDocument()
    {
        if (_shadowCommandStore is null)
            return;

        if (ShadowLogHasEntries(_shadowCommandStore.FilePath))
            return;

        var bootstrapOperation = new BusinessPatchOperation
        {
            Op = "set_project",
            Data =
            {
                ["id"] = _currentDocument.Project.Id,
                ["name"] = _currentDocument.Project.Name,
                ["description"] = _currentDocument.Project.Description,
                ["icon"] = _currentDocument.Project.Icon,
                ["version"] = _currentDocument.Project.Version,
            }
        };

        var bootstrapContext = new BusinessPatchCommandContext
        {
            StreamId = GetStreamId(),
            IssuedAt = DateTimeOffset.UtcNow,
            MutationId = $"bootstrap-{Guid.NewGuid():N}",
            IssuedBy = new CommandIssuedBy
            {
                ActorType = "system",
                ActorId = "authoring-runtime",
                DisplayName = "MetaForge Authoring Runtime",
            },
            Source = CommandSource.System,
            Provenance = new CommandProvenance
            {
                Mode = "system-bootstrap",
                Reason = "bootstrap-current-document",
                Producer = "authoring-conversation-service",
                PromptVersion = "command-log-bootstrap/v1",
            },
        };

        new JsonlShadowCommandStore(_shadowCommandStore.FilePath).Append(_commandMapper.Map(bootstrapOperation, bootstrapContext));
    }

    private void ResetShadowLogForCurrentDocument()
    {
        if (_shadowCommandStore is null)
            return;

        var filePath = _shadowCommandStore.FilePath;
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(filePath, string.Empty);
        EnsureShadowLogBootstrapForCurrentDocument();
    }

    private void SyncCurrentDocumentFromProjection()
    {
        if (_projectionQueryService is null)
            return;

        var projection = _projectionQueryService.GetProjectionAsync(GetStreamId()).GetAwaiter().GetResult();
        if (!projection.Success || projection.TotalCommandCount == 0)
            return;

        if (projection.TotalCommandCount == 1 && HasRicherPersistedDocument(_currentDocument, projection.Document))
            return;

        if (projection.Success && projection.TotalCommandCount > 0)
            _currentDocument = projection.Document;
    }

    private static bool HasRicherPersistedDocument(BusinessAuthoringDocument persistedDocument, BusinessAuthoringDocument replayDocument)
    {
        return persistedDocument.Entities.Count > replayDocument.Entities.Count
            || persistedDocument.Relations.Count > replayDocument.Relations.Count
            || persistedDocument.Workflows.Count > replayDocument.Workflows.Count
            || persistedDocument.PendingQuestions.Count > replayDocument.PendingQuestions.Count
            || persistedDocument.Notes.Count > replayDocument.Notes.Count;
    }

    private BusinessAuthoringDocument LoadInitialDocument()
    {
        if (_configuration.Persistence.Enabled && _configuration.Persistence.LoadPersistedDocumentOnStartup)
        {
            var persisted = _documentStore.TryLoad();
            if (persisted is not null)
                return persisted;
        }

        var empty = _documentStore.CreateEmpty();
        if (!_configuration.Persistence.Enabled)
            return empty;

        return _documentStore.Save(empty, _configuration.Persistence.ReloadPersistedDocumentAfterSave);
    }

    private BusinessAuthoringDocument PersistIfEnabled(BusinessAuthoringDocument document)
    {
        if (!_configuration.Persistence.Enabled)
            return document;

        return _documentStore.Save(document, _configuration.Persistence.ReloadPersistedDocumentAfterSave);
    }

    private static bool ShadowLogHasEntries(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return stream.Length > 0;
    }

    internal sealed class WriteApplyResult
    {
        public bool Success { get; init; }
        public IReadOnlyList<BusinessValidationIssue> Issues { get; init; } = [];
        public IReadOnlyList<PendingQuestionNode> GeneratedQuestions { get; init; } = [];
        public int AppliedOperationCount { get; init; }
        public string? ShadowLogErrorMessage { get; init; }
        public BusinessValidationIssue? ShadowLogIssue { get; init; }
        public bool HasShadowLogFailure => ShadowLogErrorMessage is not null || ShadowLogIssue is not null;
    }

    internal sealed record ShadowLogAppendFailure(string ErrorMessage, BusinessValidationIssue Issue);
}
