using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Generators;

/// <summary>
/// Generátor C# kódu — jediný aktivní generátor.
/// Převádí Core elementy na kompilovatelný C# kód pomocí Scriban šablon.
/// Není sealed — umožňuje rozšíření o TieredCodeGenerator.
/// </summary>
public class CodeGenerator : BaseCodeGenerator
{
    private const string FileExtension = ".cs";

    /// <summary>Renderer pro Expression a Statement AST.</summary>
    private readonly ExpressionRenderer _renderer = new();

    public override GeneratedCodeArtifact Generate(RootElement element)
    {
        if (string.IsNullOrWhiteSpace(element.Name))
        {
            return new GeneratedCodeArtifact(
                FileName: "error.cs",
                SourceCode: "// ERROR: Element bez názvu",
                Diagnostics: new[] { new DiagnosticInfo("Element nemá nastavený název.", DiagnosticSeverity.Error, element.Id.ToString()) }
            );
        }

        var code = element switch
        {
            ClassElement cls => GenerateClass(cls),
            InterfaceElement iface => GenerateInterface(iface),
            EnumElement enm => GenerateEnum(enm),
            StructElement str => GenerateStruct(str),
            DelegateElement dele => GenerateDelegate(dele),
            _ => $"// Nepodporovaný element typu: {element.GetType().Name}"
        };

        var diagnostics = new List<DiagnosticInfo>();
        if (string.IsNullOrWhiteSpace(code) || code.StartsWith("// Nepodporovaný"))
        {
            diagnostics.Add(new DiagnosticInfo(
                $"Nepodporovaný element: {element.Kind}",
                DiagnosticSeverity.Warning,
                element.Id.ToString(),
                element.Name));
        }

        return new GeneratedCodeArtifact(
            FileName: $"{element.Name}{FileExtension}",
            SourceCode: code,
            Diagnostics: diagnostics.Count > 0 ? diagnostics.AsReadOnly() : null
        );
    }

    // === Generování jednotlivých typů přes šablony ===

    private string GenerateClass(ClassElement cls)
    {
        // Dědičnost
        var inheritance = new List<string>();
        if (!string.IsNullOrWhiteSpace(cls.BaseClassName))
            inheritance.Add(cls.BaseClassName);
        inheritance.AddRange(cls.ImplementedInterfaces);

        var model = new Dictionary<string, object?>
        {
            { "name", cls.Name },
            { "access_modifier", MapAccessModifier(cls.AccessModifier) },
            { "is_static", cls.IsStatic },
            { "is_abstract", cls.IsAbstract },
            { "is_sealed", cls.IsSealed },
            { "is_partial", cls.IsPartial },
            { "is_record", cls.IsRecord },
            { "type_params", RenderTypeParameterList(cls.TypeParameters) },
            { "constraints", RenderConstraintClauses(cls.TypeParameters) },
            { "inheritance", inheritance.Count > 0 ? string.Join(", ", inheritance) : null },
            { "usings", cls.Usings },
            { "attributes", cls.Attributes.Select(RenderAttribute).ToList() },
            { "properties", cls.Properties.Select(RenderProperty).ToList() },
            { "methods", cls.Methods.Select(RenderMethod).ToList() },
            { "events", cls.Events.Select(RenderEvent).ToList() },
            { "operators", cls.Operators.Select(RenderOperator).ToList() }
        };

        return RenderTemplate("Class", model);
    }

    private string GenerateInterface(InterfaceElement iface)
    {
        var model = new Dictionary<string, object?>
        {
            { "name", iface.Name },
            { "access_modifier", MapAccessModifier(iface.AccessModifier) },
            { "type_params", RenderTypeParameterList(iface.TypeParameters) },
            { "constraints", RenderConstraintClauses(iface.TypeParameters) },
            { "usings", iface.Usings },
            { "properties", iface.Properties.Select(RenderPropertySignature).ToList() },
            { "methods", iface.Methods.Select(RenderMethodSignature).ToList() }
        };

        return RenderTemplate("Interface", model);
    }

    private string GenerateEnum(EnumElement enm)
    {
        var model = new Dictionary<string, object?>
        {
            { "name", enm.Name },
            { "access_modifier", MapAccessModifier(enm.AccessModifier) },
            { "underlying_type", enm.UnderlyingType != DataType.Int32 ? MapDataType(enm.UnderlyingType) : null },
            { "is_flags", enm.IsFlags },
            { "usings", enm.Usings },
            { "attributes", enm.Attributes.Select(RenderAttribute).ToList() },
            { "members", enm.Members.Select(m => new Dictionary<string, object?>
            {
                { "name", m.Name },
                { "value", m.Value }
            }).ToList() }
        };

        return RenderTemplate("Enum", model);
    }

