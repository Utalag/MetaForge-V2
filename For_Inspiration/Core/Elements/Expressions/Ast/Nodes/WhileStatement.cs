namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class WhileStatement : StatementNode
{
    public override string NodeType => "While";
    public ExpressionNode Condition { get; init; } = EmptyNode.Instance;
    public StatementNode? Body { get; init; }

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
