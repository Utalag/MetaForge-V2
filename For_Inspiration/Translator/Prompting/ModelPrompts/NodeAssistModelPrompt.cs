using System.Text;
using System.Text.Json;

namespace MetaForge.Translator;

/// <summary>
/// Prompt builder pro node-level AI asistenci.
/// </summary>
internal static class NodeAssistModelPrompt
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static string BuildSystemPrompt()
    {
        return """
            Jsi lokalni radce pro jeden node v MetaForge business modelu.
            Tvym ukolem je navrhnout hodnoty poli v ramci tohoto jednoho node.
            
            Pravidla:
            - Navrhuj pouze hodnoty pro dany node, nikdy neprovadej zmeny mimo jeho scope.
            - Pokud je node attribute, navrhuj typ, constraints, summary, default value.
            - Pokud je node behavior, navrhuj summary, inputs (name, type, required) a returns.
            - Pokud je cilem cela entita, navrhuj summary, icon, nebo chybejici atributy/behaviory.
            - Vracis striktne JSON ve formatu popsanym uzivatelem.
            - Nevracej markdown code blocks, pouze cisty JSON.
            """;
    }

    public static string BuildUserPrompt(NodeAssistContext context, string userPrompt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Node context:");
        builder.AppendLine(JsonSerializer.Serialize(context, JsonOptions));
        builder.AppendLine();
        builder.AppendLine("Uzivatelsky pozadavek:");
        builder.AppendLine(userPrompt);
        builder.AppendLine();
        builder.AppendLine("""
            Ocekavany JSON format:
            {
              "summary": "volitelny navrh summary",
              "inputs": [
                { "name": "inputName", "type": "string", "required": true, "summary": "popis" }
              ],
              "returns": "volitelny navratovy typ",
              "explanation": "strucne vysvetleni navrhu",
              "proposedOperations": [
                { "op": "update_behavior", "entityId": "...", "behaviorId": "...", "data": { "summary": "..." } }
              ]
            }
            
            Polozky inputs, returns a proposedOperations jsou volitelne — vracej jen ty, ktere davaji smysl pro dany node a pozadavek.
            Pokud nemas konkretni navrh, vrat prazdny JSON {} nebo jen explanation.
            """);

        return builder.ToString();
    }
}
