namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class ForStatement : StatementNode
{
    public override string NodeType => "For";
    public string Variable { get; init; } = "";
    public ExpressionNode Start { get; init; } = EmptyNode.Instance;
    public ExpressionNode End { get; init; } = EmptyNode.Instance;
    public StatementNode? Body { get; init; }

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
