using System.Text;
using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Generators;

/// <summary>
/// Renderuje ComputedExpression do C# kódu.
/// Pokrývá všechny běžné C# výrazy a statementy: podmínky, cykly, volání metod, operátory.
/// </summary>
public sealed class ExpressionRenderer
{
    private int _indent;

    /// <summary>
    /// Přeloží ComputedExpression na C# kód.
    /// </summary>
    public string Render(ComputedExpression expr)
    {
        _indent = 0;
        return RenderInternal(expr);
    }

    /// <summary>
    /// Přeloží ComputedExpression na C# kód s počátečním odsazením.
    /// </summary>
    public string Render(ComputedExpression expr, int indent)
    {
        _indent = indent;
        return RenderInternal(expr);
    }

    private string RenderInternal(ComputedExpression expr)
    {
        return expr.Operation.OperationId switch
        {
            "return" => $"{Indent()}return {RenderOperand(expr.Operands.FirstOrDefault())};",
            "assign" => $"{Indent()}{RenderOperand(expr.Operands.FirstOrDefault())} = {RenderOperand(expr.Operands.Skip(1).FirstOrDefault())};",
            "declare-variable" => RenderVariableDeclaration(expr),
            "throw-if-null" => RenderThrowIfNull(expr),
            "throw-if-empty" => RenderThrowIfEmpty(expr),
            "comparison" => RenderComparison(expr),
            "member-access" => $"{RenderOperand(expr.Operands.FirstOrDefault())}.{RenderOperand(expr.Operands.Skip(1).FirstOrDefault())}",
            "string-format" => RenderStringFormat(expr),
            "raw" => RenderRaw(expr),
            "literal" => RenderLiteral(expr),
            "variable-ref" => RenderVariableRef(expr),
            "binary" => RenderBinary(expr),
            "unary" => RenderUnary(expr),
            "ternary" => RenderTernary(expr),
            "method-call" => RenderMethodCall(expr),
            "block" => RenderBlock(expr),
            "if" => RenderIf(expr),
            "for" => RenderFor(expr),
            "while" => RenderWhile(expr),
            "throw" => RenderThrow(expr),
            _ => $"/* Neznámá operace: {expr.Operation.OperationId} */"
        };
    }

    // === Operandy ===

    private string RenderOperand(Expression? operand)
    {
        if (operand is null) return "null";

        if (operand is ComputedExpression computed)
            return RenderInternal(computed);

        return operand.ToString() ?? "null";
    }

    // === Raw / Literal / Variable ===

    private string RenderRaw(ComputedExpression expr)
    {
        // První operand obsahuje raw kód
        var raw = expr.Operands.FirstOrDefault();
        return raw?.ToString() ?? "";
    }

    private string RenderLiteral(ComputedExpression expr)
    {
        // První operand je hodnota literálu
        return RenderOperand(expr.Operands.FirstOrDefault());
    }

    private string RenderVariableRef(ComputedExpression expr)
    {
        // První operand je název proměnné
        return expr.Operands.FirstOrDefault()?.ToString() ?? "unknown";
    }

    // === Binární a unární operace ===

    private string RenderBinary(ComputedExpression expr)
    {
        // operands[0] = left, operands[1] = operator, operands[2] = right
        var left = RenderOperand(expr.Operands.ElementAtOrDefault(0));
        var op = expr.Operands.Count > 1
            ? RenderOperand(expr.Operands[1])
            : "+";
        var right = expr.Operands.Count > 2
            ? RenderOperand(expr.Operands[2])
            : "null";

        // Zjednodušíme: pokud je operátor krátký, zabalíme do závorek
        return op.Length <= 4 ? $"({left} {op} {right})" : $"{left} {op} {right}";
    }

    private string RenderUnary(ComputedExpression expr)
    {
        // operands[0] = operator, operands[1] = operand (prefix)
        // nebo operands[0] = operand with implicit operator (postfix)
        if (expr.Operands.Count >= 2)
        {
            var op = RenderOperand(expr.Operands[0]);
            var operand = RenderOperand(expr.Operands[1]);
            return $"{op}{operand}";
        }

        return RenderOperand(expr.Operands.FirstOrDefault());
    }

    // === Ternární výraz ===

    private string RenderTernary(ComputedExpression expr)
    {
        // operands[0] = condition, operands[1] = trueBranch, operands[2] = falseBranch
        var condition = RenderOperand(expr.Operands.ElementAtOrDefault(0));
        var trueBranch = RenderOperand(expr.Operands.ElementAtOrDefault(1));
        var falseBranch = RenderOperand(expr.Operands.ElementAtOrDefault(2));

        return $"{condition} ? {trueBranch} : {falseBranch}";
    }

    // === Volání metody ===

    private string RenderMethodCall(ComputedExpression expr)
    {
        // operands[0] = target (volitelné), operands[1] = methodName, operands[2..] = arguments
        var target = expr.Operands.Count > 0 ? RenderOperand(expr.Operands[0]) : null;
        var methodBaseIdx = target != null ? 1 : 0;
        var methodName = expr.Operands.Count > methodBaseIdx
            ? RenderOperand(expr.Operands[methodBaseIdx])
            : "UnknownMethod";
        var args = expr.Operands.Skip(methodBaseIdx + 1).Select(RenderOperand);

        var targetPrefix = target != null ? $"{target}." : "";
        return $"{targetPrefix}{methodName}({string.Join(", ", args)})";
    }

