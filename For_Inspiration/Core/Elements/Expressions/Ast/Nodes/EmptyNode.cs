namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class EmptyNode : ExpressionNode
{
    public static EmptyNode Instance { get; } = new();
    public override string NodeType => "Empty";

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
