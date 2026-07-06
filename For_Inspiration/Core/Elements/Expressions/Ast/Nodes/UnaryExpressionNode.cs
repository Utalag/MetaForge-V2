namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class UnaryExpressionNode : ExpressionNode
{
    public override string NodeType => "Unary";
    public string Operator { get; init; } = "";
    public ExpressionNode Operand { get; init; } = EmptyNode.Instance;
    public bool IsPrefix { get; init; } = true;

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
