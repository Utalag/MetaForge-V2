namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class LiteralNode : ExpressionNode
{
    public override string NodeType => "Literal";
    public string Value { get; init; } = "";
    public string? CSharpType { get; init; }

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