    private string GenerateStruct(StructElement str)
    {
        var model = new Dictionary<string, object?>
        {
            { "name", str.Name },
            { "access_modifier", MapAccessModifier(str.AccessModifier) },
            { "is_readonly", str.IsReadOnly },
            { "is_record", str.IsRecord },
            { "type_params", RenderTypeParameterList(str.TypeParameters) },
            { "constraints", RenderConstraintClauses(str.TypeParameters) },
            { "usings", str.Usings },
            { "attributes", str.Attributes.Select(RenderAttribute).ToList() },
            { "properties", str.Properties.Select(RenderProperty).ToList() },
            { "methods", str.Methods.Select(RenderMethod).ToList() },
            { "operators", str.Operators.Select(RenderOperator).ToList() }
        };

        return RenderTemplate("Struct", model);
    }

    // === Renderování memberů do stringů pro šablony ===

    private static string RenderEvent(EventElement evt)
    {
        var accessMod = MapAccessModifier(evt.AccessModifier);
        var staticMod = evt.IsStatic ? "static " : "";
        return $"{accessMod} {staticMod}event {evt.DelegateTypeName} {evt.Name};";
    }

    private string RenderOperator(OperatorElement op)
    {
        var accessMod = MapAccessModifier(op.AccessModifier);
        var returnType = MapType(op.ReturnType);
        var opToken = MapOperatorToken(op.Operator);
        var parameters = string.Join(", ", op.Parameters.Select(p => $"{MapType(p.Type)} {p.Name}"));
        var body = op.Body is null ? "throw new NotImplementedException();" : _renderer.Render(op.Body);

        return $$"""
            {{accessMod}} static {{returnType}} operator {{opToken}}({{parameters}})
            {
                {{body}}
            }
            """;
    }

    private static string MapOperatorToken(OperatorKind op) => op switch
    {
        OperatorKind.Add => "+",
        OperatorKind.Subtract => "-",
        OperatorKind.Multiply => "*",
        OperatorKind.Divide => "/",
        OperatorKind.Modulo => "%",
        OperatorKind.Equality => "==",
        OperatorKind.Inequality => "!=",
        OperatorKind.GreaterThan => ">",
        OperatorKind.LessThan => "<",
        OperatorKind.GreaterThanOrEqual => ">=",
        OperatorKind.LessThanOrEqual => "<=",
        OperatorKind.UnaryPlus => "+",
        OperatorKind.UnaryNegation => "-",
        OperatorKind.LogicalNot => "!",
        OperatorKind.BitwiseComplement => "~",
        OperatorKind.Increment => "++",
        OperatorKind.Decrement => "--",
        OperatorKind.True => "true",
        OperatorKind.False => "false",
        OperatorKind.BitwiseAnd => "&",
        OperatorKind.BitwiseOr => "|",
        OperatorKind.ExclusiveOr => "^",
        OperatorKind.LeftShift => "<<",
        OperatorKind.RightShift => ">>",
        _ => "+",
    };

    private string GenerateDelegate(DelegateElement dele)
    {
        var accessMod = MapAccessModifier(dele.AccessModifier);
        var returnType = MapType(dele.ReturnType);
        var typeParams = RenderTypeParameterList(dele.TypeParameters);
        var constraints = RenderConstraintClauses(dele.TypeParameters);
        var parameters = string.Join(", ", dele.Parameters.Select(p =>
        {
            var modifier = MapParameterModifier(p.Modifier);
            var modifierStr = modifier.Length > 0 ? $"{modifier} " : "";
            return $"{modifierStr}{MapType(p.Type)} {p.Name}";
        }));
        var usings = dele.Usings.Count > 0
            ? string.Join(Environment.NewLine, dele.Usings.Select(u => $"using {u};")) + Environment.NewLine + Environment.NewLine
            : "";

        return $"{usings}{accessMod} delegate {returnType} {dele.Name}{typeParams}({parameters}){constraints};";
    }

