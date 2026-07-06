using System.Text;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Generators;

/// <summary>
/// Renderuje Expression a Statement AST do C# kódu.
/// Podporuje: binary/unary/constant/method-call/member-access/conditional expressiony
/// a block/return/if/for/while/assignment/expression statementy.
/// </summary>
public sealed class ExpressionRenderer
{
    private int _indent;

    /// <summary>
    /// Vyrenderuje celý blok statementů (tělo metody).
    /// </summary>
    public string Render(BlockStatement block)
    {
        _indent = 0;
        return RenderStatement(block);
    }

    /// <summary>
    /// Vyrenderuje tělo metody s počátečním odsazením.
    /// </summary>
    public string Render(BlockStatement block, int indent)
    {
        _indent = indent;
        return RenderStatement(block);
    }

    // ========================================================================
    // STATEMENTY
    // ========================================================================

    /// <summary>
    /// Typově bezpečný dispatch — renderuje libovolný Statement do C#.
    /// </summary>
    public string RenderStatement(Statement stmt) => stmt switch
    {
        BlockStatement block => RenderBlock(block),
        ReturnStatement ret => RenderReturn(ret),
        IfStatement ifs => RenderIf(ifs),
        ForStatement forS => RenderFor(forS),
        WhileStatement whileS => RenderWhile(whileS),
        AssignmentStatement assign => RenderAssignment(assign),
        ExpressionStatement exprStmt => RenderExprStmt(exprStmt),
        _ => $"{Indent()}/* Neznámý statement: {stmt.StatementKind} */"
    };

    private string RenderBlock(BlockStatement block)
    {
        var sb = new StringBuilder();

        if (_indent > 0)
            sb.AppendLine("{");
        else
            sb.AppendLine("{");

        _indent++;
        foreach (var stmt in block.Statements)
        {
            sb.AppendLine(RenderStatement(stmt));
        }
        _indent--;

        sb.Append(Indent() + "}");
        return sb.ToString();
    }

    private string RenderReturn(ReturnStatement ret)
    {
        if (ret.Value is null)
            return $"{Indent()}return;";

        var expr = RenderExpression(ret.Value);
        return $"{Indent()}return {expr};";
    }

    private string RenderIf(IfStatement ifs)
    {
        var sb = new StringBuilder();
        var condition = RenderExpression(ifs.Condition);

        sb.Append($"{Indent()}if ({condition})");
        sb.AppendLine();

        if (ifs.TrueBranch is not null)
        {
            _indent++;
            sb.AppendLine(RenderStatement(ifs.TrueBranch));
            _indent--;
        }
        else
        {
            _indent++;
            sb.AppendLine($"{Indent()}{{ }}");
            _indent--;
        }

        if (ifs.FalseBranch is not null)
        {
            sb.AppendLine($"{Indent()}else");
            _indent++;
            sb.AppendLine(RenderStatement(ifs.FalseBranch));
            _indent--;
        }

        return sb.ToString().TrimEnd();
    }

    private string RenderFor(ForStatement forS)
    {
        var sb = new StringBuilder();
        var init = $"int {forS.Variable} = {RenderExpression(forS.Start)}";
        var condition = $"{forS.Variable} < {RenderExpression(forS.End)}";
        var increment = $"{forS.Variable}++";

        sb.Append($"{Indent()}for ({init}; {condition}; {increment})");
        sb.AppendLine();

        if (forS.Body is not null)
        {
            _indent++;
            sb.AppendLine(RenderStatement(forS.Body));
            _indent--;
        }
        else
        {
            _indent++;
            sb.AppendLine($"{Indent()}{{ }}");
            _indent--;
        }

        return sb.ToString().TrimEnd();
    }

    private string RenderWhile(WhileStatement whileS)
    {
        var sb = new StringBuilder();
        var condition = RenderExpression(whileS.Condition);

        sb.Append($"{Indent()}while ({condition})");
        sb.AppendLine();

        _indent++;
        sb.AppendLine(RenderStatement(whileS.Body));
        _indent--;

        return sb.ToString().TrimEnd();
    }

