namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class BlockStatement : StatementNode
{
    public override string NodeType => "Block";
    public List<StatementNode> Statements { get; init; } = new();

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
