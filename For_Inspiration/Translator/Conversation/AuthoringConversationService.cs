using MetaForge.BusinessModel;
using MetaForge.Core.Discovery;
using System.Text.Json;

namespace MetaForge.Translator;

public sealed class AuthoringConversationService
{
    private static readonly IAuthoringConversationAiClient DisabledConversationAiClient = new DisabledAuthoringConversationAiClient();

    private readonly CommandWriteService _writeService;
    private readonly AuthoringConversationConfiguration _configuration;
    private readonly IAuthoringConversationAiClient _conversationAiClient;
    private readonly IAuthoringAiClient _translationAiClient;
    private readonly bool _conversationAiEnabled;
    private readonly bool _translationAiEnabled;
    private readonly int _conversationTimeoutMs;
    private readonly int _translationTimeoutMs;
    private readonly IDiscoverySession? _discoverySession;
    private SemanticBriefJson? _pendingBrief;

    public AuthoringConversationService(
        AuthoringConversationConfiguration? configuration = null,
        IAuthoringAiClient? aiClient = null,
        BusinessPatchEngine? patchEngine = null,
        IShadowCommandStore? shadowCommandStore = null,
        BusinessPatchToCommandMapper? commandMapper = null,
        IProjectionQueryService? projectionQueryService = null,
        IDiscoverySession? discoverySession = null)
        : this(configuration, aiClient is null ? null : DisabledConversationAiClient, aiClient, patchEngine, shadowCommandStore, commandMapper, projectionQueryService, discoverySession)
    {
    }

    public AuthoringConversationService(
        AuthoringConversationConfiguration? configuration,
        IAuthoringConversationAiClient? conversationAiClient,
        IAuthoringAiClient? translationAiClient,
        BusinessPatchEngine? patchEngine = null,
        IShadowCommandStore? shadowCommandStore = null,
        BusinessPatchToCommandMapper? commandMapper = null,
        IProjectionQueryService? projectionQueryService = null,
        IDiscoverySession? discoverySession = null)
    {
        _discoverySession = discoverySession;
        _configuration = configuration ?? AuthoringConversationConfiguration.Load();

        var resolvedConfig = _configuration;
        var documentStore = new BusinessDocumentStore(resolvedConfig.GetResolvedDocumentPath());
        var resolvedPatchEngine = patchEngine ?? new BusinessPatchEngine();
        var resolvedCommandMapper = commandMapper ?? new BusinessPatchToCommandMapper();
        var resolvedShadowStore = ResolveShadowCommandStore(resolvedConfig, shadowCommandStore);
        var resolvedProjectionQueryService = ResolveProjectionQueryService(resolvedConfig, resolvedShadowStore, projectionQueryService);

        _writeService = new CommandWriteService(
            resolvedConfig,
            documentStore,
            resolvedPatchEngine,
            resolvedShadowStore,
            resolvedCommandMapper,
            resolvedProjectionQueryService);

        AuthoringAiClientDefaults? aiDefaults = null;
        if (conversationAiClient is null || translationAiClient is null)
            aiDefaults = AuthoringAiClientFactory.CreateDefaults();

        if (conversationAiClient is not null)
        {
            _conversationAiClient = conversationAiClient;
            _conversationAiEnabled = true;
            _conversationTimeoutMs = 0;
        }
        else
        {
            _conversationAiClient = aiDefaults!.ConversationClient;
            _conversationAiEnabled = aiDefaults.ConversationEnabled;
            _conversationTimeoutMs = aiDefaults.ConversationTimeoutMs;
        }

        if (translationAiClient is not null)
        {
            _translationAiClient = translationAiClient;
            _translationAiEnabled = true;
            _translationTimeoutMs = 0;
        }
        else
        {
            _translationAiClient = aiDefaults!.TranslationClient;
            _translationAiEnabled = aiDefaults.TranslationEnabled;
            _translationTimeoutMs = aiDefaults.TranslationTimeoutMs;
        }

        TreeDetailLevel = _configuration.Tree.DefaultDetailLevel;
    }

    public BusinessAuthoringDocument CurrentWriteDocument => _writeService.CurrentWriteDocument;

    public string PersistedDocumentPath => _writeService.PersistedDocumentPath;

    public BusinessTreeDetailLevel TreeDetailLevel { get; private set; }

    internal bool AutoApplyPresetsEnabled => _configuration.Enrichment.AutoApplyPresets;

    internal IAuthoringAiClient TranslationAiClient => _translationAiClient;

    internal IProjectionQueryService? ProjectionQueryService => _writeService.ProjectionQueryService;

    internal BusinessPatchCommandContext? PendingShadowCommandContext
    {
        get => _writeService.PendingShadowCommandContext;
        private set => _writeService.PendingShadowCommandContext = value;
    }

    public async Task<ConversationTurnResult> ProcessMessageAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return CreateResult(false, "Zprava nesmi byt prazdna.");

        var normalizedMessage = userMessage.Trim();
        var turnId = CreateTurnId();
        PendingShadowCommandContext = null;

        var commandResult = await TryHandleAuthoringCommandAsync(normalizedMessage, turnId, cancellationToken);
        if (commandResult is not null)
            return commandResult;

        if (_conversationAiEnabled && _conversationAiClient.IsAvailable)
        {
            return await ProcessConversationTurnAsync(normalizedMessage, turnId, cancellationToken);
        }

        var fallbackEnvelope = TryBuildDeterministicEnvelope(normalizedMessage);
        if (fallbackEnvelope is not null)
        {
            PendingShadowCommandContext = _writeService.CreateDeterministicFallbackCommandContext(turnId);
            _pendingBrief = null;
            return FinalizeEnvelope(fallbackEnvelope);
        }

        var message = _conversationAiEnabled
            ? "AI authoring vrstva nevratila zpracovatelny vysledek a zprava neodpovida podporovanemu deterministic fallbacku."
            : "Conversation AI neni v konfiguraci povolena a zprava neodpovida podporovanemu deterministic fallbacku.";

