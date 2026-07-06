namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class IfStatement : StatementNode
{
    public override string NodeType => "If";
    public ExpressionNode Condition { get; init; } = EmptyNode.Instance;
    public StatementNode? TrueBranch { get; init; }
    public StatementNode? FalseBranch { get; init; }

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
