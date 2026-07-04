using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.Core.Catalog;
using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Core.Inference;
using MetaForge.Translator.Host;
using MetaForge.Translator.Translation;

// === Composition Root pro CLI ===
var builder = Host.CreateApplicationBuilder(args);

// Core — Singleton
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();

// BusinessModel — Scoped
builder.Services.AddScoped<BusinessAuthoringDocument>();
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();

// Translator — Scoped
builder.Services.AddScoped<IBusinessTranslator, DefaultBusinessTranslator>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<WriteBackService>();
builder.Services.AddScoped<BusinessAuthoringHostFacade>();

var app = builder.Build();

// === Jednoduchý CLI dispatcher ===
var argsList = args.ToList();
if (argsList.Count == 0)
{
    PrintHelp();
    return 0;
}

// Získej Facade z DI
var facade = app.Services.GetRequiredService<BusinessAuthoringHostFacade>();

try
{
    var command = argsList[0].ToLowerInvariant();
    switch (command)
    {
        case "add-entity":
            HandleAddEntity(facade, argsList);
            break;
        case "update-entity":
            HandleUpdateEntity(facade, argsList);
            break;
        case "delete-entity":
            HandleDeleteEntity(facade, argsList);
            break;
        case "add-attribute":
            HandleAddAttribute(facade, argsList);
            break;
        case "projection":
            HandleProjection(facade);
            break;
        case "list-entities":
            HandleListEntities(facade);
            break;
        case "help" or "--help" or "-h":
            PrintHelp();
            break;
        default:
            Console.Error.WriteLine($"Neznámý příkaz: {command}");
            PrintHelp();
            return 1;
    }

    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Chyba: {ex.Message}");
    return 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Chyba: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Neošetřená chyba: {ex.Message}");
    return 2;
}

// === Command handlers ===

static void HandleAddEntity(BusinessAuthoringHostFacade facade, List<string> args)
{
    if (args.Count < 2)
    {
        Console.Error.WriteLine("Použití: add-entity <název>");
        return;
    }
    var id = facade.AddEntity(args[1]);
    Console.WriteLine($"Entita '{args[1]}' přidána. Id: {id}");
}

static void HandleUpdateEntity(BusinessAuthoringHostFacade facade, List<string> args)
{
    if (args.Count < 3)
    {
        Console.Error.WriteLine("Použití: update-entity <id> <nový-název>");
        return;
    }
    facade.UpdateEntity(args[1], args[2]);
    Console.WriteLine($"Entita '{args[1]}' přejmenována na '{args[2]}'.");
}

static void HandleDeleteEntity(BusinessAuthoringHostFacade facade, List<string> args)
{
    if (args.Count < 2)
    {
        Console.Error.WriteLine("Použití: delete-entity <id>");
        return;
    }
    facade.DeleteEntity(args[1]);
    Console.WriteLine($"Entita '{args[1]}' smazána.");
}

static void HandleAddAttribute(BusinessAuthoringHostFacade facade, List<string> args)
{
    if (args.Count < 3)
    {
        Console.Error.WriteLine("Použití: add-attribute <entity-id> <název> [typ] [required]");
        return;
    }
    var entityId = args[1];
    var name = args[2];
    var type = args.Count > 3 ? args[3] : "string";
    var required = args.Count > 4 && bool.TryParse(args[4], out var r) && r;

    var attrId = facade.AddAttribute(entityId, name, type, required);
    Console.WriteLine($"Atribut '{name}' (typ: {type}) přidán k entitě. Id: {attrId}");
}

static void HandleProjection(BusinessAuthoringHostFacade facade)
{
    var projection = facade.GetProjection();
    Console.WriteLine($"Projekt: {projection.ProjectName}");
    Console.WriteLine($"Počet entit: {projection.Entities.Count}");
    Console.WriteLine();

    foreach (var entity in projection.Entities)
    {
        Console.WriteLine($"  Entita: {entity.Name} ({entity.Id})");
        foreach (var attr in entity.Attributes)
        {
            var req = attr.IsRequired ? " [required]" : "";
            var maxLen = attr.MaxLength.HasValue ? $" [max:{attr.MaxLength}]" : "";
            Console.WriteLine($"    - {attr.Name}: {attr.CoreType.BaseType}{req}{maxLen}");
        }
    }
}

static void HandleListEntities(BusinessAuthoringHostFacade facade)
{
    var doc = facade.GetDocument();
    Console.WriteLine($"Projekt: {doc.ProjectName}");
    Console.WriteLine($"Verze schématu: {doc.SchemaVersion}");
    Console.WriteLine($"Commandů v logu: {facade.GetCommandCount()}");
    Console.WriteLine();

    foreach (var entity in doc.Entities)
    {
        Console.WriteLine($"  [{entity.Id}] {entity.Name} ({entity.Attributes.Count} atributů)");
    }
}

static void PrintHelp()
{
    Console.WriteLine("MetaForge CLI — C#-first platforma pro modelování a generování");
    Console.WriteLine();
    Console.WriteLine("Příkazy:");
    Console.WriteLine("  add-entity <název>                    Přidá novou entitu");
    Console.WriteLine("  update-entity <id> <nový-název>       Přejmenuje entitu");
    Console.WriteLine("  delete-entity <id>                    Smaže entitu");
    Console.WriteLine("  add-attribute <entity-id> <název> [typ] [required]  Přidá atribut");
    Console.WriteLine("  projection                            Zobrazí aktuální projekci");
    Console.WriteLine("  list-entities                         Vypíše všechny entity");
    Console.WriteLine("  help                                  Tato nápověda");
}
