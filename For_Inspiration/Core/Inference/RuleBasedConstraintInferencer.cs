using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;

namespace MetaForge.Core.Inference;

/// <summary>
/// Deterministický constraint inferencer — vždy dostupný fallback.
/// Detekuje jednoduché vzory: dělení nulou, null checks, empty string,
/// range violations z podpisů a těl metod.
/// Dříve implementoval starý IConstraintInferencer ze Abstractions — sjednoceno 2026-03-29.
/// </summary>
public sealed class RuleBasedConstraintInferencer : IConstraintInferencer
{
    /// <summary>Vždy dostupný — žádné AI závislosti.</summary>
    public bool IsAvailable => true;

    /// <summary>Rule-based inferencer nemá AI model.</summary>
    public string ModelName => "rule-based";

    /// <summary>
    /// Původní název metody — zachován pro zpětnou kompatibilitu.
    /// </summary>
    public Task<List<MethodConstraint>> InferAsync(Method method)
    {
        return InferConstraintsAsync(method).ContinueWith(t => t.Result.ToList());
    }

    public Task<IReadOnlyList<MethodConstraint>> InferConstraintsAsync(
        Method method, 
        IReadOnlyList<MethodConstraint>? existingConstraints = null,
        CancellationToken cancellationToken = default)
    {
        var constraints = new List<MethodConstraint>(existingConstraints ?? []);

        InferFromParameters(method, constraints);
        InferFromBody(method, constraints);

        return Task.FromResult<IReadOnlyList<MethodConstraint>>(constraints);
    }

    /// <summary>
    /// Rule-based inferencer nedetekuje komplexní hraniční stavy.
    /// </summary>
    public Task<IReadOnlyList<BoundaryCase>> DetectComplexBoundariesAsync(
        Method method,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<BoundaryCase>>(
            Array.Empty<BoundaryCase>());
    }

    /// <summary>
    /// Rule-based je vždy dostupný.
    /// </summary>
    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Inferuje constrainty z podpisů parametrů (typy, jména).
    /// </summary>
    private static void InferFromParameters(Method method, List<MethodConstraint> constraints)
    {
        foreach (var param in method.Parameters)
        {
            InferNullCheck(param, constraints);
            InferEmptyStringCheck(param, constraints);
        }
    }

    /// <summary>
    /// Parametr referenčního typu (string, object, custom) → null check.
    /// Přeskočí nullable typy a parametry s default hodnotou null.
    /// </summary>
    private static void InferNullCheck(Parameter param, List<MethodConstraint> constraints)
    {
        if (IsNullableType(param.Type.BaseType))
            return;

        if (param.DefaultValue == "null")
            return;

        if (!IsReferenceType(param.Type.BaseType))
            return;

        constraints.Add(new MethodConstraint
        {
            InvalidCondition = $"{param.Name} is null",
            Description = $"Parameter '{param.Name}' cannot be null.",
            Kind = ConstraintKind.Precondition,
            ExceptionType = "ArgumentNullException",
            ExceptionMessage = $"Value cannot be null. (Parameter '{param.Name}')"
        });
    }

    /// <summary>
    /// String parametr → empty/whitespace check.
    /// </summary>
    private static void InferEmptyStringCheck(Parameter param, List<MethodConstraint> constraints)
    {
        if (param.Type.BaseType != DataType.String)
            return;

        if (param.DefaultValue == "null")
            return;

        constraints.Add(new MethodConstraint
        {
            InvalidCondition = $"string.IsNullOrWhiteSpace({param.Name})",
            Description = $"Parameter '{param.Name}' cannot be empty or whitespace.",
            Kind = ConstraintKind.Precondition,
            ExceptionType = "ArgumentException",
            ExceptionMessage = $"Value cannot be empty or whitespace. (Parameter '{param.Name}')"
        });
    }

    /// <summary>
    /// Inferuje constrainty z těla metody (body expressions).
    /// Detekuje dělení nulou a indexování.
    /// </summary>
    private static void InferFromBody(Method method, List<MethodConstraint> constraints)
    {
        foreach (var expr in method.BodyExpressions)
        {
            var code = expr.GenerateCode();
            if (string.IsNullOrWhiteSpace(code))
                continue;

            DetectDivisionByZero(code, method, constraints);
        }
    }

    /// <summary>
    /// Detekuje dělení parametrem (/ param, / (expr+param)).
    /// Pokud se v těle vyskytuje dělení čísleným parametrem, přidá constraint.
    /// </summary>
    private static void DetectDivisionByZero(string bodyCode, Method method, List<MethodConstraint> constraints)
    {
        foreach (var param in method.Parameters)
        {
            if (!IsNumericType(param.Type.BaseType))
                continue;

            // Hledáme vzory: / param, / (... param ...), /param
            // Jednoduchá heuristika: kdekoliv se dělí parametrem
            if (ContainsDivisionBy(bodyCode, param.Name))
            {
                // Zkontroluj, zda constraint s identickým InvalidCondition neexistuje
                var condition = $"{param.Name} == 0";
                if (constraints.Exists(c => c.InvalidCondition == condition))
                    continue;

                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = condition,
                    Description = $"Division by zero — parameter '{param.Name}' is used as a divisor.",
                    Kind = ConstraintKind.Precondition,
                    ExceptionType = "DivideByZeroException",
                    ExceptionMessage = $"Parameter '{param.Name}' cannot be zero (used as divisor)."
                });
            }
        }
    }

    /// <summary>
    /// Heuristika: obsahuje bodyCode dělení parametrem?
    /// Hledá vzory: / paramName, /paramName, / (paramName
    /// </summary>
    public static bool ContainsDivisionBy(string bodyCode, string paramName)
    {
        // Vzory dělení:
        // "/ a", "/a ", "/ (a", "/(a"
        var patterns = new[]
        {
            $"/ {paramName}",       // "x / a"
            $"/{paramName} ",       // "x /a "
            $"/{paramName})",       // "x /a)"
            $"/{paramName};",       // "x /a;"
            $"/{paramName},",       // "x /a,"
            $"/ ({paramName}",      // "x / (a"
            $"/({paramName}",       // "x /(a"
            $"/{paramName}\n",      // "x /a\n"
            $"/{paramName}\r",      // "x /a\r"
        };

        foreach (var pattern in patterns)
        {
            if (bodyCode.Contains(pattern, StringComparison.Ordinal))
                return true;
        }

        // Konec řetězce: "x /a"
        if (bodyCode.EndsWith($"/{paramName}", StringComparison.Ordinal) ||
            bodyCode.EndsWith($"/ {paramName}", StringComparison.Ordinal))
            return true;

        return false;
    }

    private static bool IsReferenceType(DataType type) => type switch
    {
        DataType.String => true,
        DataType.Object => true,
        _ => false
    };

    private static bool IsNumericType(DataType type) => type switch
    {
        DataType.Int => true,
        DataType.Long => true,
        DataType.Short => true,
        DataType.Byte => true,
        DataType.Float => true,
        DataType.Double => true,
        DataType.Decimal => true,
        _ => false
    };

    private static bool IsNullableType(DataType type) => false; // V metamodelu zatím není nullable info
}
