namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class BinaryExpressionNode : ExpressionNode
{
    public override string NodeType => "Binary";
    public string Operator { get; init; } = "";
    public ExpressionNode Left { get; init; } = EmptyNode.Instance;
    public ExpressionNode Right { get; init; } = EmptyNode.Instance;

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
