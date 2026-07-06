namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class MethodCallNode : ExpressionNode
{
    public override string NodeType => "MethodCall";
    public string MethodName { get; init; } = "";
    public List<ExpressionNode> Arguments { get; init; } = new();
    public ExpressionNode? Target { get; init; }

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
