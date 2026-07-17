using MetaForge.Feedback;
using MetaForge.Feedback.Models;

namespace MetaForge.Mcp.Tools;

public static class FeedbackTools
{
    public static async Task<AuthoringFeedbackSnapshot> GetFeedbackAsync(
        IAuthoringFeedbackService feedback, string projectId)
    {
        return await feedback.GetCurrentAsync(projectId, CancellationToken.None);
    }

    public static async Task DismissFeedbackAsync(
        IAuthoringFeedbackService feedback, Guid feedbackId)
    {
        await feedback.MarkDismissedAsync(feedbackId, CancellationToken.None);
    }
}
