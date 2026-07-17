using MetaForge.Feedback;
using MetaForge.Feedback.Models;
using Spectre.Console;

namespace MetaForge.Cli.Commands;

public static class FeedbackCommands
{
    public static async Task ListFeedbackAsync(IAuthoringFeedbackService feedback, string projectId)
    {
        var snapshot = await feedback.GetCurrentAsync(projectId, CancellationToken.None);

        if (snapshot.OpenItems.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]✓ Žádné otevřené warningy.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Code")
            .AddColumn("Severity")
            .AddColumn("Stage")
            .AddColumn("Element")
            .AddColumn("Message");

        foreach (var item in snapshot.OpenItems)
        {
            var severityColor = item.CoreDiagnostic.Severity == Core.Diagnostics.DiagnosticSeverity.Error
                ? "red" : "yellow";

            table.AddRow(
                item.CoreDiagnostic.Code,
                $"[{severityColor}]{item.CoreDiagnostic.Severity}[/]",
                item.Stage,
                $"{item.ElementKind}:{item.CoreDiagnostic.Location.Element}",
                item.CoreDiagnostic.Message
            );
        }

        AnsiConsole.Write(table);
        var errorColor = snapshot.ErrorCount > 0 ? "red" : "green";
        AnsiConsole.MarkupLine($"[grey]Celkem: {snapshot.TotalCount} | Chyby: [{errorColor}]{snapshot.ErrorCount}[/] | Varování: [yellow]{snapshot.WarningCount}[/][/]");
    }

    public static async Task DismissFeedbackAsync(IAuthoringFeedbackService feedback, Guid feedbackId)
    {
        await feedback.MarkDismissedAsync(feedbackId, CancellationToken.None);
        AnsiConsole.MarkupLine($"[grey]✓ Feedback {feedbackId} označen jako dismissed.[/]");
    }
}
