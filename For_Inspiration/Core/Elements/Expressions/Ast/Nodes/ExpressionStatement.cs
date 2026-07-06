namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class ExpressionStatement : StatementNode
{
    public override string NodeType => "Expression";
    public ExpressionNode Expression { get; init; } = EmptyNode.Instance;

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
