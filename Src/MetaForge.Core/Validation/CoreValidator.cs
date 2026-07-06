using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Validation;

/// <summary>
/// Validátor Core elementů — kontroluje nevalidní kombinace dle integrační matice.
/// Pokrývá všechny ❌ řádky z Docs/Integration/01-Integration-Test-Matrix.md.
/// </summary>
public static class CoreValidator
{
    // === Validační kódy matice ===
    private static class Codes
    {
        public const string C9 = "C9";   // abstract + sealed
        public const string C10 = "C10"; // abstract + static
        public const string C12 = "C12"; // static + record
        public const string A3 = "A3";   // private top-level
        public const string A4 = "A4";   // protected top-level
        public const string A5 = "A5";   // private protected top-level
        public const string I5 = "I5";   // dědění od sealed typu
        public const string E5 = "E5";   // string jako underlying enum typ
        public const string E6 = "E6";   // bool jako underlying enum typ
        public const string P7 = "P7";   // property bez getteru i setteru
        public const string T19 = "T19"; // void jako property type
        public const string T20 = "T20"; // void? jako property type
        public const string T21 = "T21"; // List<void> jako property type
        public const string M9 = "M9";   // abstract + virtual
        public const string M10 = "M10"; // abstract + override
        public const string M11 = "M11"; // static + abstract
        public const string M12 = "M12"; // virtual + override
        public const string B11 = "B11"; // if s ne-bool podmínkou
        public const string B12 = "B12"; // return value ve void metodě
        public const string B13 = "B13"; // return bez value v non-void metodě
    }

    /// <summary>
    /// Vrátí seznam validačních problémů. Prázdný seznam = validní.
    /// </summary>
    public static IReadOnlyList<ValidationIssue> Validate(RootElement element)
    {
        return element switch
        {
            ClassElement c => ValidateClass(c),
            EnumElement e => ValidateEnum(e),
            StructElement => [], // Všechny kombinace structu jsou validní (S1-S4)
            InterfaceElement => [], // Interface nemá konfliktní kombinace
            _ => [],
        };
    }

    /// <summary>
    /// Vyhodí <see cref="InvalidOperationException"/> při prvním validačním problému.
    /// Pro fail-fast scénáře.
    /// </summary>
    public static void EnsureValid(RootElement element)
    {
        var issues = Validate(element);
        if (issues.Count > 0)
        {
            throw new InvalidOperationException(
                $"Element '{element.Name}' není validní: {string.Join("; ", issues)}");
        }
    }

    /// <summary>
    /// Zvaliduje <see cref="MethodElement"/> (není RootElement, proto samostatná metoda).
    /// </summary>
    public static IReadOnlyList<ValidationIssue> ValidateMethod(MethodElement method)
    {
        var issues = new List<ValidationIssue>();

        // M9: abstract + virtual — konflikt
        if (method.IsAbstract && method.IsVirtual)
            issues.Add(new(Codes.M9, ValidationCategories.ConflictingModifiers,
                "abstract a virtual nelze kombinovat — abstract je implicitně virtual"));

        // M10: abstract + override — konflikt
        if (method.IsAbstract && method.IsOverride)
            issues.Add(new(Codes.M10, ValidationCategories.ConflictingModifiers,
                "abstract a override nelze kombinovat — abstract deklaruje, override přepisuje"));

        // M11: static + abstract — konflikt (mimo interface)
        if (method.IsStatic && method.IsAbstract)
            issues.Add(new(Codes.M11, ValidationCategories.ConflictingModifiers,
                "static abstract lze pouze v interface (C# 11+), ne v class/struct"));

        // M12: virtual + override — konflikt
        if (method.IsVirtual && method.IsOverride)
            issues.Add(new(Codes.M12, ValidationCategories.ConflictingModifiers,
                "virtual a override nelze kombinovat — override přepisuje virtual metodu"));

        // M13: abstraktní metoda bez těla — jen varování
        if (method.IsAbstract && method.Body is not null)
            issues.Add(new(Codes.M10 + "_body", ValidationCategories.Warning,
                "Abstraktní metoda by neměla mít tělo (Body bude ignorováno)"));

        // Kontrola statementů v těle (B11-B13)
        if (method.Body is not null)
            issues.AddRange(ValidateStatements(method.Body, method));

        return issues;
    }

    /// <summary>
    /// Zvaliduje <see cref="PropertyElement"/>.
    /// </summary>
    public static IReadOnlyList<ValidationIssue> ValidateProperty(PropertyElement property)
    {
        var issues = new List<ValidationIssue>();

        // P7: property bez getteru i setteru
        if (!property.HasGetter && !property.HasSetter)
            issues.Add(new(Codes.P7, ValidationCategories.MissingRequired,
                "Property musí mít alespoň getter nebo setter"));

        // T19: void jako property type
        if (property.Type.BaseType == DataType.Void && !property.Type.IsCollection)
            issues.Add(new(Codes.T19, ValidationCategories.InvalidType,
                "void nelze použít jako typ property"));

        // T20: void? — nevalidní
        if (property.Type.BaseType == DataType.Void && property.Type.IsNullable)
            issues.Add(new(Codes.T20, ValidationCategories.InvalidType,
                "void? není validní typ"));

        // T21: List<void> — nevalidní
        if (property.Type.BaseType == DataType.Void && property.Type.IsCollection)
            issues.Add(new(Codes.T21, ValidationCategories.InvalidType,
                "List<void> není validní typ"));

        return issues;
    }

