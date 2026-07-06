using MetaForge.Core.Common;

namespace MetaForge.Core.Elements.Expressions;

public static class ExpressionRendererRegistry
{
    private static readonly Dictionary<ProgramLanguage, IExpressionRenderer> _renderers = new();
    private static IExpressionRenderer? _defaultRenderer;

    public static void Register(IExpressionRenderer renderer)
    {
        _renderers[renderer.Language] = renderer;
    }

    public static IExpressionRenderer? Get(ProgramLanguage language)
    {
        return _renderers.TryGetValue(language, out var renderer) ? renderer : _defaultRenderer;
    }

    public static void SetDefault(IExpressionRenderer renderer)
    {
        _defaultRenderer = renderer;
    }

    public static void Clear()
    {
        _renderers.Clear();
        _defaultRenderer = null;
    }
}