    private static string RenderProperty(PropertyElement prop)
    {
        return TemplateManager.Instance.Render("Property", new Dictionary<string, object?>
        {
            { "name", prop.Name },
            { "type", MapType(prop.Type) },
            { "access_modifier", MapAccessModifier(prop.AccessModifier) },
            { "is_static", prop.IsStatic },
            { "is_required", prop.IsRequired },
            { "has_getter", prop.HasGetter },
            { "has_setter", prop.HasSetter },
            { "is_init_only", prop.IsInitOnly },
            { "default_value", !string.IsNullOrWhiteSpace(prop.DefaultValue) ? prop.DefaultValue : null }
        });
    }

    private static string RenderPropertySignature(PropertyElement prop)
    {
        // Pro interface — vždy get; set;
        var getter = prop.HasGetter ? "get; " : "";
        var setter = prop.HasSetter ? "set; " : "";
        return $"{MapType(prop.Type)} {prop.Name} {{ {getter}{setter}}}";
    }

    private string RenderMethod(MethodElement method)
    {
        var parameters = method.Parameters.Select((p, idx) => new Dictionary<string, object?>
        {
            { "name", p.Name },
            { "type", MapType(p.Type) },
            { "modifier", idx == 0 && method.IsExtensionMethod ? "this" : MapParameterModifier(p.Modifier) },
            { "default_value", p.HasDefaultValue && p.DefaultValue is not null ? p.DefaultValue : null }
        }).ToList();

        // Určení návratového typu (async wrapping)
        var returnType = MapType(method.ReturnType);
        if (method.IsAsync && method.ReturnType.BaseType == DataType.Void)
            returnType = "Task";
        else if (method.IsAsync)
            returnType = $"Task<{returnType}>";

        var model = new Dictionary<string, object?>
        {
            { "name", method.Name },
            { "return_type", returnType },
            { "access_modifier", MapAccessModifier(method.AccessModifier) },
            { "is_static", method.IsStatic },
            { "is_virtual", method.IsVirtual },
            { "is_abstract", method.IsAbstract },
            { "is_override", method.IsOverride },
            { "is_async", method.IsAsync },
            { "type_params", RenderTypeParameterList(method.TypeParameters) },
            { "constraints", RenderConstraintClauses(method.TypeParameters) },
            { "parameters", parameters },
            { "attributes", method.Attributes.Select(RenderAttribute).ToList() },
            { "body", RenderMethodBody(method) }
        };

        return RenderTemplate("Method", model);
    }

    private static string RenderMethodSignature(MethodElement method)
    {
        var returnType = MapType(method.ReturnType);
        if (method.IsAsync && method.ReturnType.BaseType == DataType.Void)
            returnType = "Task";
        else if (method.IsAsync)
            returnType = $"Task<{returnType}>";

        var parameters = string.Join(", ", method.Parameters.Select(p =>
        {
            var modifier = MapParameterModifier(p.Modifier);
            var modifierStr = modifier.Length > 0 ? $"{modifier} " : "";
            var type = MapType(p.Type);
            var defaultValue = p.HasDefaultValue && p.DefaultValue is not null
                ? $" = {p.DefaultValue}" : "";
            return $"{modifierStr}{type} {p.Name}{defaultValue}";
        }));

        var accessMod = MapAccessModifier(method.AccessModifier);
        var staticMod = method.IsStatic ? "static " : "";
        var asyncMod = method.IsAsync ? "async " : "";
        var typeParams = RenderTypeParameterList(method.TypeParameters);
        var constraints = RenderConstraintClauses(method.TypeParameters);

        return $"{accessMod} {staticMod}{asyncMod}{returnType} {method.Name}{typeParams}({parameters}){constraints}";
    }

    /// <summary>
    /// Vyrenderuje tělo metody — buď pomocí AST (BlockStatement) nebo vrátí prázdný string.
    /// Abstraktní metody (Body == null) vracejí ";" (bez těla).
    /// </summary>
    private string RenderMethodBody(MethodElement method)
    {
        if (method.IsAbstract || method.Body is null)
            return ";";

        return _renderer.Render(method.Body);
    }

    private static string RenderAttribute(AttributeElement attr)
    {
        var args = attr.Arguments.Count > 0
            ? string.Join(", ", attr.Arguments.Select(a => a is string s ? $"\"{s}\"" : a?.ToString() ?? "null"))
            : "";
        return $"[{attr.Name}{(args.Length > 0 ? $"({args})" : "")}]";
    }

