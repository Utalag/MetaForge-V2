namespace MetaForge.Core.Elements.Expressions.Ast.Nodes;

public class AssignmentStatement : StatementNode
{
    public override string NodeType => "Assignment";
    public string VariableName { get; init; } = "";
    public ExpressionNode Value { get; init; } = EmptyNode.Instance;

    public override T Accept<T>(IAstNodeVisitor<T> visitor) => visitor.Visit(this);
}