    // === Blok ===

    private string RenderBlock(ComputedExpression expr)
    {
        // Každý operand je statement uvnitř bloku
        var sb = new StringBuilder();
        sb.AppendLine("{");

        _indent++;
        foreach (var stmt in expr.Operands)
        {
            if (stmt is ComputedExpression computed)
                sb.AppendLine(RenderInternal(computed));
        }
        _indent--;

        sb.Append(Indent() + "}");
        return sb.ToString();
    }

    // === Podmínky ===

    private string RenderIf(ComputedExpression expr)
    {
        // operands[0] = condition, operands[1] = then, operands[2] = else (volitelné)
        var condition = RenderOperand(expr.Operands.ElementAtOrDefault(0));
        var sb = new StringBuilder();

        sb.Append($"{Indent()}if ({condition})");
        sb.AppendLine();

        if (expr.Operands.Count > 1 && expr.Operands[1] is ComputedExpression thenExpr)
        {
            _indent++;
            var thenCode = RenderInternal(thenExpr);
            _indent--;
            sb.AppendLine(thenCode);
        }

        if (expr.Operands.Count > 2 && expr.Operands[2] is ComputedExpression elseExpr)
        {
            sb.AppendLine($"{Indent()}else");
            _indent++;
            var elseCode = RenderInternal(elseExpr);
            _indent--;
            sb.AppendLine(elseCode);
        }

        return sb.ToString().TrimEnd();
    }

    // === Cykly ===

    private string RenderFor(ComputedExpression expr)
    {
        // operands[0] = init, operands[1] = condition, operands[2] = increment, operands[3] = body
        var init = expr.Operands.Count > 0 ? RenderOperand(expr.Operands[0]) : "int i = 0";
        var condition = expr.Operands.Count > 1 ? RenderOperand(expr.Operands[1]) : "i < 10";
        var increment = expr.Operands.Count > 2 ? RenderOperand(expr.Operands[2]) : "i++";
        var sb = new StringBuilder();

        sb.Append($"{Indent()}for ({init}; {condition}; {increment})");
        sb.AppendLine();

        if (expr.Operands.Count > 3 && expr.Operands[3] is ComputedExpression bodyExpr)
        {
            _indent++;
            var bodyCode = RenderInternal(bodyExpr);
            _indent--;
            sb.AppendLine(bodyCode);
        }

        return sb.ToString().TrimEnd();
    }

    private string RenderWhile(ComputedExpression expr)
    {
        // operands[0] = condition, operands[1] = body
        var condition = RenderOperand(expr.Operands.ElementAtOrDefault(0));
        var sb = new StringBuilder();

        sb.Append($"{Indent()}while ({condition})");
        sb.AppendLine();

        if (expr.Operands.Count > 1 && expr.Operands[1] is ComputedExpression bodyExpr)
        {
            _indent++;
            var bodyCode = RenderInternal(bodyExpr);
            _indent--;
            sb.AppendLine(bodyCode);
        }

        return sb.ToString().TrimEnd();
    }

    // === Throw ===

    private string RenderThrow(ComputedExpression expr)
    {
        // operands[0] = exception type, operands[1] = message (volitelné)
        var exceptionType = expr.Operands.Count > 0
            ? RenderOperand(expr.Operands[0])
            : "InvalidOperationException";
        var message = expr.Operands.Count > 1
            ? RenderOperand(expr.Operands[1])
            : null;

        return message != null
            ? $"throw new {exceptionType}({message});"
            : $"throw new {exceptionType}();";
    }

    // === Stávající operace (vylepšené) ===

    private string RenderVariableDeclaration(ComputedExpression expr)
    {
        // operands[0] = type, operands[1] = name, operands[2] = initializer (volitelné)
        var type = expr.Operands.Count > 0 ? RenderOperand(expr.Operands[0]) : "var";
        var name = expr.Operands.Count > 1 ? RenderOperand(expr.Operands[1]) : "value";
        var init = expr.Operands.Count > 2 ? $" = {RenderOperand(expr.Operands[2])}" : "";
        return $"{Indent()}{type} {name}{init};";
    }

    private string RenderThrowIfNull(ComputedExpression expr)
    {
        var operand = expr.Operands.FirstOrDefault();
        var name = RenderOperand(operand);
        return $"{Indent()}if ({name} is null) throw new ArgumentNullException(nameof({name}));";
    }

    private string RenderThrowIfEmpty(ComputedExpression expr)
    {
        var operand = expr.Operands.FirstOrDefault();
        var name = RenderOperand(operand);
        return $"{Indent()}if (string.IsNullOrWhiteSpace({name})) throw new ArgumentException(\"{name} cannot be empty.\", nameof({name}));";
    }

    private string RenderComparison(ComputedExpression expr)
    {
        var left = RenderOperand(expr.Operands.FirstOrDefault());
        var op = expr.Operands.Count > 1 ? RenderOperand(expr.Operands[1]) : "==";
        var right = expr.Operands.Count > 2 ? RenderOperand(expr.Operands[2]) : "null";
        return $"{left} {op} {right}";
    }

    private string RenderStringFormat(ComputedExpression expr)
    {
        var parts = expr.Operands.Select(o => RenderOperand(o));
        return $"$\"{string.Join("", parts)}\"";
    }

    // === Pomocné ===

    private string Indent() => _indent > 0 ? new string(' ', _indent * 4) : "";
}
