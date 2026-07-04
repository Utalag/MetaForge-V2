using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.Core.Catalog;
using MetaForge.Core.ForgeBlockPackages;
using MetaForge.Core.Inference;
using MetaForge.Mcp.Models;
using MetaForge.Translator.Host;
using MetaForge.Translator.Translation;

// === MCP JSON-RPC stdio server ===
// Čte JSON-RPC requesty ze stdin, zapisuje response na stdout.
// Logování jde na stderr (aby nerušilo JSON-RPC komunikaci).

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
var facade = app.Services.GetRequiredService<BusinessAuthoringHostFacade>();

// === Hlavní smyčka — čtení requestů ze stdin ===
var stdin = Console.In;
var stdout = Console.Out;

// Pošli inicializační zprávu
var initResponse = new JsonRpcResponse
{
    Id = null,
    Result = new { name = "metaforge-mcp", version = "1.0.0", tools = GetToolList() }
};
stdout.WriteLine(JsonSerializer.Serialize(initResponse));

// Zpracovávej requesty
string? line;
while ((line = stdin.ReadLine()) is not null)
{
    if (string.IsNullOrWhiteSpace(line)) continue;

    try
    {
        var request = JsonSerializer.Deserialize<JsonRpcRequest>(line);
        if (request is null) continue;

        var response = HandleRequest(request, facade);
        stdout.WriteLine(JsonSerializer.Serialize(response));
    }
    catch (Exception ex)
    {
        var errorResponse = new JsonRpcResponse
        {
            Id = null,
            Error = new JsonRpcError { Code = -1, Message = ex.Message }
        };
        stdout.WriteLine(JsonSerializer.Serialize(errorResponse));
    }
}

// === Tool handler ===

static JsonRpcResponse HandleRequest(JsonRpcRequest request, BusinessAuthoringHostFacade facade)
{
    return request.Method switch
    {
        "tools/list" => new JsonRpcResponse { Id = request.Id, Result = new { tools = GetToolList() } },
        "tools/call" => HandleToolCall(request, facade),
        _ => new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Code = -32601, Message = $"Neznámá metoda: {request.Method}" } }
    };
}

static JsonRpcResponse HandleToolCall(JsonRpcRequest request, BusinessAuthoringHostFacade facade)
{
    var toolName = request.Params?.GetProperty("name").GetString() ?? "";
    var args = request.Params?.GetProperty("arguments");

    try
    {
        object result = toolName switch
        {
            "add_entity" => facade.AddEntity(args?.GetProperty("name").GetString() ?? ""),
            "add_attribute" => facade.AddAttribute(
                args?.GetProperty("entity_id").GetString() ?? "",
                args?.GetProperty("name").GetString() ?? "",
                args?.GetProperty("type").GetString() ?? "string",
                args?.GetProperty("required").GetBoolean() ?? false
            ),
            "get_projection" => facade.GetProjection(),
            "list_entities" => facade.GetDocument().Entities.Select(e => new { e.Id, e.Name, AttributeCount = e.Attributes.Count }).ToList(),
            _ => throw new InvalidOperationException($"Neznámý tool: {toolName}")
        };

        return new JsonRpcResponse { Id = request.Id, Result = result };
    }
    catch (Exception ex)
    {
        return new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Code = -32000, Message = ex.Message } };
    }
}

static List<object> GetToolList() => new()
{
    new { name = "add_entity", description = "Přidá novou business entitu", inputSchema = new { type = "object", properties = new { name = new { type = "string", description = "Název entity" } }, required = new[] { "name" } } },
    new { name = "add_attribute", description = "Přidá atribut k entitě", inputSchema = new { type = "object", properties = new { entity_id = new { type = "string" }, name = new { type = "string" }, type = new { type = "string" }, required = new { type = "boolean" } }, required = new[] { "entity_id", "name" } } },
    new { name = "get_projection", description = "Vrátí aktuální projekci business modelu", inputSchema = new { type = "object", properties = new { } } },
    new { name = "list_entities", description = "Vypíše všechny entity", inputSchema = new { type = "object", properties = new { } } },
};

// === JSON-RPC modely — přesunuty do MetaForge.Mcp.Models.JsonRpcModels.cs ===