        return CreateResult(false, message);
    }

    public void ResetDocument(string projectName = "NewProject")
    {
        _pendingBrief = null;
        _writeService.ResetDocument(projectName);
    }

    public bool TryLoadDocument(string filePath)
    {
        _pendingBrief = null;
        return _writeService.TryLoadDocument(filePath);
    }

    public void SetTreeDetailLevel(BusinessTreeDetailLevel detailLevel)
    {
        TreeDetailLevel = detailLevel;
    }

    public string GetCurrentTree()
    {
        return BusinessTreeRenderer.Render(GetCurrentReadDocument(), TreeDetailLevel);
    }

    public string GetCurrentReadDocumentJson()
    {
        return BusinessDocumentJsonSerializer.Serialize(GetCurrentReadDocument());
    }

    public string GetCurrentWriteDocumentJson()
    {
        return BusinessDocumentJsonSerializer.Serialize(CurrentWriteDocument);
    }

    public BusinessAuthoringDocument GetCurrentReadDocumentModel()
    {
        return GetCurrentReadDocument();
    }

    public BusinessProjectionView GetCurrentProjectionView()
    {
        return _writeService.GetCurrentProjectionView();
    }

    private AuthoringContextView BuildAuthoringContextView()
    {
        return AuthoringContextBuilder.Build(
            _writeService.CurrentWriteDocument,
            includeDiscovery: _discoverySession is not null,
            discoverySession: _discoverySession);
    }

    public ConversationTurnResult ApplyExplicitOperations(
        string commandName,
        IReadOnlyList<BusinessPatchOperation> operations,
        CommandSource source = CommandSource.Unknown,
        string? assistantMessage = null,
        IReadOnlyList<string>? warnings = null,
        IReadOnlyList<string>? questions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(operations);

        PendingShadowCommandContext = _writeService.CreateUserCommandContext(CreateTurnId(), commandName, source);
        _writeService.PendingProposal = null;

        return ApplyEnvelopeChanges(new AuthoringResponseEnvelope
        {
            Mode = AuthoringResponseMode.Apply,
            AssistantMessage = string.IsNullOrWhiteSpace(assistantMessage)
                ? $"Provadim prikaz {commandName}."
                : assistantMessage,
            Warnings = warnings ?? [],
            Questions = questions ?? [],
            Patches = operations.ToArray(),
        });
    }

    public ConversationTurnResult ApplyEnrichment(
        string entityId,
        string attributeId,
        BusinessAttributeCoreDetail coreDetail,
        CommandSource source = CommandSource.Unknown)
    {
        var writeResult = _writeService.ApplyEnrichment(entityId, attributeId, coreDetail);

        if (!writeResult.Success)
        {
            return CreateResult(
                false,
                writeResult.ShadowLogErrorMessage ?? "Enrichment atributu selhal.",
                issues: writeResult.Issues);
        }

        return CreateResult(
            true,
            $"CoreDetail pro atribut {attributeId} v entite {entityId} byl aktualizovan.",
            AuthoringResponseMode.Apply);
    }

    private async Task<ConversationTurnResult?> TryHandleAuthoringCommandAsync(string userMessage, string turnId, CancellationToken cancellationToken)
    {
        if (TryHandleProposalCommand(userMessage, turnId, out var proposalResult))
            return proposalResult;

        if (!userMessage.Equals("translate", StringComparison.OrdinalIgnoreCase)
            && !userMessage.Equals("/translate", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (_pendingBrief is null)
        {
            return CreateResult(false, "Neni pripraven zadny cekajici semantic brief k prekladu.");
        }

        if (!_translationAiEnabled || !_translationAiClient.IsAvailable)
        {
            return CreateResult(false, "Prekladovy AI model neni dostupny. Nastav AuthoringTranslation nebo MainChat fallback.");
        }

        if (!CanTranslateBriefManually(_pendingBrief, out var manualReason, out var manualQuestions))
        {
            return CreateResult(
                false,
                manualReason,
                manualQuestions.Count > 0 ? AuthoringResponseMode.Ask : AuthoringResponseMode.Answer,
                questions: manualQuestions);
        }

        var brief = _pendingBrief;
        var (envelope, translationError) = await TryCompleteTranslationEnvelopeAsync("translate", brief, cancellationToken, isManualTranslateCommand: true);
        if (envelope is null)
        {
            return CreateResult(false, translationError ?? "Prekladovy model nevratil authoring envelope pro cekajici semantic brief.");
        }

        PendingShadowCommandContext = _writeService.CreateAiTranslatedCommandContext(turnId, brief, manualTranslateCommand: true);
        _pendingBrief = null;

        var translatedEnvelope = CopyEnvelope(
            envelope,
            envelope.Mode,
            string.IsNullOrWhiteSpace(envelope.AssistantMessage)
                ? "Prekladam cekajici semantic brief do authoring patchu."
                : $"Prekladam cekajici semantic brief. {envelope.AssistantMessage}");

        return FinalizeEnvelope(translatedEnvelope);
    }

    private async Task<ConversationTurnResult> ProcessConversationTurnAsync(string userMessage, string turnId, CancellationToken cancellationToken)
    {
        var conversationRequest = new ConversationPromptRequest
        {
            UserMessage = userMessage,
            Document = _writeService.CurrentWriteDocument,
            TreeDetailLevel = TreeDetailLevel,
            CurrentTree = GetCurrentTree(),
            PendingBrief = _pendingBrief,
            AuthoringContext = BuildAuthoringContextView(),
        };

        var (conversationResult, conversationError) = await TryCompleteConversationAsync(conversationRequest, cancellationToken);
        if (conversationResult is null)
        {
            var fallbackEnvelope = TryBuildDeterministicEnvelope(userMessage);
            if (fallbackEnvelope is not null)
            {
                PendingShadowCommandContext = _writeService.CreateDeterministicFallbackCommandContext(turnId);
                _pendingBrief = null;
                return FinalizeEnvelope(AddWarning(fallbackEnvelope, conversationError));
            }

            return CreateResult(
                false,
                conversationError
                    ?? "Conversation AI nevratila semantic brief ani textovou odpoved a fallback zpravu nerozpoznal.");
        }

        if (conversationResult.Brief is null)
        {
            _pendingBrief = null;
            return CreateResult(
                true,
                string.IsNullOrWhiteSpace(conversationResult.AssistantMessage)
                    ? "Konverzacni vrstva pripravila odpoved bez modelove zmeny."
                    : conversationResult.AssistantMessage,
                AuthoringResponseMode.Answer,
                conversationResult.Warnings,
                conversationResult.Questions);
        }

        _pendingBrief = conversationResult.Brief;

        if (ShouldAutoTranslate(conversationResult.Brief))
        {
            var (envelope, translationError) = await TryCompleteTranslationEnvelopeAsync(userMessage, conversationResult.Brief, cancellationToken);
            if (envelope is not null)
            {
                PendingShadowCommandContext = _writeService.CreateAiTranslatedCommandContext(turnId, conversationResult.Brief, manualTranslateCommand: false);
                _pendingBrief = null;
                return FinalizeEnvelope(MergeConversationResult(conversationResult, envelope));
            }

            if (!string.IsNullOrWhiteSpace(translationError))
            {
                return CreatePendingBriefResult(
                    new ConversationAiResult
                    {
                        AssistantMessage = conversationResult.AssistantMessage,
                        Questions = conversationResult.Questions,
                        Warnings = MergeWarningLists(conversationResult.Warnings, [translationError]),
                        Brief = conversationResult.Brief,
                    },
                    conversationResult.Brief);
            }
        }

        return CreatePendingBriefResult(conversationResult, conversationResult.Brief);
    }

    private async Task<(ConversationAiResult? Result, string? Error)> TryCompleteConversationAsync(
        ConversationPromptRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return (await _conversationAiClient.CompleteConversationAsync(request, cancellationToken), null);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return (null, CreateAiTimeoutMessage("Conversation", _conversationTimeoutMs, ex));
        }
        catch (Exception ex)
        {
            return (null, CreateAiFailureMessage("Conversation", ex));
        }
    }

    private async Task<(AuthoringResponseEnvelope? Envelope, string? Error)> TryCompleteTranslationEnvelopeAsync(
        string userMessage,
        SemanticBriefJson? semanticBrief,
        CancellationToken cancellationToken,
        bool isManualTranslateCommand = false)
    {
        var request = new AuthoringPromptRequest
        {
            UserMessage = userMessage,
            IsManualTranslateCommand = isManualTranslateCommand,
            Document = _writeService.CurrentWriteDocument,
            TreeDetailLevel = TreeDetailLevel,
            CurrentTree = GetCurrentTree(),
            SemanticBrief = semanticBrief,
            AutoApplyModeApply = _configuration.Prompting.AutoApplyModeApply,
            RequireConfirmationForPropose = _configuration.Prompting.RequireConfirmationForPropose,
            AuthoringContext = BuildAuthoringContextView(),
        };

        try
        {
            return (await _translationAiClient.CompleteAuthoringAsync(request, cancellationToken), null);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return (null, CreateAiTimeoutMessage("AuthoringTranslation", _translationTimeoutMs, ex));
        }
        catch (Exception ex)
        {
            return (null, CreateAiFailureMessage("AuthoringTranslation", ex));
        }
    }

    private ConversationTurnResult CreatePendingBriefResult(ConversationAiResult conversationResult, SemanticBriefJson brief)
    {
        var questions = MergeQuestionLists(conversationResult.Questions, brief.OpenQuestions.Select(question => question.Text));
        var warnings = MergeWarningLists(conversationResult.Warnings, GetBriefWarnings(brief));
        var mode = brief.OpenQuestions.Any(question => question.Blocking)
            ? AuthoringResponseMode.Ask
            : AuthoringResponseMode.Answer;

        var assistantMessage = string.IsNullOrWhiteSpace(conversationResult.AssistantMessage)
            ? "Semantic brief je pripraven a ceka na dalsi krok."
            : conversationResult.AssistantMessage;

        var normalizedState = NormalizeTranslationState(brief.TranslationIntent.State);
        if (CanTranslateBriefManually(brief, out _, out _) && normalizedState != "recommended")
        {
            assistantMessage = AppendSentence(assistantMessage, "Pro preklad pouzij prikaz translate.");
        }
        else if (normalizedState == "recommended")
        {
            var reason = brief.TranslationIntent.Reason;
            if (CanTranslateBriefManually(brief, out _, out _))
            {
                if (!string.IsNullOrWhiteSpace(reason))
                    assistantMessage = AppendSentence(assistantMessage, $"Mam navrh — {reason}. Muzes ho potvrdit prikazem translate, nebo upresnit.");
                else
                    assistantMessage = AppendSentence(assistantMessage, "Mam navrh — muzes ho potvrdit prikazem translate, nebo upresnit.");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(reason))
                    assistantMessage = AppendSentence(assistantMessage, $"Doporuceno: {reason}. Potvrd nebo uprav pred prekladem.");
                else
                    assistantMessage = AppendSentence(assistantMessage, "Doporuceno. Potvrd nebo uprav pred prekladem.");
            }
        }

        return CreateResult(true, assistantMessage, mode, warnings, questions);
    }

    private bool ShouldAutoTranslate(SemanticBriefJson brief)
    {
        if (!_translationAiEnabled || !_translationAiClient.IsAvailable)
            return false;

        if (!CanTranslateBriefAutomatically(brief))
            return false;

        var state = NormalizeTranslationState(brief.TranslationIntent.State);
        return state switch
        {
            "ready" => true,
            "recommended" => false,
            _ => false,
        };
    }

    private static bool CanTranslateBriefAutomatically(SemanticBriefJson brief)
    {
        var state = NormalizeTranslationState(brief.TranslationIntent.State);
        return state != "blocked"
            && state != "none"
            && !brief.OpenQuestions.Any(question => question.Blocking);
    }

    private static bool CanTranslateBriefManually(
        SemanticBriefJson brief,
        out string reason,
        out IReadOnlyList<string> questions)
    {
        var blockingQuestions = brief.OpenQuestions
            .Where(question => question.Blocking)
            .Select(question => question.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToArray();

        if (blockingQuestions.Length > 0)
        {
            reason = "Semantic brief ma blokujici otevrene otazky a zatim ho nelze prelozit do patchu.";
            questions = blockingQuestions;
            return false;
        }

        var state = NormalizeTranslationState(brief.TranslationIntent.State);
        if (state == "blocked")
        {
            reason = string.IsNullOrWhiteSpace(brief.TranslationIntent.Reason)
                ? "Semantic brief je oznacen jako blocked a nelze ho prelozit."
                : brief.TranslationIntent.Reason;
            questions = [];
            return false;
        }

        if (state == "none")
        {
            reason = "Posledni semantic brief neobsahuje prekladatelnou modelovou zmenu.";
            questions = [];
            return false;
        }

        reason = string.Empty;
        questions = [];
        return true;
    }

    private static AuthoringResponseEnvelope MergeConversationResult(ConversationAiResult conversationResult, AuthoringResponseEnvelope envelope)
    {
        var mergedWarnings = MergeWarningLists(conversationResult.Warnings, envelope.Warnings);
        var mergedQuestions = MergeQuestionLists(conversationResult.Questions, envelope.Questions);
        var mergedMessage = string.IsNullOrWhiteSpace(conversationResult.AssistantMessage)
            ? envelope.AssistantMessage
            : string.IsNullOrWhiteSpace(envelope.AssistantMessage)
                ? conversationResult.AssistantMessage
                : $"{conversationResult.AssistantMessage} {envelope.AssistantMessage}";

        return new AuthoringResponseEnvelope
        {
            Mode = envelope.Mode,
            AssistantMessage = mergedMessage,
            Questions = mergedQuestions,
            Warnings = mergedWarnings,
            Patches = envelope.Patches,
        };
    }

    private static AuthoringResponseEnvelope AddWarning(AuthoringResponseEnvelope envelope, string? warning)
    {
        if (string.IsNullOrWhiteSpace(warning))
            return envelope;

        return new AuthoringResponseEnvelope
        {
            Mode = envelope.Mode,
            AssistantMessage = envelope.AssistantMessage,
            Questions = envelope.Questions,
            Warnings = MergeWarningLists(envelope.Warnings, [warning]),
            Patches = envelope.Patches,
        };
    }

    private static IReadOnlyList<string> MergeWarningLists(IEnumerable<string> first, IEnumerable<string> second)
    {
        return first
            .Concat(second)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> MergeQuestionLists(IEnumerable<string> first, IEnumerable<string> second)
    {
        return first
            .Concat(second)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> GetBriefWarnings(SemanticBriefJson brief)
    {
        var warnings = new List<string>();
        var state = NormalizeTranslationState(brief.TranslationIntent.State);

        if (!string.IsNullOrWhiteSpace(brief.TranslationIntent.Reason))
            warnings.Add(brief.TranslationIntent.Reason);

        if (state is "manualonly" or "ready")
            warnings.Add("Semantic brief je ulozeny v pameti konverzace a lze ho prelozit prikazem translate.");

        return warnings;
    }

    private static string NormalizeTranslationState(string? state)
    {
        return string.IsNullOrWhiteSpace(state)
            ? "none"
            : state.Replace("-", string.Empty, StringComparison.Ordinal).Replace("_", string.Empty, StringComparison.Ordinal).Trim().ToLowerInvariant();
    }

    private static string AppendSentence(string message, string sentence)
    {
        if (string.IsNullOrWhiteSpace(message))
            return sentence;

        if (string.IsNullOrWhiteSpace(sentence))
            return message;

        return message.EndsWith('.') || message.EndsWith('!') || message.EndsWith('?')
            ? $"{message} {sentence}"
            : $"{message}. {sentence}";
    }

    private static string CreateAiTimeoutMessage(string segmentName, int timeoutMs, Exception exception)
    {
        var timeoutText = timeoutMs > 0
            ? $" po {Math.Max(1, timeoutMs / 1000)} s"
            : string.Empty;

        var message = $"{segmentName} AI vyprsela na timeout{timeoutText}. Zkus znovu, zvys TimeoutMs nebo zkrat zadani.";
        var detail = exception.GetBaseException().Message;

        return string.IsNullOrWhiteSpace(detail)
            ? message
            : $"{message} Detail: {detail}";
    }

    private static string CreateAiFailureMessage(string segmentName, Exception exception)
    {
        var detail = exception.GetBaseException().Message;
        return string.IsNullOrWhiteSpace(detail)
            ? $"{segmentName} AI selhala. Zkontroluj provider, endpoint nebo API klic a zkus to znovu."
            : $"{segmentName} AI selhala. Detail: {detail}";
    }

    private ConversationTurnResult FinalizeEnvelope(AuthoringResponseEnvelope envelope)
    {
        var requiresConfirmation = (envelope.Mode == AuthoringResponseMode.Propose
            && _configuration.Prompting.RequireConfirmationForPropose)
            || (envelope.Mode == AuthoringResponseMode.Apply
                && _configuration.Prompting.RequireConfirmationForApply
                && envelope.Patches.Count > 0);

        if (requiresConfirmation)
        {
            _writeService.PendingProposal = envelope;

            var proposalQuestions = envelope.Questions.Count > 0
                ? envelope.Questions.Concat(["Potvrd navrh prikazem accept nebo ho odmitni prikazem reject."]).ToArray()
                : ["Potvrd navrh prikazem accept nebo ho odmitni prikazem reject."];

            return CreateResult(
                true,
                string.IsNullOrWhiteSpace(envelope.AssistantMessage)
                    ? GetDefaultModeMessage(AuthoringResponseMode.Propose, success: true)
                    : envelope.AssistantMessage,
                AuthoringResponseMode.Propose,
                envelope.Warnings,
                proposalQuestions);
        }

        var effectiveEnvelope = envelope.Mode == AuthoringResponseMode.Propose
            && !_configuration.Prompting.RequireConfirmationForPropose
            && envelope.Patches.Count > 0
                ? CopyEnvelope(envelope, AuthoringResponseMode.Apply)
                : envelope;

        _writeService.PendingProposal = null;

        if (ShouldApplyEnvelope(effectiveEnvelope))
            return ApplyEnvelopeChanges(effectiveEnvelope);

        return CreateResult(
            true,
            string.IsNullOrWhiteSpace(effectiveEnvelope.AssistantMessage)
                ? GetDefaultModeMessage(effectiveEnvelope.Mode, success: true)
                : effectiveEnvelope.AssistantMessage,
            effectiveEnvelope.Mode,
            effectiveEnvelope.Warnings,
            effectiveEnvelope.Questions);
    }

    private ConversationTurnResult ApplyEnvelopeChanges(AuthoringResponseEnvelope envelope)
    {
        if (envelope.Patches.Count == 0)
        {
            return CreateResult(
                false,
                string.IsNullOrWhiteSpace(envelope.AssistantMessage)
                    ? "AI vratila mode=apply, ale bez patch operaci."
                    : envelope.AssistantMessage,
                envelope.Mode,
                envelope.Warnings,
                envelope.Questions);
        }

        var writeResult = _writeService.ApplyPatches(envelope.Patches);
        var warnings = envelope.Warnings;

        if (writeResult.HasShadowLogFailure)
        {
            return CreateResult(
                false,
                string.IsNullOrWhiteSpace(writeResult.ShadowLogErrorMessage)
                    ? "Zmeny se nepodarilo bezpecne zapsat do command logu. Snapshot nebyl aktualizovan."
                    : writeResult.ShadowLogErrorMessage,
                envelope.Mode,
                warnings,
                envelope.Questions,
                writeResult.ShadowLogIssue is not null ? [writeResult.ShadowLogIssue] : [],
                appliedOperationCount: 0);
        }

        var mergedQuestions = envelope.Questions
            .Concat(writeResult.GeneratedQuestions.Select(question => question.Text))
            .ToArray();

        return CreateResult(
            writeResult.Success,
            string.IsNullOrWhiteSpace(envelope.AssistantMessage)
                ? GetDefaultModeMessage(envelope.Mode, writeResult.Success)
                : envelope.AssistantMessage,
            envelope.Mode,
            warnings,
            mergedQuestions,
            writeResult.Issues,
            writeResult.AppliedOperationCount);
    }

    private bool ShouldApplyEnvelope(AuthoringResponseEnvelope envelope)
    {
        return envelope.Mode == AuthoringResponseMode.Apply
            && _configuration.Prompting.AutoApplyModeApply;
    }

    internal static string CreateTurnId()
    {
        return $"turn-{Guid.NewGuid():N}";
    }

    private bool TryHandleProposalCommand(string userMessage, string turnId, out ConversationTurnResult result)
    {
        if (!userMessage.Equals("accept", StringComparison.OrdinalIgnoreCase)
            && !userMessage.Equals("reject", StringComparison.OrdinalIgnoreCase))
        {
            result = default!;
            return false;
        }

        if (_writeService.PendingProposal is null)
        {
            result = CreateResult(false, "Neni pripraven zadny cekajici navrh k potvrzeni.");
            return true;
        }

        if (userMessage.Equals("reject", StringComparison.OrdinalIgnoreCase))
        {
            _writeService.PendingProposal = null;
            result = CreateResult(true, "Navrh byl odmitnut a nebyl aplikovan.", AuthoringResponseMode.Answer);
            return true;
        }

        var proposal = _writeService.PendingProposal;
        _writeService.PendingProposal = null;
        PendingShadowCommandContext = _writeService.CreateUserCommandContext(turnId, userMessage);

        result = FinalizeEnvelope(CopyEnvelope(
            proposal,
            AuthoringResponseMode.Apply,
            string.IsNullOrWhiteSpace(proposal.AssistantMessage)
                ? "Potvrzuji navrzene zmeny."
                : proposal.AssistantMessage));

        return true;
    }

    private AuthoringResponseEnvelope? TryBuildDeterministicEnvelope(string userMessage)
    {
        if (TryMatchPrefix(userMessage, out var projectName, "nastav projekt ", "set project "))
        {
            return CreateApplyEnvelope(
                $"Nastavuji projekt {projectName}.",
                new BusinessPatchOperation
                {
                    Op = "set_project",
                    Data =
                    {
                        ["name"] = projectName,
                    }
                });
        }

        if (TryMatchCreateDraftEntity(userMessage, out var draftEntityName))
        {
            return CreateDraftEntityEnvelope(draftEntityName);
        }

        if (EqualsAny(userMessage, "pridej entitu", "přidej entitu", "add entity"))
        {
            return CreateAskEnvelope("Chybi nazev nove entity.");
        }

        if (TryMatchPrefix(userMessage, out var entityName, "pridej entitu ", "přidej entitu ", "add entity "))
        {
            return CreateApplyEnvelope(
                $"Pridavam entitu {entityName}.",
                new BusinessPatchOperation
                {
                    Op = "add_entity",
                    Data =
                    {
                        ["name"] = entityName,
                    }
                });
        }

        if (TryMatchRenameEntity(userMessage, out var sourceEntityName, out var targetEntityName))
        {
            var entity = FindEntity(sourceEntityName);
            if (entity is null)
                return CreateAskEnvelope($"Entitu {sourceEntityName} jsem nenasel. Kterou entitu mam prejmenovat?");

            return CreateApplyEnvelope(
                $"Prejmenovavam entitu {entity.Name} na {targetEntityName}.",
                new BusinessPatchOperation
                {
                    Op = "update_entity",
                    EntityId = entity.Id,
                    Data =
                    {
                        ["name"] = targetEntityName,
                    }
                });
        }

        if (TryMatchDeleteEntity(userMessage, out var entityNameForDelete))
        {
            var entity = FindEntity(entityNameForDelete);
            if (entity is null)
                return CreateAskEnvelope($"Entitu {entityNameForDelete} jsem nenasel. Kterou entitu mam smazat?");

            return CreateApplyEnvelope(
                $"Mazu entitu {entity.Name}.",
                new BusinessPatchOperation
                {
                    Op = "delete_entity",
                    EntityId = entity.Id,
                });
        }

        if (EqualsAny(userMessage, "pridej atribut", "přidej atribut", "add attribute"))
        {
            return CreateAskEnvelope("Pouzij tvar: pridej atribut Nazev do entity Entita.");
        }

        if (TryMatchAddAttribute(userMessage, out var attributeName, out var targetEntityNameForAttribute))
        {
            var entity = FindEntity(targetEntityNameForAttribute);
            if (entity is null)
                return CreateAskEnvelope($"Entitu {targetEntityNameForAttribute} jsem nenasel. Do ktere entity mam atribut pridat?");

            return CreateApplyEnvelope(
                $"Pridavam atribut {attributeName} do entity {entity.Name}.",
                new BusinessPatchOperation
                {
                    Op = "add_attribute",
                    EntityId = entity.Id,
                    Data =
                    {
                        ["name"] = attributeName,
                        ["type"] = "text",
                    }
                });
        }

        if (TryMatchRenameAttribute(userMessage, out var attributeNameForRename, out var entityNameForRename, out var renamedAttribute))
        {
            var attributeMatch = FindAttribute(entityNameForRename, attributeNameForRename);
            if (attributeMatch is null)
                return CreateAskEnvelope($"Atribut {attributeNameForRename} v entite {entityNameForRename} jsem nenasel. Ktery atribut mam prejmenovat?");

            return CreateApplyEnvelope(
                $"Prejmenovavam atribut {attributeMatch.Value.Attribute.Name} v entite {attributeMatch.Value.Entity.Name} na {renamedAttribute}.",
                new BusinessPatchOperation
                {
                    Op = "update_attribute",
                    EntityId = attributeMatch.Value.Entity.Id,
                    AttributeId = attributeMatch.Value.Attribute.Id,
                    Data =
                    {
                        ["name"] = renamedAttribute,
                    }
                });
        }

        if (TryMatchMoveAttribute(userMessage, out var movedAttributeName, out var sourceEntityForMove, out var targetEntityForMove))
        {
            var attributeMatch = FindAttribute(sourceEntityForMove, movedAttributeName);
            if (attributeMatch is null)
                return CreateAskEnvelope($"Atribut {movedAttributeName} v entite {sourceEntityForMove} jsem nenasel. Co mam presunout?");

            var targetEntity = FindEntity(targetEntityForMove);
            if (targetEntity is null)
                return CreateAskEnvelope($"Cilovou entitu {targetEntityForMove} jsem nenasel. Kam mam atribut presunout?");

            return CreateApplyEnvelope(
                $"Presouvam atribut {attributeMatch.Value.Attribute.Name} z entity {attributeMatch.Value.Entity.Name} do entity {targetEntity.Name}.",
                new BusinessPatchOperation
                {
                    Op = "move_attribute",
                    EntityId = attributeMatch.Value.Entity.Id,
                    AttributeId = attributeMatch.Value.Attribute.Id,
                    Data =
                    {
                        ["targetEntityId"] = targetEntity.Id,
                    }
                });
        }

        if (TryMatchDeleteAttribute(userMessage, out var attributeNameForDelete, out var entityNameForAttributeDelete))
        {
            var attributeMatch = FindAttribute(entityNameForAttributeDelete, attributeNameForDelete);
            if (attributeMatch is null)
                return CreateAskEnvelope($"Atribut {attributeNameForDelete} v entite {entityNameForAttributeDelete} jsem nenasel. Ktery atribut mam smazat?");

            return CreateApplyEnvelope(
                $"Mazu atribut {attributeMatch.Value.Attribute.Name} z entity {attributeMatch.Value.Entity.Name}.",
                new BusinessPatchOperation
                {
                    Op = "delete_attribute",
                    EntityId = attributeMatch.Value.Entity.Id,
                    AttributeId = attributeMatch.Value.Attribute.Id,
                });
        }

        if (TryMatchSetAttributeType(userMessage, out var attributeNameForType, out var entityNameForType, out var targetType))
        {
            var attributeMatch = FindAttribute(entityNameForType, attributeNameForType);
            if (attributeMatch is null)
                return CreateAskEnvelope($"Atribut {attributeNameForType} v entite {entityNameForType} jsem nenasel. Kteremu atributu mam zmenit typ?");

            return CreateApplyEnvelope(
                $"Nastavuji typu atributu {attributeMatch.Value.Attribute.Name} v entite {attributeMatch.Value.Entity.Name} na {targetType}.",
                new BusinessPatchOperation
                {
                    Op = "update_attribute",
                    EntityId = attributeMatch.Value.Entity.Id,
                    AttributeId = attributeMatch.Value.Attribute.Id,
                    Data =
                    {
                        ["type"] = targetType,
                    }
                });
        }

        if (TryMatchTypeCorrection(userMessage, out var correctedAttributeName, out var currentTypeHint, out var correctedTargetType))
        {
            var attributeMatches = FindAttributeMatches(correctedAttributeName)
                .Where(match => string.IsNullOrWhiteSpace(currentTypeHint)
                    || string.Equals(match.Attribute.Type, currentTypeHint, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (attributeMatches.Count == 0)
            {
                return CreateAskEnvelope($"Atribut {correctedAttributeName} jsem pro zmenu typu nenasel. Upresni prosim entitu nebo nazev atributu.");
            }

            if (attributeMatches.Count > 1)
            {
                var entityNames = string.Join(", ", attributeMatches.Select(match => match.Entity.Name).Distinct(StringComparer.OrdinalIgnoreCase));
                return CreateAskEnvelope($"Atribut {correctedAttributeName} je ve vice entitach ({entityNames}). Upresni prosim, ktere entite mam typ zmenit.");
            }

            var attributeMatch = attributeMatches[0];
            return CreateApplyEnvelope(
                $"Menim typ atributu {attributeMatch.Attribute.Name} v entite {attributeMatch.Entity.Name} z {attributeMatch.Attribute.Type} na {correctedTargetType}.",
                new BusinessPatchOperation
                {
                    Op = "update_attribute",
                    EntityId = attributeMatch.Entity.Id,
                    AttributeId = attributeMatch.Attribute.Id,
                    Data =
                    {
                        ["type"] = correctedTargetType,
                    }
                });
        }

        if (TryMatchAddBehavior(userMessage, out var behaviorName, out var targetEntityNameForBehavior))
        {
            var entity = FindEntity(targetEntityNameForBehavior);
            if (entity is null)
                return CreateAskEnvelope($"Entitu {targetEntityNameForBehavior} jsem nenasel. Do ktere entity mam behavior pridat?");

            return CreateApplyEnvelope(
                $"Pridavam behavior {behaviorName} do entity {entity.Name}.",
                new BusinessPatchOperation
                {
                    Op = "add_behavior",
                    EntityId = entity.Id,
                    Data =
                    {
                        ["name"] = behaviorName,
                        ["kind"] = BusinessBehaviorKind.Command.ToString(),
                    }
                });
        }

        return null;
    }

    private BusinessEntityNode? FindEntity(string entityNameOrId)
    {
        return _writeService.CurrentWriteDocument.Entities.FirstOrDefault(entity =>
            string.Equals(entity.Id, entityNameOrId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Name, entityNameOrId, StringComparison.OrdinalIgnoreCase));
    }

    private (BusinessEntityNode Entity, BusinessAttributeNode Attribute)? FindAttribute(string entityNameOrId, string attributeNameOrId)
    {
        var entity = FindEntity(entityNameOrId);
        if (entity is null)
        {
            return null;
        }

        var attribute = entity.Attributes.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, attributeNameOrId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.Name, attributeNameOrId, StringComparison.OrdinalIgnoreCase));

        return attribute is null ? null : (entity, attribute);
    }

    private List<(BusinessEntityNode Entity, BusinessAttributeNode Attribute)> FindAttributeMatches(string attributeNameOrId)
    {
        return _writeService.CurrentWriteDocument.Entities
            .SelectMany(entity => entity.Attributes.Select(attribute => (Entity: entity, Attribute: attribute)))
            .Where(match => string.Equals(match.Attribute.Id, attributeNameOrId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(match.Attribute.Name, attributeNameOrId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static AuthoringResponseEnvelope CopyEnvelope(
        AuthoringResponseEnvelope source,
        AuthoringResponseMode mode,
        string? assistantMessage = null)
    {
        return new AuthoringResponseEnvelope
        {
            Mode = mode,
            AssistantMessage = assistantMessage ?? source.AssistantMessage,
            Questions = source.Questions,
            Warnings = source.Warnings,
            Patches = source.Patches,
        };
    }

    private static AuthoringResponseEnvelope CreateAskEnvelope(string question)
    {
        return new AuthoringResponseEnvelope
        {
            Mode = AuthoringResponseMode.Ask,
            AssistantMessage = question,
            Questions = [question],
        };
    }

    private static AuthoringResponseEnvelope CreateApplyEnvelope(string message, params BusinessPatchOperation[] patches)
    {
        return new AuthoringResponseEnvelope
        {
            Mode = AuthoringResponseMode.Apply,
            AssistantMessage = message,
            Patches = patches,
        };
    }

    private static bool TryMatchPrefix(string input, out string value, params string[] prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = input[prefix.Length..].Trim();
                return !string.IsNullOrWhiteSpace(value);
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool TryMatchCreateDraftEntity(string input, out string entityName)
    {
        return TryMatchDraftEntityIntent(input, out entityName, "zaloz mi ", "založ mi ", "vytvor mi ", "vytvoř mi ", "udelaj mi ", "udělej mi ", "create ", "create me ")
            || TryMatchDraftEntityIntent(input, out entityName, "zaloz entitu ", "založ entitu ", "vytvor entitu ", "vytvoř entitu ", "create entity ")
            || TryMatchDraftEntityIntent(input, out entityName, "vytvor mi entitu ", "vytvoř mi entitu ", "udelaj mi entitu ", "udělej mi entitu ");
    }

    private static bool TryMatchDraftEntityIntent(string input, out string entityName, params string[] prefixes)
    {
        if (!TryMatchPrefix(input, out var remainder, prefixes))
        {
            entityName = string.Empty;
            return false;
        }

        entityName = ExtractDraftEntityName(remainder);
        return !string.IsNullOrWhiteSpace(entityName);
    }

    private static string ExtractDraftEntityName(string value)
    {
        var trimmed = TrimValue(value);
        if (string.IsNullOrWhiteSpace(trimmed))
            return string.Empty;

        trimmed = RemoveTrailingDraftNoise(trimmed);

        if (trimmed.StartsWith("entitu ", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("entity ", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("entita ", StringComparison.OrdinalIgnoreCase))
        {
            var firstSpace = trimmed.IndexOf(' ');
            trimmed = firstSpace >= 0 ? trimmed[(firstSpace + 1)..].Trim() : trimmed;
        }

        var firstToken = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstToken))
            return string.Empty;

        return firstToken.Length == 1
            ? firstToken.ToUpperInvariant()
            : char.ToUpperInvariant(firstToken[0]) + firstToken[1..];
    }

    private static string RemoveTrailingDraftNoise(string value)
    {
        var separators = new[]
        {
            " v programovacim jazyce",
            " v programovacím jazyce",
            " v jsonu",
            " v json",
            " v pythonu",
            " v c#",
            " v csharp",
            " v jave",
            " v java",
            " v typescriptu",
            " v typescript",
            " v javascriptu",
            " v javascript",
        };

        foreach (var separator in separators)
        {
            var index = value.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return value[..index].Trim();
        }

        return value;
    }

    private static AuthoringResponseEnvelope CreateDraftEntityEnvelope(string entityName)
    {
        var operation = CreateDraftEntityOperation(entityName);
        return CreateApplyEnvelope(
            $"Zakladam jazykove neutralni JSON draft entity {entityName}. Je to prvni authoring navrh, ktery muzeme dal upravit.",
            operation);
    }

    private static BusinessPatchOperation CreateDraftEntityOperation(string entityName)
    {
        return TryCreateKnownEntityDraft(entityName, out var operation)
            ? operation
            : new BusinessPatchOperation
            {
                Op = "add_entity",
                Data =
                {
                    ["name"] = entityName,
                    ["summary"] = $"Jazykove neutralni authoring entita {entityName}.",
                }
            };
    }

    private static bool TryCreateKnownEntityDraft(string entityName, out BusinessPatchOperation operation)
    {
        if (EqualsAny(entityName, "Auto", "Car", "Vehicle", "Vozidlo"))
        {
            operation = new BusinessPatchOperation
            {
                Op = "add_entity",
                Data =
                {
                    ["name"] = "Auto",
                    ["summary"] = "Jazykove neutralni reprezentace automobilu pro dalsi modelovani.",
                    ["attributes"] = new object?[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["name"] = "Znacka",
                            ["type"] = "text",
                            ["required"] = true,
                            ["summary"] = "Vyrobce nebo znacka vozidla.",
                        },
                        new Dictionary<string, object?>
                        {
                            ["name"] = "Model",
                            ["type"] = "text",
                            ["required"] = true,
                            ["summary"] = "Obchodni nebo produktovy model auta.",
                        },
                        new Dictionary<string, object?>
                        {
                            ["name"] = "RokVyroby",
                            ["type"] = "number",
                            ["required"] = false,
                            ["summary"] = "Rok vyroby nebo uvedeni modelu.",
                        },
                        new Dictionary<string, object?>
                        {
                            ["name"] = "Barva",
                            ["type"] = "text",
                            ["required"] = false,
                            ["summary"] = "Aktualni barva vozidla.",
                        },
                        new Dictionary<string, object?>
                        {
                            ["name"] = "Nastartovano",
                            ["type"] = "bool",
                            ["required"] = true,
                            ["defaultValue"] = "false",
                            ["summary"] = "Stav, jestli je vozidlo nastartovane.",
                        },
                    },
                    ["behaviors"] = new object?[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["name"] = "Nastartovat",
                            ["kind"] = BusinessBehaviorKind.Command.ToString(),
                            ["summary"] = "Zmeni stav auta do rezimu nastartovano.",
                        },
                        new Dictionary<string, object?>
                        {
                            ["name"] = "ZiskatInfo",
                            ["kind"] = BusinessBehaviorKind.Query.ToString(),
                            ["returns"] = "text",
                            ["summary"] = "Vrati souhrnne informace o aute.",
                        },
                    },
                }
            };

            return true;
        }

        operation = null!;
        return false;
    }

    private static bool TryMatchRenameEntity(string input, out string sourceEntityName, out string targetEntityName)
    {
        if (TryMatchSplitPattern(input, out sourceEntityName, out targetEntityName, "prejmenuj entitu ", " na ")
            || TryMatchSplitPattern(input, out sourceEntityName, out targetEntityName, "přejmenuj entitu ", " na ")
            || TryMatchSplitPattern(input, out sourceEntityName, out targetEntityName, "rename entity ", " to "))
        {
            return true;
        }

        sourceEntityName = string.Empty;
        targetEntityName = string.Empty;
        return false;
    }

    private static bool TryMatchDeleteEntity(string input, out string entityName)
    {
        return TryMatchPrefix(input, out entityName, "smaz entitu ", "smaž entitu ", "delete entity ");
    }

    private static bool TryMatchAddAttribute(string input, out string attributeName, out string entityName)
    {
        if (TryMatchSplitPattern(input, out attributeName, out entityName, "pridej atribut ", " do entity ")
            || TryMatchSplitPattern(input, out attributeName, out entityName, "přidej atribut ", " do entity ")
            || TryMatchSplitPattern(input, out attributeName, out entityName, "add attribute ", " to entity "))
        {
            return true;
        }

        attributeName = string.Empty;
        entityName = string.Empty;
        return false;
    }

    private static bool TryMatchRenameAttribute(string input, out string attributeName, out string entityName, out string targetAttributeName)
    {
        if (TryMatchThreePartPattern(input, out attributeName, out entityName, out targetAttributeName, "prejmenuj atribut ", " v entite ", " na ")
            || TryMatchThreePartPattern(input, out attributeName, out entityName, out targetAttributeName, "přejmenuj atribut ", " v entite ", " na ")
            || TryMatchThreePartPattern(input, out attributeName, out entityName, out targetAttributeName, "rename attribute ", " in entity ", " to "))
        {
            return true;
        }

        attributeName = string.Empty;
        entityName = string.Empty;
        targetAttributeName = string.Empty;
        return false;
    }

    private static bool TryMatchMoveAttribute(string input, out string attributeName, out string sourceEntityName, out string targetEntityName)
    {
        if (TryMatchThreePartPattern(input, out attributeName, out sourceEntityName, out targetEntityName, "presun atribut ", " z ", " do ")
            || TryMatchThreePartPattern(input, out attributeName, out sourceEntityName, out targetEntityName, "přesuň atribut ", " z ", " do ")
            || TryMatchThreePartPattern(input, out attributeName, out sourceEntityName, out targetEntityName, "presun ", " z ", " do ")
            || TryMatchThreePartPattern(input, out attributeName, out sourceEntityName, out targetEntityName, "přesuň ", " z ", " do ")
            || TryMatchThreePartPattern(input, out attributeName, out sourceEntityName, out targetEntityName, "move attribute ", " from ", " to "))
        {
            return true;
        }

        attributeName = string.Empty;
        sourceEntityName = string.Empty;
        targetEntityName = string.Empty;
        return false;
    }

    private static bool TryMatchDeleteAttribute(string input, out string attributeName, out string entityName)
    {
        if (TryMatchSplitPattern(input, out attributeName, out entityName, "smaz atribut ", " z entity ")
            || TryMatchSplitPattern(input, out attributeName, out entityName, "smaž atribut ", " z entity ")
            || TryMatchSplitPattern(input, out attributeName, out entityName, "smaz ", " z entity ")
            || TryMatchSplitPattern(input, out attributeName, out entityName, "smaž ", " z entity ")
            || TryMatchSplitPattern(input, out attributeName, out entityName, "delete attribute ", " from entity "))
        {
            return true;
        }

        attributeName = string.Empty;
        entityName = string.Empty;
        return false;
    }

    private static bool TryMatchSetAttributeType(string input, out string attributeName, out string entityName, out string targetType)
    {
        if (TryMatchThreePartPattern(input, out attributeName, out entityName, out targetType, "nastav typ atributu ", " v entite ", " na ")
            || TryMatchThreePartPattern(input, out attributeName, out entityName, out targetType, "set attribute type ", " in entity ", " to "))
        {
            return true;
        }

        attributeName = string.Empty;
        entityName = string.Empty;
        targetType = string.Empty;
        return false;
    }

    private static bool TryMatchTypeCorrection(string input, out string attributeName, out string currentType, out string targetType)
    {
        if (TryMatchFreeThreePartPattern(input, out attributeName, out currentType, out targetType, " nebude ", " ale ")
            || TryMatchFreeThreePartPattern(input, out attributeName, out currentType, out targetType, " není ", " ale ")
            || TryMatchFreeThreePartPattern(input, out attributeName, out currentType, out targetType, " neni ", " ale "))
        {
            return true;
        }

        attributeName = string.Empty;
        currentType = string.Empty;
        targetType = string.Empty;
        return false;
    }

    private static bool TryMatchAddBehavior(string input, out string behaviorName, out string entityName)
    {
        if (TryMatchSplitPattern(input, out behaviorName, out entityName, "pridej behavior ", " do entity ")
            || TryMatchSplitPattern(input, out behaviorName, out entityName, "přidej behavior ", " do entity ")
            || TryMatchSplitPattern(input, out behaviorName, out entityName, "pridej chovani ", " do entity ")
            || TryMatchSplitPattern(input, out behaviorName, out entityName, "přidej chování ", " do entity ")
            || TryMatchSplitPattern(input, out behaviorName, out entityName, "add behavior ", " to entity "))
        {
            return true;
        }

        behaviorName = string.Empty;
        entityName = string.Empty;
        return false;
    }

    private static bool TryMatchThreePartPattern(
        string input,
        out string first,
        out string second,
        out string third,
        string prefix,
        string separator1,
        string separator2)
    {
        if (!input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            first = string.Empty;
            second = string.Empty;
            third = string.Empty;
            return false;
        }

        var remainder = input[prefix.Length..].Trim();
        return TrySplitThreeSegments(remainder, out first, out second, out third, separator1, separator2);
    }

    private static bool TryMatchFreeThreePartPattern(
        string input,
        out string first,
        out string second,
        out string third,
        string separator1,
        string separator2)
    {
        return TrySplitThreeSegments(input.Trim(), out first, out second, out third, separator1, separator2);
    }

    private static bool TrySplitThreeSegments(
        string input,
        out string first,
        out string second,
        out string third,
        string separator1,
        string separator2)
    {
        var separator1Index = input.IndexOf(separator1, StringComparison.OrdinalIgnoreCase);
        if (separator1Index < 0)
        {
            first = string.Empty;
            second = string.Empty;
            third = string.Empty;
            return false;
        }

        first = TrimValue(input[..separator1Index]);
        var remainder = input[(separator1Index + separator1.Length)..].Trim();
        var separator2Index = remainder.IndexOf(separator2, StringComparison.OrdinalIgnoreCase);
        if (separator2Index < 0)
        {
            first = string.Empty;
            second = string.Empty;
            third = string.Empty;
            return false;
        }

        second = TrimValue(remainder[..separator2Index]);
        third = TrimValue(remainder[(separator2Index + separator2.Length)..]);
        return !string.IsNullOrWhiteSpace(first)
            && !string.IsNullOrWhiteSpace(second)
            && !string.IsNullOrWhiteSpace(third);
    }

    private static bool TryMatchSplitPattern(string input, out string left, out string right, string prefix, string separator)
    {
        if (!input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            left = string.Empty;
            right = string.Empty;
            return false;
        }

        var remainder = input[prefix.Length..].Trim();
        var separatorIndex = remainder.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
        if (separatorIndex < 0)
        {
            left = string.Empty;
            right = string.Empty;
            return false;
        }

        left = remainder[..separatorIndex].Trim();
        right = remainder[(separatorIndex + separator.Length)..].Trim();
        return !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right);
    }

    private static bool EqualsAny(string value, params string[] candidates)
    {
        return candidates.Any(candidate => value.Equals(candidate, StringComparison.OrdinalIgnoreCase));
    }

    private static string TrimValue(string value)
    {
        return value.Trim().Trim('.', ',', ';', ':');
    }

    private static IShadowCommandStore? ResolveShadowCommandStore(
        AuthoringConversationConfiguration configuration,
        IShadowCommandStore? shadowCommandStore)
    {
        if (!configuration.ShadowLog.Enabled)
            return null;

        return shadowCommandStore ?? new JsonlShadowCommandStore(configuration.GetResolvedShadowLogPath());
    }

    private static IProjectionQueryService? ResolveProjectionQueryService(
        AuthoringConversationConfiguration configuration,
        IShadowCommandStore? shadowCommandStore,
        IProjectionQueryService? projectionQueryService)
    {
        if (!configuration.ShadowLog.Enabled)
            return null;

        return projectionQueryService ?? new ReplayProjectionQueryService(
            new JsonlShadowCommandReader(shadowCommandStore?.FilePath ?? configuration.GetResolvedShadowLogPath()));
    }

    private BusinessAuthoringDocument GetCurrentReadDocument()
    {
        if (_writeService.ProjectionQueryService is null)
            return _writeService.CurrentWriteDocument;

        return _writeService.GetCurrentProjectionView().Document;
    }

    private ConversationTurnResult CreateResult(
        bool success,
        string assistantMessage,
        AuthoringResponseMode mode = AuthoringResponseMode.Answer,
        IReadOnlyList<string>? warnings = null,
        IReadOnlyList<string>? questions = null,
        IReadOnlyList<BusinessValidationIssue>? issues = null,
        int appliedOperationCount = 0)
    {
        return new ConversationTurnResult
        {
            Success = success,
            Mode = mode,
            AssistantMessage = assistantMessage,
            Tree = GetCurrentTree(),
            ReadDocumentJson = GetCurrentReadDocumentJson(),
            WriteDocumentJson = GetCurrentWriteDocumentJson(),
            PersistedDocumentPath = PersistedDocumentPath,
            TreeDetailLevel = TreeDetailLevel,
            Warnings = warnings ?? [],
            Questions = questions ?? [],
            Issues = issues ?? [],
            AppliedOperationCount = appliedOperationCount,
            PendingBriefJson = _pendingBrief is not null ? JsonSerializer.Serialize(_pendingBrief) : null,
        };
    }

    private static string GetDefaultModeMessage(AuthoringResponseMode mode, bool success)
    {
        return mode switch
        {
            AuthoringResponseMode.Ask => "Je potreba doplnit dalsi informace.",
            AuthoringResponseMode.Propose => "Navrh zmen je pripraven k potvrzeni.",
            AuthoringResponseMode.Apply when success => "Zmeny byly aplikovany.",
            AuthoringResponseMode.Apply => "Zmeny se nepodarilo aplikovat.",
            _ => success ? "Odpoved byla pripravena." : "Pozadavek se nepodarilo zpracovat.",
        };
    }

    private sealed class DisabledAuthoringConversationAiClient : IAuthoringConversationAiClient
    {
        public bool IsAvailable => false;

        public Task<ConversationAiResult?> CompleteConversationAsync(
            ConversationPromptRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<ConversationAiResult?>(null);
        }
    }
}