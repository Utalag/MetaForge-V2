using System.Text.Json;
using MetaForge.Ai.Configuration;
using MetaForge.BusinessModel;
using MetaForge.BusinessModel.Catalog;
using MetaForge.Core;
using MetaForge.Core.Catalog;
using MetaForge.Core.Common;
using MetaForge.Core.Discovery;
using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Dto;
using MetaForge.Generators.ForgeBlockPackages;
using MetaForge.Translator.Telemetry;
using MetaForge.Translator.Trace;

namespace MetaForge.Translator;

/// <summary>
/// Sdilena host facade pro business authoring nad command/query backend vrstvou.
/// Umoznuje tenkym hostum typu CLI a budouci MCP server pouzivat stejny read/write a exportni tok.
/// </summary>
public sealed class BusinessAuthoringHostFacade
{
    private static readonly JsonSerializerOptions BehaviorInputsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CatalogManager _catalogManager;
    private readonly AuthoringConversationService _conversationService;
    private readonly IDiscoverySession _discoverySession;
    private readonly ForgeBlockPackageRegistry _forgeBlockPackageRegistry;
    private readonly IBusinessTranslator _translator;
    private readonly NodePresetSuggester _nodePresetSuggester;
    private readonly IExecutionTraceRecorder _traceRecorder;
    private NodeAssistService? _nodeAssistService;
    private ProjectionReadService? _projectionReadService;

    public static IReadOnlyList<BusinessAuthoringAiApiKeyPromptRequest> GetMissingAiApiKeyPromptRequests(string? aiConfigurationPath = null)
    {
        var configuration = AiPlatformConfiguration.Load(aiConfigurationPath);

        return AuthoringAiHealthProbe
            .GetMissingApiKeyPromptRequests(configuration)
            .Select(request => new BusinessAuthoringAiApiKeyPromptRequest(
                request.Name,
                request.ApiKeyEnvironmentVariable))
            .ToArray();
    }

