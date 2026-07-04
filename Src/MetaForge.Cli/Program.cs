using System.CommandLine;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.Core.Catalog;
using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Core.Inference;
using MetaForge.Cli.Formatting;
using MetaForge.Translator.Host;
using MetaForge.Translator.Translation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<CatalogManager>();
builder.Services.AddSingleton<ForgeBlockRegistry>();
builder.Services.AddSingleton<ICatalogProvider, BuiltInCatalogProvider>();
builder.Services.AddSingleton<IConstraintInferencer, RuleBasedConstraintInferencer>();
builder.Services.AddScoped<BusinessAuthoringDocument>();
builder.Services.AddScoped<CommandLogStore>();
builder.Services.AddScoped<PatchEngine>();
builder.Services.AddScoped<ReplayEngine>();
builder.Services.AddScoped<IBusinessTranslator, DefaultBusinessTranslator>();
builder.Services.AddScoped<ProjectionReadService>();
builder.Services.AddScoped<WriteBackService>();
builder.Services.AddScoped<BusinessAuthoringHostFacade>();
var app = builder.Build();

BusinessAuthoringHostFacade GetFacade() => app.Services.GetRequiredService<BusinessAuthoringHostFacade>();

var root = new RootCommand("MetaForge - AI-powered C# code generation platform");

var addEntity = new Command("add-entity", "Prida novou business entitu");
var nameArg = new Argument<string>("name", "Nazev entity");
addEntity.AddArgument(nameArg);
addEntity.SetHandler((ctx) => {
    var name = ctx.ParseResult.GetValueForArgument(nameArg);
    var id = GetFacade().AddEntity(name);
    CliOutputFormatter.Success($"Entita '{name}' vytvorena (ID: {id})");
});

var listCmd = new Command("list-entities", "Vypise vsechny entity");
listCmd.SetHandler(() => {
    CliOutputFormatter.RenderEntityList(GetFacade().GetProjection());
});

var projection = new Command("projection", "Zobrazi projekci modelu");
var entityOpt = new Option<string>("--entity", "Detail konkretni entity");
projection.AddOption(entityOpt);
projection.SetHandler((ctx) => {
    var view = GetFacade().GetProjection();
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
    var eid = ctx.ParseResult.GetValueForArgument(entIdArg);
    var nm = ctx.ParseResult.GetValueForArgument(attrNameArg2);
    var tp = ctx.ParseResult.GetValueForOption(typeOpt);
    var rq = ctx.ParseResult.GetValueForOption(reqOpt);
    var id = GetFacade().AddAttribute(eid, nm, tp ?? "string", rq);
    CliOutputFormatter.Success($"Atribut '{nm}' (typ: {tp}) pridan (ID: {id})");
});

var deleteCmd = new Command("delete-entity", "Smaze entitu");
var delArg = new Argument<string>("id", "ID entity");
deleteCmd.AddArgument(delArg);
deleteCmd.SetHandler((ctx) => {
    GetFacade().DeleteEntity(ctx.ParseResult.GetValueForArgument(delArg));
    CliOutputFormatter.Success("Entita smazana.");
});

var infoCmd = new Command("info", "Informace o projektu");
infoCmd.SetHandler(() => {
    var view = GetFacade().GetProjection();
    CliOutputFormatter.RenderHeader(view);
    var doc = GetFacade().GetDocument();
    CliOutputFormatter.Info($"Schema: {doc.SchemaVersion} | Commandu: {GetFacade().GetCommandCount()}");
});

root.AddCommand(addEntity); root.AddCommand(listCmd); root.AddCommand(projection);
root.AddCommand(addAttr); root.AddCommand(deleteCmd); root.AddCommand(infoCmd);

return root.Invoke(args);
