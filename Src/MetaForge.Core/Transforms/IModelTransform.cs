using MetaForge.Core.DataTypes;
using MetaForge.Core.Diagnostics;

namespace MetaForge.Core.Transforms;

/// <summary>
/// Transformace modelu — čistá funkce, nemutuje vstup.
/// </summary>
public interface IModelTransform
{
    /// <summary>Unikátní název transformu (pro logging/diagnostiku).</summary>
    string Name { get; }

    /// <summary>
    /// Aplikuje transformaci na model.
    /// Nesmí mutovat vstup — vrací nový model.
    /// </summary>
    TypeModel Apply(TypeModel model, TransformContext context);
}
