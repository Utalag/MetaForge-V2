namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class ConditionalNode : ExpressionNode
{
    public override string NodeType => "Conditional";
    public ExpressionNode Condition { get; init; } = EmptyNode.Instance;
    public ExpressionNode TrueBranch { get; init; } = EmptyNode.Instance;
    public ExpressionNode FalseBranch { get; init; } = EmptyNode.Instance;

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
