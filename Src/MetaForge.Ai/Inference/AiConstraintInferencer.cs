using MetaForge.Ai.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Inference;

namespace MetaForge.Ai.Inference;

/// <summary>
/// AI implementace IConstraintInferencer — používá AI backend pro odvození constraintů.
/// Při selhání vrací prázdný seznam (graceful fallback).
/// </summary>
public sealed class AiConstraintInferencer : IConstraintInferencer
{
    private readonly IAiBackendAdapter _backend;

    public AiConstraintInferencer(IAiBackendAdapter backend)
    {
        _backend = backend;
    }

    public IReadOnlyList<string> Infer(string attributeName, TypeModel type)
    {
        // Postav prompt
        var prompt = $"""
            Jsi expert na datové modelování. Pro atribut '{attributeName}' typu '{type.BaseType}' odvoď validační pravidla.
            
            Vrať POUZE JSON pole stringů, např.: ["not_empty", "max_length:200"].
            
            Pravidla:
            - not_empty: atribut nesmí být prázdný
            - email_format: musí být validní email
            - phone_format: musí být validní telefon
            - url_format: musí být validní URL
            - min_length:N: minimální délka N
            - max_length:N: maximální délka N
            - range:MIN-MAX: číselný rozsah
            - not_negative: nesmí být záporné
            """;

        // Synchronní volání s bezpečným sync-over-async (bez deadlock rizika)
        try
        {
            var result = Task.Run(() => _backend.SendJsonAsync<List<string>>(prompt))
                .GetAwaiter().GetResult();
            return result is not null
                ? (IReadOnlyList<string>)result.AsReadOnly()
                : Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>(); // Graceful fallback
        }
    }
}
