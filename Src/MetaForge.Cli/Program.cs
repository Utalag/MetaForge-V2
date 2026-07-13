using System.CommandLine;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.Core.Catalog;
using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Core.Inference;
using MetaForge.Cli.Formatting;
using MetaForge.Generators;
using MetaForge.Infrastructure;
using MetaForge.Infrastructure.Persistence;
using MetaForge.Translator.Host;
using MetaForge.Translator.Translation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// === Infrastructure: Persistence (CODE-002) ===
builder.Services.AddMetaForgeInfrastructure(useJsonPersistence: true);

// === Core services ===
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();

// === BusinessModel + Translator (scoped = per-command isolation) ===
builder.Services.AddScoped<BusinessAuthoringDocument>(sp =>
{
    // Load from persistence if exists, otherwise create new (CODE-002)
    var docRepo = sp.GetRequiredService<IDocumentRepository>();
    var doc = docRepo.LoadAsync().GetAwaiter().GetResult();
    return doc ?? new BusinessAuthoringDocument { ProjectName = "New Project" };
});
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();
builder.Services.AddScoped<IBusinessTranslator, DefaultBusinessTranslator>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<WriteBackService>();
builder.Services.AddScoped<BusinessAuthoringHostFacade>();
var app = builder.Build();
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

var root = new RootCommand("MetaForge - AI-powered C# code generation platform");

// === Existing commands ===

var addEntity = new Command("add-entity", "Prida novou business entitu");
var nameArg = new Argument<string>("name", "Nazev entity");
addEntity.AddArgument(nameArg);
addEntity.SetHandler((ctx) => {
    using var scope = scopeFactory.CreateScope();
    var facade = scope.ServiceProvider.GetRequiredService<BusinessAuthoringHostFacade>();
    var name = ctx.ParseResult.GetValueForArgument(nameArg);
    var id = facade.AddEntity(name);
    CliOutputFormatter.Success($"Entita '{name}' vytvorena (ID: {id})");
});

var listCmd = new Command("list-entities", "Vypise vsechny entity");
listCmd.SetHandler(() => {
    using var scope = scopeFactory.CreateScope();
    var facade = scope.ServiceProvider.GetRequiredService<BusinessAuthoringHostFacade>();
    CliOutputFormatter.RenderEntityList(facade.GetProjection());
});

var projection = new Command("projection", "Zobrazi projekci modelu");
var entityOpt = new Option<string>("--entity", "Detail konkretni entity");
projection.AddOption(entityOpt);
projection.SetHandler((ctx) => {
    using var scope = scopeFactory.CreateScope();
    var facade = scope.ServiceProvider.GetRequiredService<BusinessAuthoringHostFacade>();
    var view = facade.GetProjection();
    var name = ctx.ParseResult.GetValueForOption(entityOpt);
    if (!string.IsNullOrWhiteSpace(name))
        CliOutputFormatter.RenderEntityTable(view, name);
    else { CliOutputFormatter.RenderHeader(view); CliOutputFormatter.RenderEntityList(view); }
});

var addAttr = new Command("add-attribute", "Prida atribut k entite");
var entIdArg = new Argument<string>("entity-id", "ID entity");
var attrNameArg2 = new Argument<string>("name", "Nazev atributu");
var typeOpt = new Option<string>("--type", () => "string", "Typ atributu");
var reqOpt = new Option<bool>("--required", "Je povinny?");
addAttr.AddArgument(entIdArg); addAttr.AddArgument(attrNameArg2);
addAttr.AddOption(typeOpt); addAttr.AddOption(reqOpt);
addAttr.SetHandler((ctx) => {
    using var scope = scopeFactory.CreateScope();
    var facade = scope.ServiceProvider.GetRequiredService<BusinessAuthoringHostFacade>();
    var eid = ctx.ParseResult.GetValueForArgument(entIdArg);
    var nm = ctx.ParseResult.GetValueForArgument(attrNameArg2);
    var tp = ctx.ParseResult.GetValueForOption(typeOpt);
    var rq = ctx.ParseResult.GetValueForOption(reqOpt);
    var id = facade.AddAttribute(eid, nm, tp ?? "string", rq);
    CliOutputFormatter.Success($"Atribut '{nm}' (typ: {tp}) pridan (ID: {id})");
});

