namespace MetaForge.Core.Elements.Expressions.Ast;

public interface IAstNode
{
    string NodeType { get; }
    int Depth { get; set; }
    T Accept<T>(IAstNodeVisitor<T> visitor);
}