    private string RenderAssignment(AssignmentStatement assign)
    {
        var value = RenderExpression(assign.Value);
        return $"{Indent()}{assign.Variable} = {value};";
    }

    private string RenderExprStmt(ExpressionStatement stmt)
    {
        var expr = RenderExpression(stmt.Expr);
        return $"{Indent()}{expr};";
    }

    // ========================================================================
    // EXPRESSIONY
    // ========================================================================

    /// <summary>
    /// Renderuje libovolný Expression do C# výrazu (bez středníku).
    /// </summary>
    public string RenderExpression(Expression expr) => expr switch
    {
        ConstantExpression constant => RenderConstant(constant),
        BinaryExpression binary => RenderBinary(binary),
        UnaryExpression unary => RenderUnary(unary),
        MethodCallExpression methodCall => RenderMethodCall(methodCall),
        MemberAccessExpression memberAccess => RenderMemberAccess(memberAccess),
        ConditionalExpression conditional => RenderConditional(conditional),
        _ => $"/* Neznámý výraz: {expr.Kind} */"
    };

    private string RenderConstant(ConstantExpression constant)
    {
        return constant.Value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            char c => $"'{c}'",
            bool b => b ? "true" : "false",
            _ => constant.Value.ToString() ?? "null"
        };
    }

    private string RenderBinary(BinaryExpression binary)
    {
        var left = RenderExpression(binary.Left);
        var op = RenderBinaryOperator(binary.Operator);
        var right = RenderExpression(binary.Right);

        // Krátké operátory (aritmetika) dáváme do závorek pro správnou prioritu
        return op.Length <= 2 ? $"({left} {op} {right})" : $"{left} {op} {right}";
    }

    private string RenderUnary(UnaryExpression unary)
    {
        var op = RenderUnaryOperator(unary.Operator);
        var operand = RenderExpression(unary.Operand);
        return $"{op}{operand}";
    }

    private string RenderMethodCall(MethodCallExpression methodCall)
    {
        var args = string.Join(", ", methodCall.Arguments.Select(RenderExpression));
        return $"{methodCall.MethodName}({args})";
    }

    private string RenderMemberAccess(MemberAccessExpression memberAccess)
    {
        return memberAccess.MemberPath;
    }

    private string RenderConditional(ConditionalExpression conditional)
    {
        var condition = RenderExpression(conditional.Condition);
        var whenTrue = RenderExpression(conditional.WhenTrue);
        var whenFalse = RenderExpression(conditional.WhenFalse);
        return $"{condition} ? {whenTrue} : {whenFalse}";
    }

    // ========================================================================
    // OPERÁTORY
    // ========================================================================

    private static string RenderBinaryOperator(BinaryOperator op) => op switch
    {
        BinaryOperator.Add => "+",
        BinaryOperator.Subtract => "-",
        BinaryOperator.Multiply => "*",
        BinaryOperator.Divide => "/",
        BinaryOperator.Modulo => "%",
        BinaryOperator.Equal => "==",
        BinaryOperator.NotEqual => "!=",
        BinaryOperator.GreaterThan => ">",
        BinaryOperator.LessThan => "<",
        BinaryOperator.GreaterThanOrEqual => ">=",
        BinaryOperator.LessThanOrEqual => "<=",
        BinaryOperator.And => "&&",
        BinaryOperator.Or => "||",
        BinaryOperator.Concat => "+",
        BinaryOperator.NullCoalesce => "??",
        _ => "??"
    };

    private static string RenderUnaryOperator(UnaryOperator op) => op switch
    {
        UnaryOperator.Not => "!",
        UnaryOperator.Negate => "-",
        UnaryOperator.BitwiseNot => "~",
        UnaryOperator.Increment => "++",
        UnaryOperator.Decrement => "--",
        _ => "?"
    };

    // ========================================================================
    // POMOCNÉ
    // ========================================================================

    private string Indent() => _indent > 0 ? new string(' ', _indent * 4) : "";
}
