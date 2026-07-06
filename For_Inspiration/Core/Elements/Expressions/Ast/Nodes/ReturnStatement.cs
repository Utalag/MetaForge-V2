namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class ReturnStatement : StatementNode
{
    public override string NodeType => "Return";
    public ExpressionNode? Value { get; init; }

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