    public static async Task<IReadOnlyList<BusinessAuthoringAiHealthStatus>> ProbeAiHealthAsync(
        string? aiConfigurationPath = null,
        CancellationToken cancellationToken = default,
        CommandSource source = CommandSource.Cli)
    {
        var hostTag = ResolveHostTag(source);

        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.AiHealthDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag));

        var configuration = AiPlatformConfiguration.Load(aiConfigurationPath);
        var statuses = await AuthoringAiHealthProbe.ProbeAsync(configuration, cancellationToken: cancellationToken);

        var allOk = statuses.All(s => !s.IsEnabled || s.IsOk);
        MetaForgeTelemetry.AiHealthProbes.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryHelper.ResolveResultTag(allOk)));

        return statuses
            .Select(status => new BusinessAuthoringAiHealthStatus(
                status.Name,
                status.IsOk,
                status.IsEnabled,
                status.RequiresApiKey,
                status.HasApiKeyConfigured,
                status.ApiKeyEnvironmentVariable))
            .ToArray();
    }

    public static async Task<IReadOnlyList<BusinessAuthoringAiSegmentStatus>> GetAiSegmentStatusesAsync(
        string? aiConfigurationPath = null,
        CancellationToken cancellationToken = default,
        CommandSource source = CommandSource.Cli)
    {
        var configuration = AiPlatformConfiguration.Load(aiConfigurationPath);
        var healthStatuses = await ProbeAiHealthAsync(aiConfigurationPath, cancellationToken, source);

        return
        [
            CreateAiSegmentStatus("AI-Conversation", AiSegment.Conversation, configuration, healthStatuses),
            CreateAiSegmentStatus("AI-AuthoringTranslation", AiSegment.AuthoringTranslation, configuration, healthStatuses),
            CreateAiSegmentStatus("AI-NodeAssist", AiSegment.NodeAssist, configuration, healthStatuses),
        ];
    }

    public BusinessAuthoringHostFacade(string? configurationPath = null)
    {
        var configuration = AuthoringConversationConfiguration.Load(configurationPath);
        _forgeBlockPackageRegistry = CreateBuiltInForgeBlockPackageRegistry();
        _catalogManager = CreateCatalogManager(_forgeBlockPackageRegistry);
        _discoverySession = new DefaultDiscoverySession(_catalogManager, _forgeBlockPackageRegistry);
        _conversationService = new AuthoringConversationService(configuration, discoverySession: _discoverySession);
        _translator = new DefaultBusinessTranslator(_catalogManager);
        _nodePresetSuggester = new NodePresetSuggester(_catalogManager);
        _traceRecorder = new OtelExecutionTraceRecorder(MetaForgeTelemetry.ActivitySource);
    }

    internal BusinessAuthoringHostFacade(AuthoringConversationService conversationService, IBusinessTranslator translator, CatalogManager? catalogManager = null, ForgeBlockPackageRegistry? forgeBlockPackageRegistry = null, IDiscoverySession? discoverySession = null, IExecutionTraceRecorder? traceRecorder = null)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        _forgeBlockPackageRegistry = forgeBlockPackageRegistry ?? CreateBuiltInForgeBlockPackageRegistry();
        _catalogManager = catalogManager ?? CreateCatalogManager(_forgeBlockPackageRegistry);
        _discoverySession = discoverySession ?? new DefaultDiscoverySession(_catalogManager, _forgeBlockPackageRegistry);
        _nodePresetSuggester = new NodePresetSuggester(_catalogManager);
        _traceRecorder = traceRecorder ?? new OtelExecutionTraceRecorder(MetaForgeTelemetry.ActivitySource);
    }

    public string PersistedDocumentPath => _conversationService.PersistedDocumentPath;

    public BusinessTreeDetailLevel TreeDetailLevel => _conversationService.TreeDetailLevel;

    public string GetCurrentReadDocumentJson()
    {
        return _conversationService.GetCurrentReadDocumentJson();
    }

    public string GetCurrentWriteDocumentJson()
    {
        return _conversationService.GetCurrentWriteDocumentJson();
    }

    public bool SaveDocumentTo(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = GetCurrentReadDocumentJson();
            File.WriteAllText(filePath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public BusinessAuthoringDocument GetCurrentReadDocument()
    {
        return _conversationService.GetCurrentReadDocumentModel();
    }

    public BusinessAuthoringDocument GetCurrentWriteDocument()
    {
        return _conversationService.CurrentWriteDocument;
    }

    public string GetCurrentTree(BusinessTreeDetailLevel? detailLevel = null)
    {
        return detailLevel.HasValue
            ? BusinessTreeRenderer.Render(GetCurrentReadDocument(), detailLevel.Value)
            : _conversationService.GetCurrentTree();
    }

    public void SetTreeDetailLevel(BusinessTreeDetailLevel detailLevel)
    {
        _conversationService.SetTreeDetailLevel(detailLevel);
    }

    public async Task<string> GetProjectionJsonAsync(
        ProjectionOptions? options = null,
        CancellationToken cancellationToken = default,
        CommandSource source = CommandSource.Cli)
    {
        var effectiveOptions = options ?? ProjectionOptions.Basic();
        var exportKind = effectiveOptions.HasAnyExpertSection ? "expert_json" : "projection_json";
        var hostTag = ResolveHostTag(source);

        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.ExportDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.ExportKind, exportKind));

        var view = await GetProjectionAsync(effectiveOptions, cancellationToken: cancellationToken, source: source);
        var result = effectiveOptions.HasAnyExpertSection
            ? JsonSerializer.Serialize(view.Expert, BehaviorInputsJsonOptions)
            : BusinessDocumentJsonSerializer.Serialize(view.Replay.Document);

        RecordExport(exportKind, source);
        return result;
    }

    public async Task<string> GetProjectionTreeAsync(
        ProjectionOptions? options = null,
        BusinessTreeDetailLevel? detailLevel = null,
        CancellationToken cancellationToken = default,
        CommandSource source = CommandSource.Cli)
    {
        var effectiveOptions = options ?? ProjectionOptions.Basic();
        var exportKind = effectiveOptions.HasAnyExpertSection ? "expert_tree" : "projection_tree";
        var hostTag = ResolveHostTag(source);

        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.ExportDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.ExportKind, exportKind));

        var view = await GetProjectionAsync(effectiveOptions, cancellationToken: cancellationToken, source: source);
        var effectiveDetailLevel = detailLevel ?? TreeDetailLevel;
        var result = effectiveOptions.HasAnyExpertSection
            ? view.Expert is not null ? ExpertProjectionRenderer.Render(view.Expert) : string.Empty
            : BusinessTreeRenderer.Render(view.Replay.Document, effectiveDetailLevel);

        RecordExport(exportKind, source);
        return result;
    }

    public async Task<ProjectionView> GetProjectionAsync(
        ProjectionOptions? options = null,
        string? streamId = null,
        CancellationToken cancellationToken = default,
        CommandSource source = CommandSource.Cli)
    {
        _projectionReadService ??= CreateProjectionReadService();

        var detailTag = ResolveDetailTag(options);
        var hostTag = ResolveHostTag(source);
        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.ProjectionDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Detail, detailTag));

        using var traceScope = _traceRecorder.TraceComponentScope(
            nameof(BusinessAuthoringHostFacade),
            "GetProjectionAsync",
            $"detail={detailTag}");

        try
        {
            var result = await _projectionReadService.GetProjectionAsync(streamId, options, cancellationToken);
            MetaForgeTelemetry.ProjectionRequests.Add(1,
                new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
                new KeyValuePair<string, object?>(TelemetryTags.Detail, detailTag),
                new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultOk));
            return result;
        }
        catch
        {
            MetaForgeTelemetry.ProjectionRequests.Add(1,
                new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
                new KeyValuePair<string, object?>(TelemetryTags.Detail, detailTag),
                new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultError));
            throw;
        }
    }

    public async Task<NodeAssistProposal> AssistNodeAsync(
        NodeAssistRequest request,
        CommandSource source = CommandSource.Cli,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var hostTag = ResolveHostTag(source);
        var detailTag = request.IncludeDiscovery ? "with_discovery" : "basic";

        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.NodeAssistDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Detail, detailTag));

        using var traceScope = _traceRecorder.TraceComponentScope(
            nameof(BusinessAuthoringHostFacade),
            "AssistNodeAsync",
            $"entity={request.NodePath.EntityNameOrId},kind={request.NodePath.Kind}");

        try
        {
            var options = ProjectionOptions.NodeAssist(request.IncludeDiscovery);
            var projection = await GetProjectionAsync(options, cancellationToken: cancellationToken, source: source);

            var context = NodeAssistContextBuilder.Build(projection, request.NodePath, request.IncludeDiscovery);

            if (context is null)
            {
                _traceRecorder.RecordFallbackUsed(
                    nameof(BusinessAuthoringHostFacade),
                    "NodeAssistContextNull",
                    "Node not found, ambiguous, or unsupported kind.");

                MetaForgeTelemetry.NodeAssistRequests.Add(1,
                    new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
                    new KeyValuePair<string, object?>(TelemetryTags.Detail, detailTag),
                    new KeyValuePair<string, object?>(TelemetryTags.Result, "not_found"));

                return new NodeAssistProposal
                {
                    Success = false,
                    FailureReason = "Node not found, ambiguous, or unsupported kind.",
                    UserPrompt = request.UserPrompt,
                };
            }

            NodeAssistResult? aiResult = null;
            var nodeAssistService = _nodeAssistService ??= CreateNodeAssistService();
            if (nodeAssistService is not null && nodeAssistService.IsAvailable)
            {
                try
                {
                    aiResult = await nodeAssistService.AssistAsync(context, request.UserPrompt, cancellationToken);
                    MetaForgeTelemetry.NodeAssistAiCalls.Add(1,
                        new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
                        new KeyValuePair<string, object?>(TelemetryTags.Result, aiResult is not null ? TelemetryTags.ResultOk : "empty"));
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    MetaForgeTelemetry.NodeAssistAiCalls.Add(1,
                        new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
                        new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultError));

                    _traceRecorder.RecordFallbackUsed(
                        nameof(BusinessAuthoringHostFacade),
                        "AssistNodeAsync_AI_Fallback",
                        $"AI call failed: {ex.Message}");

                    // Graceful fallback — pokracujeme bez AI vysledku
                }
            }
            else
            {
                _traceRecorder.RecordFallbackUsed(
                    nameof(BusinessAuthoringHostFacade),
                    "AssistNodeAsync_NoAI",
                    "AI service not available or not configured.");
            }

            MetaForgeTelemetry.NodeAssistRequests.Add(1,
                new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
                new KeyValuePair<string, object?>(TelemetryTags.Detail, detailTag),
                new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultOk));

            return new NodeAssistProposal
            {
                Success = true,
                Context = context,
                UserPrompt = request.UserPrompt,
                AiResult = aiResult,
            };
        }
        catch
        {
            MetaForgeTelemetry.NodeAssistRequests.Add(1,
                new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
                new KeyValuePair<string, object?>(TelemetryTags.Detail, detailTag),
                new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultError));
            throw;
        }
    }

    /// <summary>
    /// Explicitně aplikuje AI-generované operace pro node-level asistenci.
    /// Všechny operace musí být povoleného typu a mířit do cílové entity.
    /// </summary>
    public ConversationTurnResult ApplyNodeAssistOperations(
        NodePath nodePath,
        IReadOnlyList<BusinessPatchOperation> operations,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentNullException.ThrowIfNull(nodePath);
        ArgumentNullException.ThrowIfNull(operations);

        if (operations.Count == 0)
            throw new ArgumentException("Seznam operaci nesmi byt prazdny.", nameof(operations));

        var hostTag = ResolveHostTag(source);
        var targetEntity = ResolveEntity(nodePath.EntityNameOrId);

        if (!NodeAssistOperationValidator.Validate(operations, targetEntity))
        {
            MetaForgeTelemetry.NodeAssistApplyRejected.Add(1,
                new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag));
            throw new InvalidOperationException(
                "Jedna nebo vice operaci neni povolena, nebo mimo cilovou entitu.");
        }

        var normalizedOps = NodeAssistOperationValidator.NormalizeEntityIds(operations, targetEntity);

        var assistantMessage = $"Aplikuji {normalizedOps.Count} operaci navrzenych asistentem pro {nodePath.NodeNameOrId ?? nodePath.EntityNameOrId}.";

        MetaForgeTelemetry.NodeAssistApplyRequests.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Detail, $"ops:{normalizedOps.Count}"));
        MetaForgeTelemetry.NodeAssistApplyOperations.Add(normalizedOps.Count,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag));

        return ApplyOperations("node-assist-apply", normalizedOps, source, assistantMessage);
    }

    private ProjectionReadService CreateProjectionReadService()
    {
        var queryService = _conversationService.ProjectionQueryService;
        if (queryService is null)
        {
            // Shadow log neni zapnut — expert projection bude stavena z read dokumentu
            return new ProjectionReadService(new ReadDocumentProjectionAdapter(_conversationService), _catalogManager, _discoverySession);
        }
        return new ProjectionReadService(queryService, _catalogManager, _discoverySession);
    }

    public async Task<ConversationTurnResult> ProcessMessageAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.AuthoringTurnDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, TelemetryTags.HostChat),
            new KeyValuePair<string, object?>(TelemetryTags.Source, TelemetryTags.SourceChat));

        using var traceScope = _traceRecorder.TraceComponentScope(
            nameof(BusinessAuthoringHostFacade),
            "ProcessMessageAsync",
            $"userMessage={userMessage}");

        var result = await _conversationService.ProcessMessageAsync(userMessage, cancellationToken);

        _traceRecorder.RecordDecisionEvaluated(
            nameof(BusinessAuthoringHostFacade),
            "conversation_result",
            ["ok", "error"],
            result.Success ? "ok" : "error",
            isPredetermined: false);

        if (!result.Success && result.Issues.Count > 0)
        {
            _traceRecorder.RecordErrorPathTriggered(
                nameof(BusinessAuthoringHostFacade),
                "ConversationError",
                string.Join("; ", result.Issues.Select(i => i.Message)));
        }

        var resultTag = TelemetryHelper.ResolveResultTag(result.Success, result.Issues);
        MetaForgeTelemetry.AuthoringTurns.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, TelemetryTags.HostChat),
            new KeyValuePair<string, object?>(TelemetryTags.Source, TelemetryTags.SourceChat),
            new KeyValuePair<string, object?>(TelemetryTags.Result, resultTag));

        return result;
    }

    public void ResetDocument(string projectName = "NewProject", CommandSource source = CommandSource.Cli)
    {
        var sourceTag = ResolveSourceTag(source);
        var hostTag = ResolveHostTag(source);

        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.AuthoringOperationDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Command, "reset-document"),
            new KeyValuePair<string, object?>(TelemetryTags.Source, sourceTag));

        _conversationService.ResetDocument(projectName);

        MetaForgeTelemetry.AuthoringOperations.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Command, "reset-document"),
            new KeyValuePair<string, object?>(TelemetryTags.Source, sourceTag),
            new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultOk));
    }

    public bool TryLoadDocument(string filePath)
    {
        return _conversationService.TryLoadDocument(filePath);
    }

    public ConversationTurnResult ApplyOperations(
        string commandName,
        IReadOnlyList<BusinessPatchOperation> operations,
        CommandSource source = CommandSource.Cli,
        string? assistantMessage = null,
        IReadOnlyList<string>? warnings = null,
        IReadOnlyList<string>? questions = null)
    {
        var sourceTag = ResolveSourceTag(source);
        var hostTag = ResolveHostTag(source);

        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.AuthoringOperationDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Command, commandName),
            new KeyValuePair<string, object?>(TelemetryTags.Source, sourceTag));

        using var traceScope = _traceRecorder.TraceComponentScope(
            nameof(BusinessAuthoringHostFacade),
            "ApplyOperations",
            $"command={commandName},operations={operations.Count}");

        var result = _conversationService.ApplyExplicitOperations(commandName, operations, source, assistantMessage, warnings, questions);

        _traceRecorder.RecordDecisionEvaluated(
            nameof(BusinessAuthoringHostFacade),
            "operation_result",
            ["ok", "error", "validation_error"],
            result.Success ? "ok" : "error",
            isPredetermined: false);

        var resultTag = TelemetryHelper.ResolveResultTag(result.Success, result.Issues);
        MetaForgeTelemetry.AuthoringOperations.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Command, commandName),
            new KeyValuePair<string, object?>(TelemetryTags.Source, sourceTag),
            new KeyValuePair<string, object?>(TelemetryTags.Result, resultTag));

        return result;
    }

    public ConversationTurnResult SetProject(
        string name,
        string? description = null,
        string? icon = null,
        int? version = null,
        CommandSource source = CommandSource.Cli,
        bool clearDescription = false,
        bool clearIcon = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var operation = new BusinessPatchOperation
        {
            Op = "set_project",
        };

        operation.Data["name"] = name;

        SetOptionalStringValue(operation.Data, "description", string.IsNullOrWhiteSpace(description) ? null : description, clearDescription, "project description");
        SetOptionalStringValue(operation.Data, "icon", string.IsNullOrWhiteSpace(icon) ? null : icon, clearIcon, "project icon");

        if (version.HasValue)
            operation.Data["version"] = version.Value;

        return ApplyOperations("set-project", [operation], source, $"Nastavuji projekt {name}.");
    }

    public ConversationTurnResult AddEntity(
        string name,
        string? summary = null,
        string? icon = null,
        string? presetId = null,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var operation = new BusinessPatchOperation
        {
            Op = "add_entity",
        };

        operation.Data["name"] = name;

        if (!string.IsNullOrWhiteSpace(summary))
            operation.Data["summary"] = summary;

        if (!string.IsNullOrWhiteSpace(icon))
            operation.Data["icon"] = icon;

        if (!string.IsNullOrWhiteSpace(presetId))
            operation.Data["presetId"] = presetId;

        return ApplyOperations("add-entity", [operation], source, $"Pridavam entitu {name}.");
    }

    public ConversationTurnResult DeleteEntity(string entityNameOrId, CommandSource source = CommandSource.Cli)
    {
        var entity = ResolveEntity(entityNameOrId);
        return ApplyOperations(
            "delete-entity",
            [new BusinessPatchOperation
            {
                Op = "delete_entity",
                EntityId = entity.Id,
            }],
            source,
            $"Mazu entitu {entity.Name}.");
    }

    public ConversationTurnResult RenameEntity(
        string entityNameOrId,
        string newName,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        var entity = ResolveEntity(entityNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "update_entity",
            EntityId = entity.Id,
        };

        operation.Data["name"] = newName;

        return ApplyOperations(
            "rename-entity",
            [operation],
            source,
            $"Prejmenovavam entitu {entity.Name} na {newName}.");
    }

    public ConversationTurnResult UpdateEntity(
        string entityNameOrId,
        string? newName = null,
        string? summary = null,
        string? icon = null,
        string? presetId = null,
        CommandSource source = CommandSource.Cli,
        bool clearSummary = false,
        bool clearIcon = false,
        bool clearPresetId = false)
    {
        if (string.IsNullOrWhiteSpace(newName)
            && summary is null
            && icon is null
            && presetId is null
            && !clearSummary
            && !clearIcon
            && !clearPresetId)
        {
            throw new InvalidOperationException("Update entity vyzaduje aspon jednu zmenu.");
        }

        var entity = ResolveEntity(entityNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "update_entity",
            EntityId = entity.Id,
        };

        if (!string.IsNullOrWhiteSpace(newName))
            operation.Data["name"] = newName;

        SetOptionalStringValue(operation.Data, "summary", summary, clearSummary, "entity summary");
        SetOptionalStringValue(operation.Data, "icon", icon, clearIcon, "entity icon");
        SetOptionalStringValue(operation.Data, "presetId", presetId, clearPresetId, "entity preset");

        return ApplyOperations(
            "update-entity",
            [operation],
            source,
            $"Aktualizuji entitu {entity.Name}.");
    }

    private static readonly IReadOnlySet<string> BuiltInTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "text", "int", "decimal", "bool", "date", "datetime", "uuid",
    };

    public ConversationTurnResult AddAttribute(
        string entityNameOrId,
        string name,
        string type = "text",
        bool required = false,
        string? summary = null,
        string? defaultValue = null,
        string? customType = null,
        string? constraintsJson = null,
        string? computed = null,
        string? presetId = null,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = ResolveEntity(entityNameOrId);
        var operations = new List<BusinessPatchOperation>();

        // Auto-registrace CustomType (blok F) — nejprve, aby byl pri ApplyAddAttribute k dispozici
        var effectiveType = string.IsNullOrWhiteSpace(customType) ? type : customType;
        if (!BuiltInTypes.Contains(effectiveType) && FindCustomTypeByName(effectiveType) is null)
        {
            operations.Add(new BusinessPatchOperation
            {
                Op = "add_customtype",
                Data =
                {
                    ["name"] = effectiveType,
                    ["underlyingType"] = type,
                    ["source"] = "inferred",
                },
            });
        }

        var operation = new BusinessPatchOperation
        {
            Op = "add_attribute",
            EntityId = entity.Id,
        };

        operation.Data["name"] = name;
        operation.Data["type"] = string.IsNullOrWhiteSpace(type) ? "text" : type;

        if (required)
            operation.Data["required"] = true;

        if (!string.IsNullOrWhiteSpace(summary))
            operation.Data["summary"] = summary;

        if (!string.IsNullOrWhiteSpace(defaultValue))
            operation.Data["defaultValue"] = defaultValue;

        if (!string.IsNullOrWhiteSpace(customType))
            operation.Data["customType"] = customType;

        if (!string.IsNullOrWhiteSpace(constraintsJson))
            operation.Data["constraints"] = ParseStringListJson(constraintsJson, nameof(constraintsJson), "Attribute constraints");

        if (computed is not null)
            operation.Data["computed"] = computed;

        if (presetId is not null)
            operation.Data["presetId"] = presetId;

        operations.Add(operation);

        var result = ApplyOperations(
            "add-attribute",
            operations,
            source,
            $"Pridavam atribut {name} do entity {entity.Name}.");

        if (result.Success && presetId is null && _conversationService.AutoApplyPresetsEnabled)
            TryAutoApplyPreset(entity, name);

        return result;
    }

    public ConversationTurnResult DeleteAttribute(
        string entityNameOrId,
        string attributeNameOrId,
        CommandSource source = CommandSource.Cli)
    {
        var match = ResolveAttribute(entityNameOrId, attributeNameOrId);
        return ApplyOperations(
            "delete-attribute",
            [new BusinessPatchOperation
            {
                Op = "delete_attribute",
                EntityId = match.Entity.Id,
                AttributeId = match.Attribute.Id,
            }],
            source,
            $"Mazu atribut {match.Attribute.Name} z entity {match.Entity.Name}.");
    }

    public ConversationTurnResult RenameAttribute(
        string entityNameOrId,
        string attributeNameOrId,
        string newName,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        var match = ResolveAttribute(entityNameOrId, attributeNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "update_attribute",
            EntityId = match.Entity.Id,
            AttributeId = match.Attribute.Id,
        };

        operation.Data["name"] = newName;

        return ApplyOperations(
            "rename-attribute",
            [operation],
            source,
            $"Prejmenovavam atribut {match.Attribute.Name} v entite {match.Entity.Name} na {newName}.");
    }

    public ConversationTurnResult UpdateAttribute(
        string entityNameOrId,
        string attributeNameOrId,
        string? newName = null,
        string? type = null,
        bool? required = null,
        string? summary = null,
        string? defaultValue = null,
        string? customType = null,
        string? constraintsJson = null,
        string? computed = null,
        string? presetId = null,
        CommandSource source = CommandSource.Cli,
        bool clearSummary = false,
        bool clearDefaultValue = false,
        bool clearCustomType = false,
        bool clearConstraints = false,
        bool clearComputed = false,
        bool clearPresetId = false)
    {
        if (string.IsNullOrWhiteSpace(newName)
            && type is null
            && !required.HasValue
            && summary is null
            && defaultValue is null
            && customType is null
            && constraintsJson is null
            && computed is null
            && presetId is null
            && !clearSummary
            && !clearDefaultValue
            && !clearCustomType
            && !clearConstraints
            && !clearComputed
            && !clearPresetId)
        {
            throw new InvalidOperationException("Update attribute vyzaduje aspon jednu zmenu.");
        }

        var match = ResolveAttribute(entityNameOrId, attributeNameOrId);
        var operations = new List<BusinessPatchOperation>();

        // Auto-registrace CustomType (blok F) — nejprve, aby byl pri update k dispozici
        var effectiveNewType = customType ?? (type ?? match.Attribute.CustomType ?? match.Attribute.Type);
        if (!BuiltInTypes.Contains(effectiveNewType) && FindCustomTypeByName(effectiveNewType) is null)
        {
            operations.Add(new BusinessPatchOperation
            {
                Op = "add_customtype",
                Data =
                {
                    ["name"] = effectiveNewType,
                    ["underlyingType"] = type ?? match.Attribute.Type,
                    ["source"] = "inferred",
                },
            });
        }

        var operation = new BusinessPatchOperation
        {
            Op = "update_attribute",
            EntityId = match.Entity.Id,
            AttributeId = match.Attribute.Id,
        };

        if (!string.IsNullOrWhiteSpace(newName))
            operation.Data["name"] = newName;

        if (type is not null)
            operation.Data["type"] = type;

        if (required.HasValue)
            operation.Data["required"] = required.Value;

        SetOptionalStringValue(operation.Data, "summary", summary, clearSummary, "attribute summary");
        SetOptionalStringValue(operation.Data, "defaultValue", defaultValue, clearDefaultValue, "attribute default value");
        SetOptionalStringValue(operation.Data, "customType", customType, clearCustomType, "attribute custom type");
        SetOptionalStringListValue(operation.Data, "constraints", constraintsJson, clearConstraints, nameof(constraintsJson), "Attribute constraints", "attribute constraints");
        SetOptionalStringValue(operation.Data, "computed", computed, clearComputed, "attribute computed expression");
        SetOptionalStringValue(operation.Data, "presetId", presetId, clearPresetId, "attribute preset");

        operations.Add(operation);

        return ApplyOperations(
            "update-attribute",
            operations,
            source,
            $"Aktualizuji atribut {match.Attribute.Name} v entite {match.Entity.Name}.");
    }

    public ConversationTurnResult MoveAttribute(
        string sourceEntityNameOrId,
        string attributeNameOrId,
        string targetEntityNameOrId,
        int? newIndex = null,
        CommandSource source = CommandSource.Cli)
    {
        var sourceMatch = ResolveAttribute(sourceEntityNameOrId, attributeNameOrId);
        var targetEntity = ResolveEntity(targetEntityNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "move_attribute",
            EntityId = sourceMatch.Entity.Id,
            AttributeId = sourceMatch.Attribute.Id,
            NewIndex = newIndex,
        };

        operation.Data["targetEntityId"] = targetEntity.Id;

        return ApplyOperations(
            "move-attribute",
            [operation],
            source,
            $"Presouvam atribut {sourceMatch.Attribute.Name} z entity {sourceMatch.Entity.Name} do entity {targetEntity.Name}.");
    }

    public ConversationTurnResult AddBehavior(
        string entityNameOrId,
        string name,
        BusinessBehaviorKind kind = BusinessBehaviorKind.Query,
        string? summary = null,
        string? returns = null,
        string? inputsJson = null,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = ResolveEntity(entityNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "add_behavior",
            EntityId = entity.Id,
        };

        operation.Data["name"] = name;
        operation.Data["kind"] = kind.ToString();

        if (!string.IsNullOrWhiteSpace(summary))
            operation.Data["summary"] = summary;

        if (!string.IsNullOrWhiteSpace(returns))
            operation.Data["returns"] = returns;

        if (!string.IsNullOrWhiteSpace(inputsJson))
            operation.Data["inputs"] = ParseBehaviorInputsJson(inputsJson);

        return ApplyOperations(
            "add-behavior",
            [operation],
            source,
            $"Pridavam behavior {name} do entity {entity.Name}.");
    }

    public ConversationTurnResult UpdateBehavior(
        string entityNameOrId,
        string behaviorNameOrId,
        string? newName = null,
        BusinessBehaviorKind? kind = null,
        string? summary = null,
        string? returns = null,
        string? inputsJson = null,
        CommandSource source = CommandSource.Cli,
        bool clearSummary = false,
        bool clearReturns = false,
        bool clearInputs = false)
    {
        if (string.IsNullOrWhiteSpace(newName)
            && !kind.HasValue
            && summary is null
            && returns is null
            && string.IsNullOrWhiteSpace(inputsJson)
            && !clearSummary
            && !clearReturns
            && !clearInputs)
        {
            throw new InvalidOperationException("Update behavior vyzaduje aspon jednu zmenu.");
        }

        var match = ResolveBehavior(entityNameOrId, behaviorNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "update_behavior",
            EntityId = match.Entity.Id,
            BehaviorId = match.Behavior.Id,
        };

        if (!string.IsNullOrWhiteSpace(newName))
            operation.Data["name"] = newName;

        if (kind.HasValue)
            operation.Data["kind"] = kind.Value.ToString();

        SetOptionalStringValue(operation.Data, "summary", summary, clearSummary, "behavior summary");
        SetOptionalStringValue(operation.Data, "returns", returns, clearReturns, "behavior returns");
        SetOptionalBehaviorInputsValue(operation.Data, inputsJson, clearInputs, "behavior inputs");

        return ApplyOperations(
            "update-behavior",
            [operation],
            source,
            $"Aktualizuji behavior {match.Behavior.Name} v entite {match.Entity.Name}.");
    }

    public ConversationTurnResult RenameBehavior(
        string entityNameOrId,
        string behaviorNameOrId,
        string newName,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        var match = ResolveBehavior(entityNameOrId, behaviorNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "update_behavior",
            EntityId = match.Entity.Id,
            BehaviorId = match.Behavior.Id,
        };

        operation.Data["name"] = newName;

        return ApplyOperations(
            "rename-behavior",
            [operation],
            source,
            $"Prejmenovavam behavior {match.Behavior.Name} v entite {match.Entity.Name} na {newName}.");
    }

    public ConversationTurnResult DeleteBehavior(
        string entityNameOrId,
        string behaviorNameOrId,
        CommandSource source = CommandSource.Cli)
    {
        var match = ResolveBehavior(entityNameOrId, behaviorNameOrId);
        return ApplyOperations(
            "delete-behavior",
            [new BusinessPatchOperation
            {
                Op = "delete_behavior",
                EntityId = match.Entity.Id,
                BehaviorId = match.Behavior.Id,
            }],
            source,
            $"Mazu behavior {match.Behavior.Name} z entity {match.Entity.Name}.");
    }

    public ConversationTurnResult AddRelation(
        string sourceEntityNameOrId,
        string targetEntityNameOrId,
        BusinessRelationKind kind = BusinessRelationKind.BelongsTo,
        string? sourceNavigationName = null,
        string? targetNavigationName = null,
        CommandSource source = CommandSource.Cli)
    {
        var sourceEntity = ResolveEntity(sourceEntityNameOrId);
        var targetEntity = ResolveEntity(targetEntityNameOrId);

        var operation = new BusinessPatchOperation
        {
            Op = "add_relation",
        };

        operation.Data["sourceEntityId"] = sourceEntity.Id;
        operation.Data["targetEntityId"] = targetEntity.Id;
        operation.Data["kind"] = kind.ToString();

        if (!string.IsNullOrWhiteSpace(sourceNavigationName))
            operation.Data["sourceNavigationName"] = sourceNavigationName;

        if (!string.IsNullOrWhiteSpace(targetNavigationName))
            operation.Data["targetNavigationName"] = targetNavigationName;

        return ApplyOperations(
            "add-relation",
            [operation],
            source,
            $"Pridavam relaci {kind} mezi {sourceEntity.Name} a {targetEntity.Name}.");
    }

    public ConversationTurnResult UpdateRelation(
        string sourceEntityNameOrId,
        string targetEntityNameOrId,
        BusinessRelationKind? matchKind = null,
        string? newSourceEntityNameOrId = null,
        string? newTargetEntityNameOrId = null,
        BusinessRelationKind? newKind = null,
        string? sourceNavigationName = null,
        string? targetNavigationName = null,
        CommandSource source = CommandSource.Cli,
        bool clearSourceNavigationName = false,
        bool clearTargetNavigationName = false)
    {
        if (string.IsNullOrWhiteSpace(newSourceEntityNameOrId)
            && string.IsNullOrWhiteSpace(newTargetEntityNameOrId)
            && !newKind.HasValue
            && sourceNavigationName is null
            && targetNavigationName is null
            && !clearSourceNavigationName
            && !clearTargetNavigationName)
        {
            throw new InvalidOperationException("Update relation vyzaduje aspon jednu zmenu.");
        }

        var match = ResolveRelation(sourceEntityNameOrId, targetEntityNameOrId, matchKind);
        var updatedSourceEntity = string.IsNullOrWhiteSpace(newSourceEntityNameOrId)
            ? match.SourceEntity
            : ResolveEntity(newSourceEntityNameOrId);
        var updatedTargetEntity = string.IsNullOrWhiteSpace(newTargetEntityNameOrId)
            ? match.TargetEntity
            : ResolveEntity(newTargetEntityNameOrId);

        var operation = new BusinessPatchOperation
        {
            Op = "update_relation",
            RelationId = match.Relation.Id,
        };

        if (!string.IsNullOrWhiteSpace(newSourceEntityNameOrId))
            operation.Data["sourceEntityId"] = updatedSourceEntity.Id;

        if (!string.IsNullOrWhiteSpace(newTargetEntityNameOrId))
            operation.Data["targetEntityId"] = updatedTargetEntity.Id;

        if (newKind.HasValue)
            operation.Data["kind"] = newKind.Value.ToString();

        SetOptionalStringValue(operation.Data, "sourceNavigationName", sourceNavigationName, clearSourceNavigationName, "relation source navigation");
        SetOptionalStringValue(operation.Data, "targetNavigationName", targetNavigationName, clearTargetNavigationName, "relation target navigation");

        return ApplyOperations(
            "update-relation",
            [operation],
            source,
            $"Aktualizuji relaci mezi {match.SourceEntity.Name} a {match.TargetEntity.Name}. Nove cile jsou {updatedSourceEntity.Name} a {updatedTargetEntity.Name}.");
    }

    public ConversationTurnResult DeleteRelation(
        string sourceEntityNameOrId,
        string targetEntityNameOrId,
        BusinessRelationKind? kind = null,
        CommandSource source = CommandSource.Cli)
    {
        var match = ResolveRelation(sourceEntityNameOrId, targetEntityNameOrId, kind);
        return ApplyOperations(
            "delete-relation",
            [new BusinessPatchOperation
            {
                Op = "delete_relation",
                RelationId = match.Relation.Id,
            }],
            source,
            $"Mazu relaci {match.Relation.Kind} mezi {match.SourceEntity.Name} a {match.TargetEntity.Name}.");
    }

    public ConversationTurnResult AddNote(
        string text,
        string? entityNameOrId = null,
        string? behaviorNameOrId = null,
        string? sourceEntityNameOrId = null,
        string? targetEntityNameOrId = null,
        BusinessRelationKind? relationKind = null,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        string assistantMessage;
        string? entityId = null;
        string? behaviorId = null;
        string? relationId = null;
        var targetsRelation = !string.IsNullOrWhiteSpace(sourceEntityNameOrId)
            || !string.IsNullOrWhiteSpace(targetEntityNameOrId)
            || relationKind.HasValue;

        if (targetsRelation)
        {
            if (!string.IsNullOrWhiteSpace(entityNameOrId) || !string.IsNullOrWhiteSpace(behaviorNameOrId))
                throw new InvalidOperationException("Relation note nelze kombinovat s entity nebo behavior targetem.");

            if (string.IsNullOrWhiteSpace(sourceEntityNameOrId) || string.IsNullOrWhiteSpace(targetEntityNameOrId))
                throw new InvalidOperationException("Relation note vyzaduje source i target entitu.");

            var match = ResolveRelation(sourceEntityNameOrId, targetEntityNameOrId, relationKind);
            relationId = match.Relation.Id;
            assistantMessage = $"Pridavam poznamku k relaci {match.Relation.Kind} mezi {match.SourceEntity.Name} a {match.TargetEntity.Name}.";
        }
        else if (!string.IsNullOrWhiteSpace(behaviorNameOrId))
        {
            if (string.IsNullOrWhiteSpace(entityNameOrId))
                throw new InvalidOperationException("Behavior note vyzaduje i entitu.");

            var match = ResolveBehavior(entityNameOrId, behaviorNameOrId);
            entityId = match.Entity.Id;
            behaviorId = match.Behavior.Id;
            assistantMessage = $"Pridavam poznamku k behavioru {match.Behavior.Name} v entite {match.Entity.Name}.";
        }
        else if (!string.IsNullOrWhiteSpace(entityNameOrId))
        {
            var entity = ResolveEntity(entityNameOrId);
            entityId = entity.Id;
            assistantMessage = $"Pridavam poznamku k entite {entity.Name}.";
        }
        else
        {
            assistantMessage = "Pridavam projektovou poznamku.";
        }

        var operation = new BusinessPatchOperation
        {
            Op = "add_note",
            EntityId = entityId,
            BehaviorId = behaviorId,
            RelationId = relationId,
        };

        operation.Data["text"] = text;

        return ApplyOperations("add-note", [operation], source, assistantMessage);
    }

    public ConversationTurnResult ResolveQuestion(
        string questionIdOrText,
        PendingQuestionStatus status = PendingQuestionStatus.Resolved,
        string? updatedText = null,
        CommandSource source = CommandSource.Cli)
    {
        var question = ResolvePendingQuestion(questionIdOrText);
        var operation = new BusinessPatchOperation
        {
            Op = "resolve_question",
            QuestionId = question.Id,
        };

        operation.Data["status"] = status.ToString();

        if (updatedText is not null)
            operation.Data["text"] = updatedText;

        return ApplyOperations(
            "resolve-question",
            [operation],
            source,
            $"Nastavuji otazku {question.Id} na stav {status}.");
    }

    public string GetDiscoveryHelp(CommandSource source = CommandSource.Cli)
    {
        var hostTag = ResolveHostTag(source);

        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.DiscoveryDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag));

        var result = DiscoveryTextRenderer.Render(ExecuteDiscoveryQuery(string.Empty));

        MetaForgeTelemetry.DiscoveryRequests.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultOk));

        return result;
    }

    public DiscoveryQueryResult QueryDiscovery(string query, CommandSource source = CommandSource.Cli)
    {
        var hostTag = ResolveHostTag(source);

        using var timer = TelemetryHelper.StartDuration(
            MetaForgeTelemetry.DiscoveryDurationMs,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag));

        var result = ExecuteDiscoveryQuery(query);

        MetaForgeTelemetry.DiscoveryRequests.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultOk));

        return result;
    }

    private DiscoveryQueryResult ExecuteDiscoveryQuery(string query)
    {
        var rawQuery = query?.Trim() ?? string.Empty;
        var normalizedQuery = DiscoveryQuery.Parse(rawQuery);
        var session = new DefaultDiscoverySession(_catalogManager, _forgeBlockPackageRegistry);
        DiscoveryQueryResult result;

        if (!normalizedQuery.HasCategory)
        {
            var shortcutItem = session.TryResolveShortcut(rawQuery);
            result = shortcutItem is not null
                ? new DiscoveryQueryResult { Item = shortcutItem }
                : new DiscoveryQueryResult { Root = session.GetRoot() };
        }
        else if (!session.IsKnownCategory(normalizedQuery.Category))
        {
            var shortcutItem = session.TryResolveShortcut(rawQuery);
            result = shortcutItem is not null
                ? new DiscoveryQueryResult { Item = shortcutItem }
                : new DiscoveryQueryResult { Category = session.GetCategory(normalizedQuery) };
        }
        else if (normalizedQuery.HasSubCategory && !normalizedQuery.HasItem)
        {
            var itemCandidate = session.GetItem(new DiscoveryQuery
            {
                Category = normalizedQuery.Category,
                Item = normalizedQuery.SubCategory,
            });
            result = itemCandidate is not null
                ? new DiscoveryQueryResult { Item = itemCandidate }
                : new DiscoveryQueryResult { Category = session.GetCategory(normalizedQuery) };
        }
        else if (normalizedQuery.HasItem)
        {
            var item = session.GetItem(normalizedQuery);
            result = item is not null
                ? new DiscoveryQueryResult { Item = item }
                : new DiscoveryQueryResult { Category = session.GetCategory(normalizedQuery) };
        }
        else
        {
            result = new DiscoveryQueryResult
            {
                Category = session.GetCategory(normalizedQuery)
            };
        }

        return result;
    }

    public IReadOnlyList<DiscoveryItemSummary> SearchByTag(string tag)
    {
        var session = new DefaultDiscoverySession(_catalogManager, _forgeBlockPackageRegistry);
        return session.SearchByTag(tag);
    }

    public AttributeResolution ResolveAttributeType(string type, string? presetId = null, string? attributeName = null)
    {
        var resolution = _catalogManager.ResolveType(type);
        var resolvedType = type;
        string? underlyingType = null;
        string? valueObjectName = null;
        string? catalogId = null;
        var isStrongType = false;
        var candidateRules = Array.Empty<string>();
        var suggestedPresets = Array.Empty<string>();

        if (resolution.IsPrimitive)
        {
            resolvedType = Core.Catalog.CatalogManager.GetPrimitiveName(resolution.Primitive!.Value) ?? type;
        }
        else if (resolution.IsStrongType)
        {
            catalogId = resolution.CatalogId;
            isStrongType = true;

            try
            {
                var catalogItem = _catalogManager.FindById(catalogId!);
                valueObjectName = catalogItem?.DisplayName ?? catalogId;
            }
            catch
            {
                valueObjectName = catalogId;
            }

            try
            {
                var preset = _catalogManager.LoadValueObjectPresetAsync(catalogId!).GetAwaiter().GetResult();
                underlyingType = preset?.Definition?.UnderlyingType;
                resolvedType = valueObjectName ?? catalogId!;

                if (preset?.Definition?.ValidationRules is { Count: > 0 } rules)
                    candidateRules = rules.Select(r => FormatValidationRule(r)).ToArray();
            }
            catch
            {
                // Preset se nepodařilo načíst — použij základní informace
            }
        }

        if (isStrongType && string.IsNullOrWhiteSpace(underlyingType))
            underlyingType = resolvedType;

        var searchName = !string.IsNullOrWhiteSpace(attributeName) ? attributeName : type;
        var suggestions = _catalogManager.SuggestPresets(searchName, type);
        suggestedPresets = suggestions
            .Take(5)
            .Select(s => s.DisplayName ?? s.Id)
            .ToArray();

        return new AttributeResolution
        {
            ResolvedType = resolvedType,
            UnderlyingType = underlyingType,
            IsStrongType = isStrongType,
            ValueObjectName = valueObjectName,
            CatalogId = catalogId,
            CandidateValidationRules = candidateRules,
            SuggestedPresets = suggestedPresets,
        };
    }

    public ConversationTurnResult ResolveAndUpdateAttribute(
        string entityNameOrId,
        string attributeNameOrId,
        string? newName = null,
        string? type = null,
        bool? required = null,
        string? summary = null,
        string? defaultValue = null,
        string? customType = null,
        string? constraintsJson = null,
        string? presetId = null,
        CommandSource source = CommandSource.Cli,
        bool clearSummary = false,
        bool clearDefaultValue = false,
        bool clearCustomType = false,
        bool clearConstraints = false,
        bool clearComputed = false,
        bool clearPresetId = false)
    {
        var updateResult = UpdateAttribute(
            entityNameOrId, attributeNameOrId,
            newName, type, required,
            summary, defaultValue, customType, constraintsJson,
            null, presetId, source,
            clearSummary, clearDefaultValue, clearCustomType,
            clearConstraints, clearComputed, clearPresetId);

        if (!updateResult.Success)
            return updateResult;

        if (type is not null || presetId is not null || constraintsJson is not null || defaultValue is not null)
        {
            var match = ResolveAttribute(entityNameOrId, attributeNameOrId);
            var resolution = ResolveAttributeType(match.Attribute.Type, match.Attribute.PresetId, match.Attribute.Name);

            ApplyEnrichment(
                entityNameOrId, attributeNameOrId,
                new BusinessAttributeCoreDetail
                {
                    Source = CoreInfoSource.Manual,
                    ResolvedPresetId = resolution.CatalogId,
                    ValueObjectName = resolution.ValueObjectName,
                    IsStrongType = resolution.IsStrongType,
                    LastSyncedAt = DateTimeOffset.UtcNow,
                },
                source);
        }

        return updateResult;
    }

    private static string FormatValidationRule(global::MetaForge.Core.Catalog.ValidationRulePreset rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Parameter))
            return rule.RuleType;

        return $"{rule.RuleType}({rule.Parameter})";
    }

    public IReadOnlyList<PresetSearchResult> SearchPresets(string? kind = null, string? searchText = null)
    {
        var results = new List<PresetSearchResult>();

        var catalogItems = _catalogManager.Items;
        foreach (var item in catalogItems)
        {
            if (!string.IsNullOrWhiteSpace(kind) && !string.Equals(item.ItemType.ToString(), kind, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrWhiteSpace(searchText)
                && !(item.DisplayName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                && !(item.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                continue;

            results.Add(new PresetSearchResult(
                item.Id,
                item.DisplayName ?? item.Id,
                item.ItemType.ToString(),
                item.Source ?? "built-in",
                item.Description ?? string.Empty));
        }

        var document = GetCurrentReadDocument();
        foreach (var customType in document.CustomTypes)
        {
            if (!string.IsNullOrWhiteSpace(kind) && !string.Equals("ValueObject", kind, StringComparison.OrdinalIgnoreCase) && !string.Equals("CustomType", kind, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrWhiteSpace(searchText)
                && !customType.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                && !(customType.Summary?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                continue;

            results.Add(new PresetSearchResult(
                customType.Id,
                customType.Name,
                "CustomType",
                "workspace",
                customType.Summary ?? customType.UnderlyingType));
        }

        return results;
    }

    public ConversationTurnResult SaveWorkspacePreset(string name, string forKind, string category, IReadOnlyList<string> tags, string seedJson)
    {
        // Workspace presets are stored as CustomTypeDefinitions
        var document = GetCurrentReadDocument();
        var existing = document.CustomTypes
            .FirstOrDefault(ct => string.Equals(ct.Name, name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return UpdateCustomType(existing.Id, newName: name, summary: $"[{forKind}] {category} — {seedJson[..Math.Min(seedJson.Length, 100)]}", commandSource: CommandSource.Desktop);
        }

        return AddCustomType(name, underlyingType: "string", summary: $"[{forKind}] {category}", source: "workspace", commandSource: CommandSource.Desktop);
    }

    public IReadOnlyList<PresetSearchResult> GetWorkspacePresets(string? kind = null)
    {
        var document = GetCurrentReadDocument();
        return document.CustomTypes
            .Select(ct => new PresetSearchResult(ct.Id, ct.Name, "CustomType", "workspace", ct.Summary ?? ct.UnderlyingType))
            .ToArray();
    }

    public ConversationTurnResult DeleteWorkspacePreset(string presetId)
    {
        return DeleteCustomType(presetId, CommandSource.Desktop);
    }

    public IReadOnlyList<NodePresetSuggestion> SuggestNodePresets(NodeCreateContext context, CommandSource source = CommandSource.Cli)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = _nodePresetSuggester.SuggestForNode(context);

        var hostTag = ResolveHostTag(source);
        MetaForgeTelemetry.NodeAssistPresetsSuggested.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.Result, result.Count > 0 ? "found" : "empty"));

        return result;
    }

    public ConversationTurnResult AddEntities(
        IReadOnlyList<(string Name, string? Summary, string? Icon, string? PresetId)> entities,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
            throw new InvalidOperationException("AddEntities vyzaduje aspon jednu entitu.");

        var operations = new List<BusinessPatchOperation>();
        foreach (var (name, summary, icon, presetId) in entities)
        {
            var operation = new BusinessPatchOperation { Op = "add_entity" };
            operation.Data["name"] = name;
            if (!string.IsNullOrWhiteSpace(summary))
                operation.Data["summary"] = summary;
            if (!string.IsNullOrWhiteSpace(icon))
                operation.Data["icon"] = icon;
            if (!string.IsNullOrWhiteSpace(presetId))
                operation.Data["presetId"] = presetId;
            operations.Add(operation);
        }

        return ApplyOperations(
            "add-entities",
            operations,
            source,
            $"Pridavam {entities.Count} entit najednou.");
    }

    public ConversationTurnResult DeleteEntities(
        IReadOnlyList<string> entityNamesOrIds,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentNullException.ThrowIfNull(entityNamesOrIds);

        if (entityNamesOrIds.Count == 0)
            throw new InvalidOperationException("DeleteEntities vyzaduje aspon jednu entitu.");

        var operations = new List<BusinessPatchOperation>();
        foreach (var nameOrId in entityNamesOrIds)
        {
            var entity = ResolveEntity(nameOrId);
            operations.Add(new BusinessPatchOperation
            {
                Op = "delete_entity",
                EntityId = entity.Id,
            });
        }

        return ApplyOperations(
            "delete-entities",
            operations,
            source,
            $"Mazu {entityNamesOrIds.Count} entit najednou.");
    }

    public ConversationTurnResult AddAttributes(
        string entityNameOrId,
        IReadOnlyList<(string Name, string Type, bool Required, string? Summary, string? DefaultValue, string? CustomType, string? ConstraintsJson, string? Computed, string? PresetId)> attributes,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        if (attributes.Count == 0)
            throw new InvalidOperationException("AddAttributes vyzaduje aspon jeden atribut.");

        var entity = ResolveEntity(entityNameOrId);
        var operations = new List<BusinessPatchOperation>();

        foreach (var (name, type, required, summary, defaultValue, customType, constraintsJson, computed, presetId) in attributes)
        {
            var operation = new BusinessPatchOperation
            {
                Op = "add_attribute",
                EntityId = entity.Id,
            };

            operation.Data["name"] = name;
            operation.Data["type"] = string.IsNullOrWhiteSpace(type) ? "text" : type;
            if (required)
                operation.Data["required"] = true;
            if (!string.IsNullOrWhiteSpace(summary))
                operation.Data["summary"] = summary;
            if (!string.IsNullOrWhiteSpace(defaultValue))
                operation.Data["defaultValue"] = defaultValue;
            if (!string.IsNullOrWhiteSpace(customType))
                operation.Data["customType"] = customType;
            if (!string.IsNullOrWhiteSpace(constraintsJson))
                operation.Data["constraints"] = ParseStringListJson(constraintsJson, nameof(constraintsJson), "Attribute constraints");
            if (computed is not null)
                operation.Data["computed"] = computed;
            if (presetId is not null)
                operation.Data["presetId"] = presetId;

            operations.Add(operation);
        }

        return ApplyOperations(
            "add-attributes",
            operations,
            source,
            $"Pridavam {attributes.Count} atributu do entity {entity.Name} najednou.");
    }

    public MetaForgeTransportDto GetCurrentTransportDto(ProgramLanguage language)
    {
        return _translator.Translate(GetCurrentReadDocument(), language);
    }

    public string GetCurrentTransportDtoJson(ProgramLanguage language)
    {
        return JsonSerializer.Serialize(GetCurrentTransportDto(language));
    }

    public MetaProject GetCurrentCoreProject(ProgramLanguage language)
    {
        return GetCurrentTransportDto(language).ToCore();
    }

    public string GetCurrentCoreProjectJson(ProgramLanguage language)
    {
        return MetaProjectSerializer.ToJson(GetCurrentCoreProject(language));
    }

    public ConversationTurnResult ApplyEnrichment(
        string entityNameOrId,
        string attributeNameOrId,
        BusinessAttributeCoreDetail coreDetail,
        CommandSource source = CommandSource.Cli)
    {
        var match = ResolveAttribute(entityNameOrId, attributeNameOrId);
        return _conversationService.ApplyEnrichment(match.Entity.Id, match.Attribute.Id, coreDetail, source);
    }

    public ConversationTurnResult UpdateCoreDetail(
        string entityNameOrId,
        string attributeNameOrId,
        string? valueObjectName = null,
        bool? isStrongType = null,
        string? resolvedPresetId = null,
        string? source = null,
        CommandSource commandSource = CommandSource.Cli)
    {
        if (valueObjectName is null && !isStrongType.HasValue && resolvedPresetId is null && source is null)
            throw new InvalidOperationException("UpdateCoreDetail vyzaduje aspon jednu zmenu.");

        var match = ResolveAttribute(entityNameOrId, attributeNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "update_coredetail",
            EntityId = match.Entity.Id,
            AttributeId = match.Attribute.Id,
        };

        if (valueObjectName is not null)
            operation.Data["valueObjectName"] = valueObjectName;

        if (isStrongType.HasValue)
            operation.Data["isStrongType"] = isStrongType.Value;

        if (resolvedPresetId is not null)
            operation.Data["resolvedPresetId"] = resolvedPresetId;

        if (source is not null)
            operation.Data["source"] = source;

        return ApplyOperations(
            "update-coredetail",
            [operation],
            commandSource,
            $"Aktualizuji CoreDetail atributu {match.Attribute.Name} v entite {match.Entity.Name}.");
    }

    public ConversationTurnResult ApplyBusinessTemplate(
        CatalogItem templateItem,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentNullException.ThrowIfNull(templateItem);

        if (!CatalogApplicationDispatch.IsBusinessTemplate(templateItem.ItemType))
            throw new InvalidOperationException($"Polozka {templateItem.Id} neni business sablona (typ {templateItem.ItemType}).");

        var operations = CatalogApplicationDispatch.ExtractBusinessOperations(templateItem);

        if (operations.Count == 0)
            throw new InvalidOperationException($"Business sablona {templateItem.Id} neobsahuje zadne operace.");

        return ApplyOperations(
            "apply-business-template",
            operations,
            source,
            $"Aplikuji business sablonu {templateItem.DisplayName} ({operations.Count} operaci).");
    }

    public ConversationTurnResult ApplyBusinessTemplateById(
        string catalogItemId,
        CommandSource source = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogItemId);

        var item = _catalogManager.FindById(catalogItemId)
            ?? throw new InvalidOperationException($"Polozka {catalogItemId} nebyla nalezena v katalogu.");

        return ApplyBusinessTemplate(item, source);
    }

    private BusinessEntityNode ResolveEntity(string entityNameOrId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityNameOrId);

        var entity = GetCurrentReadDocument().Entities.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, entityNameOrId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.Name, entityNameOrId, StringComparison.OrdinalIgnoreCase));

        return entity ?? throw new InvalidOperationException($"Entita {entityNameOrId} nebyla nalezena.");
    }

    private (BusinessEntityNode Entity, BusinessAttributeNode Attribute) ResolveAttribute(string entityNameOrId, string attributeNameOrId)
    {
        var entity = ResolveEntity(entityNameOrId);
        var attribute = entity.Attributes.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, attributeNameOrId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.Name, attributeNameOrId, StringComparison.OrdinalIgnoreCase));

        if (attribute is null)
            throw new InvalidOperationException($"Atribut {attributeNameOrId} v entite {entity.Name} nebyl nalezen.");

        return (entity, attribute);
    }

    private void TryAutoApplyPreset(BusinessEntityNode entity, string attributeName)
    {
        var suggestedPreset = _catalogManager.SuggestPreset(attributeName);
        if (suggestedPreset is null)
            return;

        var addedAttribute = GetCurrentReadDocument().Entities
            .FirstOrDefault(e => string.Equals(e.Id, entity.Id, StringComparison.OrdinalIgnoreCase))
            ?.Attributes.FirstOrDefault(a => string.Equals(a.Name, attributeName, StringComparison.OrdinalIgnoreCase));

        if (addedAttribute is null)
            return;

        ValueObjectPreset? preset;
        try
        {
            preset = _catalogManager.LoadValueObjectPresetAsync(suggestedPreset.Id).GetAwaiter().GetResult();
        }
        catch
        {
            return;
        }

        var presetOperation = new BusinessPatchOperation
        {
            Op = "apply_preset",
            EntityId = entity.Id,
            AttributeId = addedAttribute.Id,
            Data =
            {
                ["presetId"] = suggestedPreset.Id,
                ["source"] = CoreInfoSource.Generated.ToString(),
                ["valueObjectName"] = preset?.Definition.Name,
                ["isStrongType"] = preset?.Definition.Name is not null,
            },
        };

        ApplyOperations(
            "auto-apply-preset",
            [presetOperation],
            CommandSource.System,
            $"Auto-apply preset {suggestedPreset.Id} na atribut {attributeName}.");
    }

    private (BusinessEntityNode Entity, BusinessBehaviorNode Behavior) ResolveBehavior(string entityNameOrId, string behaviorNameOrId)
    {
        var entity = ResolveEntity(entityNameOrId);
        var behavior = entity.Behaviors.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, behaviorNameOrId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.Name, behaviorNameOrId, StringComparison.OrdinalIgnoreCase));

        if (behavior is null)
            throw new InvalidOperationException($"Behavior {behaviorNameOrId} v entite {entity.Name} nebyl nalezen.");

        return (entity, behavior);
    }

    private (BusinessRelationNode Relation, BusinessEntityNode SourceEntity, BusinessEntityNode TargetEntity) ResolveRelation(
        string sourceEntityNameOrId,
        string targetEntityNameOrId,
        BusinessRelationKind? kind = null)
    {
        var sourceEntity = ResolveEntity(sourceEntityNameOrId);
        var targetEntity = ResolveEntity(targetEntityNameOrId);
        var matches = GetCurrentReadDocument().Relations
            .Where(candidate => string.Equals(candidate.SourceEntityId, sourceEntity.Id, StringComparison.OrdinalIgnoreCase)
                && string.Equals(candidate.TargetEntityId, targetEntity.Id, StringComparison.OrdinalIgnoreCase)
                && (!kind.HasValue || candidate.Kind == kind.Value))
            .ToList();

        if (matches.Count == 0)
            throw new InvalidOperationException($"Relace mezi {sourceEntity.Name} a {targetEntity.Name} nebyla nalezena.");

        if (matches.Count > 1)
            throw new InvalidOperationException($"Mezi {sourceEntity.Name} a {targetEntity.Name} existuje vice relaci. Upresni kind.");

        return (matches[0], sourceEntity, targetEntity);
    }

    private PendingQuestionNode ResolvePendingQuestion(string questionIdOrText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionIdOrText);

        var document = GetCurrentReadDocument();
        var idMatch = document.PendingQuestions.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, questionIdOrText, StringComparison.OrdinalIgnoreCase));

        if (idMatch is not null)
            return idMatch;

        var textMatches = document.PendingQuestions
            .Where(candidate => string.Equals(candidate.Text, questionIdOrText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return textMatches.Count switch
        {
            1 => textMatches[0],
            > 1 => throw new InvalidOperationException($"Existuje vice otazek se stejnym textem {questionIdOrText}. Pouzij question id."),
            _ => throw new InvalidOperationException($"Otazka {questionIdOrText} nebyla nalezena."),
        };
    }

    private static void SetOptionalStringValue(IDictionary<string, object?> data, string key, string? value, bool clear, string fieldLabel)
    {
        EnsureExplicitClearDoesNotConflict(value is not null, clear, fieldLabel);

        if (clear)
        {
            data[key] = null;
            return;
        }

        if (value is not null)
            data[key] = value;
    }

    private static void SetOptionalStringListValue(
        IDictionary<string, object?> data,
        string key,
        string? valuesJson,
        bool clear,
        string parameterName,
        string label,
        string fieldLabel)
    {
        EnsureExplicitClearDoesNotConflict(valuesJson is not null, clear, fieldLabel);

        if (clear)
        {
            data[key] = null;
            return;
        }

        if (valuesJson is not null)
            data[key] = ParseStringListJson(valuesJson, parameterName, label);
    }

    private static void SetOptionalBehaviorInputsValue(IDictionary<string, object?> data, string? inputsJson, bool clear, string fieldLabel)
    {
        EnsureExplicitClearDoesNotConflict(!string.IsNullOrWhiteSpace(inputsJson), clear, fieldLabel);

        if (clear)
        {
            data["inputs"] = null;
            return;
        }

        if (!string.IsNullOrWhiteSpace(inputsJson))
            data["inputs"] = ParseBehaviorInputsJson(inputsJson);
    }

    private static void EnsureExplicitClearDoesNotConflict(bool hasValue, bool clear, string fieldLabel)
    {
        if (hasValue && clear)
            throw new InvalidOperationException($"Pole {fieldLabel} nelze zaroven nastavit a vycistit.");
    }

    private static IReadOnlyList<Dictionary<string, object?>> ParseBehaviorInputsJson(string inputsJson)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<List<BehaviorInputPayload>>(inputsJson, BehaviorInputsJsonOptions) ?? [];
            return payload.Select(input =>
            {
                var item = new Dictionary<string, object?>
                {
                    ["name"] = input.Name ?? string.Empty,
                    ["type"] = string.IsNullOrWhiteSpace(input.Type) ? "text" : input.Type,
                };

                if (!string.IsNullOrWhiteSpace(input.Id))
                    item["id"] = input.Id;

                if (input.Required)
                    item["required"] = true;

                if (input.Summary is not null)
                    item["summary"] = input.Summary;

                return item;
            }).ToList();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Behavior inputs musi byt validni JSON pole objektu.", nameof(inputsJson), ex);
        }
    }

    private static IReadOnlyList<string> ParseStringListJson(string valuesJson, string parameterName, string label)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<List<string>>(valuesJson) ?? [];
            return payload
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"{label} musi byt validni JSON pole stringu.", parameterName, ex);
        }
    }

    private sealed class BehaviorInputPayload
    {
        public string? Id { get; init; }

        public string? Name { get; init; }

        public string? Type { get; init; }

        public bool Required { get; init; }

        public string? Summary { get; init; }
    }

    private static string ResolveSourceTag(CommandSource source)
    {
        return source switch
        {
            CommandSource.Cli => TelemetryTags.SourceCli,
            CommandSource.Chat => TelemetryTags.SourceChat,
            CommandSource.Mcp => TelemetryTags.SourceMcp,
            CommandSource.Desktop => TelemetryTags.SourceDesktop,
            _ => TelemetryTags.SourceSystem,
        };
    }

    private static string ResolveHostTag(CommandSource source)
    {
        return source switch
        {
            CommandSource.Cli => TelemetryTags.HostCli,
            CommandSource.Chat => TelemetryTags.HostChat,
            CommandSource.Mcp => TelemetryTags.HostMcp,
            CommandSource.Desktop => TelemetryTags.HostDesktop,
            _ => TelemetryTags.HostTests,
        };
    }

    private static string ResolveDetailTag(ProjectionOptions? options)
    {
        if (options is null)
            return TelemetryTags.DetailBasic;

        if (options.HasAnyExpertSection)
            return TelemetryTags.DetailExpert;

        return TelemetryTags.DetailBasic;
    }

    private void RecordExport(string exportKind, CommandSource source)
    {
        var hostTag = ResolveHostTag(source);

        MetaForgeTelemetry.ExportRequests.Add(1,
            new KeyValuePair<string, object?>(TelemetryTags.Host, hostTag),
            new KeyValuePair<string, object?>(TelemetryTags.ExportKind, exportKind),
            new KeyValuePair<string, object?>(TelemetryTags.Result, TelemetryTags.ResultOk));
    }

    private static BusinessAuthoringAiSegmentStatus CreateAiSegmentStatus(
        string name,
        AiSegment segment,
        AiPlatformConfiguration configuration,
        IReadOnlyList<BusinessAuthoringAiHealthStatus> healthStatuses)
    {
        var settings = configuration.GetSettingsForSegment(segment);
        var healthStatus = healthStatuses.FirstOrDefault(status => string.Equals(status.Name, name, StringComparison.Ordinal));

        return new BusinessAuthoringAiSegmentStatus(
            Name: name,
            Segment: segment.ToString(),
            Provider: settings.Provider.ToString(),
            Model: settings.Model,
            Endpoint: settings.Endpoint,
            IsEnabled: settings.Enabled,
            IsOnline: healthStatus?.IsOk ?? false,
            RequiresApiKey: healthStatus?.RequiresApiKey ?? false,
            HasApiKeyConfigured: healthStatus?.HasApiKeyConfigured ?? !string.IsNullOrWhiteSpace(settings.ApiKey),
            ApiKeyEnvironmentVariable: healthStatus?.ApiKeyEnvironmentVariable ?? string.Empty);
    }

    private static CatalogManager CreateCatalogManager(ForgeBlockPackageRegistry? packageRegistry = null)
    {
        var catalogManager = new CatalogManager();
        catalogManager.RegisterProvider(new BuiltInCatalogProvider());
        catalogManager.RegisterProvider(new ForgeBlockRegistryCatalogProvider(packageRegistry ?? CreateBuiltInForgeBlockPackageRegistry()));
        catalogManager.LoadAsync().GetAwaiter().GetResult();
        return catalogManager;
    }

    private static ForgeBlockPackageRegistry CreateBuiltInForgeBlockPackageRegistry()
    {
        return BuiltInForgeBlockPackageBootstrap.CreateRegistry();
    }

    private NodeAssistService? CreateNodeAssistService()
    {
        try
        {
            if (_conversationService.TranslationAiClient is not IPromptCompletionAiClient promptCompletionClient)
                return null;

            if (!promptCompletionClient.IsAvailable)
                return null;

            return new NodeAssistService(promptCompletionClient);
        }
        catch
        {
            return null;
        }
    }

    private sealed class ReadDocumentProjectionAdapter : IProjectionQueryService
    {
        private readonly AuthoringConversationService _service;

        public ReadDocumentProjectionAdapter(AuthoringConversationService service)
        {
            _service = service;
        }

        public Task<BusinessProjectionView> GetProjectionAsync(string? streamId = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new BusinessProjectionView
            {
                Success = true,
                Document = _service.GetCurrentReadDocumentModel(),
                TotalCommandCount = 0,
                ReplayedCommandCount = 0,
            });
        }
    }
    // --- CustomType ---

    public ConversationTurnResult AddCustomType(
        string name,
        string? underlyingType = null,
        string? summary = null,
        IReadOnlyList<string>? constraints = null,
        string? source = null,
        CommandSource commandSource = CommandSource.Cli)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var operation = new BusinessPatchOperation
        {
            Op = "add_customtype",
        };

        operation.Data["name"] = name;

        if (!string.IsNullOrWhiteSpace(underlyingType))
            operation.Data["underlyingType"] = underlyingType;

        if (!string.IsNullOrWhiteSpace(summary))
            operation.Data["summary"] = summary;

        if (constraints is { Count: > 0 })
            operation.Data["constraints"] = constraints;

        if (!string.IsNullOrWhiteSpace(source))
            operation.Data["source"] = source;

        return ApplyOperations("add-customtype", [operation], commandSource, $"Pridavam custom type {name}.");
    }

    public ConversationTurnResult UpdateCustomType(
        string customTypeNameOrId,
        string? newName = null,
        string? underlyingType = null,
        string? summary = null,
        IReadOnlyList<string>? constraints = null,
        CommandSource commandSource = CommandSource.Cli)
    {
        var customType = ResolveCustomType(customTypeNameOrId);
        var operation = new BusinessPatchOperation
        {
            Op = "update_customtype",
        };

        operation.Data["id"] = customType.Id;

        if (!string.IsNullOrWhiteSpace(newName))
            operation.Data["name"] = newName;

        if (!string.IsNullOrWhiteSpace(underlyingType))
            operation.Data["underlyingType"] = underlyingType;

        if (summary is not null)
            operation.Data["summary"] = summary;

        if (constraints is not null)
            operation.Data["constraints"] = constraints;

        return ApplyOperations("update-customtype", [operation], commandSource, $"Aktualizuji custom type {customType.Name}.");
    }

    public ConversationTurnResult DeleteCustomType(string customTypeNameOrId, CommandSource commandSource = CommandSource.Cli)
    {
        var customType = ResolveCustomType(customTypeNameOrId);
        return ApplyOperations(
            "delete-customtype",
            [new BusinessPatchOperation
            {
                Op = "delete_customtype",
                Data = { ["id"] = customType.Id },
            }],
            commandSource,
            $"Mazu custom type {customType.Name}.");
    }

    public IReadOnlyList<CustomTypeDefinition> GetCustomTypes()
    {
        return GetCurrentReadDocument().CustomTypes;
    }

    public CustomTypeDefinition? FindCustomTypeByName(string name)
    {
        return GetCurrentReadDocument().CustomTypes
            .FirstOrDefault(ct => string.Equals(ct.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private CustomTypeDefinition ResolveCustomType(string customTypeNameOrId)
    {
        var document = GetCurrentReadDocument();
        var customType = document.CustomTypes
            .FirstOrDefault(ct => string.Equals(ct.Id, customTypeNameOrId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(ct.Name, customTypeNameOrId, StringComparison.OrdinalIgnoreCase));

        if (customType is null)
            throw new InvalidOperationException($"CustomType '{customTypeNameOrId}' nebyl nalezen.");

        return customType;
    }
}

public sealed record BusinessAuthoringAiApiKeyPromptRequest(
    string Name,
    string ApiKeyEnvironmentVariable);

public sealed record BusinessAuthoringAiHealthStatus(
    string Name,
    bool IsOk,
    bool IsEnabled,
    bool RequiresApiKey,
    bool HasApiKeyConfigured,
    string ApiKeyEnvironmentVariable);

public sealed record BusinessAuthoringAiSegmentStatus(
    string Name,
    string Segment,
    string Provider,
    string Model,
    string Endpoint,
    bool IsEnabled,
    bool IsOnline,
    bool RequiresApiKey,
    bool HasApiKeyConfigured,
    string ApiKeyEnvironmentVariable);