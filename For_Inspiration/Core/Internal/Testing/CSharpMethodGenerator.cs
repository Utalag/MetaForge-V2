using System.Text;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Modifiers;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Internal.Testing;

internal static class CSharpMethodGenerator
{
    public static string Generate(Method method)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace MetaForge.Internal.Generated {{");
        sb.AppendLine($"    public class {SanitizeClassName(method.Name)}_Method {{");

        var parameters = string.Join(", ", method.Parameters.Select(p =>
            $"{GetCSharpType(p.Type)} {p.Name}"));

        var accessModifier = GetCSharpModifier(method.AccessModifier);
        var staticModifier = method.IsStatic ? "static " : "";
        var asyncModifier = method.IsAsync ? "async " : "";
        var returnType = GetCSharpType(method.ReturnType);

        sb.AppendLine($"        {accessModifier}{staticModifier}{asyncModifier}{returnType} {method.Name}({parameters}) {{");

        foreach (var expr in method.BodyExpressions)
        {
            sb.AppendLine($"            {expr.GenerateCode()}");
        }

        if (!method.BodyExpressions.Any())
        {
            sb.AppendLine("            throw new NotImplementedException();");
        }

        sb.AppendLine($"        }}");
        sb.AppendLine($"    }}");
        sb.AppendLine($"}}");

        return sb.ToString();
    }

    internal static string GetCSharpType(TypeModel type) => type.BaseType switch
    {
        DataType.String => "string",
        DataType.Int => "int",
        DataType.Long => "long",
        DataType.Double => "double",
        DataType.Float => "float",
        DataType.Decimal => "decimal",
        DataType.Boolean => "bool",
        DataType.DateTime => "DateTime",
        DataType.Guid => "Guid",
        DataType.Object => "object",
        DataType.Custom => type.CustomTypeName ?? "object",
        _ => "object"
    };

    private static string GetCSharpModifier(AccessModifier modifier) => modifier switch
    {
        AccessModifier.Public => "public",
        AccessModifier.Private => "private",
        AccessModifier.Protected => "protected",
        AccessModifier.Internal => "internal",
        _ => "public"
    };

    internal static string GetDefaultValue(TypeModel type) => type.BaseType switch
    {
        DataType.String => "string.Empty",
        DataType.Int => "0",
        DataType.Long => "0L",
        DataType.Double => "0.0",
        DataType.Float => "0.0f",
        DataType.Decimal => "0m",
        DataType.Boolean => "false",
        DataType.DateTime => "DateTime.MinValue",
        DataType.Guid => "Guid.Empty",
        DataType.Object => "null",
        _ => "null"
    };

    internal static string SanitizeClassName(string name)
    {
        var sb = new StringBuilder();
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else sb.Append('_');
        }
        return sb.ToString();
    }
}
