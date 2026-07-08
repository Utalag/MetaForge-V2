namespace MetaForge.Core.Transforms;

/// <summary>
/// Volby pro spuštění transformační pipeline.
/// </summary>
public sealed class PipelineOptions
{
    /// <summary>Zastavit pipeline při první chybě?</summary>
    public bool FailFast { get; set; } = true;

    /// <summary>
    /// Povolit AttributeReflectionTransform (automatické mapování
    /// AttributeElement → MetadataBag)?
    /// </summary>
    public bool EnableReflection { get; set; } = true;
}
