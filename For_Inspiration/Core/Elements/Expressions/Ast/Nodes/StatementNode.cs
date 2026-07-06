namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public abstract class StatementNode : IAstNode
{
    public abstract string NodeType { get; }
    public int Depth { get; set; }
    public abstract T Accept<T>(IAstNodeVisitor<T> visitor);
}