var deleteCmd = new Command("delete-entity", "Smaze entitu");
var delArg = new Argument<string>("id", "ID entity");
deleteCmd.AddArgument(delArg);
deleteCmd.SetHandler((ctx) => {
    using var scope = scopeFactory.CreateScope();
    var facade = scope.ServiceProvider.GetRequiredService<BusinessAuthoringHostFacade>();
    facade.DeleteEntity(ctx.ParseResult.GetValueForArgument(delArg));
    CliOutputFormatter.Success("Entita smazana.");
});

var infoCmd = new Command("info", "Informace o projektu");
infoCmd.SetHandler(() => {
    using var scope = scopeFactory.CreateScope();
    var facade = scope.ServiceProvider.GetRequiredService<BusinessAuthoringHostFacade>();
    var view = facade.GetProjection();
    CliOutputFormatter.RenderHeader(view);
    var doc = facade.GetDocument();
    CliOutputFormatter.Info($"Schema: {doc.SchemaVersion} | Commandu: {facade.GetCommandCount()}");
});

// === CODE-001: Generate command ===
var generateCmd = new Command("generate", "Vygeneruje C# kod z business modelu");
var outputOpt = new Option<string>("--output", () => "./output", "Vystupni adresar");
generateCmd.AddOption(outputOpt);
generateCmd.SetHandler((ctx) => {
    using var scope = scopeFactory.CreateScope();
    var facade = scope.ServiceProvider.GetRequiredService<BusinessAuthoringHostFacade>();
    var output = ctx.ParseResult.GetValueForOption(outputOpt);

    var document = facade.GetDocument();
    var catalog = new CatalogManager();
    var translator = new DefaultBusinessTranslator(catalog);
    var elements = translator.TranslateDocument(document);

    var forgeBlockRegistry = app.Services.GetRequiredService<ForgeBlockRegistry>();
    var integrator = new MetaForge.Generators.ForgeBlockPackages.ForgeBlockPackageIntegrator(forgeBlockRegistry);
    var generator = new CodeGenerator();
    var generatedCount = 0;
    Directory.CreateDirectory(output ?? "./output");

    foreach (var element in elements)
    {
        var artifact = generator.Generate(element);
        artifact = integrator.Enrich(artifact, element);

        var filePath = Path.Combine(output ?? "./output", artifact.FileName);
        File.WriteAllText(filePath, artifact.SourceCode);

        // Write required packages info
        if (artifact.RequiredPackages?.Count > 0)
        {
            var pkgInfo = string.Join(", ", artifact.RequiredPackages.Select(p => $"{p.PackageId} v{p.Version}"));
            File.AppendAllText(filePath, $"\n// Required NuGet: {pkgInfo}");
        }

        generatedCount++;
    }

    CliOutputFormatter.Success($"Vygenerovano {generatedCount} souboru do '{Path.GetFullPath(output ?? "./output")}'");
});

// === CODE-002: Save command ===
var saveCmd = new Command("save", "Ulozi dokument na disk");
saveCmd.SetHandler(() => {
    using var scope = scopeFactory.CreateScope();
    var facade = scope.ServiceProvider.GetRequiredService<BusinessAuthoringHostFacade>();
    var docRepo = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();

    var doc = facade.GetDocument();
    docRepo.SaveAsync(doc).GetAwaiter().GetResult();
    CliOutputFormatter.Success("Dokument ulozen.");
});

root.AddCommand(addEntity); root.AddCommand(listCmd); root.AddCommand(projection);
root.AddCommand(addAttr); root.AddCommand(deleteCmd); root.AddCommand(infoCmd);
root.AddCommand(generateCmd); root.AddCommand(saveCmd);

return root.Invoke(args);
