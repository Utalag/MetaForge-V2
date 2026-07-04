using MetaForge.Translator.Host;
using Spectre.Console;

namespace MetaForge.Cli.Formatting;

/// <summary>
/// Formátovaný CLI výstup pomocí Spectre.Console.
/// Tabulky, panely, barvy pro lepší čitelnost.
/// </summary>
public static class CliOutputFormatter
{
    /// <summary>
    /// Vypíše hlavičku s informacemi o projektu.
    /// </summary>
    public static void RenderHeader(ProjectionView view)
    {
        var panel = new Panel(new Markup($"""
            [bold]Projekt:[/] [cyan]{view.ProjectName}[/]
            [bold]Entit:[/] [green]{view.Entities.Count}[/]
            """))
        {
            Header = new PanelHeader(" MetaForge CLI "),
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1),
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Vypíše entitu jako Spectre.Console tabulku.
    /// </summary>
    public static void RenderEntityTable(ProjectionView view, string entityName)
    {
        var entity = view.Entities.FirstOrDefault(e =>
            e.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

        if (entity is null)
        {
            AnsiConsole.MarkupLine($"[red]Entita '{entityName}' nenalezena.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Atribut[/]").Centered())
            .AddColumn(new TableColumn("[bold]Typ[/]").Centered())
            .AddColumn(new TableColumn("[bold]Povinný[/]").Centered());

        foreach (var attr in entity.Attributes)
        {
            var required = attr.IsRequired ? "[green]✓[/]" : "[grey]–[/]";
            table.AddRow(attr.Name, attr.CoreType.BaseType.ToString(), required);
        }

        AnsiConsole.Write(new Rule($"[bold]📋 Entita: {entity.Name}[/]").RuleStyle("grey"));
        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Vypíše seznam všech entit.
    /// </summary>
    public static void RenderEntityList(ProjectionView view)
    {
        if (view.Entities.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Žádné entity k zobrazení.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Entita[/]")
            .AddColumn("[bold]Atributů[/]");

        foreach (var entity in view.Entities)
        {
            table.AddRow(
                $"[cyan]{entity.Name}[/]",
                entity.Attributes.Count.ToString());
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Vypíše úspěšnou akci.
    /// </summary>
    public static void Success(string message)
    {
        AnsiConsole.MarkupLine($"[green]✅ {message}[/]");
    }

    /// <summary>
    /// Vypíše chybu.
    /// </summary>
    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]❌ {message}[/]");
    }

    /// <summary>
    /// Vypíše varování.
    /// </summary>
    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠️ {message}[/]");
    }

    /// <summary>
    /// Vypíše informaci.
    /// </summary>
    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"[blue]ℹ️ {message}[/]");
    }
}
