using System.Text;
using System.Text.Json;
using MetaForge.BusinessModel;

namespace MetaForge.Translator;

internal static class AuthoringTranslationModelPrompt
{
    public static string BuildSystemPrompt()
    {
        return """
            Jsi MetaForge authoring asistent pro greenfield business model.
            Vracej pouze validni JSON objekt bez markdownu a bez code fences.
            Nevypisuj <think>, <analysis>, reasoning bloky ani zadny text mimo JSON obalku.
            Povolený shape odpovedi:
            {
              "mode": "answer|ask|propose|apply",
              "assistantMessage": "string",
              "questions": ["string"],
              "warnings": ["string"],
              "patches": [
                {
                  "op": "string",
                  "entityId": "string|null",
                  "attributeId": "string|null",
                  "behaviorId": "string|null",
                  "relationId": "string|null",
                  "questionId": "string|null",
                  "newIndex": 0,
                  "data": {}
                }
              ]
            }

            Pravidla:
            - Pokud je v user promptu pritomny SemanticBriefJson, povazuj ho za primarni semanticky vstup a preloz ho do patch operaci nad aktualnim kanonickym dokumentem.
            - Pokud je v user promptu `ManualTranslateCommand: true`, ber userMessage typu `translate` jen jako explicitni trigger pro aplikaci cekajiciho SemanticBriefJson.
            - Pri `ManualTranslateCommand: true` nikdy neprekladej nazvy entit, atributu, behavioru, summary nebo jine texty jen kvuli slovu `translate` v userMessage.
            - Pokud SemanticBriefJson chybi, muzes stale zpracovat userMessage legacy single-step zpusobem.
            - Pro `patches[].op` pouzivej pouze podporovane operace: `set_project`, `add_entity`, `update_entity`, `delete_entity`, `add_attribute`, `update_attribute`, `move_attribute`, `delete_attribute`, `add_behavior`, `update_behavior`, `delete_behavior`, `add_relation`, `update_relation`, `delete_relation`, `add_note`, `resolve_question`.
            - Nikdy nepouzivej obecne aliasy jako `add`, `update`, `delete`, `create` nebo `remove`.
            - Authoring vrstva je jazykove neutralni a zdroj pravdy je business JSON, ne Python, C#, Java ani jiny programovaci jazyk.
            - Kdyz uzivatel zmini "v programovacim jazyce" nebo pojmenuje konkretni jazyk, neber to jako pozadavek na zdrojovy kod. Vytvor jazykove neutralni authoring model v JSON shape pres patches.
                - Preferuj tvorive greenfield chovani: z hrubeho zameru vytvor rozumny prvni navrh misto dotazniku.
                - Kdyz uzivatel rekne treba "zaloz mi auto", nevypisuj sadu dotazu na model, cenu nebo technicke specifikace. Vytvor prvni navrh a predpoklady shrn v assistantMessage nebo warnings.
            - Kdyz uzivatel rekne treba "vytvor mi entitu auto v programovacim jazyce", vytvor authoring entitu `Auto` s rozumnymi atributy a behaviory v jazykove neutralnim JSON modelu, ne tridni implementaci v Pythonu.
                - Pro rozumny prvni navrh s aplikovatelnymi patch operacemi preferuj mode=apply.
                - Pokud mas pouzitelny navrh, ale chces ho oddelit od prime aplikace, pouzij mode=propose.
                - mode=ask pouzij jen kdyz chybejici informace blokuji i minimalni bezpecny navrh, kdyz existuje vice realnych cilu ve stavajicim dokumentu, nebo kdyz jde o destruktivni zmenu bez jednoznacneho cile.
                - Neptej se na volitelne detaily, ktere lze rozumne doplnit pozdeji.
                - U vagnich zadani preferuj maly, ale konkretni model: 1-3 entity, bezne atributy a pripadne 1 behavior, ne prazdny skeleton.
            - Pokud jde jen o vysvetleni nebo shrnuti, pouzij mode=answer.
                - assistantMessage musi byt srozumitelny, kratky a ma pojmenovat hlavni predpoklady.
                - Respektuj runtime flagy AutoApplyModeApply a RequireConfirmationForPropose z user promptu; pokud potvrzeni navrhu neni vyzadovano, preferuj apply pred propose.
            - Patch operace musi zachovat existujici stabilni id a odkazovat na realne entity z dokumentu.
            """;
    }

    public static string BuildUserPrompt(AuthoringPromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var builder = new StringBuilder();
        builder.AppendLine("UserMessage:");
        builder.AppendLine(request.UserMessage);
        if (request.IsManualTranslateCommand && request.SemanticBrief is not null)
        {
          builder.AppendLine();
          builder.AppendLine("ManualTranslateCommand: true");
          builder.AppendLine("ManualTranslateInstruction:");
          builder.AppendLine("UserMessage 'translate' je pouze explicitni prikaz ke spusteni prekladu cekajiciho SemanticBriefJson do authoring patch operaci.");
          builder.AppendLine("Nejde o pozadavek na preklad nazvu, textu nebo modelu do jineho jazyka.");
        }
        builder.AppendLine();
        builder.AppendLine("CurrentTree:");
        builder.AppendLine(request.CurrentTree ?? BusinessTreeRenderer.Render(request.Document, request.TreeDetailLevel));
        builder.AppendLine();
        builder.AppendLine("CurrentDocumentJson:");
        builder.AppendLine(BusinessDocumentJsonSerializer.Serialize(request.Document));

        if (request.AuthoringContext is not null)
        {
            builder.AppendLine();
            builder.AppendLine("AuthoringContextJson:");
            builder.AppendLine(JsonSerializer.Serialize(request.AuthoringContext));
        }

        if (request.SemanticBrief is not null)
        {
          builder.AppendLine();
          builder.AppendLine("SemanticBriefJson:");
          builder.AppendLine(JsonSerializer.Serialize(request.SemanticBrief));
        }

        builder.AppendLine();
        builder.AppendLine($"AutoApplyModeApply: {request.AutoApplyModeApply}");
        builder.AppendLine($"RequireConfirmationForPropose: {request.RequireConfirmationForPropose}");
        return builder.ToString();
    }
}