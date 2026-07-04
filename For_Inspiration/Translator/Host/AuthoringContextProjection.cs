using MetaForge.BusinessModel;
using MetaForge.Core.Discovery;

namespace MetaForge.Translator;

/// <summary>
/// Kompaktni AI-ready projekcni pohled na authoring context.
/// Obsahuje workflow summary, pending questions a discovery highlights.
/// </summary>
public sealed class AuthoringContextView
{
    /// <summary>Souhrn workflow v dokumentu.</summary>
    public WorkflowSummary Workflow { get; init; } = new();

    /// <summary>Souhrn otevrenych otazek.</summary>
    public PendingQuestionsSummary PendingQuestions { get; init; } = new();

    /// <summary>Discovery nebo capability highlights — null pokud nebyl requestovan.</summary>
    public DiscoverySummary? Discovery { get; init; }
}

/// <summary>
/// Souhrn workflow pro authoring context.
/// </summary>
public sealed class WorkflowSummary
{
    /// <summary>Pocet workflow definic v dokumentu.</summary>
    public int WorkflowCount { get; init; }

    /// <summary>Celkovy pocet kroku napric vsemi workflow.</summary>
    public int TotalSteps { get; init; }

    /// <summary>Pocet kroku s navazanym binding detailen.</summary>
    public int BoundSteps { get; init; }

    /// <summary>Pocet kroku bez binding detailu.</summary>
    public int UnboundSteps { get; init; }
}

/// <summary>
/// Souhrn pending questions pro authoring context.
/// </summary>
public sealed class PendingQuestionsSummary
{
    /// <summary>Pocet otevrenych otazek.</summary>
    public int OpenCount { get; init; }

    /// <summary>Pocet vyresenych otazek.</summary>
    public int ResolvedCount { get; init; }

    /// <summary>Pocet odmitnutych otazek.</summary>
    public int DismissedCount { get; init; }

    /// <summary>Texty otevrenych otazek (maximalne prvni 10 pro kompaktnost).</summary>
    public IReadOnlyList<string> OpenQuestionTexts { get; init; } = [];
}

/// <summary>
/// Discovery nebo capability highlights pro authoring context.
/// </summary>
public sealed class DiscoverySummary
{
    /// <summary>Celkovy pocet polozek napric discovery kategoriemi.</summary>
    public int TotalItemCount { get; init; }

    /// <summary>Pocet discovery kategorii.</summary>
    public int CategoryCount { get; init; }

    /// <summary>Nazvy kategorii s alespon jednou polozkou.</summary>
    public IReadOnlyList<string> CategoryNames { get; init; } = [];
}

/// <summary>
/// Builder pro <see cref="AuthoringContextView"/> z <see cref="BusinessAuthoringDocument"/>.
/// </summary>
internal static class AuthoringContextBuilder
{
    public static AuthoringContextView Build(
        BusinessAuthoringDocument document,
        bool includeDiscovery = false,
        IDiscoverySession? discoverySession = null)
    {
        var workflowSummary = BuildWorkflowSummary(document);
        var pendingQuestionsSummary = BuildPendingQuestionsSummary(document);
        var discoverySummary = includeDiscovery && discoverySession is not null
            ? BuildDiscoverySummary(discoverySession)
            : null;

        return new AuthoringContextView
        {
            Workflow = workflowSummary,
            PendingQuestions = pendingQuestionsSummary,
            Discovery = discoverySummary,
        };
    }

    private static WorkflowSummary BuildWorkflowSummary(BusinessAuthoringDocument document)
    {
        var totalSteps = document.Workflows.Sum(w => w.Steps.Count);
        var boundSteps = document.Workflows
            .SelectMany(w => w.Steps)
            .Count(s => s.BindingDetail is not null);

        return new WorkflowSummary
        {
            WorkflowCount = document.Workflows.Count,
            TotalSteps = totalSteps,
            BoundSteps = boundSteps,
            UnboundSteps = totalSteps - boundSteps,
        };
    }

    private static PendingQuestionsSummary BuildPendingQuestionsSummary(BusinessAuthoringDocument document)
    {
        var openQuestions = document.PendingQuestions
            .Where(q => q.Status == PendingQuestionStatus.Open)
            .Select(q => q.Text)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Take(10)
            .ToArray();

        return new PendingQuestionsSummary
        {
            OpenCount = document.PendingQuestions.Count(q => q.Status == PendingQuestionStatus.Open),
            ResolvedCount = document.PendingQuestions.Count(q => q.Status == PendingQuestionStatus.Resolved),
            DismissedCount = document.PendingQuestions.Count(q => q.Status == PendingQuestionStatus.Dismissed),
            OpenQuestionTexts = openQuestions,
        };
    }

    private static DiscoverySummary BuildDiscoverySummary(IDiscoverySession discoverySession)
    {
        var root = discoverySession.GetRoot();
        var categories = root.Categories;
        var totalItems = categories.Sum(c => c.ItemCount);

        return new DiscoverySummary
        {
            TotalItemCount = totalItems,
            CategoryCount = categories.Count,
            CategoryNames = categories.Select(c => c.Name).ToArray(),
        };
    }
}
