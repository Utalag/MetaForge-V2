using System.Text;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
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
    /// Vyrenderuje OBSAH bloku — samotné statementy bez { }.
    /// Scriban šablony (Method.scriban, Constructor.scriban) už { } a odsazení poskytují.
    /// </summary>
    public string RenderBodyOnly(BlockStatement block)
    {
        var sb = new StringBuilder();
        foreach (var stmt in block.Statements)
        {
            sb.AppendLine(RenderStatement(stmt));
        }
        return sb.ToString().TrimEnd();
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
        ForEachStatement fe => RenderForEach(fe),
        SwitchStatement sw => RenderSwitch(sw),
        TryCatchStatement tc => RenderTryCatch(tc),
        UsingStatement u => RenderUsing(u),
        UsingDeclarationStatement ud => RenderUsingDeclaration(ud),
        LocalFunctionStatement lf => RenderLocalFunction(lf),
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

    private string RenderForEach(ForEachStatement fe)
    {
        var sb = new StringBuilder();
        var typeHint = fe.VariableType != null ? $"{fe.VariableType} " : "var ";
        var collection = RenderExpression(fe.Collection);
        sb.Append($"{Indent()}foreach ({typeHint}{fe.VariableName} in {collection})");
        sb.AppendLine();
        _indent++;
        sb.AppendLine(fe.Body != null ? RenderStatement(fe.Body) : $"{Indent()}{{ }}");
        _indent--;
        return sb.ToString().TrimEnd();
    }

    private string RenderSwitch(SwitchStatement sw)
    {
        var sb = new StringBuilder();
        var selector = RenderExpression(sw.Selector);
        sb.AppendLine($"{Indent()}switch ({selector})");
        sb.AppendLine($"{Indent()}{{");
        _indent++;
        foreach (var c in sw.Cases)
        {
            sb.AppendLine($"{Indent()}case {RenderExpression(c.Pattern)}:");
            _indent++;
            sb.AppendLine(RenderStatement(c.Body));
            if (!c.Body.ToString()?.Contains("break") == true)
                sb.AppendLine($"{Indent()}break;");
            _indent--;
        }
        if (sw.DefaultCase != null)
        {
            sb.AppendLine($"{Indent()}default:");
            _indent++;
            sb.AppendLine(RenderStatement(sw.DefaultCase));
            _indent--;
        }
        _indent--;
        sb.Append($"{Indent()}}}");
        return sb.ToString();
    }

    private string RenderTryCatch(TryCatchStatement tc)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Indent()}try");
        _indent++;
        sb.AppendLine(RenderStatement(tc.TryBody));
        _indent--;
        foreach (var cc in tc.Catches)
        {
            var exType = cc.ExceptionType ?? "";
            var varName = cc.VariableName ?? "";
            var filter = cc.Filter != null ? $" when ({cc.Filter})" : "";
            sb.AppendLine($"{Indent()}catch ({exType} {varName}){filter}");
            _indent++;
            sb.AppendLine(RenderStatement(cc.Body));
            _indent--;
        }
        if (tc.FinallyBody != null)
        {
            sb.AppendLine($"{Indent()}finally");
            _indent++;
            sb.AppendLine(RenderStatement(tc.FinallyBody));
            _indent--;
        }
        return sb.ToString().TrimEnd();
    }

    private string RenderUsing(UsingStatement u)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Indent()}using ({u.ResourceDeclaration})");
        _indent++;
        sb.AppendLine(u.Body != null ? RenderStatement(u.Body) : $"{Indent()}{{ }}");
        _indent--;
        return sb.ToString().TrimEnd();
    }

    private string RenderUsingDeclaration(UsingDeclarationStatement ud)
    {
        var init = RenderExpression(ud.Initializer);
        return $"{Indent()}using var {ud.VariableName} = {init};";
    }

    private string RenderLocalFunction(LocalFunctionStatement lf)
    {
        var sb = new StringBuilder();
        var method = lf.Function;
        var returnType = MapType(method.ReturnType);
        var parameters = string.Join(", ", method.Parameters.Select(p => RenderParameter(p)));
        sb.Append($"{Indent()}{returnType} {method.Name}({parameters})");
        if (method.ExpressionBody != null)
        {
            sb.AppendLine($" => {RenderExpression(method.ExpressionBody)};");
        }
        else if (method.Body != null)
        {
            sb.AppendLine();
            _indent++;
            sb.AppendLine(RenderStatement(method.Body));
            _indent--;
        }
        else
        {
            sb.AppendLine(";");
        }
        return sb.ToString();
    }

    private static string MapType(MetaForge.Core.DataTypes.TypeModel type) => type?.BaseType switch
    {
        MetaForge.Core.DataTypes.DataType.Void => "void",
        MetaForge.Core.DataTypes.DataType.Int32 => "int",
        MetaForge.Core.DataTypes.DataType.String => "string",
        MetaForge.Core.DataTypes.DataType.Bool => "bool",
        _ => type?.CustomTypeName ?? "var"
    };

    private static string RenderParameter(MetaForge.Core.Elements.Members.ParameterElement p)
    {
        var mod = p.Modifier switch
        {
            ParameterModifier.Ref => "ref ",
            ParameterModifier.Out => "out ",
            ParameterModifier.In => "in ",
            ParameterModifier.Params => "params ",
            _ => ""
        };
        var def = p.HasDefaultValue ? $" = {p.DefaultValue}" : "";
        return $"{mod}{MapType(p.Type)} {p.Name}{def}";
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
        NewExpression newExpr => RenderNew(newExpr),
        AwaitExpression awaitExpr => RenderAwait(awaitExpr),
        ConversionExpression conv => RenderConversion(conv),
        DefaultExpression def => RenderDefault(def),
        IsPatternExpression ip => RenderIsPattern(ip),
        LambdaExpression lam => RenderLambda(lam),
        NullCoalescingExpression nc => RenderNullCoalescing(nc),
        SwitchExpression sw => RenderSwitchExpr(sw),
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

    private string RenderNew(NewExpression newExpr)
    {
        var args = string.Join(", ", newExpr.ConstructorArguments.Select(RenderExpression));
        var bindings = newExpr.MemberBindings?.Count > 0
            ? " { " + string.Join(", ", newExpr.MemberBindings.Select(b => $"{b.MemberName} = {RenderExpression(b.Value)}")) + " }"
            : "";
        return $"new {newExpr.TypeName}({args}){bindings}";
    }

    private string RenderAwait(AwaitExpression awaitExpr)
    {
        return $"await {RenderExpression(awaitExpr.Operand)}";
    }

    private string RenderConversion(ConversionExpression conv)
    {
        if (conv.IsExplicit)
            return $"({conv.TargetType}){RenderExpression(conv.Operand)}";
        return RenderExpression(conv.Operand);
    }

    private string RenderDefault(DefaultExpression def)
    {
        return $"default({def.TargetType})";
    }

    private string RenderIsPattern(IsPatternExpression ip)
    {
        var pattern = ip.TargetTypeName ?? ip.PatternKind switch
        {
            PatternKind.Null => "null",
            PatternKind.Constant => "constant",
            _ => "var"
        };
        var negated = ip.IsNegated ? "not " : "";
        return $"{RenderExpression(ip.Operand)} is {negated}{pattern}";
    }

    private string RenderLambda(LambdaExpression lam)
    {
        var parameters = string.Join(", ", lam.ParameterNames);
        var body = RenderExpression(lam.Body);
        return $"({parameters}) => {body}";
    }

    private string RenderNullCoalescing(NullCoalescingExpression nc)
    {
        return $"{RenderExpression(nc.Left)} ?? {RenderExpression(nc.Right)}";
    }

    private string RenderSwitchExpr(SwitchExpression sw)
    {
        var arms = string.Join(", ", sw.Arms.Select(a => $"{RenderExpression(a.Pattern)} => {RenderExpression(a.Value)}"));
        return $"{RenderExpression(sw.Selector)} switch {{ {arms} }}";
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