    // === Mapovací helpery ===

    /// <summary>Vyrenderuje `&lt;T, U&gt;` seznam generických typových parametrů (prázdné, pokud žádné nejsou).</summary>
    private static string RenderTypeParameterList(List<TypeParameterElement> typeParameters)
    {
        if (typeParameters.Count == 0)
            return "";

        var names = typeParameters.Select(tp => tp.Variance switch
        {
            GenericVariance.Out => $"out {tp.Name}",
            GenericVariance.In => $"in {tp.Name}",
            _ => tp.Name,
        });

        return $"<{string.Join(", ", names)}>";
    }

    /// <summary>Vyrenderuje `where T : ... where U : ...` klauzule (prázdné, pokud žádná omezení nejsou).</summary>
    private static string RenderConstraintClauses(List<TypeParameterElement> typeParameters)
    {
        var clauses = typeParameters
            .Where(tp => tp.Constraints.Count > 0)
            .Select(tp => $" where {tp.Name} : {string.Join(", ", tp.Constraints.Select(RenderConstraint))}");

        return string.Concat(clauses);
    }

    private static string RenderConstraint(GenericConstraint constraint) => constraint.Kind switch
    {
        GenericConstraintKind.Class => "class",
        GenericConstraintKind.Struct => "struct",
        GenericConstraintKind.NotNull => "notnull",
        GenericConstraintKind.Unmanaged => "unmanaged",
        GenericConstraintKind.NewConstructor => "new()",
        GenericConstraintKind.Default => "default",
        GenericConstraintKind.BaseType => constraint.TypeName ?? "object",
        GenericConstraintKind.Interface => constraint.TypeName ?? "object",
        _ => "class",
    };

    private static string MapAccessModifier(AccessModifier am) => am switch
    {
        AccessModifier.Public => "public",
        AccessModifier.Internal => "internal",
        AccessModifier.Protected => "protected",
        AccessModifier.Private => "private",
        AccessModifier.ProtectedInternal => "protected internal",
        AccessModifier.PrivateProtected => "private protected",
        _ => "public"
    };

    private static string MapParameterModifier(ParameterModifier pm) => pm switch
    {
        ParameterModifier.Ref => "ref",
        ParameterModifier.Out => "out",
        ParameterModifier.In => "in",
        ParameterModifier.Params => "params",
        _ => ""
    };

    internal static string MapType(TypeModel type)
    {
        var nullable = type.IsNullable ? "?" : "";
        var baseType = MapDataType(type.BaseType);

        // Custom název
        if (!string.IsNullOrWhiteSpace(type.CustomTypeName))
            return $"{type.CustomTypeName}{nullable}";

        // Kolekce
        if (type.IsCollection)
        {
            var innerType = type.GenericArguments.Count > 0
                ? MapType(type.GenericArguments[0])
                : "object";
            return $"List<{innerType}>";
        }

        return $"{baseType}{nullable}";
    }

    internal static string MapDataType(DataType dataType) => dataType switch
    {
        DataType.Bool => "bool",
        DataType.Byte => "byte",
        DataType.SByte => "sbyte",
        DataType.Int16 => "short",
        DataType.UInt16 => "ushort",
        DataType.Int32 => "int",
        DataType.UInt32 => "uint",
        DataType.Int64 => "long",
        DataType.UInt64 => "ulong",
        DataType.Int128 => "Int128",
        DataType.Half => "Half",
        DataType.Single => "float",
        DataType.Double => "double",
        DataType.Decimal => "decimal",
        DataType.NInt => "nint",
        DataType.NUInt => "nuint",
        DataType.Char => "char",
        DataType.String => "string",
        DataType.Binary => "byte[]",
        DataType.DateOnly => "DateOnly",
        DataType.TimeOnly => "TimeOnly",
        DataType.DateTime => "DateTime",
        DataType.DateTimeOffset => "DateTimeOffset",
        DataType.TimeSpan => "TimeSpan",
        DataType.Guid => "Guid",
        DataType.Uri => "Uri",
        DataType.Version => "Version",
        DataType.Object => "object",
        DataType.Void => "void",
        DataType.Dynamic => "dynamic",
        DataType.Entity => "object",
        DataType.EnumValue => "int",
        DataType.Array => "object[]",
        DataType.Nullable => "object",
        DataType.Struct => "object",
        DataType.Record => "object",
        _ => "object"
    };
}
