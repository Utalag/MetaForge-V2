using MetaForge.Core.Elements.Expressions.Ast.Nodes;

namespace MetaForge.Core.Elements.Expressions.Ast;

public interface IAstNodeVisitor<T>
{
    T Visit(AssignmentStatement node);
    T Visit(BinaryExpressionNode node);
    T Visit(BlockStatement node);
    T Visit(ConditionalNode node);
    T Visit(EmptyNode node);
    T Visit(ExpressionStatement node);
    T Visit(ForStatement node);
    T Visit(IfStatement node);
    T Visit(LiteralNode node);
    T Visit(MethodCallNode node);
    T Visit(ReturnStatement node);
    T Visit(UnaryExpressionNode node);
    T Visit(VariableRefNode node);
    T Visit(WhileStatement node);
}