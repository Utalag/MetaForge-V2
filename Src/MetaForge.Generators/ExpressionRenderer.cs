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
        SwitchStatement switchS => RenderSwitch(switchS),
        ForEachStatement forEach => RenderForEach(forEach),
        TryCatchStatement tryCatch => RenderTryCatch(tryCatch),
        UsingStatement usingS => RenderUsing(usingS),
        LocalFunctionStatement localFunc => RenderLocalFunction(localFunc),
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

    private string RenderSwitch(SwitchStatement switchS)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Indent()}switch ({RenderExpression(switchS.Selector)})");
        sb.AppendLine($"{Indent()}{{");

        _indent++;
        foreach (var switchCase in switchS.Cases)
        {
            sb.Append(Indent());
            sb.Append(switchCase.Pattern is null ? "default" : $"case {RenderPattern(switchCase.Pattern)}");
            if (switchCase.Guard is not null)
                sb.Append($" when {RenderExpression(switchCase.Guard)}");
            sb.AppendLine(":");

            _indent++;
            sb.AppendLine(RenderStatement(switchCase.Body));
            sb.AppendLine($"{Indent()}break;");
            _indent--;
        }
        _indent--;

        sb.Append($"{Indent()}}}");
        return sb.ToString();
    }

    private string RenderPattern(PatternExpression pattern) => pattern.PatternKind switch
    {
        PatternKind.Discard => "_",
        PatternKind.Var => $"var {pattern.BindingName}",
        PatternKind.Type => pattern.BindingName is null ? pattern.TypeName! : $"{pattern.TypeName} {pattern.BindingName}",
        PatternKind.Constant => RenderConstantValue(pattern.ConstantValue),
        PatternKind.Relational => pattern.ConstantValue?.ToString() ?? "_",
        _ => "_",
    };

    private static string RenderConstantValue(object? value) => value switch
    {
        null => "null",
        string s => $"\"{s}\"",
        char c => $"'{c}'",
        bool b => b ? "true" : "false",
        _ => value.ToString() ?? "null",
    };

    private string RenderForEach(ForEachStatement forEach)
    {
        var sb = new StringBuilder();
        var typeName = forEach.ElementType is null ? "var" : RenderTypeName(forEach.ElementType);
        sb.Append($"{Indent()}foreach ({typeName} {forEach.Variable} in {RenderExpression(forEach.Collection)})");
        sb.AppendLine();

        _indent++;
        sb.AppendLine(RenderStatement(forEach.Body));
        _indent--;

        return sb.ToString().TrimEnd();
    }

    private string RenderTryCatch(TryCatchStatement tryCatch)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Indent()}try");
        sb.AppendLine(RenderStatement(tryCatch.TryBlock));

        foreach (var clause in tryCatch.CatchClauses)
        {
            sb.Append(Indent());
            sb.Append("catch");
            if (clause.ExceptionType is not null)
            {
                sb.Append($" ({RenderTypeName(clause.ExceptionType)}");
                if (clause.VariableName is not null)
                    sb.Append($" {clause.VariableName}");
                sb.Append(')');
            }
            if (clause.Filter is not null)
                sb.Append($" when ({RenderExpression(clause.Filter)})");
            sb.AppendLine();
            sb.AppendLine(RenderStatement(clause.Body));
        }

        if (tryCatch.FinallyBlock is not null)
        {
            sb.AppendLine($"{Indent()}finally");
            sb.AppendLine(RenderStatement(tryCatch.FinallyBlock));
        }

        return sb.ToString().TrimEnd();
    }

    private string RenderUsing(UsingStatement usingS)
    {
        var typeName = usingS.VariableType is null ? "var" : RenderTypeName(usingS.VariableType);
        var declaration = $"{typeName} {usingS.Variable} = {RenderExpression(usingS.Resource)}";

        if (usingS.Body is null)
            return $"{Indent()}using {declaration};";

        var sb = new StringBuilder();
        sb.AppendLine($"{Indent()}using ({declaration})");

        _indent++;
        sb.AppendLine(RenderStatement(usingS.Body));
        _indent--;

        return sb.ToString().TrimEnd();
    }

    private string RenderLocalFunction(LocalFunctionStatement localFunc)
    {
        var sb = new StringBuilder();
        var modifiers = string.Concat(
            localFunc.IsStatic ? "static " : "",
            localFunc.IsAsync ? "async " : "");
        var parameters = string.Join(", ", localFunc.Parameters.Select(
            p => $"{RenderTypeName(p.Type)} {p.Name}"));

        sb.Append($"{Indent()}{modifiers}{RenderTypeName(localFunc.ReturnType)} {localFunc.Name}({parameters})");
        sb.AppendLine();
        sb.Append(RenderStatement(localFunc.Body));

        return sb.ToString();
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
        LambdaExpression lambda => RenderLambda(lambda),
        NewExpression newExpr => RenderNew(newExpr),
        DefaultExpression defaultExpr => RenderDefault(defaultExpr),
        ConversionExpression conversion => RenderConversion(conversion),
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
        var args = string.Join(", ", methodCall.Arguments.Select(RenderArgument));
        return $"{methodCall.MethodName}({args})";
    }

    private string RenderArgument(NamedArgument arg)
    {
        var value = RenderExpression(arg.Value);
        return arg.Name is null ? value : $"{arg.Name}: {value}";
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

    private string RenderLambda(LambdaExpression lambda)
    {
        var parameters = lambda.Parameters.Count == 1
            ? lambda.Parameters[0]
            : $"({string.Join(", ", lambda.Parameters)})";
        return $"{parameters} => {RenderExpression(lambda.Body)}";
    }

    private string RenderNew(NewExpression newExpr)
    {
        var args = string.Join(", ", newExpr.Arguments.Select(RenderArgument));
        var result = $"new {newExpr.TypeName}({args})";

        if (newExpr.Initializers.Count > 0)
        {
            var initializers = string.Join(", ", newExpr.Initializers.Select(
                i => $"{i.Name} = {RenderExpression(i.Value)}"));
            result += $" {{ {initializers} }}";
        }

        return result;
    }

    private string RenderDefault(DefaultExpression defaultExpr)
    {
        return $"default({RenderTypeName(defaultExpr.ResultType)})";
    }

    private string RenderConversion(ConversionExpression conversion)
    {
        var operand = RenderExpression(conversion.Operand);
        return conversion.ConversionKind switch
        {
            ConversionKind.As => $"({operand} as {RenderTypeName(conversion.TargetType)})",
            ConversionKind.Checked => $"checked(({RenderTypeName(conversion.TargetType)}){operand})",
            _ => $"(({RenderTypeName(conversion.TargetType)}){operand})",
        };
    }

    private static string RenderTypeName(MetaForge.Core.DataTypes.TypeModel type) =>
        type.CustomTypeName ?? type.BaseType.ToString();

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
