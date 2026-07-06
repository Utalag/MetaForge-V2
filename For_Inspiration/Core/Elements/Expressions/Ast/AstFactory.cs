using System.Text.Json;
using MetaForge.Core.Elements.Expressions.Ast.Nodes;

namespace MetaForge.Core.Elements.Expressions.Ast;

public static class AstFactory
{
    public static ExpressionNode? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            return ParseNode(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static ExpressionNode? ParseNode(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var type = ReadString(element, "type") ?? InferType(element);
        return type?.ToLowerInvariant() switch
        {
            "binary" => ParseBinaryFromJson(element),
            "variableref" => ParseVariableRefFromJson(element),
            "methodcall" => ParseMethodCallFromJson(element),
            "literal" => ParseLiteralFromJson(element),
            "conditional" => ParseConditionalFromJson(element),
            _ => null,
        };
    }

    private static BinaryExpressionNode ParseBinaryFromJson(JsonElement element)
    {
        var left = TryParseChildNode(element, "left") ?? new VariableRefNode { Name = "a" };
        var right = TryParseChildNode(element, "right") ?? new VariableRefNode { Name = "b" };

        return new BinaryExpressionNode
        {
            Operator = ReadString(element, "operator") ?? "+",
            Left = left,
            Right = right,
        };
    }

    private static VariableRefNode ParseVariableRefFromJson(JsonElement element)
    {
        var name = ReadString(element, "name") ?? "unknown";
        var binding = ParseBinding(ReadString(element, "binding"));

        return new VariableRefNode
        {
            Name = name,
            Binding = binding,
            SourceName = ReadString(element, "sourceName") ?? name,
            BindRef = ReadString(element, "bindRef"),
        };
    }

    private static MethodCallNode ParseMethodCallFromJson(JsonElement element)
    {
        var arguments = new List<ExpressionNode>();
        if (element.TryGetProperty("arguments", out var argsElement) && argsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var argument in argsElement.EnumerateArray())
            {
                if (ParseNode(argument) is { } parsedArgument)
                    arguments.Add(parsedArgument);
            }
        }

        return new MethodCallNode
        {
            MethodName = ReadString(element, "method") ?? ReadString(element, "methodName") ?? "Method",
            Arguments = arguments,
            Target = TryParseChildNode(element, "target"),
        };
    }

    private static LiteralNode ParseLiteralFromJson(JsonElement element)
    {
        return new LiteralNode
        {
            Value = ReadScalar(element, "value") ?? "0",
            CSharpType = ReadString(element, "csharpType"),
        };
    }

    private static ConditionalNode ParseConditionalFromJson(JsonElement element)
    {
        return new ConditionalNode
        {
            Condition = TryParseChildNode(element, "condition") ?? new VariableRefNode { Name = "cond" },
            TrueBranch = TryParseChildNode(element, "trueBranch") ?? new LiteralNode { Value = "true" },
            FalseBranch = TryParseChildNode(element, "falseBranch") ?? new LiteralNode { Value = "false" },
        };
    }

    private static ExpressionNode? TryParseChildNode(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var childElement))
            return null;

        return ParseNode(childElement);
    }

    private static string? InferType(JsonElement element)
    {
        if (element.TryGetProperty("operator", out _) && element.TryGetProperty("left", out _) && element.TryGetProperty("right", out _))
            return "Binary";

        if (element.TryGetProperty("condition", out _) && element.TryGetProperty("trueBranch", out _) && element.TryGetProperty("falseBranch", out _))
            return "Conditional";

        if (element.TryGetProperty("method", out _) || element.TryGetProperty("methodName", out _))
            return "MethodCall";

        if (element.TryGetProperty("value", out _))
            return "Literal";

        if (element.TryGetProperty("name", out _))
            return "VariableRef";

        return null;
    }

    private static BindingType ParseBinding(string? binding)
    {
        return binding?.ToLowerInvariant() switch
        {
            "property" => BindingType.Property,
            "localvariable" => BindingType.LocalVariable,
            "unknown" => BindingType.Unknown,
            _ => BindingType.Parameter,
        };
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? ReadScalar(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            _ => property.GetRawText(),
        };
    }
}
