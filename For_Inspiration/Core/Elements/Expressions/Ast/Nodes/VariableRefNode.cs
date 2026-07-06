using MetaForge.Core.Elements.Expressions.Ast;

namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class VariableRefNode : ExpressionNode
{
    public override string NodeType => "VariableRef";
    public string Name { get; init; } = "";
    public BindingType Binding { get; init; } = BindingType.Parameter;
    public string? SourceName { get; init; }
    public string? BindRef { get; set; }

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
