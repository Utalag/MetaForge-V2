namespace MetaForge.Translator.Prompting.ModelPrompts;

/// <summary>
/// Prompt pro AI překlad business atributu na TypeModel + constraints.
/// Definuje šablonu systémového a uživatelského promptu pro Ollama/OpenAI.
/// </summary>
public static class AuthoringTranslationModelPrompt
{
    /// <summary>
    /// Systémový prompt — instrukce pro model.
    /// </summary>
    public const string SystemPrompt = """
        Jsi expertní C# vývojář specializující se na doménové modelování.
        Na základě business atributu a kontextu navrhni:

        1. **Konkrétní C# typ** — vyber nejvhodnější z: string, int, decimal, bool, DateTime, Guid, Email, PhoneNumber, Url
        2. **Validační pravidla** — jedno nebo více z: not_empty, email_format, phone_format, url_format, min_length:N, max_length:N, range:MIN-MAX, not_negative, decimal_places:N
        3. **Výchozí hodnotu** — pokud dává smysl (např. DateTime.UtcNow pro "CreatedAt")
        4. **Maximální/minimální délku** — pro string atributy
        5. **Regex pattern** — pro speciální formáty

        Vstup: JSON s atributem a kontextem okolních atributů entity.
        Výstup: POUZE JSON ve formátu:
        {
          "suggestedType": "string",
          "validationRules": ["not_empty", "max_length:200"],
          "defaultValue": null,
          "maxLength": 200,
          "minLength": 1,
          "minValue": null,
          "maxValue": null,
          "regexPattern": null,
          "confidence": 0.85
        }

        Důležité: Pokud atribut nepotřebuje enrichment, nastav confidence na 0 a vynech ostatní pole.
        """;

    /// <summary>
    /// Sestaví uživatelský prompt pro enrichment atributu.
    /// </summary>
    public static string BuildUserPrompt(
        string attributeName,
        string attributeType,
        IEnumerable<string> siblingAttributes,
        string? entityName = null)
    {
        var entityContext = entityName is not null ? $"Entita: {entityName}\n" : "";
        var siblingList = string.Join(", ", siblingAttributes);

        return $$"""
            {{entityContext}}Kontext — ostatní atributy entity: {{siblingList}}
            
            Atribut k analýze:
            - Název: {{attributeName}}
            - Typ: {{attributeType}}
            
            Navrhni zpřesnění typu, validační pravidla a výchozí hodnotu.
            """;
    }
}
