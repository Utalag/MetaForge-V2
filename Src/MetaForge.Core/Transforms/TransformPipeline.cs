using MetaForge.Core.DataTypes;
using MetaForge.Core.Diagnostics;

namespace MetaForge.Core.Transforms;

/// <summary>
/// Transformační pipeline — řetězí IModelTransformy.
/// Každý transform je čistá funkce. Pipeline se zastaví při první chybě (FailFast).
/// </summary>
public sealed class TransformPipeline
{
    private readonly List<IModelTransform> _transforms = [];

    /// <summary>Počet registrovaných transformů.</summary>
    public int Count => _transforms.Count;

    /// <summary>Přidá transform na konec pipeline.</summary>
    public TransformPipeline Add(IModelTransform transform)
    {
        _transforms.Add(transform);
        return this;
    }

    /// <summary>
    /// Podmíněně přidá transform — jen pokud predikát vrátí true
    /// při vyhodnocení v runtime (při spuštění pipeline).
    /// </summary>
    public TransformPipeline AddIf(Func<TransformContext, bool> predicate, IModelTransform transform)
    {
        _transforms.Add(new ConditionalTransform(predicate, transform));
        return this;
    }

    /// <summary>
    /// Spustí pipeline na modelu.
    /// </summary>
    public BuildResult<TypeModel> Run(TypeModel model, PipelineOptions? options = null)
    {
        var bag = new DiagnosticBag();
        var ctx = new TransformContext(bag, options ?? new PipelineOptions());
        var current = model;

        foreach (var step in _transforms)
        {
            current = step.Apply(current, ctx);

            if (ctx.Diagnostics.HasErrors && ctx.Options.FailFast)
                break;
        }

        return new BuildResult<TypeModel>(current, bag);
    }

    /// <summary>
    /// Vnitřní wrapper pro podmíněné transformy.
    /// </summary>
    private sealed class ConditionalTransform : IModelTransform
    {
        private readonly Func<TransformContext, bool> _predicate;
        private readonly IModelTransform _inner;

        public string Name => _inner.Name;

        public ConditionalTransform(Func<TransformContext, bool> predicate, IModelTransform inner)
        {
            _predicate = predicate;
            _inner = inner;
        }

        public TypeModel Apply(TypeModel model, TransformContext context) =>
            _predicate(context) ? _inner.Apply(model, context) : model;
    }
}