    // === Privátní validátory ===

    private static IReadOnlyList<ValidationIssue> ValidateClass(ClassElement c)
    {
        var issues = new List<ValidationIssue>();

        // C9: abstract + sealed — konfliktní
        if (c.IsAbstract && c.IsSealed)
            issues.Add(new(Codes.C9, ValidationCategories.ConflictingModifiers,
                "abstract sealed — konfliktní kombinace"));

        // C10: abstract + static — konfliktní
        if (c.IsAbstract && c.IsStatic)
            issues.Add(new(Codes.C10, ValidationCategories.ConflictingModifiers,
                "abstract static — konfliktní kombinace"));

        // C12: static + record — nelze
        if (c.IsStatic && c.IsRecord)
            issues.Add(new(Codes.C12, ValidationCategories.ConflictingModifiers,
                "static record — nelze kombinovat"));

        // A3: private top-level — nevalidní
        if (c.AccessModifier is AccessModifier.Private)
            issues.Add(new(Codes.A3, ValidationCategories.InvalidAccess,
                "private nelze použít na top-level třídě"));

        // A4: protected top-level — nevalidní
        if (c.AccessModifier is AccessModifier.Protected)
            issues.Add(new(Codes.A4, ValidationCategories.InvalidAccess,
                "protected nelze použít na top-level třídě"));

        // A5: private protected top-level — nevalidní
        if (c.AccessModifier is AccessModifier.PrivateProtected)
            issues.Add(new(Codes.A5, ValidationCategories.InvalidAccess,
                "private protected nelze použít na top-level třídě"));

        // I5: dědění od sealed typu — kontrola názvu (MVP: jen varování pro známé sealed typy)
        if (!string.IsNullOrEmpty(c.BaseClassName) && IsKnownSealedType(c.BaseClassName))
            issues.Add(new(Codes.I5, ValidationCategories.InvalidInheritance,
                $"Nelze dědit od '{c.BaseClassName}' — typ je sealed"));

        // Validace properties a metod uvnitř třídy
        foreach (var prop in c.Properties)
            issues.AddRange(ValidateProperty(prop));

        foreach (var method in c.Methods)
            issues.AddRange(ValidateMethod(method));

        return issues;
    }

    private static IReadOnlyList<ValidationIssue> ValidateEnum(EnumElement e)
    {
        var issues = new List<ValidationIssue>();

        // E5: string není validní underlying typ pro enum
        if (e.UnderlyingType == DataType.String)
            issues.Add(new(Codes.E5, ValidationCategories.InvalidType,
                "string není validní underlying typ pro enum (pouze celočíselné typy)"));

        // E6: bool není validní underlying typ pro enum
        if (e.UnderlyingType == DataType.Bool)
            issues.Add(new(Codes.E6, ValidationCategories.InvalidType,
                "bool není validní underlying typ pro enum (pouze celočíselné typy)"));

        return issues;
    }

    private static IReadOnlyList<ValidationIssue> ValidateStatements(BlockStatement block, MethodElement method)
    {
        var issues = new List<ValidationIssue>();
        foreach (var stmt in block.Statements)
            issues.AddRange(ValidateStatement(stmt, method));
        return issues;
    }

    private static IReadOnlyList<ValidationIssue> ValidateStatement(Statement stmt, MethodElement method)
    {
        var issues = new List<ValidationIssue>();

        switch (stmt)
        {
            case IfStatement ifStmt:
                // B11: if s ne-bool podmínkou
                if (ifStmt.Condition.ResultType.BaseType != DataType.Bool)
                    issues.Add(new(Codes.B11, ValidationCategories.StatementTypeError,
                        $"if podmínka musí být bool, ale je '{ifStmt.Condition.ResultType.BaseType}'"));
                break;

            case ReturnStatement retStmt:
                var returnsVoid = method.ReturnType.IsVoid;
                // B12: return value ve void metodě
                if (returnsVoid && retStmt.Value is not null)
                    issues.Add(new(Codes.B12, ValidationCategories.StatementTypeError,
                        "return s hodnotou ve void metodě"));
                // B13: return bez value v non-void metodě
                if (!returnsVoid && retStmt.Value is null)
                    issues.Add(new(Codes.B13, ValidationCategories.StatementTypeError,
                        "return bez hodnoty v non-void metodě"));
                break;

            case BlockStatement block:
                issues.AddRange(ValidateStatements(block, method));
                break;
        }

        return issues;
    }

    // === Pomocné metody ===

    /// <summary>
    /// Známé sealed typy v .NET — pro kontrolu I5.
    /// </summary>
    private static readonly HashSet<string> KnownSealedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "String",
        "int", "Int32",
        "long", "Int64",
        "bool", "Boolean",
        "decimal", "Decimal",
        "double", "Double",
        "float", "Single",
        "byte", "Byte",
        "short", "Int16",
        "char", "Char",
        "Guid",
        "DateTime", "DateTimeOffset",
        "TimeSpan",
        "Uri",
        "Version",
    };

    private static bool IsKnownSealedType(string typeName) =>
        KnownSealedTypes.Contains(typeName);
}
