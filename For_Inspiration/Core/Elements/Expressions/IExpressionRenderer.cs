using MetaForge.Core.Common;

namespace MetaForge.Core.Elements.Expressions;

public interface IExpressionRenderer
{
    ProgramLanguage Language { get; }
    string Render(ComputedExpression expression);
    string RenderComment(Comment comment);
}
