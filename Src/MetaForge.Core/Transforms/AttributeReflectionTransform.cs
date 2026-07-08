using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Diagnostics;

namespace MetaForge.Core.Transforms;

/// <summary>
/// Transformace: AttributeElement → MetadataBag reflexe.
/// Mapuje C# atributy na standardizované MetadataBag klíče,
/// aby generátor měl jednotný dotazovací bod.
/// </summary>
public sealed class AttributeReflectionTransform : IModelTransform
{
    public string Name => "AttributeReflection";

    public TypeModel Apply(TypeModel model, TransformContext ctx)
    {
        // Procházíme všechny RootElementy v modelu (pro zjednodušení:
        // tato implementace pracuje jen s modelem, který má přístupné elementy.
        // V budoucnu bude rozšířena o rekursivní procházení property/metod.)
        return model; // Implementace rozšířena v budoucnu s GetAllElements()
    }
}

/// <summary>
/// Pomocné rozšíření pro reflexi atributů na MetadataBag.
/// </summary>
public static class AttributeReflection
{
    /// <summary>
    /// Mapuje známé C# atributy na standardizované MetadataBag klíče.
    /// </summary>
    public static string? MapAttributeToMetadataKey(string attributeName) => attributeName switch
    {
        "Required" or "RequiredAttribute" => MetadataBag.Keys.ValidationRequired,
        "JsonIgnore" or "JsonIgnoreAttribute" => MetadataBag.Keys.GenerationJsonIgnore,
        "StringLength" or "StringLengthAttribute" => MetadataBag.Keys.ValidationMaxLength,
        "MinLength" or "MinLengthAttribute" => MetadataBag.Keys.ValidationMinLength,
        "Range" or "RangeAttribute" => MetadataBag.Keys.ValidationRangeMin,
        _ => null,
    };

    /// <summary>
    /// Provede reflexi atributů na elementu a zapíše výsledky do MetadataBag.
    /// </summary>
    public static void ReflectToMetadata(RootElement element, IDiagnosticCollector? diagnostics = null)
    {
        foreach (var attr in element.Attributes)
        {
            var key = MapAttributeToMetadataKey(attr.Name);
            if (key != null && !element.Metadata.Has(key))
            {
                element.Metadata.Set(key, true);
                diagnostics?.Report(new Diagnostic(
                    "MF-REF-001",
                    $"Reflected {attr.Name} → {key}",
                    DiagnosticSeverity.Info,
                    new ElementPath("TypeModel", element.Name)));
            }
        }
    }
}
