using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Inference;

/// <summary>
/// Inferuje constrainty (omezení) pro typ na základě názvu atributu.
/// Např. atribut "Email" → constraint ["email_format", "not_empty"].
/// </summary>
public interface IConstraintInferencer
{
    /// <summary>Odvodí constrainty pro daný název atributu a typ.</summary>
    IReadOnlyList<string> Infer(string attributeName, TypeModel type);
}
